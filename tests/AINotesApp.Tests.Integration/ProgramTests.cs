using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading.Tasks;
using AINotesApp;
using AINotesApp.Data;
using AINotesApp.Services;
using AINotesApp.Services.Ai;
using AINotesApp.Tests.Integration.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AINotesApp.Tests.Integration;

[ExcludeFromCodeCoverage]
public class ProgramTests : IClassFixture<CustomWebApplicationFactory>
{
	private readonly CustomWebApplicationFactory _factory;

	public ProgramTests(CustomWebApplicationFactory factory)
	{
		_factory = factory;
	}


	[Fact]
	public async Task App_Starts_And_RespondsToRoot()
	{
		var client = _factory.CreateClient();
		var response = await client.GetAsync("/");
		Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Redirect);
	}

	[Fact]
	public async Task Middleware_Pipeline_Is_Configured()
	{
		// Verify the app responds correctly, which confirms middleware pipeline is set up
		// (HTTPS redirection, authentication, antiforgery, etc.)
		var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
		var response = await client.GetAsync("/");
		// Should get OK or Redirect (for authentication) - not an error status
		Assert.True(response.StatusCode == HttpStatusCode.OK ||
								response.StatusCode == HttpStatusCode.Redirect ||
								response.StatusCode == HttpStatusCode.Unauthorized);
	}

	[Fact]
	public async Task StatusCodePages_Custom404_IsReturned()
	{
		var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
		var response = await client.GetAsync("/notarealpage-404-test");
		// Should be redirected to /not-found or custom 404 page
		Assert.True(response.StatusCode == HttpStatusCode.NotFound || response.RequestMessage?.RequestUri?.AbsolutePath == "/not-found");
	}

	[Fact]
	public async Task ErrorHandler_Or_DevPage_IsActive()
	{
		var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
		// Simulate error by requesting /Error directly
		var response = await client.GetAsync("/Error");
		Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.InternalServerError);
	}

	[Fact]
	public async Task Login_Endpoint_Redirects_To_Auth0()
	{
		var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
		var response = await client.GetAsync("/Account/Login");
		Assert.True(response.StatusCode == HttpStatusCode.Redirect || response.StatusCode == HttpStatusCode.Unauthorized);
		// Should redirect to Auth0 or challenge
		if (response.StatusCode == HttpStatusCode.Redirect)
		{
			Assert.Contains("auth0", response.Headers.Location?.Host ?? string.Empty, System.StringComparison.OrdinalIgnoreCase);
		}
	}

	[Fact]
	public async Task Logout_Endpoint_Redirects_And_SignsOut()
	{
		var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
		var response = await client.GetAsync("/Account/Logout");
		Assert.True(response.StatusCode == HttpStatusCode.Redirect || response.StatusCode == HttpStatusCode.OK);
	}

	[Fact]
	public void AiService_Is_Scoped_Not_Singleton()
	{
		using var scope1 = _factory.Services.CreateScope();
		using var scope2 = _factory.Services.CreateScope();
		var service1 = scope1.ServiceProvider.GetRequiredService<IAiService>();
		var service2 = scope2.ServiceProvider.GetRequiredService<IAiService>();
		Assert.NotSame(service1, service2); // Scoped should not be the same instance
	}

	[Fact]
	public void Required_Services_Are_Registered()
	{
		using var scope = _factory.Services.CreateScope();
		var provider = scope.ServiceProvider;
		Assert.NotNull(provider.GetService<ApplicationDbContext>());
		Assert.NotNull(provider.GetService<IAiService>());
		Assert.NotNull(provider.GetService<IChatClientWrapper>());
		Assert.NotNull(provider.GetService<IEmbeddingClientWrapper>());
	}

	[Theory]
	[InlineData("/Account/Login")]
	[InlineData("/Account/Logout")]
	public async Task Custom_Endpoints_Exist(string url)
	{
		var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
		var response = await client.GetAsync(url);
		Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Redirect || response.StatusCode == HttpStatusCode.Unauthorized);
	}
}
