using ArabFaker.NET;
using ArabFaker.NET.Generation;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace ArabFaker.Tests.Generation
{
    public class FakerDefinitionTests
    {
        private static readonly DateOnly AsOfDate = new(2026, 11, 6);

        // ---------- Define<T>() ----------

        [Fact]
        public void Define_ShouldReturnValidEmptyDefinition()
        {
            var faker = new Faker(2026);

            var definition = faker.Define<DefinitionStudent>();

            Assert.True(definition.IsValid);
            Assert.Empty(definition.ConfigurationErrors);
            Assert.Equal(0, definition.RuleCount);
            Assert.False(definition.IsFrozen);
        }

        [Fact]
        public void Define_DefaultFactory_ShouldCreateNewInstances()
        {
            var definition = new Faker(2026).Define<DefinitionStudent>();

            Assert.True(definition.TryCreateInstance(out var first));
            Assert.True(definition.TryCreateInstance(out var second));

            Assert.NotNull(first);
            Assert.NotNull(second);
            Assert.NotSame(first, second);
        }

        [Fact]
        public void Define_CustomFactory_ShouldBeUsedToCreateInstances()
        {
            var definition = new Faker(2026)
                .Define(() => new DefinitionStudentWithConstructor("Default"));

            Assert.True(definition.IsValid);
            Assert.True(definition.TryCreateInstance(out var instance));
            Assert.Equal("Default", instance!.DefaultName);
        }

        [Fact]
        public void Define_CustomFactory_ShouldNotBeInvokedAtDefinitionTime()
        {
            int calls = 0;

            var definition = new Faker(2026).Define(() =>
            {
                calls++;
                return new DefinitionStudent();
            });

            Assert.Equal(0, calls);

            definition.TryCreateInstance(out _);

            Assert.Equal(1, calls);
        }

        [Fact]
        public void Define_NullFactory_ShouldReportConfigurationError()
        {
            var definition = new Faker(2026).Define<DefinitionStudent>(null!);

            Assert.False(definition.IsValid);

            var error = Assert.Single(definition.ConfigurationErrors);

            Assert.Equal(ConfigurationErrorCodes.NullFactory, error.Code);
            Assert.False(definition.TryCreateInstance(out var instance));
            Assert.Null(instance);
        }

        [Fact]
        public void Define_FactoryReturningNull_ShouldMakeCreateInstanceFail()
        {
            var definition = new Faker(2026).Define<DefinitionStudent>(() => null!);

            Assert.False(definition.TryCreateInstance(out var instance));
            Assert.Null(instance);
        }

        [Fact]
        public void Define_EachDefinitionShouldHaveItsOwnScope()
        {
            var faker = new Faker(2026);

            var first = faker.Define<DefinitionStudent>();
            var second = faker.Define<DefinitionStudent>();

            Assert.NotEqual(first.ScopeKey, second.ScopeKey);

            first.CreateContext(0).Text.TryAlphaNumeric(64, out string firstText);
            second.CreateContext(0).Text.TryAlphaNumeric(64, out string secondText);

            Assert.NotEqual(firstText, secondText);
        }

        [Fact]
        public void Define_SameSeedAndOrder_ShouldProduceSameScopesAndValues()
        {
            var firstFaker = new Faker(2026);
            var secondFaker = new Faker(2026);

            var firstDefinition = firstFaker.Define<DefinitionStudent>();
            var secondDefinition = secondFaker.Define<DefinitionStudent>();

            Assert.Equal(firstDefinition.ScopeKey, secondDefinition.ScopeKey);

            firstDefinition.CreateContext(3).Text.TryAlphaNumeric(64, out string firstText);
            secondDefinition.CreateContext(3).Text.TryAlphaNumeric(64, out string secondText);

            Assert.Equal(firstText, secondText);
        }

        [Fact]
        public void Define_ConcurrentCalls_ShouldAllocateUniqueScopes()
        {
            var faker = new Faker(2026);

            var keys = new ConcurrentBag<string>();

            Parallel.For(0, 500, _ => keys.Add(faker.Define<DefinitionStudent>().ScopeKey));

            Assert.Equal(500, keys.Count);
            Assert.Equal(500, keys.Distinct().Count());
        }

        // ---------- RuleFor ----------

        [Fact]
        public void RuleFor_ShouldReturnSameDefinitionForChaining()
        {
            var definition = new Faker(2026).Define<DefinitionStudent>();

            var result = definition.RuleFor(x => x.FirstName, _ => "A");

            Assert.Same(definition, result);
            Assert.Equal(1, definition.RuleCount);
        }

        [Fact]
        public void RuleFor_ShouldNotInvokeValueFactoryWhenRegistered()
        {
            int calls = 0;

            var definition = new Faker(2026).Define<DefinitionStudent>()
                .RuleFor(x => x.FirstName, _ =>
                {
                    calls++;
                    return "A";
                });

            Assert.Equal(0, calls);
            Assert.True(definition.IsValid);
        }

        [Fact]
        public void ApplyRules_ShouldSetAllConfiguredProperties()
        {
            var definition = new Faker(2026).Define<DefinitionStudent>()
                .RuleFor(x => x.FirstName, _ => "Youssef")
                .RuleFor(x => x.Age, _ => 25)
                .RuleFor(x => x.BirthDate, _ => new DateOnly(2000, 1, 1))
                .RuleFor(x => x.Nickname, _ => (string?)null)
                .RuleFor(x => x.Score, _ => (int?)7);

            var student = Generate(definition, 0);

            Assert.Equal("Youssef", student.FirstName);
            Assert.Equal(25, student.Age);
            Assert.Equal(new DateOnly(2000, 1, 1), student.BirthDate);
            Assert.Null(student.Nickname);
            Assert.Equal(7, student.Score);
        }

        [Fact]
        public void ApplyRules_ShouldPassFakerContextToValueFactory()
        {
            var definition = new Faker(2026).Define<DefinitionStudent>()
                .RuleFor(x => x.Email, context => context.Internet.TryEmail(out var email) ? email : string.Empty);

            var student = Generate(definition, 0);

            Assert.EndsWith("@example.test", student.Email);
        }

        [Fact]
        public void RuleFor_InheritedProperty_ShouldBeSupported()
        {
            var definition = new Faker(2026).Define<DefinitionDerived>()
                .RuleFor(x => x.BaseValue, _ => "base")
                .RuleFor(x => x.DerivedValue, _ => 5);

            Assert.True(definition.IsValid);

            Assert.True(definition.TryCreateInstance(out var instance));
            definition.ApplyRules(instance!, definition.CreateContext(0));

            Assert.Equal("base", instance!.BaseValue);
            Assert.Equal(5, instance.DerivedValue);
        }

        [Fact]
        public void RuleFor_DuplicateProperty_LastRuleWins()
        {
            var definition = new Faker(2026).Define<DefinitionStudent>()
                .RuleFor(x => x.FirstName, _ => "First")
                .RuleFor(x => x.FirstName, _ => "Second");

            Assert.True(definition.IsValid);
            Assert.Equal(1, definition.RuleCount);

            Assert.Equal("Second", Generate(definition, 0).FirstName);
        }

        [Fact]
        public void ApplyRules_ShouldRunInRegistrationOrder_ReplacementKeepsPosition()
        {
            var log = new List<string>();

            var definition = new Faker(2026).Define<DefinitionStudent>()
                .RuleFor(x => x.FirstName, _ => Log(log, "first-1"))
                .RuleFor(x => x.Email, _ => Log(log, "email"))
                .RuleFor(x => x.FirstName, _ => Log(log, "first-2"));

            Generate(definition, 0);

            Assert.Equal(new[] { "first-2", "email" }, log);
        }

        [Fact]
        public void ApplyRules_ValueFactoryException_ShouldPropagate()
        {
            var definition = new Faker(2026).Define<DefinitionStudent>()
                .RuleFor<string>(x => x.FirstName, _ => throw new InvalidOperationException("boom"));

            Assert.True(definition.TryCreateInstance(out var instance));

            Assert.Throws<InvalidOperationException>(
                () => definition.ApplyRules(instance!, definition.CreateContext(0)));
        }

        // ---------- RuleFor validation ----------

        [Fact]
        public void RuleFor_NullSelector_ShouldReportError()
        {
            var definition = new Faker(2026).Define<DefinitionStudent>()
                .RuleFor<string>(null!, _ => "A");

            AssertSingleError(definition, ConfigurationErrorCodes.NullPropertySelector);
        }

        [Fact]
        public void RuleFor_NullValueFactory_ShouldReportError()
        {
            var definition = new Faker(2026).Define<DefinitionStudent>()
                .RuleFor<string>(x => x.FirstName, (Func<FakerContext, string>)null!);

            AssertSingleError(definition, ConfigurationErrorCodes.NullValueFactory);
        }

        [Fact]
        public void RuleFor_NullSelectorAndFactory_ShouldReportBothErrors()
        {
            var definition = new Faker(2026).Define<DefinitionStudent>()
                .RuleFor<string>(null!, (Func<FakerContext, string>)null!);

            Assert.Equal(2, definition.ConfigurationErrors.Count);
            Assert.Equal(0, definition.RuleCount);
        }

        [Fact]
        public void RuleFor_Field_ShouldReportError()
        {
            var definition = new Faker(2026).Define<DefinitionStudent>()
                .RuleFor(x => x.Code, _ => "A");

            AssertSingleError(definition, ConfigurationErrorCodes.FieldNotSupported);
        }

        [Fact]
        public void RuleFor_NestedProperty_ShouldReportError()
        {
            var definition = new Faker(2026).Define<DefinitionStudent>()
                .RuleFor(x => x.Address.City, _ => "Cairo");

            AssertSingleError(definition, ConfigurationErrorCodes.NestedPropertyNotSupported);
        }

        [Fact]
        public void RuleFor_ComputedNestedMember_ShouldReportError()
        {
            var definition = new Faker(2026).Define<DefinitionStudent>()
                .RuleFor(x => x.FirstName.Length, _ => 10);

            AssertSingleError(definition, ConfigurationErrorCodes.NestedPropertyNotSupported);
        }

        [Fact]
        public void RuleFor_PropertyWithoutSetter_ShouldReportError()
        {
            var definition = new Faker(2026).Define<DefinitionStudent>()
                .RuleFor(x => x.Display, _ => "A");

            AssertSingleError(definition, ConfigurationErrorCodes.PropertyNotWritable);
        }

        [Fact]
        public void RuleFor_ReadOnlyProperty_ShouldReportError()
        {
            var definition = new Faker(2026).Define<DefinitionStudent>()
                .RuleFor(x => x.Created, _ => "A");

            AssertSingleError(definition, ConfigurationErrorCodes.PropertyNotWritable);
        }

        [Fact]
        public void RuleFor_PrivateSetter_ShouldReportError()
        {
            var definition = new Faker(2026).Define<DefinitionStudent>()
                .RuleFor(x => x.Secret, _ => "A");

            AssertSingleError(definition, ConfigurationErrorCodes.PropertyNotWritable);
        }

        [Fact]
        public void RuleFor_InitOnlyProperty_ShouldReportError()
        {
            var definition = new Faker(2026).Define<DefinitionStudent>()
                .RuleFor(x => x.Tag, _ => "A");

            AssertSingleError(definition, ConfigurationErrorCodes.InitOnlyPropertyNotSupported);
        }

        [Fact]
        public void RuleFor_Indexer_ShouldReportError()
        {
            var definition = new Faker(2026).Define<DefinitionStudent>()
                .RuleFor(x => x[0], _ => "A");

            AssertSingleError(definition, ConfigurationErrorCodes.IndexerNotSupported);
        }

        [Fact]
        public void RuleFor_MethodCall_ShouldReportError()
        {
            var definition = new Faker(2026).Define<DefinitionStudent>()
                .RuleFor(x => x.GetName(), _ => "A");

            AssertSingleError(definition, ConfigurationErrorCodes.UnsupportedSelector);
        }

        [Fact]
        public void RuleFor_StaticMember_ShouldReportError()
        {
            var definition = new Faker(2026).Define<DefinitionStudent>()
                .RuleFor(x => DefinitionStudent.Counter, _ => 1);

            AssertSingleError(definition, ConfigurationErrorCodes.StaticMemberNotSupported);
        }

        [Fact]
        public void RuleFor_SelectorWithBoxingConversion_ShouldReportError()
        {
            // int -> object is a boxing conversion, so the compiler wraps the property in Convert(...).
            var definition = new Faker(2026).Define<DefinitionStudent>()
                .RuleFor<object>(x => x.Age, _ => 1);

            AssertSingleError(definition, ConfigurationErrorCodes.UnsupportedSelector);
        }

        [Fact]
        public void RuleFor_ReferenceTypeWidenedToObject_ShouldReportTypeMismatch()
        {
            // string -> object is a reference conversion: the compiler adds no Convert node,
            // so the selector is a plain property access whose type differs from TProperty.
            var definition = new Faker(2026).Define<DefinitionStudent>()
                .RuleFor<object>(x => x.FirstName, _ => "A");

            AssertSingleError(definition, ConfigurationErrorCodes.PropertyTypeMismatch);
        }

        [Fact]
        public void RuleFor_PropertyTypeMismatch_ShouldReportError()
        {
            // A hand-built selector whose body type (string) is only reference-assignable to object.
            ParameterExpression parameter = Expression.Parameter(typeof(DefinitionStudent), "x");

            var selector = Expression.Lambda<Func<DefinitionStudent, object>>(
                Expression.Property(parameter, nameof(DefinitionStudent.FirstName)),
                parameter);

            var definition = new Faker(2026).Define<DefinitionStudent>()
                .RuleFor(selector, _ => "A");

            AssertSingleError(definition, ConfigurationErrorCodes.PropertyTypeMismatch);
        }

        [Fact]
        public void RuleFor_InvalidRule_ShouldNotBreakTheChain()
        {
            var definition = new Faker(2026).Define<DefinitionStudent>()
                .RuleFor(x => x.Code, _ => "ignored")
                .RuleFor(x => x.FirstName, _ => "Valid")
                .RuleFor(x => x.Address.City, _ => "ignored");

            Assert.Equal(2, definition.ConfigurationErrors.Count);
            Assert.Equal(1, definition.RuleCount);

            Assert.Equal("Valid", Generate(definition, 0).FirstName);
        }

        // ---------- Freeze / thread-safety ----------

        [Fact]
        public void Freeze_ShouldBeIdempotentAndKeepRules()
        {
            var definition = new Faker(2026).Define<DefinitionStudent>()
                .RuleFor(x => x.FirstName, _ => "A");

            definition.Freeze();
            definition.Freeze();

            Assert.True(definition.IsFrozen);
            Assert.True(definition.IsValid);
            Assert.Equal("A", Generate(definition, 0).FirstName);
        }

        [Fact]
        public void RuleFor_AfterFreeze_ShouldBeIgnoredAndReported()
        {
            var definition = new Faker(2026).Define<DefinitionStudent>()
                .RuleFor(x => x.FirstName, _ => "Original")
                .Freeze();

            definition.RuleFor(x => x.FirstName, _ => "Changed");
            definition.RuleFor(x => x.Email, _ => "new@example.test");

            Assert.Equal(1, definition.RuleCount);
            Assert.Equal(2, definition.ConfigurationErrors.Count);
            Assert.All(definition.ConfigurationErrors, e => Assert.Equal(ConfigurationErrorCodes.DefinitionFrozen, e.Code));

            var student = Generate(definition, 0);

            Assert.Equal("Original", student.FirstName);
            Assert.Equal(string.Empty, student.Email);
        }

        [Fact]
        public void ConfigurationErrors_ShouldBeASnapshot()
        {
            var definition = new Faker(2026).Define<DefinitionStudent>();

            var before = definition.ConfigurationErrors;

            definition.RuleFor(x => x.Code, _ => "A");

            Assert.Empty(before);
            Assert.Single(definition.ConfigurationErrors);
        }

        [Fact]
        public void RuleFor_ConcurrentRegistration_ShouldKeepSingleRulePerProperty()
        {
            var definition = new Faker(2026).Define<DefinitionStudent>();

            Parallel.For(0, 200, i =>
            {
                definition.RuleFor(x => x.FirstName, _ => "A");
                definition.RuleFor(x => x.Email, _ => "B");
            });

            Assert.True(definition.IsValid);
            Assert.Equal(2, definition.RuleCount);
        }

        [Fact]
        public void ApplyRules_ConcurrentGenerationFromFrozenDefinition_ShouldBeConsistent()
        {
            var definition = BuildStudentDefinition(new Faker(2026)).Freeze();

            var results = new ConcurrentDictionary<ulong, DefinitionStudent>();

            Parallel.For(0, 200, i =>
            {
                results[(ulong)i] = Generate(definition, (ulong)i);
            });

            Assert.Equal(200, results.Count);

            // The same item index must always produce the same object, whatever the thread.
            for (ulong i = 0; i < 200; i++)
            {
                AssertSameStudent(Generate(definition, i), results[i]);
            }
        }

        // ---------- Reproducibility & isolation ----------

        [Fact]
        public void SameSeedAndDefinitionOrder_ShouldProduceSameObjects()
        {
            var first = BuildStudentDefinition(new Faker(2026));
            var second = BuildStudentDefinition(new Faker(2026));

            for (ulong i = 0; i < 20; i++)
            {
                AssertSameStudent(Generate(first, i), Generate(second, i));
            }
        }

        [Fact]
        public void DifferentSeeds_ShouldProduceDifferentObjects()
        {
            var first = Generate(BuildStudentDefinition(new Faker(2026)), 0);
            var second = Generate(BuildStudentDefinition(new Faker(2027)), 0);

            Assert.NotEqual(first.Email, second.Email);
        }

        [Fact]
        public void DifferentItemIndexes_ShouldProduceDifferentObjects()
        {
            var definition = BuildStudentDefinition(new Faker(2026));

            var first = Generate(definition, 0);
            var second = Generate(definition, 1);

            Assert.NotEqual(first.Email, second.Email);
        }

        [Fact]
        public void ConsumingOneProvider_ShouldNotAffectAnotherProvider()
        {
            var definition = new Faker(2026).Define<DefinitionStudent>();

            var untouched = definition.CreateContext(0);
            var busy = definition.CreateContext(0);

            for (int i = 0; i < 25; i++)
            {
                busy.Names.TryFullName(out _);
            }

            untouched.Internet.TryEmail(out string expected);
            busy.Internet.TryEmail(out string actual);

            Assert.Equal(expected, actual);
        }

        // ---------- helpers ----------

        private static FakerDefinition<DefinitionStudent> BuildStudentDefinition(Faker faker)
        {
            return faker.Define<DefinitionStudent>()
                .RuleFor(x => x.FirstName, context => context.Names.TryFirstName(out var name) ? name : "Unknown")
                .RuleFor(x => x.Email, context => context.Internet.TryEmail(out var email) ? email : string.Empty)
                .RuleFor(x => x.Age, context => context.RandomData.TryInt(18, 40, out var age) ? age : 18)
                .RuleFor(x => x.BirthDate, context => context.Dates.TryDateOfBirth(AsOfDate, 18, 40, out var date) ? date : default);
        }

        private static DefinitionStudent Generate(FakerDefinition<DefinitionStudent> definition, ulong itemIndex)
        {
            Assert.True(definition.TryCreateInstance(out var student));

            definition.ApplyRules(student!, definition.CreateContext(itemIndex));

            return student!;
        }

        private static void AssertSameStudent(DefinitionStudent expected, DefinitionStudent actual)
        {
            Assert.Equal(expected.FirstName, actual.FirstName);
            Assert.Equal(expected.Email, actual.Email);
            Assert.Equal(expected.Age, actual.Age);
            Assert.Equal(expected.BirthDate, actual.BirthDate);
        }

        private static void AssertSingleError(FakerDefinition<DefinitionStudent> definition, string expectedCode)
        {
            Assert.False(definition.IsValid);

            var error = Assert.Single(definition.ConfigurationErrors);

            Assert.Equal(expectedCode, error.Code);
            Assert.False(string.IsNullOrWhiteSpace(error.Message));
            Assert.Equal(0, definition.RuleCount);
        }

        private static string Log(List<string> log, string entry)
        {
            log.Add(entry);

            return entry;
        }
    }
}
