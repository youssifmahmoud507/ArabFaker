using ArabFaker.NET;
using ArabFaker.NET.Generation;
using ArabFaker.NET.Providers.Arab.Names;

namespace ArabFaker.Tests.Generation
{
    public class AutoFillTests
    {
        private static readonly DateOnly Reference = new(2026, 1, 1);

        private static bool HasArabic(string text) => text.Any(ch => ch >= '؀' && ch <= 'ۿ');

        // ---------- Filling by name and type ----------

        [Fact]
        public void AutoFill_ShouldFillKnownPropertiesWithSensibleValues()
        {
            var person = new Faker(2026).Define<AutoFillPerson>().AutoFill().Generate().Value!;

            Assert.False(string.IsNullOrWhiteSpace(person.FirstName));
            Assert.False(string.IsNullOrWhiteSpace(person.MiddleName));
            Assert.False(string.IsNullOrWhiteSpace(person.LastName));
            Assert.False(string.IsNullOrWhiteSpace(person.FullName));
            Assert.Contains("@", person.Email);
            Assert.False(string.IsNullOrWhiteSpace(person.Username));
            Assert.False(string.IsNullOrWhiteSpace(person.Governorate));
            Assert.False(string.IsNullOrWhiteSpace(person.City));
            Assert.InRange(person.Age, 18, 60);
            Assert.InRange(person.Salary, 1m, 10_000m);
            Assert.True(Enum.IsDefined(person.Status));
            Assert.True(person.OptionalStatus.HasValue);
            Assert.NotEqual(Guid.Empty, person.Token);
        }

        [Fact]
        public void AutoFill_DateOfBirth_ShouldRespectAgeRangeRelativeToReferenceDate()
        {
            var people = new Faker(2026).Define<AutoFillPerson>().AutoFill().Generate(50).Value!;

            Assert.All(people, person =>
            {
                Assert.True(person.BirthDate <= Reference.AddYears(-18));
                Assert.True(person.BirthDate >= Reference.AddYears(-61));
            });
        }

        [Fact]
        public void AutoFill_OtherDates_ShouldStayWithinFiveYearsBeforeReference()
        {
            var people = new Faker(2026).Define<AutoFillPerson>().AutoFill().Generate(50).Value!;

            Assert.All(people, person =>
            {
                Assert.InRange(person.CreatedAt, Reference.AddYears(-5).ToDateTime(TimeOnly.MinValue), Reference.ToDateTime(TimeOnly.MinValue));
            });
        }

        [Fact]
        public void AutoFill_CustomOptions_ShouldBeUsed()
        {
            var options = new AutoFillOptions
            {
                ReferenceDate = new DateOnly(2000, 6, 15),
                MinAge = 30,
                MaxAge = 31
            };

            var people = new Faker(2026).Define<AutoFillPerson>().AutoFill(options).Generate(30).Value!;

            Assert.All(people, person =>
            {
                Assert.InRange(person.Age, 30, 31);
                Assert.True(person.BirthDate <= options.ReferenceDate.AddYears(-30));
                Assert.True(person.BirthDate >= options.ReferenceDate.AddYears(-32));
            });
        }

        [Fact]
        public void AutoFill_DefaultLanguage_ShouldProduceArabicNames()
        {
            var person = new Faker(2026).Define<AutoFillPerson>().AutoFill().Generate().Value!;

            Assert.True(HasArabic(person.FirstName));
            Assert.True(HasArabic(person.Governorate));
        }

        [Fact]
        public void AutoFill_LatinLanguage_ShouldProduceLatinNamesAndEnglishLocations()
        {
            var options = new AutoFillOptions { Language = NameLanguage.Latin };

            var person = new Faker(2026).Define<AutoFillPerson>().AutoFill(options).Generate().Value!;

            Assert.False(HasArabic(person.FirstName));
            Assert.False(HasArabic(person.LastName));
            Assert.False(HasArabic(person.Governorate));
        }

        [Fact]
        public void AutoFill_SameSeed_ShouldProduceSameObjects()
        {
            var first = new Faker(7).Define<AutoFillPerson>().AutoFill().Generate(10).Value!;
            var second = new Faker(7).Define<AutoFillPerson>().AutoFill().Generate(10).Value!;

            for (int i = 0; i < first.Count; i++)
            {
                Assert.Equal(first[i].FullName, second[i].FullName);
                Assert.Equal(first[i].Email, second[i].Email);
                Assert.Equal(first[i].BirthDate, second[i].BirthDate);
                Assert.Equal(first[i].Token, second[i].Token);
                Assert.Equal(first[i].Status, second[i].Status);
            }
        }

        [Fact]
        public void AutoFill_StandaloneClassWithoutNavigationProperties_ShouldWorkWithNoSetup()
        {
            var result = new Faker(2026).Define<AutoFillOrder>().AutoFill().Generate(3);

            Assert.True(result.IsSuccess);
            Assert.Equal(3, result.Value!.Count);
            Assert.All(result.Value, order => Assert.InRange(order.Quantity, 1, 100));
        }

        // ---------- Keys ----------

        [Fact]
        public void AutoFill_IntId_ShouldBeSequentialFromOne()
        {
            var people = new Faker(2026).Define<AutoFillPerson>().AutoFill().Generate(5).Value!;

            Assert.Equal(new[] { 1, 2, 3, 4, 5 }, people.Select(p => p.Id));
        }

        [Fact]
        public void AutoFill_IntId_ShouldContinueAcrossGenerateCalls()
        {
            var definition = new Faker(2026).Define<AutoFillPerson>().AutoFill();

            Assert.Equal(1, definition.Generate().Value!.Id);
            Assert.Equal(new[] { 2, 3 }, definition.Generate(2).Value!.Select(p => p.Id));
        }

        [Fact]
        public void AutoFill_IntId_ShouldClampAtMaxValue()
        {
            var definition = new Faker(2026).Define<AutoFillPerson>().AutoFill();

            definition.SetNextIndexForTesting((UInt128)((ulong)int.MaxValue + 5UL));

            Assert.Equal(int.MaxValue, definition.Generate().Value!.Id);
        }

        [Fact]
        public void AutoFill_ForeignKeyLikeProperty_ShouldNotBeGuessed()
        {
            var definition = new Faker(2026).Define<AutoFillOrder>().AutoFill();

            var order = definition.Generate().Value!;

            Assert.Equal(0, order.CustomerId);
            Assert.Contains("CustomerId", definition.UnmappedProperties);
            Assert.DoesNotContain("Id", definition.UnmappedProperties);
        }

        [Fact]
        public void AutoFill_NameEndingInLowercaseId_ShouldNotBeTreatedAsKey()
        {
            var definition = new Faker(2026).Define<AutoFillOrder>().AutoFill();

            // Id, Quantity, Price and Paid are filled; only CustomerId is skipped.
            Assert.Equal(4, definition.RuleCount);
            Assert.Equal(new[] { "CustomerId" }, definition.UnmappedProperties);
        }

        [Fact]
        public void AutoFill_GuidKeys_ShouldFillIdButNotForeignKey()
        {
            var definition = new Faker(2026).Define<AutoFillGuidEntity>().AutoFill();

            var first = definition.Generate().Value!;
            var second = definition.Generate().Value!;

            Assert.NotEqual(Guid.Empty, first.Id);
            Assert.NotEqual(first.Id, second.Id);
            Assert.Equal(Guid.Empty, first.OwnerId);
            Assert.Equal(new[] { "OwnerId" }, definition.UnmappedProperties);
        }

        [Fact]
        public void AutoFill_StringKeys_ShouldFillIdWithGuidTextButNotForeignKey()
        {
            var definition = new Faker(2026).Define<AutoFillStringKeyEntity>().AutoFill();

            var entity = definition.Generate().Value!;

            Assert.True(Guid.TryParse(entity.Id, out _));
            Assert.Equal(string.Empty, entity.ParentId);
            Assert.Equal(new[] { "ParentId" }, definition.UnmappedProperties);
        }

        [Fact]
        public void AutoFill_LongKeys_ShouldFillIdButNotNullableForeignKey()
        {
            var definition = new Faker(2026).Define<AutoFillLongKeyEntity>().AutoFill();

            var items = definition.Generate(3).Value!;

            Assert.Equal(new long[] { 1, 2, 3 }, items.Select(x => x.Id));
            Assert.All(items, item => Assert.Null(item.ParentId));
            Assert.Equal(new[] { "ParentId" }, definition.UnmappedProperties);
        }

        // ---------- Precedence ----------

        [Fact]
        public void AutoFill_ExplicitRuleRegisteredBefore_ShouldWin()
        {
            var definition = new Faker(2026).Define<AutoFillPerson>()
                .RuleFor(x => x.FirstName, _ => "Fixed")
                .AutoFill();

            Assert.Equal("Fixed", definition.Generate().Value!.FirstName);
        }

        [Fact]
        public void AutoFill_ExplicitRuleRegisteredAfter_ShouldReplaceConventionRule()
        {
            var definition = new Faker(2026).Define<AutoFillPerson>().AutoFill();

            int countAfterAutoFill = definition.RuleCount;

            definition.RuleFor(x => x.FirstName, _ => "Fixed");

            Assert.Equal(countAfterAutoFill, definition.RuleCount);
            Assert.Equal("Fixed", definition.Generate().Value!.FirstName);
            Assert.True(definition.IsValid);
        }

        [Fact]
        public void AutoFill_ExplicitRule_ShouldRemovePropertyFromUnmappedList()
        {
            var definition = new Faker(2026).Define<AutoFillPerson>()
                .RuleFor(x => x.Notes, _ => "note")
                .AutoFill();

            Assert.DoesNotContain("Notes", definition.UnmappedProperties);
            Assert.Contains("Name", definition.UnmappedProperties);
        }

        [Fact]
        public void AutoFill_Twice_ShouldNotChangeRules()
        {
            var definition = new Faker(2026).Define<AutoFillPerson>().AutoFill();

            int count = definition.RuleCount;
            var unmapped = definition.UnmappedProperties.ToList();

            definition.AutoFill();

            Assert.Equal(count, definition.RuleCount);
            Assert.Equal(unmapped, definition.UnmappedProperties);
        }

        // ---------- Unmapped and skipped ----------

        [Fact]
        public void AutoFill_BareNameAndUnknownStrings_ShouldStayUnmapped()
        {
            var definition = new Faker(2026).Define<AutoFillPerson>().AutoFill();

            var person = definition.Generate().Value!;

            Assert.Equal(string.Empty, person.Name);
            Assert.Equal(string.Empty, person.Notes);
            Assert.Equal(new[] { "Name", "Notes" }, definition.UnmappedProperties);
        }

        [Fact]
        public void AutoFill_ShouldSkipNonWritableAndUnsupportedMembers()
        {
            var definition = new Faker(2026).Define<DefinitionStudent>().AutoFill();

            // FirstName, Email, Age, BirthDate and Score are filled.
            // Nickname and Address have no convention. Counter (static), Secret (private set),
            // Created (get-only), Tag (init-only), Code (field) and the indexer are never considered.
            Assert.Equal(5, definition.RuleCount);
            Assert.Equal(new[] { "Address", "Nickname" }, definition.UnmappedProperties);
            Assert.True(definition.IsValid);

            var student = definition.Generate().Value!;

            Assert.True(student.Score.HasValue);
            Assert.Equal(string.Empty, student.Tag);
        }

        [Fact]
        public void AutoFill_InheritedProperties_ShouldBeConsidered()
        {
            var definition = new Faker(2026).Define<DefinitionDerived>().AutoFill();

            Assert.Equal(1, definition.RuleCount);
            Assert.Equal(new[] { "BaseValue" }, definition.UnmappedProperties);
        }

        [Fact]
        public void AutoFill_EmptyEnum_ShouldStayUnmapped()
        {
            var definition = new Faker(2026).Define<AutoFillEmptyEnumEntity>().AutoFill();

            Assert.Equal(0, definition.RuleCount);
            Assert.Equal(new[] { "Kind" }, definition.UnmappedProperties);
        }

        [Fact]
        public void AutoFill_WithoutAnyConvention_ShouldStillGenerateDefaultInstances()
        {
            var result = new Faker(2026).Define<DefinitionBase>().AutoFill().Generate(2);

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value!.Count);
        }

        // ---------- Validation ----------

        [Fact]
        public void AutoFill_NullOptions_ShouldReportErrorAndAddNoRules()
        {
            var definition = new Faker(2026).Define<AutoFillPerson>().AutoFill(null!);

            Assert.Equal(ConfigurationErrorCodes.InvalidAutoFillOptions, Assert.Single(definition.ConfigurationErrors).Code);
            Assert.Equal(0, definition.RuleCount);
        }

        [Theory]
        [InlineData(-1, 60, 2026)]
        [InlineData(30, 20, 2026)]
        [InlineData(18, 151, 2026)]
        [InlineData(18, 60, 100)]
        public void AutoFill_InvalidOptions_ShouldReportErrorAndAddNoRules(int minAge, int maxAge, int year)
        {
            var options = new AutoFillOptions
            {
                MinAge = minAge,
                MaxAge = maxAge,
                ReferenceDate = new DateOnly(year, 1, 1)
            };

            var definition = new Faker(2026).Define<AutoFillPerson>().AutoFill(options);

            Assert.Equal(ConfigurationErrorCodes.InvalidAutoFillOptions, Assert.Single(definition.ConfigurationErrors).Code);
            Assert.Equal(0, definition.RuleCount);
            Assert.False(definition.Generate().IsSuccess);
        }

        [Fact]
        public void AutoFill_UndefinedLanguage_ShouldReportError()
        {
            var options = new AutoFillOptions { Language = (NameLanguage)99 };

            var definition = new Faker(2026).Define<AutoFillPerson>().AutoFill(options);

            Assert.Equal(ConfigurationErrorCodes.InvalidAutoFillOptions, Assert.Single(definition.ConfigurationErrors).Code);
        }

        [Fact]
        public void AutoFill_AfterFreeze_ShouldBeIgnoredAndReported()
        {
            var definition = new Faker(2026).Define<AutoFillPerson>().Freeze().AutoFill();

            Assert.Equal(ConfigurationErrorCodes.DefinitionFrozen, Assert.Single(definition.ConfigurationErrors).Code);
            Assert.Equal(0, definition.RuleCount);
        }

        // ---------- FakerContext.ItemIndex ----------

        [Fact]
        public void FakerContext_ItemIndex_ShouldMatchPositionInGeneratedSequence()
        {
            var definition = new Faker(2026).Define<DefinitionStudent>()
                .RuleFor(x => x.Age, context => (int)context.ItemIndex);

            var ages = definition.Generate(4).Value!.Select(x => x.Age);

            Assert.Equal(new[] { 0, 1, 2, 3 }, ages);
        }
    }
}
