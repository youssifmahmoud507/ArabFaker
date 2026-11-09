using ArabFaker.NET;
using ArabFaker.NET.Generation;
using ArabFaker.NET.Providers.Arab.Names;
using ArabFaker.NET.Providers.Arab.Reference;

namespace ArabFaker.Tests.Generation
{
    internal sealed class DependentEmployee
    {
        public int Id { get; set; }
        public string Department { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
    }

    internal sealed class DependentCourse
    {
        public int Id { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
    }

    internal sealed class DependentSteps
    {
        public string First { get; set; } = string.Empty;
        public string Second { get; set; } = string.Empty;
        public string Third { get; set; } = string.Empty;
    }

    public class DependentRuleTests
    {
        private static readonly AutoFillOptions Latin = new() { Language = NameLanguage.Latin };

        [Fact]
        public void DependentRule_ShouldKeepAJobTitleInsideTheDepartmentOfTheSameRow()
        {
            var rows = new Faker(1).Define<DependentEmployee>()
                .AutoFill(Latin)
                .RuleFor(
                    x => x.JobTitle,
                    (x, c) => c.Reference.TryJobTitleInDepartment(x.Department, out string title, NameLanguage.Latin) ? title : string.Empty)
                .Generate(200).Value!;

            Assert.All(rows, row =>
            {
                var department = ReferenceCatalog.WorkDepartments.Single(d => d.Group.English == row.Department);

                Assert.Contains(department.Items, item => item.English == row.JobTitle);
            });
        }

        [Fact]
        public void DependentRule_ShouldKeepACourseInsideTheDepartmentEvenWhenItsNameSortsFirst()
        {
            // "CourseName" comes before "Department" alphabetically, so AutoFill fills it first.
            // A dependent rule still sees the final Department, because dependent rules run last.
            var rows = new Faker(1).Define<DependentCourse>()
                .AutoFill(Latin)
                .RuleFor(
                    x => x.CourseName,
                    (x, c) => c.Reference.TryCourseInDepartment(x.Department, out string course, NameLanguage.Latin) ? course : string.Empty)
                .Generate(200).Value!;

            Assert.All(rows, row =>
            {
                var department = ReferenceCatalog.AcademicDepartments.Single(d => d.Group.English == row.Department);

                Assert.Contains(department.Items, item => item.English == row.CourseName);
            });
        }

        [Fact]
        public void DependentRule_RegisteredBeforeOrdinaryRules_ShouldStillSeeTheirValues()
        {
            var item = new Faker(1).Define<DependentSteps>()
                .RuleFor(x => x.Second, (x, _) => x.First + "-2")
                .RuleFor(x => x.First, _ => "one")
                .RuleFor(x => x.Third, _ => "three")
                .Generate().Value!;

            Assert.Equal("one", item.First);
            Assert.Equal("one-2", item.Second);
            Assert.Equal("three", item.Third);
        }

        [Fact]
        public void DependentRules_ShouldRunInRegistrationOrder()
        {
            var item = new Faker(1).Define<DependentSteps>()
                .RuleFor(x => x.First, _ => "a")
                .RuleFor(x => x.Second, (x, _) => x.First + "b")
                .RuleFor(x => x.Third, (x, _) => x.Second + "c")
                .Generate().Value!;

            Assert.Equal("abc", item.Third);
        }

        [Fact]
        public void LastRuleShouldWin_WhetherOrdinaryOrDependent()
        {
            var definition = new Faker(1).Define<DependentSteps>()
                .RuleFor(x => x.First, _ => "ordinary")
                .RuleFor(x => x.First, (x, _) => "dependent")
                .RuleFor(x => x.Second, (x, _) => "dependent")
                .RuleFor(x => x.Second, _ => "ordinary")
                .RuleFor(x => x.Third, _ => "t");

            var item = definition.Generate().Value!;

            Assert.Equal("dependent", item.First);
            Assert.Equal("ordinary", item.Second);
            Assert.Equal(3, definition.RuleCount);
        }

        [Fact]
        public void DependentRule_ShouldReplaceAnAutoFillRule_AndBeDeterministic()
        {
            DependentEmployee Generate() => new Faker(5).Define<DependentEmployee>()
                .AutoFill()
                .RuleFor(x => x.JobTitle, (x, _) => "Title for " + x.Department)
                .Generate().Value!;

            var first = Generate();
            var second = Generate();

            Assert.Equal("Title for " + first.Department, first.JobTitle);
            Assert.Equal(first.Department, second.Department);
            Assert.Equal(first.JobTitle, second.JobTitle);
        }

        [Fact]
        public void DependentRule_InvalidSelector_ShouldReportConfigurationError()
        {
            var definition = new Faker(1).Define<DependentSteps>()
                .RuleFor<string>(null!, (x, _) => x.First);

            Assert.False(definition.IsValid);
            Assert.Equal(ConfigurationErrorCodes.NullPropertySelector, Assert.Single(definition.ConfigurationErrors).Code);
        }

        [Fact]
        public void DependentRule_AfterFreeze_ShouldBeIgnoredWithAnError()
        {
            var definition = new Faker(1).Define<DependentSteps>()
                .RuleFor(x => x.First, _ => "a")
                .Freeze()
                .RuleFor(x => x.Second, (x, _) => "late");

            Assert.Equal(ConfigurationErrorCodes.DefinitionFrozen, Assert.Single(definition.ConfigurationErrors).Code);
            Assert.Equal(1, definition.RuleCount);
        }
    }
}
