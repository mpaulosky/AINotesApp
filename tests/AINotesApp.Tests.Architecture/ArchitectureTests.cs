using System.Diagnostics.CodeAnalysis;
using AINotesApp.Data;
using FluentAssertions;
using NetArchTest.Rules;

namespace AINotesApp.Tests.Architecture;

/// <summary>
/// Architecture tests to enforce coding standards and dependency rules.
/// </summary>
[ExcludeFromCodeCoverage]
public class ArchitectureTests
{
    private const string DomainNamespace = "AINotesApp";

    [Fact]
    public void Handlers_ShouldBeInFeaturesNamespace()
    {
        // Given
        var assembly = typeof(ApplicationDbContext).Assembly;

        // When
        var result = Types.InAssembly(assembly)
            .That()
            .HaveNameEndingWith("Handler")
            .Should()
            .ResideInNamespace($"{DomainNamespace}.Features")
            .GetResult();

        // Then
        result.IsSuccessful.Should().BeTrue(
            because: "All handlers should be in the Features namespace following Vertical Slice Architecture");
    }

    [Fact]
    public void Commands_ShouldBeRecords()
    {
        // Given
        var assembly = typeof(ApplicationDbContext).Assembly;

        // When
        var commandTypes = Types.InAssembly(assembly)
            .That()
            .HaveNameEndingWith("Command")
            .GetTypes();

        // Then
        foreach (var type in commandTypes)
        {
            type.IsValueType.Should().BeFalse();
            type.GetProperties().Should().NotBeEmpty(
                because: $"{type.Name} should be a record with properties");
        }
    }

    [Fact]
    public void Queries_ShouldBeRecords()
    {
        // Given
        var assembly = typeof(ApplicationDbContext).Assembly;

        // When
        var queryTypes = Types.InAssembly(assembly)
            .That()
            .HaveNameEndingWith("Query")
            .GetTypes();

        // Then
        foreach (var type in queryTypes)
        {
            type.IsValueType.Should().BeFalse();
            type.GetProperties().Should().NotBeEmpty(
                because: $"{type.Name} should be a record with properties");
        }
    }

    [Fact]
    public void Responses_ShouldBeRecords()
    {
        // Given
        var assembly = typeof(ApplicationDbContext).Assembly;

        // When
        var responseTypes = Types.InAssembly(assembly)
            .That()
            .HaveNameEndingWith("Response")
            .GetTypes();

        // Then
        foreach (var type in responseTypes)
        {
            type.IsValueType.Should().BeFalse();
            type.GetProperties().Should().NotBeEmpty(
                because: $"{type.Name} should be a record with properties");
        }
    }

    [Fact]
    public void Services_ShouldBeInServicesNamespace()
    {
        // Given
        var assembly = typeof(ApplicationDbContext).Assembly;

        // When
        var result = Types.InAssembly(assembly)
            .That()
            .AreInterfaces()
            .And()
            .HaveNameStartingWith("I")
            .And()
            .DoNotHaveName("IAsyncDisposable")
            .And()
            .DoNotHaveName("IDisposable")
            .Should()
            .ResideInNamespace($"{DomainNamespace}.Services")
            .Or()
            .ResideInNamespace("Microsoft")
            .GetResult();

        // Then
        result.IsSuccessful.Should().BeTrue(
            because: "Service interfaces should be in the Services namespace");
    }

    [Fact]
    public void Handlers_ShouldNotHaveDependencyOnComponents()
    {
        // Given
        var assembly = typeof(ApplicationDbContext).Assembly;

        // When
        var result = Types.InAssembly(assembly)
            .That()
            .HaveNameEndingWith("Handler")
            .ShouldNot()
            .HaveDependencyOn($"{DomainNamespace}.Components")
            .GetResult();

        // Then
        result.IsSuccessful.Should().BeTrue(
            because: "Handlers should not depend on UI components (Blazor)");
    }

    [Fact]
    public void DataModels_ShouldBeInDataNamespace()
    {
        // Given
        var assembly = typeof(ApplicationDbContext).Assembly;

        // When
        var result = Types.InAssembly(assembly)
            .That()
            .AreClasses()
            .And()
            .Inherit(typeof(object))
            .And()
            .HaveName("Note")
            .Or()
            .HaveName("ApplicationUser")
            .Or()
            .HaveName("ApplicationDbContext")
            .Should()
            .ResideInNamespace($"{DomainNamespace}.Data")
            .GetResult();

        // Then
        result.IsSuccessful.Should().BeTrue(
            because: "Data models should be in the Data namespace");
    }

    [Fact]
    public void Handlers_ShouldImplementIRequestHandler()
    {
        // Given
        var assembly = typeof(ApplicationDbContext).Assembly;

        // When
        var handlerTypes = Types.InAssembly(assembly)
            .That()
            .HaveNameEndingWith("Handler")
            .GetTypes();

        // Then
        foreach (var handlerType in handlerTypes)
        {
            var implementsIRequestHandler = handlerType.GetInterfaces()
                .Any(i => i.Name.Contains("IRequestHandler"));

            implementsIRequestHandler.Should().BeTrue(
                because: $"{handlerType.Name} should implement IRequestHandler for MediatR");
        }
    }

    [Fact]
    public void Features_ShouldBeOrganizedByBusinessCapability()
    {
        // Given
        var assembly = typeof(ApplicationDbContext).Assembly;

        // When
        var featureTypes = Types.InAssembly(assembly)
            .That()
            .ResideInNamespace($"{DomainNamespace}.Features")
            .GetTypes();

        // Then
        featureTypes.Should().NotBeEmpty(
            because: "Features should be organized following Vertical Slice Architecture");

        var featureNamespaces = featureTypes
            .Select(t => t.Namespace)
            .Distinct()
            .Where(ns => ns != null && ns.StartsWith($"{DomainNamespace}.Features."))
            .ToList();

        featureNamespaces.Should().Contain(ns => ns!.Contains(".Notes"),
            because: "Should have Notes feature organized by business capability");
    }

    [Fact]
    public void Services_ShouldNotDependOnFeatures()
    {
        // Given
        var assembly = typeof(ApplicationDbContext).Assembly;

        // When
        var result = Types.InAssembly(assembly)
            .That()
            .ResideInNamespace($"{DomainNamespace}.Services")
            .ShouldNot()
            .HaveDependencyOn($"{DomainNamespace}.Features")
            .GetResult();

        // Then
        result.IsSuccessful.Should().BeTrue(
            because: "Services should not depend on Features to maintain proper layering");
    }
}
