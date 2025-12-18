// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     ComponentTestHelper.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp.Tests.Unit
// =======================================================

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Bunit;
using Microsoft.AspNetCore.Components;

namespace AINotesApp.Tests.Unit.Helpers;

/// <summary>
///   Shared helpers for BUnit component tests.
/// </summary>
[ExcludeFromCodeCoverage]
public static class ComponentTestHelper
{
	/// <summary>
	///   Renders a component with a provided authentication state task as a cascading value.
	/// </summary>
	   public static IRenderedComponent<TComponent> RenderWithAuth<TComponent>(
		   BunitContext testContext,
		   Task<Microsoft.AspNetCore.Components.Authorization.AuthenticationState> authStateTask,
		   Action<ComponentParameterCollectionBuilder<TComponent>>? parameters = null)
		   where TComponent : IComponent
	   {
		   return testContext.Render<TComponent>(ps =>
		   {
			   ps.AddCascadingValue(authStateTask);
			   parameters?.Invoke(ps);
		   });
	   }

	/// <summary>
	///   Sets the value of an input element found by selector.
	/// </summary>
	public static void SetInputValue<TComponent>(IRenderedComponent<TComponent> cut, string selector, string value)
		where TComponent : IComponent
	{
		var input = cut.Find(selector);
		input.Change(value);
	}

	/// <summary>
	///   Submits the first form in the component.
	/// </summary>
	public static async Task SubmitFormAsync<TComponent>(IRenderedComponent<TComponent> cut)
		where TComponent : IComponent
	{
		var form = cut.Find("form");
		await cut.InvokeAsync(() => form.Submit());
	}
}
