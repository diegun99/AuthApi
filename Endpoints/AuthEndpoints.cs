// Endpoints/AuthEndpoints.cs
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        // ═══════════════════════════════════════════════════════
        // REGISTRO CON EMAIL/PASSWORD
        // ═══════════════════════════════════════════════════════
        group.MapPost("/register", async (
            RegisterRequest req,
            UserManager<ApplicationUser> userManager) =>
        {
            // Verificar si el email ya existe
            var existingUser = await userManager.FindByEmailAsync(req.Email);
            if (existingUser != null)
                return Results.BadRequest(new { message = "El email ya está registrado" });

            var user = new ApplicationUser
            {
                UserName = req.Email,
                Email = req.Email,
                FirstName = req.FirstName,
                LastName = req.LastName
            };

            var result = await userManager.CreateAsync(user, req.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return Results.BadRequest(new { errors });
            }

            // Asignar rol por defecto
            await userManager.AddToRoleAsync(user, "User");

            return Results.Ok(new { message = "Usuario registrado exitosamente" });
        });

        // ═══════════════════════════════════════════════════════
        // LOGIN CON EMAIL/PASSWORD
        // ═══════════════════════════════════════════════════════
        group.MapPost("/login", async (
            LoginRequest req,
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService) =>
        {
            // Buscamos por email
            var user = await userManager.FindByEmailAsync(req.Email);
            
            // ⚠️ Seguridad: mismo mensaje para usuario inexistente o password incorrecto
            // Esto evita que un atacante descubra qué emails están registrados
            if (user == null || !await userManager.CheckPasswordAsync(user, req.Password))
                return Results.Unauthorized();

            var roles = await userManager.GetRolesAsync(user);
            var token = tokenService.CreateToken(user, roles);

            return Results.Ok(new AuthResponse(
                user.Id,
                user.Email!,
                user.FirstName,
                user.LastName,
                roles,
                token,
                DateTime.UtcNow.AddMinutes(60)
            ));
        });

        // ═══════════════════════════════════════════════════════
        // GOOGLE OAUTH - PASO 1: Redirigir a Google
        // ═══════════════════════════════════════════════════════
        group.MapGet("/google-login", () =>
        {
            // Redirige al usuario a Google para autenticarse
            return Results.Challenge(
                new AuthenticationProperties 
                { 
                    RedirectUri = "/api/auth/google-callback" 
                },
                new[] { GoogleDefaults.AuthenticationScheme });
        });

        // ═══════════════════════════════════════════════════════
        // GOOGLE OAUTH - PASO 2: Callback de Google
        // ═══════════════════════════════════════════════════════
        group.MapGet("/google-callback", async (
            HttpContext context,
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService) =>
        {
            // Google nos devuelve el resultado de la autenticación
            var result = await context.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            
            if (!result.Succeeded)
                return Results.Unauthorized();

            // Extraer datos de Google
            var googleId = result.Principal.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var email = result.Principal.FindFirstValue(ClaimTypes.Email)!;
            var firstName = result.Principal.FindFirstValue(ClaimTypes.GivenName) ?? "";
            var lastName = result.Principal.FindFirstValue(ClaimTypes.Surname) ?? "";

            // Buscar usuario existente
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                // Crear nuevo usuario con Google
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    GoogleId = googleId,
                    EmailConfirmed = true // Google ya verificó el email
                };
                
                var createResult = await userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                    return Results.BadRequest(createResult.Errors);

                await userManager.AddToRoleAsync(user, "User");
            }
            else if (user.GoogleId == null)
            {
                // Vincular Google a cuenta existente
                user.GoogleId = googleId;
                await userManager.UpdateAsync(user);
            }

            var roles = await userManager.GetRolesAsync(user);
            var token = tokenService.CreateToken(user, roles);

            // Redirigir al frontend con el token
            // En producción, usa un código temporal en lugar de pasar el token en URL
            var frontendUrl = "http://localhost:4200/auth/callback";
            return Results.Redirect($"{frontendUrl}?token={token}");
        });

        // ═══════════════════════════════════════════════════════
        // OBTENER DATOS DEL USUARIO ACTUAL (protegido)
        // ═══════════════════════════════════════════════════════
        group.MapGet("/me", [Microsoft.AspNetCore.Authorization.Authorize] 
            async (ClaimsPrincipal user, UserManager<ApplicationUser> userManager) =>
        {
            var appUser = await userManager.GetUserAsync(user);
            if (appUser == null) return Results.Unauthorized();

            var roles = await userManager.GetRolesAsync(appUser);
            
            return Results.Ok(new UserDto(
                appUser.Id,
                appUser.Email!,
                appUser.FirstName,
                appUser.LastName,
                roles
            ));
        });

        return app;
    }
}

// DTOs (Data Transfer Objects)
public record RegisterRequest(string Email, string Password, string FirstName, string LastName);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string Id, string Email, string? FirstName, string? LastName, IList<string> Roles, string Token, DateTime ExpiresAt);
public record UserDto(string Id, string Email, string? FirstName, string? LastName, IList<string> Roles);