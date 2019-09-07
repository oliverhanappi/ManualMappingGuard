using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using ManualMappingGuard.Analyzers;
using ManualMappingGuard.Tests.Infrastructure;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace ManualMappingGuard.Tests
{
    [TestFixture, SetUICulture("en")]
    public class MappingAnalyzerTests : AnalyzerTestBase<MappingAnalyzer>
    {
        protected override string CodeTemplate => @"
                using System;
                using System.Threading.Tasks;
                using ManualMappingGuard;
                using XAttribute = ManualMappingGuard.MappingMethodAttribute;

                namespace TestNamespace
                {{
                    public class Person
                    {{
                        public string FirstName {{ get; set; }}
                        public string LastName {{ get; set; }}
                    }}

                    public static class TestClass
                    {{
                        {0}
                    }}
                }}
        ";

        [Test]
        public async Task MappingMethod_ReturnTypeVoid_DetectsUnsuitable()
        {
            var diagnostics = await Analyze(@"
                [MappingMethod]
                public void Map()
                {
                }
            ");

            AssertUnsuitableMappingMethod(diagnostics);
        }

        [Test]
        public async Task MappingMethod_ReturnTypeAsyncTaskWithoutResult_DetectsUnsuitable()
        {
            var diagnostics = await Analyze(@"
                [MappingMethod]
                public async Task Map()
                {
                }
            ");

            AssertUnsuitableMappingMethod(diagnostics);
        }

        [Test]
        public async Task MappingMethod_ReturnTypeAsyncVoid_DetectsUnsuitable()
        {
            var diagnostics = await Analyze(@"
                [MappingMethod]
                public async void Map()
                {
                }
            ");

            AssertUnsuitableMappingMethod(diagnostics);
        }

        [Test]
        public async Task MappingMethod_DecoratedWithShortAttributeName_DetectsUnmappedProperty()
        {
            var diagnostics = await Analyze(@"
                [MappingMethod]
                public Person Map()
                {
                    var person = new Person();
                    person.FirstName = ""Test"";
                    return person;
                }
            ");

            AssertUnmappedProperties(diagnostics, "LastName");
        }

        [Test]
        public async Task MappingMethod_DecoratedWithFullAttributeName_DetectsUnmappedProperty()
        {
            var diagnostics = await Analyze(@"
                [MappingMethodAttribute]
                public Person Map()
                {
                    var person = new Person();
                    person.FirstName = ""Test"";
                    return person;
                }
            ");

            AssertUnmappedProperties(diagnostics, "LastName");
        }

        [Test]
        public async Task MappingMethod_DecoratedWithNamespaceAndFullAttributeName_DetectsUnmappedProperty()
        {
            var diagnostics = await Analyze(@"
                [ManualMappingGuard.MappingMethodAttribute]
                public Person Map()
                {
                    var person = new Person();
                    person.FirstName = ""Test"";
                    return person;
                }
            ");

            AssertUnmappedProperties(diagnostics, "LastName");
        }

        [Test]
        public async Task MappingMethod_DecoratedWithAliasedAttribute_DetectsUnmappedProperty()
        {
            var diagnostics = await Analyze(@"
                [X]
                public Person Map()
                {
                    var person = new Person();
                    person.FirstName = ""Test"";
                    return person;
                }
            ");

            AssertUnmappedProperties(diagnostics, "LastName");
        }

        [Test]
        public async Task MappingMethod_Async_DetectsUnmappedProperty()
        {
            var diagnostics = await Analyze(@"
                [MappingMethod]
                public async Task<Person> Map()
                {
                    var person = new Person();
                    person.FirstName = ""Test"";
                    return person;
                }
            ");

            AssertUnmappedProperties(diagnostics, "LastName");
        }

        [Test]
        public async Task MappingMethod_ObjectInitializer_DetectsUnmappedProperty()
        {
            var diagnostics = await Analyze(@"
                [MappingMethod]
                public async Task<Person> Map()
                {
                    return new Person
                    {
                        FirstName = ""Test""
                    };
                }
            ");

            AssertUnmappedProperties(diagnostics, "LastName");
        }

        private void AssertUnsuitableMappingMethod(ImmutableArray<Diagnostic> diagnostics)
        {
            var expectedMessages = new[]
            {
                "(18,17): error MMG0002: This method is not a suitable mapping method because it has no return value."
            };
            
            var actualMessages = diagnostics.Select(d => d.ToString());

            Assert.That(actualMessages, Is.EquivalentTo(expectedMessages));
        }
        
        private void AssertUnmappedProperties(ImmutableArray<Diagnostic> diagnostics, params string[] propertyNames)
        {
            var expectedMessages = propertyNames
                .Select(p => $"(18,17): error MMG0001: Property {p} is not mapped.")
                .ToList();

            var actualMessages = diagnostics.Select(d => d.ToString());

            Assert.That(actualMessages, Is.EquivalentTo(expectedMessages));
        }
    }
}
