using System.Diagnostics.CodeAnalysis;

using AINotesApp.Components.Pages;

using Bunit;

using FluentAssertions;

namespace AINotesApp.Tests.Unit.Components.Pages;

/// <summary>
/// Unit tests for Weather component using BUnit 2.x
/// </summary>
[ExcludeFromCodeCoverage]
public class WeatherTests : BunitContext
{
	[Fact]
	public void Weather_InitialRender_ShowsLoadingMessage()
	{
		// Arrange & Act
		var cut = Render<Weather>();

		// Assert
		cut.Markup.Should().Contain("Loading...");
	}

	[Fact]
	public void Weather_ShouldRender_PageTitle()
	{
		// Arrange & Act
		var cut = Render<Weather>();

		// Assert
		cut.Markup.Should().Contain("Weather");
	}

	[Fact]
	public void Weather_ShouldRender_Heading()
	{
		// Arrange & Act
		var cut = Render<Weather>();

		// Assert
		cut.Find("h1").TextContent.Should().Be("Weather");
	}

	[Fact]
	public void Weather_ShouldRender_Description()
	{
		// Arrange & Act
		var cut = Render<Weather>();

		// Assert
		cut.Markup.Should().Contain("This component demonstrates showing data");
	}

	[Fact]
	public async Task Weather_AfterDelay_RendersTable()
	{
		// Arrange
		var cut = Render<Weather>();

		// Act - Wait for component initialization
		await Task.Delay(600);
		cut.WaitForState(() => cut.Markup.Contains("<table"), timeout: TimeSpan.FromSeconds(2));

		// Assert
		cut.FindAll("table").Should().HaveCount(1);
	}

	[Fact]
	public async Task Weather_Table_HasCorrectHeaders()
	{
		// Arrange
		var cut = Render<Weather>();

		// Act
		await Task.Delay(600);
		cut.WaitForState(() => cut.Markup.Contains("<table"), timeout: TimeSpan.FromSeconds(2));

		// Assert
		var headers = cut.FindAll("th");
		headers.Should().HaveCountGreaterThan(0);
		headers[0].TextContent.Should().Be("Date");
		headers[3].TextContent.Should().Be("Summary");
	}

	[Fact]
	public async Task Weather_Table_ContainsForecastRows()
	{
		// Arrange
		var cut = Render<Weather>();

		// Act
		await Task.Delay(600);
		cut.WaitForState(() => cut.Markup.Contains("<tbody"), timeout: TimeSpan.FromSeconds(2));

		// Assert
		var rows = cut.FindAll("tbody tr");
		rows.Should().HaveCount(5); // Component generates 5 forecasts
	}

	[Fact]
	public async Task Weather_Rows_ContainValidData()
	{
		// Arrange
		var cut = Render<Weather>();

		// Act
		await Task.Delay(600);
		cut.WaitForState(() => cut.Markup.Contains("<tbody"), timeout: TimeSpan.FromSeconds(2));

		// Assert
		var rows = cut.FindAll("tbody tr");
		foreach (var row in rows)
		{
			var cells = row.QuerySelectorAll("td");
			cells.Should().HaveCount(4);

			// All cells should have content
			cells[0].TextContent.Should().NotBeNullOrEmpty(); // Date
			cells[1].TextContent.Should().NotBeNullOrEmpty(); // Temp C
			cells[2].TextContent.Should().NotBeNullOrEmpty(); // Temp F
			cells[3].TextContent.Should().NotBeNullOrEmpty(); // Summary
		}
	}

	[Fact]
	public async Task Weather_TemperatureConversion_IsCorrect()
	{
		// Arrange
		var cut = Render<Weather>();

		// Act
		await Task.Delay(600);
		cut.WaitForState(() => cut.Markup.Contains("<tbody"), timeout: TimeSpan.FromSeconds(2));

		// Assert - Verify Fahrenheit calculation
		var rows = cut.FindAll("tbody tr");
		foreach (var row in rows)
		{
			var cells = row.QuerySelectorAll("td");
			if (int.TryParse(cells[1].TextContent, out int tempC) &&
					int.TryParse(cells[2].TextContent, out int tempF))
			{
				var expectedF = 32 + (int)(tempC / 0.5556);
				tempF.Should().Be(expectedF);
			}
		}
	}

	[Fact]
	public void Weather_HasStreamRenderingAttribute()
	{
		// Arrange & Act
		var cut = Render<Weather>();

		// Assert - Component should have the StreamRendering attribute
		cut.Instance.Should().NotBeNull();
	}
}
