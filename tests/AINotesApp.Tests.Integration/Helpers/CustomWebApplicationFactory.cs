// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     CustomWebApplicationFactory.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp.Tests.Integration
// =======================================================

using System.Diagnostics.CodeAnalysis;
using AINotesApp.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AINotesApp.Tests.Integration.Helpers;

/// <summary>
/// Custom WebApplicationFactory for integration tests that provides test-safe configuration
/// for Auth0 and other dependencies.
/// </summary>
[ExcludeFromCodeCoverage]
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.ConfigureAppConfiguration((context, config) =>
		{
			// Add in-memory configuration to override Auth0 settings with test values
			var testConfig = new Dictionary<string, string?>
			{
				// Provide dummy Auth0 configuration for integration tests
				// These values prevent the "Client Secret and Client Assertion can not be null" error
				["Auth0:Domain"] = "test-tenant.auth0.com",
				["Auth0:ClientId"] = "test-client-id",
				["Auth0:ClientSecret"] = "test-client-secret-for-integration-tests",
				["Auth0:Audience"] = "https://api.test.local",
				["Auth0:CallbackPath"] = "/auth/callback",
				["Auth0:LogoutPath"] = "/auth/logout",

				// Use in-memory database for testing
				["ConnectionStrings:DefaultConnection"] = "DataSource=:memory:",

				// Disable OpenAI for integration tests (can be overridden per test if needed)
				["AiService:ApiKey"] = "test-api-key",
				["AiService:ChatModel"] = "gpt-4o",
				["AiService:EmbeddingModel"] = "text-embedding-3-small"
			};

			config.AddInMemoryCollection(testConfig);
		});

		builder.ConfigureServices(services =>
		{
			// Remove the existing ApplicationDbContext registration
			var dbContextDescriptor = services.SingleOrDefault(
				d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
			if (dbContextDescriptor != null)
			{
				services.Remove(dbContextDescriptor);
			}

			var dbContextDescriptor2 = services.SingleOrDefault(
				d => d.ServiceType == typeof(ApplicationDbContext));
			if (dbContextDescriptor2 != null)
			{
				services.Remove(dbContextDescriptor2);
			}

			// Add DbContext using in-memory database for testing
			services.AddDbContext<ApplicationDbContext>(options =>
			{
				options.UseInMemoryDatabase("InMemoryTestDb");
			});
		});
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}
}
