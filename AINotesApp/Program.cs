// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     Program.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp
// =======================================================

using System.Security.Claims;

using AINotesApp.Components;
using AINotesApp.Data;
using AINotesApp.Services.Ai;

using Auth0.AspNetCore.Authentication;

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Get configuration
IConfiguration configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddRazorComponents()
		.AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();

builder.Services
		.AddAuth0WebAppAuthentication(options =>
		{
			options.Domain = configuration["Auth0:Domain"] ?? string.Empty;
			options.ClientId = configuration["Auth0:ClientId"] ?? string.Empty;
			options.ClientSecret = configuration["Auth0:ClientSecret"] ?? string.Empty;
			options.Scope = "openid profile email";
			options.CallbackPath = configuration["Auth0:CallbackPath"] ?? "/auth/callback";
		})
		.WithAccessToken(options =>
		{
			var audience = configuration["Auth0:Audience"];

			if (!string.IsNullOrWhiteSpace(audience))
			{
				options.Audience = audience;
			}
		});

builder.Services.AddAuthorizationBuilder()
		.AddPolicy("NotesAdmin", policy =>
				policy.RequireClaim(ClaimTypes.Role, "notes.admin"));

var connectionString = configuration.GetConnectionString("DefaultConnection") ??
											throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
		options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Configure OpenAI options
builder.Services.Configure<AiServiceOptions>(
		configuration.GetSection(AiServiceOptions.SectionName));

// Register AI Service
builder.Services.AddScoped<IAiService, OpenAiService>();

// Register MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AINotesApp.Program).Assembly));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseMigrationsEndPoint();
}
else
{
	app.UseExceptionHandler("/Error", true);

	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
		.AddInteractiveServerRenderMode();

app.Run();

// Make the implicit Program class accessible to tests
namespace AINotesApp
{
	/// <summary>
	///   Partial Program class to enable test accessibility for the application's entry point.
	/// </summary>
	public  class Program { }
}