using FluentAssertions;
using NetArchTest.Rules;
using System.Reflection;

namespace Spenvio.Application.Tests.Architecture

/* This test ensure that the system follows Clean Architecture principles.
 * We verify dependency rules between layers to maintain separation of concerns,
 * and to prevent unwanted coupling. Thought we had to expose Application.Test to all layers */
{
    public class ArchitectureTests
    {
        private static readonly Assembly ApplicationAssembly = typeof(FlexNet.Application.Services.UserDescriptionService).Assembly;
        private static readonly Assembly DomainAssembly = typeof(FlexNet.Domain.Entities.UserDescription).Assembly;
        private static readonly Assembly InfrastructureAssembly = typeof(FlexNet.Infrastructure.Security.JwtGenerator).Assembly;
        private static readonly Assembly ApiAssembly = typeof(FlexNet.Api.Configuration.CorsConfiguration).Assembly;

        [Fact]
        public void ApplicationLayer_Should_Not_Have_Dependency_On_Infrastructure()
        {
            // Arrange & Act
            var result = Types
                .InAssembly(ApplicationAssembly)
                .ShouldNot()
                .HaveDependencyOn(InfrastructureAssembly.GetName().Name)
                .GetResult();

            // Assert
            result.IsSuccessful.Should().BeTrue("Application-layer should not depend on Infrastructure-layer");
        }

        [Fact]
        public void DomainLayer_Should_Not_Have_Dependency_On_Application_Or_Infrastructure()
        {
            // Arrange & Act
            var result = Types
                .InAssembly(DomainAssembly)
                .ShouldNot()
                .HaveDependencyOnAny(
                    ApplicationAssembly.GetName().Name,
                    InfrastructureAssembly.GetName().Name
                )
                .GetResult();

            // Assert
            result.IsSuccessful.Should().BeTrue("Domain-layer should not depend on any layers");
        }

        [Fact]
        public void InfrastructureLayer_Should_Not_Have_Dependency_On_Api()
        {
            // Arrange & Act
            var result = Types
                .InAssembly(InfrastructureAssembly)
                .ShouldNot()
                .HaveDependencyOn(ApiAssembly.GetName().Name)
                .GetResult();

            // Assert
            result.IsSuccessful.Should().BeTrue("Infrastructure-layer should not depend on API");
        }


        [Fact]
        public void Domain_Should_Expose_Only_Entities_And_Interfaces()
        {
            // Arrange
            var allowedNamespaces = new[]
            {
                "FlexNet.Domain.Entities",
                "FlexNet.Domain.Interfaces"
            };

            // Act
            var publicTypes = Types
                .InAssembly(DomainAssembly)
                .That()
                .ArePublic()
                .GetTypes(); 

            var forbiddenTypes = publicTypes
                .Select(domainType => domainType.FullName!)
                .Where(t => !allowedNamespaces.Any(allowedNamespace => t.StartsWith(allowedNamespace)))
                .ToList();

            // Assert
            forbiddenTypes.Should().BeEmpty("Domain-layer should only expose entitis and interfaces ");
        }

        [Fact]
        public void Domain_Should_Not_Have_External_Dependencies()
        {
            // Arrange
            var forbiddenNamespaces = new[]
            {
                "Microsoft",
                "System.Data",
                "EntityFrameworkCore"
            };

            // Act
            var result = Types
                .InAssembly(DomainAssembly)
                .ShouldNot()
                .HaveDependencyOnAny(forbiddenNamespaces)
                .GetResult();

            // Assert
            result.IsSuccessful.Should().BeTrue("Domain-layer should not have framework or database connection");
        }

    }
}
