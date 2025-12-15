// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     ArchitectureTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp.Tests.Architecture
// =======================================================

using System.Diagnostics.CodeAnalysis;

using AINotesApp.Data;

using FluentAssertions;

using NetArchTest.Rules;

namespace AINotesApp.Tests.Architecture;

/// <summary>
///   Architecture tests to enforce coding standards and dependency rules.
/// </summary>
[ExcludeFromCodeCoverage]
public class ArchitectureTests
{

	private const string _domainNamespace = "AINotesApp";

	/// <summary>
	///   Ensures that all handler classes reside in the Features namespace as required by Vertical Slice Architecture.
	/// </summary>
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
				.ResideInNamespace($"{_domainNamespace}.Features")
				.GetResult();

		// Then
		result.IsSuccessful.Should().BeTrue(
				"All handlers should be in the Features namespace following Vertical Slice Architecture");
	}

	/// <summary>
	///   Ensures that all command types are implemented as records.
	/// </summary>
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
					$"{type.Name} should be a record with properties");
		}
	}

	/// <summary>
	///   Ensures that all query types are implemented as records.
	/// </summary>
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
					$"{type.Name} should be a record with properties");
		}
	}

	/// <summary>
	///   Ensures that all response types are implemented as records.
	/// </summary>
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
					$"{type.Name} should be a record with properties");
		}
	}

	/// <summary>
	///   Ensures that all service interfaces reside in the Services namespace.
	/// </summary>
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
				.ResideInNamespace($"{_domainNamespace}.Services")
				.Or()
				.ResideInNamespace("Microsoft")
				.GetResult();

		// Then
		result.IsSuccessful.Should().BeTrue(
				"Service interfaces should be in the Services namespace");
	}

	/// <summary>
	///   Verifies that handler classes do not have dependencies on UI components.
	/// </summary>
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
				.HaveDependencyOn($"{_domainNamespace}.Components")
				.GetResult();

		// Then
		result.IsSuccessful.Should().BeTrue(
				"Handlers should not depend on UI components (Blazor)");
	}

	/// <summary>
	///   Ensures that data models are located in the Data namespace.
	/// </summary>
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
				.ResideInNamespace($"{_domainNamespace}.Data")
				.GetResult();

		// Then
		result.IsSuccessful.Should().BeTrue(
				"Data models should be in the Data namespace");
	}

	/// <summary>
	///   Verifies that all handler classes implement the IRequestHandler interface from MediatR.
	/// </summary>
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
					$"{handlerType.Name} should implement IRequestHandler for MediatR");
		}
	}

	/// <summary>
	///   Ensures that features are organized by business capability.
	/// </summary>
	[Fact]
	public void Features_ShouldBeOrganizedByBusinessCapability()
	{
		// Given
		var assembly = typeof(ApplicationDbContext).Assembly;

		// When
		var featureTypes = Types.InAssembly(assembly)
				.That()
				.ResideInNamespace($"{_domainNamespace}.Features")
				.GetTypes();

		// Then
		featureTypes.Should().NotBeEmpty(
				"Features should be organized following Vertical Slice Architecture");

		var featureNamespaces = featureTypes
				.Select(t => t.Namespace)
				.Distinct()
				.Where(ns => ns != null && ns.StartsWith($"{_domainNamespace}.Features."))
				.ToList();

		featureNamespaces.Should().Contain(ns => ns!.Contains(".Notes"),
				"Should have Notes feature organized by business capability");
	}

	/// <summary>
	///   Ensures that services do not depend on features to maintain proper layering.
	/// </summary>
	[Fact]
	public void Services_ShouldNotDependOnFeatures()
	{
		// Given
		var assembly = typeof(ApplicationDbContext).Assembly;

		// When
		var result = Types.InAssembly(assembly)
				.That()
				.ResideInNamespace($"{_domainNamespace}.Services")
				.ShouldNot()
				.HaveDependencyOn($"{_domainNamespace}.Features")
				.GetResult();

		// Then
		result.IsSuccessful.Should().BeTrue(
				"Services should not depend on Features to maintain proper layering");
	}

}