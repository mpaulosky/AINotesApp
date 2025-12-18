using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;

namespace AINotesApp.Tests.Unit.Fakes;

/// <summary>
///   Minimal fake NavigationManager for Bunit test compatibility.
/// </summary>
[ExcludeFromCodeCoverage]
public class FakeNavigationManager : NavigationManager
{
	public FakeNavigationManager()
	{
		Initialize("http://localhost/", "http://localhost/");
	}

	protected override void NavigateToCore(string uri, bool forceLoad)
	{
		Uri = ToAbsoluteUri(uri).ToString();
	}
}
