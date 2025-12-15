// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     ApplicationDbContext.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp
// =======================================================

using Microsoft.EntityFrameworkCore;

namespace AINotesApp.Data;

public class ApplicationDbContext
(
		DbContextOptions<ApplicationDbContext> options
) : DbContext(options)
{

	public DbSet<AppUser> AppUsers => Set<AppUser>();

	public DbSet<Note> Notes => Set<Note>();

	protected override void OnModelCreating(ModelBuilder builder)
	{
		builder.Entity<AppUser>(entity =>
		{
			entity.HasKey(u => u.Auth0Subject);

			entity.Property(u => u.Auth0Subject)
					.IsRequired()
					.HasMaxLength(64);

			entity.Property(u => u.Name)
					.HasMaxLength(200);

			entity.Property(u => u.Email)
					.HasMaxLength(256);

			entity.Property(u => u.CreatedUtc)
					.IsRequired();

			entity.HasIndex(u => u.Email);
		});

		builder.Entity<Note>(entity =>
		{
			entity.HasKey(e => e.Id);

			entity.Property(e => e.Title)
					.IsRequired()
					.HasMaxLength(200);

			entity.Property(e => e.Content)
					.IsRequired()
					.HasMaxLength(2000);

			entity.Property(e => e.AiSummary)
					.HasMaxLength(1000);

			entity.Property(e => e.Tags)
					.HasMaxLength(500);

			entity.Property(e => e.OwnerSubject)
					.IsRequired()
					.HasMaxLength(64);

			entity.Property(e => e.CreatedAt)
					.IsRequired();

			entity.Property(e => e.UpdatedAt)
					.IsRequired();

			entity.HasOne(e => e.Owner)
					.WithMany(u => u.Notes)
					.HasForeignKey(e => e.OwnerSubject)
					.HasPrincipalKey(u => u.Auth0Subject)
					.OnDelete(DeleteBehavior.Cascade);

			entity.HasIndex(e => e.OwnerSubject);
			entity.HasIndex(e => e.CreatedAt);
		});
	}

}