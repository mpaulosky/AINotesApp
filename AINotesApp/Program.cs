// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     Program.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp
// =======================================================

using AINotesApp.Components;
using AINotesApp.Data;
using AINotesApp.Services;
using AINotesApp.Services.Ai;

using Auth0.AspNetCore.Authentication;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Get configuration
IConfiguration configuration = builder.Configuration;

// Authentication & Authorization
builder.Services.AddAuthenticationAndAuthorization(configuration);

// Add services to the container.
builder.Services.AddRazorComponents()
		.AddInteractiveServerComponents();

var connectionString = configuration.GetConnectionString("DefaultConnection") ??
											throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
		options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Configure OpenAI options
builder.Services.Configure<AiServiceOptions>(
		configuration.GetSection(AiServiceOptions.SectionName));

// Register OpenAI client wrappers
builder.Services.AddScoped<IChatClientWrapper>(sp =>
{
	var options = sp.GetRequiredService<IOptions<AiServiceOptions>>().Value;
	var chatClient = new OpenAI.Chat.ChatClient(options.ChatModel, options.ApiKey);
	return new OpenAiChatClientWrapper(chatClient);
});

builder.Services.AddScoped<IEmbeddingClientWrapper>(sp =>
{
	var options = sp.GetRequiredService<IOptions<AiServiceOptions>>().Value;
	var embeddingClient = new OpenAI.Embeddings.EmbeddingClient(options.EmbeddingModel, options.ApiKey);
	return new OpenAiEmbeddingClientWrapper(embeddingClient);
});

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
app.MapGet("/Account/Login", async void (HttpContext httpContext, string returnUrl = "/") =>
{
	var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
			.WithRedirectUri(returnUrl)
			.Build();

	await httpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
});

app.MapGet("/Account/Logout", async httpContext =>
{
	var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
			.WithRedirectUri("/")
			.Build();

	await httpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
	await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
});

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
	public class Program { }
}