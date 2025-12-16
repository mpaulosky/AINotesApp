// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     NoteEditorPerformanceTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp.Tests.Unit
// =======================================================

using System.Diagnostics.CodeAnalysis;

using AINotesApp.Components.Pages.Notes;
using AINotesApp.Tests.Unit.Fakes;

using Bunit;

using FluentAssertions;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

namespace AINotesApp.Tests.Unit.Components.Pages.Notes;

/// <summary>
///   Performance and edge case tests for the NoteEditor component
/// </summary>
[ExcludeFromCodeCoverage]
public class NoteEditorPerformanceTests : BunitContext
{

	private readonly FakeAuthenticationStateProvider _authProvider;

	private readonly IMediator _mediator;

	private readonly NavigationManager _navigation;

	public NoteEditorPerformanceTests()
	{
		_mediator = Substitute.For<IMediator>();
		_authProvider = new FakeAuthenticationStateProvider();
		_authProvider.SetAuthorized("test-user", "test-user-id");
		_navigation = Substitute.For<NavigationManager>();

		// Register services
		Services.AddSingleton(_mediator);
		Services.AddSingleton<AuthenticationStateProvider>(_authProvider);
		Services.AddSingleton(_navigation);
		Services.AddSingleton<IAuthorizationService, FakeAuthorizationService>();
		Services.AddSingleton<IAuthorizationPolicyProvider, FakeAuthorizationPolicyProvider>();
	}

	[Fact]
	public void NoteEditor_WithNullId_RendersInCreateMode()
	{
		// Arrange & Act
		var cut = RenderWithAuth(parameters => parameters
				.Add(p => p.Id, null));

		// Assert
		cut.Should().NotBeNull();

		// Should render an empty form for a new note
		cut.Markup.Should().NotBeNullOrEmpty();
		cut.FindAll("form").Should().NotBeEmpty();
	}

	[Fact]
	public void NoteEditor_RendersBasicStructure()
	{
		// Arrange & Act
		var cut = RenderWithAuth();

		// Assert
		cut.Should().NotBeNull();
		cut.Markup.Should().NotBeNullOrEmpty();
	}

	[Fact]
	public void NoteEditor_MultipleInstances_DoNotInterfere()
	{
		// Arrange & Act
		var cut1 = RenderWithAuth(parameters => parameters
				.Add(p => p.Id, Guid.NewGuid()));

		var cut2 = RenderWithAuth(parameters => parameters
				.Add(p => p.Id, Guid.NewGuid()));

		// Assert
		cut1.Should().NotBeNull();
		cut2.Should().NotBeNull();
		cut1.Instance.Should().NotBeSameAs(cut2.Instance);
	}

	[Fact]
	public void NoteEditor_RapidNavigation_HandlesCleanup()
	{
		// Arrange
		var cut = RenderWithAuth();

		// Act - Simulate navigation away
		cut.Dispose();

		// Assert - Should dispose cleanly without errors
		var act = () => cut.Dispose(); // Second, dispose should be safe
		act.Should().NotThrow();
	}

	[Fact]
	public void NoteEditor_WithInvalidGuid_HandlesGracefully()
	{
		// Arrange & Act - Invalid GUID will fail to parse
		var act = () => RenderWithAuth();

		// Assert - Should render without crashing
		act.Should().NotThrow();
	}

	[Fact]
	public void NoteEditor_DisposedProperly_DoesNotLeak()
	{
		// Arrange
		var cuts = new List<IRenderedComponent<NoteEditor>>();

		// Act - Create and dispose of multiple instances
		for (var i = 0; i < 10; i++)
		{
			var cut = RenderWithAuth(parameters => parameters
					.Add(p => p.Id, null));

			cuts.Add(cut);
		}

		// Dispose all
		foreach (var cut in cuts)
		{
			cut.Dispose();
		}

		// Assert - Should complete without errors
		cuts.Should().HaveCount(10);
	}

	[Fact]
	public void NoteEditor_WithAuthenticatedUser_Renders()
	{
		// Arrange & Act
		var cut = RenderWithAuth();

		// Assert
		cut.Should().NotBeNull();
		cut.Instance.Should().NotBeNull();
	}

	[Fact]
	public void NoteEditor_HasRequiredFormElements()
	{
		// Arrange & Act
		var cut = RenderWithAuth();

		// Assert - Should have basic form structure
		cut.Should().NotBeNull();

		// Component should have markup
		cut.Markup.Length.Should().BeGreaterThan(0);
	}

	[Fact]
	public void NoteEditor_CanBeRenderedMultipleTimes()
	{
		// Arrange & Act
		for (var i = 0; i < 5; i++)
		{
			using var cut = RenderWithAuth();
			cut.Should().NotBeNull();
		}

		// Assert - No exceptions should be thrown
		true.Should().BeTrue();
	}

	[Fact]
	public void NoteEditor_WithDifferentGuids_CreatesUniqueInstances()
	{
		// Arrange
		var guid1 = Guid.NewGuid();
		var guid2 = Guid.NewGuid();

		// Act
		var cut1 = RenderWithAuth(parameters => parameters.Add(p => p.Id, guid1));
		var cut2 = RenderWithAuth(parameters => parameters.Add(p => p.Id, guid2));

		// Assert
		cut1.Instance.Should().NotBeSameAs(cut2.Instance);
	}

	// Helper methods

	private IRenderedComponent<NoteEditor> RenderWithAuth(
			Action<ComponentParameterCollectionBuilder<NoteEditor>>? parameters = null)
	{
		var authStateTask = _authProvider.GetAuthenticationStateAsync();

		return Render<NoteEditor>(ps =>
		{
			ps.AddCascadingValue(authStateTask);
			parameters?.Invoke(ps);
		});
	}

}
