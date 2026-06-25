using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
	public AppDbContext(DbContextOptions<AppDbContext> options) : base (options)
	{
	}

	protected override void OnModelCreating(ModelBuilder builder)
	{
		base.onModelCreating(builder);
	}
}
