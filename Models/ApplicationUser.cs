using System;
// Models/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
	public ApplicationUser()
	{

	}

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? GoogleId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


}


