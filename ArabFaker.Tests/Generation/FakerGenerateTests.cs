using ArabFaker.NET;
using ArabFaker.NET.Generation;
using System.Collections.Concurrent;

namespace ArabFaker.Tests.Generation
{
    public class FakerGenerateTests
    {
        private static FakerDefinition<DefinitionStudent> StudentDefinition(Faker faker)
        {
            return faker.Define<DefinitionStudent>()
                .RuleFor(x => x.FirstName, context => context.Names.TryFirstName(out var name) ? name : "Unknown")
                .RuleFor(x => x.Age, context => context.RandomData.TryInt(0, 1_000_000, out var age) ? age : -1);
        }

        private static int ThrowBoom() => throw new InvalidOperationException("boom");

        private static string Snapshot(DefinitionStudent student) => $"{student.FirstName}|{student.Age}";

        // ---------- Generate() ----------

        [Fact]
        public void Generate_ValidDefinition_ShouldSucceedWithFilledObject()
        {
            var result = StudentDefinition(new Faker(2026)).Generate();

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);
            Assert.NotNull(result.Value);
            Assert.False(string.IsNullOrWhiteSpace(result.Value!.FirstName));
            Assert.InRange(result.Value!.Age, 0, 1_000_000);
        }

        [Fact]
        public void Generate_WithoutRules_ShouldReturnDefaultInstance()
        {
            var result = new Faker(2026).Define<DefinitionStudent>().Generate();

            Assert.True(result.IsSuccess);
            Assert.Equal(string.Empty, result.Value!.FirstName);
            Assert.Equal(0, result.Value!.Age);
        }

        [Fact]
        public void Generate_SameSeedAndSameCalls_ShouldProduceSameObjects()
        {
            var first = StudentDefinition(new Faker(2026));
            var second = StudentDefinition(new Faker(2026));

            for (int i = 0; i < 20; i++)
            {
                Assert.Equal(Snapshot(first.Generate().Value!), Snapshot(second.Generate().Value!));
            }
        }

        [Fact]
        public void Generate_DifferentSeeds_ShouldProduceDifferentObjects()
        {
            var first = StudentDefinition(new Faker(1)).Generate(20).Value!.Select(Snapshot);
            var second = StudentDefinition(new Faker(2)).Generate(20).Value!.Select(Snapshot);

            Assert.NotEqual(first, second);
        }

        [Fact]
        public void Generate_ConsecutiveCalls_ShouldProduceDifferentObjects()
        {
            var definition = StudentDefinition(new Faker(2026));

            var ages = Enumerable.Range(0, 20).Select(_ => definition.Generate().Value!.Age).Distinct().ToList();

            Assert.True(ages.Count > 1);
        }

        [Fact]
        public void Generate_InvalidDefinition_ShouldFailWithoutValue()
        {
            var definition = new Faker(2026).Define<DefinitionStudent>()
                .RuleFor(x => x.Code, _ => "x");

            var result = definition.Generate();

            Assert.False(result.IsSuccess);
            Assert.Null(result.Value);

            var error = Assert.Single(result.Errors);
            Assert.Equal(GenerationErrorCodes.InvalidDefinition, error.Code);
            Assert.Contains(ConfigurationErrorCodes.FieldNotSupported, error.Message);
        }

        [Fact]
        public void Generate_NullDefineFactory_ShouldFailAsInvalidDefinition()
        {
            var result = new Faker(2026).Define<DefinitionStudent>(null!).Generate();

            Assert.False(result.IsSuccess);
            Assert.Equal(GenerationErrorCodes.InvalidDefinition, result.Errors[0].Code);
            Assert.Contains(ConfigurationErrorCodes.NullFactory, result.Errors[0].Message);
        }

        [Fact]
        public void Generate_InvalidDefinition_ShouldReportOneErrorPerConfigurationError()
        {
            var definition = new Faker(2026).Define<DefinitionStudent>()
                .RuleFor(x => x.Code, _ => "x")
                .RuleFor(x => x.Created, _ => "y");

            var result = definition.Generate();

            Assert.Equal(2, result.Errors.Count);
        }

        [Fact]
        public void Generate_FactoryReturnsNull_ShouldFailWithoutValue()
        {
            var definition = new Faker(2026).Define<DefinitionStudent>(() => null!);

            var result = definition.Generate();

            Assert.False(result.IsSuccess);
            Assert.Null(result.Value);
            Assert.Equal(GenerationErrorCodes.FactoryReturnedNull, Assert.Single(result.Errors).Code);
        }

        [Fact]
        public void Generate_ShouldFreezeValidDefinition()
        {
            var definition = StudentDefinition(new Faker(2026));

            definition.Generate();
            definition.RuleFor(x => x.Email, _ => "late@example.com");

            Assert.True(definition.IsFrozen);
            Assert.Equal(ConfigurationErrorCodes.DefinitionFrozen, Assert.Single(definition.ConfigurationErrors).Code);
        }

        [Fact]
        public void Generate_InvalidDefinition_ShouldNotFreeze()
        {
            var definition = new Faker(2026).Define<DefinitionStudent>()
                .RuleFor(x => x.Code, _ => "x");

            definition.Generate();

            Assert.False(definition.IsFrozen);
        }

        [Fact]
        public void Generate_RuleThrows_ShouldPropagateException()
        {
            var definition = new Faker(2026).Define<DefinitionStudent>()
                .RuleFor(x => x.Age, _ => ThrowBoom());

            var exception = Assert.Throws<InvalidOperationException>(() => definition.Generate());

            Assert.Equal("boom", exception.Message);
        }

        [Fact]
        public void Generate_RulesShouldApplyInRegistrationOrder()
        {
            var log = new List<string>();

            var definition = new Faker(2026).Define<DefinitionStudent>()
                .RuleFor(x => x.FirstName, _ => { log.Add("first"); return "a"; })
                .RuleFor(x => x.Email, _ => { log.Add("email"); return "b"; });

            definition.Generate();

            Assert.Equal(new[] { "first", "email" }, log);
        }

        // ---------- Generate(count) ----------

        [Fact]
        public void GenerateCount_ShouldReturnRequestedNumberOfObjects()
        {
            var result = StudentDefinition(new Faker(2026)).Generate(25);

            Assert.True(result.IsSuccess);
            Assert.Equal(25, result.Value!.Count);
            Assert.All(result.Value!, item => Assert.NotNull(item));
            Assert.Equal(25, result.Value!.Distinct().Count());
        }

        [Fact]
        public void GenerateCount_Zero_ShouldSucceedWithEmptyList()
        {
            var result = StudentDefinition(new Faker(2026)).Generate(0);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value!);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void GenerateCount_Negative_ShouldFailWithInvalidCount(int count)
        {
            var result = StudentDefinition(new Faker(2026)).Generate(count);

            Assert.False(result.IsSuccess);
            Assert.Null(result.Value);
            Assert.Equal(GenerationErrorCodes.InvalidCount, Assert.Single(result.Errors).Code);
        }

        [Fact]
        public void GenerateCount_Negative_ShouldNotConsumeItemIndices()
        {
            var failing = StudentDefinition(new Faker(2026));
            var reference = StudentDefinition(new Faker(2026));

            failing.Generate(-5);

            Assert.Equal(Snapshot(reference.Generate().Value!), Snapshot(failing.Generate().Value!));
        }

        [Fact]
        public void GenerateCount_ShouldMatchRepeatedSingleGenerate()
        {
            var batch = StudentDefinition(new Faker(2026)).Generate(10).Value!.Select(Snapshot).ToList();

            var single = StudentDefinition(new Faker(2026));
            var singles = Enumerable.Range(0, 10).Select(_ => Snapshot(single.Generate().Value!)).ToList();

            Assert.Equal(batch, singles);
        }

        [Fact]
        public void GenerateCount_InvalidDefinition_ShouldFail()
        {
            var definition = new Faker(2026).Define<DefinitionStudent>()
                .RuleFor(x => x.Code, _ => "x");

            var result = definition.Generate(3);

            Assert.False(result.IsSuccess);
            Assert.Null(result.Value);
            Assert.Equal(GenerationErrorCodes.InvalidDefinition, result.Errors[0].Code);
        }

        [Fact]
        public void GenerateCount_OneItemFails_ShouldReturnNoObjects()
        {
            int calls = 0;

            var definition = new Faker(2026).Define<DefinitionStudent>(
                () => ++calls == 3 ? null! : new DefinitionStudent());

            var result = definition.Generate(5);

            Assert.False(result.IsSuccess);
            Assert.Null(result.Value);
            Assert.Equal(GenerationErrorCodes.FactoryReturnedNull, Assert.Single(result.Errors).Code);
        }

        // ---------- GenerateLazy ----------

        [Fact]
        public void GenerateLazy_ShouldNotGenerateUntilEnumerated()
        {
            int calls = 0;

            var definition = new Faker(2026).Define<DefinitionStudent>(
                () => { calls++; return new DefinitionStudent(); });

            var sequence = definition.GenerateLazy(5);

            Assert.Equal(0, calls);

            var results = sequence.ToList();

            Assert.Equal(5, calls);
            Assert.Equal(5, results.Count);
            Assert.All(results, result => Assert.True(result.IsSuccess));
        }

        [Fact]
        public void GenerateLazy_ShouldMatchGenerateCount()
        {
            var lazy = StudentDefinition(new Faker(2026)).GenerateLazy(10).Select(r => Snapshot(r.Value!)).ToList();
            var batch = StudentDefinition(new Faker(2026)).Generate(10).Value!.Select(Snapshot).ToList();

            Assert.Equal(batch, lazy);
        }

        [Fact]
        public void GenerateLazy_EnumeratingTwice_ShouldContinueWithFreshIndices()
        {
            var sequence = StudentDefinition(new Faker(2026)).GenerateLazy(10);

            var first = sequence.Select(r => Snapshot(r.Value!)).ToList();
            var second = sequence.Select(r => Snapshot(r.Value!)).ToList();

            Assert.NotEqual(first, second);

            var continuous = StudentDefinition(new Faker(2026)).Generate(20).Value!.Select(Snapshot).ToList();

            Assert.Equal(continuous, first.Concat(second));
        }

        [Fact]
        public void GenerateLazy_Zero_ShouldBeEmpty()
        {
            Assert.Empty(StudentDefinition(new Faker(2026)).GenerateLazy(0));
        }

        [Fact]
        public void GenerateLazy_Negative_ShouldYieldSingleInvalidCountFailure()
        {
            var results = StudentDefinition(new Faker(2026)).GenerateLazy(-1).ToList();

            var result = Assert.Single(results);

            Assert.False(result.IsSuccess);
            Assert.Equal(GenerationErrorCodes.InvalidCount, Assert.Single(result.Errors).Code);
        }

        [Fact]
        public void GenerateLazy_InvalidDefinition_ShouldYieldSingleFailure()
        {
            var definition = new Faker(2026).Define<DefinitionStudent>()
                .RuleFor(x => x.Code, _ => "x");

            var results = definition.GenerateLazy(5).ToList();

            var result = Assert.Single(results);

            Assert.False(result.IsSuccess);
            Assert.Equal(GenerationErrorCodes.InvalidDefinition, result.Errors[0].Code);
        }

        [Fact]
        public void GenerateLazy_FailedItem_ShouldNotStopSequence()
        {
            int calls = 0;

            var definition = new Faker(2026).Define<DefinitionStudent>(
                () => ++calls == 2 ? null! : new DefinitionStudent());

            var results = definition.GenerateLazy(3).ToList();

            Assert.Equal(3, results.Count);
            Assert.True(results[0].IsSuccess);
            Assert.False(results[1].IsSuccess);
            Assert.True(results[2].IsSuccess);
        }

        [Fact]
        public void GenerateLazy_CanBeStoppedEarly()
        {
            int calls = 0;

            var definition = new Faker(2026).Define<DefinitionStudent>(
                () => { calls++; return new DefinitionStudent(); });

            var firstTwo = definition.GenerateLazy(1_000_000).Take(2).ToList();

            Assert.Equal(2, firstTwo.Count);
            Assert.Equal(2, calls);
        }

        // ---------- Item index exhaustion ----------

        [Fact]
        public void Generate_AtLastIndex_ShouldSucceedOnceThenReportExhausted()
        {
            var definition = StudentDefinition(new Faker(2026));

            definition.SetNextIndexForTesting(ulong.MaxValue);

            Assert.True(definition.Generate().IsSuccess);

            var result = definition.Generate();

            Assert.False(result.IsSuccess);
            Assert.Equal(GenerationErrorCodes.ItemIndexExhausted, Assert.Single(result.Errors).Code);
        }

        [Fact]
        public void GenerateCount_NotEnoughIndicesLeft_ShouldFailWithoutConsumingAny()
        {
            var definition = StudentDefinition(new Faker(2026));

            definition.SetNextIndexForTesting(ulong.MaxValue);

            var tooMany = definition.Generate(2);

            Assert.False(tooMany.IsSuccess);
            Assert.Equal(GenerationErrorCodes.ItemIndexExhausted, Assert.Single(tooMany.Errors).Code);
            Assert.True(definition.Generate().IsSuccess);
        }

        [Fact]
        public void GenerateLazy_RunsOutOfIndices_ShouldYieldFailureAndStop()
        {
            var definition = StudentDefinition(new Faker(2026));

            definition.SetNextIndexForTesting(ulong.MaxValue);

            var results = definition.GenerateLazy(5).ToList();

            Assert.Equal(2, results.Count);
            Assert.True(results[0].IsSuccess);
            Assert.Equal(GenerationErrorCodes.ItemIndexExhausted, Assert.Single(results[1].Errors).Code);
        }

        // ---------- Concurrency ----------

        [Fact]
        public void Generate_ConcurrentCalls_ShouldUseEachIndexExactlyOnce()
        {
            const int total = 800;

            var sequential = StudentDefinition(new Faker(2026));
            var expected = Enumerable.Range(0, total)
                .Select(_ => sequential.Generate().Value!.Age)
                .OrderBy(age => age)
                .ToList();

            var concurrent = StudentDefinition(new Faker(2026));
            var bag = new ConcurrentBag<int>();

            Parallel.For(0, total, _ => bag.Add(concurrent.Generate().Value!.Age));

            Assert.Equal(expected, bag.OrderBy(age => age).ToList());
        }

        // ---------- GenerationResult ----------

        [Fact]
        public void GenerationResult_Failure_ShouldExposeReadOnlyErrorsSnapshot()
        {
            var source = new List<GenerationError> { new("code", "message") };

            var result = GenerationResult<string>.Failure(source);

            source.Add(new GenerationError("other", "other"));

            Assert.Single(result.Errors);
            Assert.False(result.IsSuccess);
            Assert.Null(result.Value);
        }

        [Fact]
        public void GenerationResult_FailureWithoutErrors_ShouldStillBeFailure()
        {
            var result = GenerationResult<string>.Failure(Array.Empty<GenerationError>());

            Assert.False(result.IsSuccess);
            Assert.NotEmpty(result.Errors);
        }
    }
}
