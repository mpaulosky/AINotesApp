using AINotesApp.Data;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;

namespace AINotesApp.Components.Account;

/// <summary>
/// Manages navigation redirects for identity-related operations in the application.
/// </summary>
internal sealed class IdentityRedirectManager(NavigationManager navigationManager)
{
    /// <summary>
    /// The name of the status message cookie used for identity operations.
    /// </summary>
    public const string StatusCookieName = "Identity.StatusMessage";

    private static readonly CookieBuilder StatusCookieBuilder = new()
    {
        SameSite = SameSiteMode.Strict,
        HttpOnly = true,
        IsEssential = true,
        MaxAge = TimeSpan.FromSeconds(5),
    };

    /// <summary>
    /// Redirects to the specified URI, ensuring it is a safe relative path.
    /// </summary>
    /// <param name="uri">The target URI to redirect to.</param>
    public void RedirectTo(string? uri)
    {
        uri ??= "";

        // Prevent open redirects.
        if (!Uri.IsWellFormedUriString(uri, UriKind.Relative))
        {
            uri = navigationManager.ToBaseRelativePath(uri);
        }

        // Use forceLoad: true to ensure proper redirect after authentication
        // This forces a full page reload which is necessary after sign-in operations
        navigationManager.NavigateTo(uri, forceLoad: true);
    }

    /// <summary>
    /// Redirects to the specified URI with query parameters.
    /// </summary>
    /// <param name="uri">The base URI.</param>
    /// <param name="queryParameters">Query parameters to append.</param>
    public void RedirectTo(string uri, Dictionary<string, object?> queryParameters)
    {
        var uriWithoutQuery = navigationManager.ToAbsoluteUri(uri).GetLeftPart(UriPartial.Path);
        var newUri = navigationManager.GetUriWithQueryParameters(uriWithoutQuery, queryParameters);
        RedirectTo(newUri);
    }

    /// <summary>
    /// Redirects to the specified URI and sets a status message in a cookie.
    /// </summary>
    /// <param name="uri">The target URI.</param>
    /// <param name="message">The status message to set.</param>
    /// <param name="context">The HTTP context for setting the cookie.</param>
    public void RedirectToWithStatus(string uri, string message, HttpContext context)
    {
        context.Response.Cookies.Append(StatusCookieName, message, StatusCookieBuilder.Build(context));
        RedirectTo(uri);
    }

    /// <summary>
    /// Gets the current absolute path from the navigation manager.
    /// </summary>
    private string CurrentPath => navigationManager.ToAbsoluteUri(navigationManager.Uri).GetLeftPart(UriPartial.Path);

    /// <summary>
    /// Redirects to the current page.
    /// </summary>
    public void RedirectToCurrentPage() => RedirectTo(CurrentPath);

    /// <summary>
    /// Redirects to the current page with a status message.
    /// </summary>
    /// <param name="message">The status message to set.</param>
    /// <param name="context">The HTTP context for setting the cookie.</param>
    public void RedirectToCurrentPageWithStatus(string message, HttpContext context)
        => RedirectToWithStatus(CurrentPath, message, context);

    /// <summary>
    /// Redirects to the invalid user page if the user cannot be loaded.
    /// </summary>
    /// <param name="userManager">The user manager instance.</param>
    /// <param name="context">The HTTP context.</param>
    public void RedirectToInvalidUser(UserManager<ApplicationUser> userManager, HttpContext context)
        => RedirectToWithStatus("Account/InvalidUser", $"Error: Unable to load user with ID '{userManager.GetUserId(context.User)}'.", context);
}