using ArabFaker.NET;
using ArabFaker.NET.Generation;
using ArabFaker.NET.Providers.Arab.Names;
using ArabFaker.NET.Providers.Arab.Reference;

namespace ArabFaker.Tests.Generation
{
    internal sealed class CoherentEmployee
    {
        public int Id { get; set; }

        public string Department { get; set; } = string.Empty;

        public string JobTitle { get; set; } = string.Empty;
    }

    internal sealed class CoherentCourse
    {
        public int Id { get; set; }

        public string AcademicDepartment { get; set; } = string.Empty;

        public string CourseName { get; set; } = string.Empty;
    }

    internal sealed class CoherentEnrollment
    {
        public int Id { get; set; }

        public string Department { get; set; } = string.Empty;

        public string CourseName { get; set; } = string.Empty;
    }

    internal sealed class LoneJobTitle
    {
        public int Id { get; set; }

        public string JobTitle { get; set; } = string.Empty;
    }

    /// <summary>
    /// AutoFill must not produce a legal-affairs officer who works as an assembly technician: a job title or course
    /// that sits next to a department comes from that department.
    /// </summary>
    public class AutoFillCoherenceTests
    {
        private static bool InGroup(ReferenceGroup[] groups, string department, string value, NameLanguage language)
        {
            string Pick(ReferenceEntry e) => language == NameLanguage.Arabic ? e.Arabic : e.English;

            return groups.Any(g => Pick(g.Group) == department && g.Items.Any(i => Pick(i) == value));
        }

        [Theory]
        [InlineData(NameLanguage.Arabic)]
        [InlineData(NameLanguage.Latin)]
        public void JobTitle_BelongsToTheObjectsDepartment(NameLanguage language)
        {
            var rows = new Faker(7).Define<CoherentEmployee>().AutoFill(new AutoFillOptions { Language = language }).Generate(300).Value!;

            Assert.All(rows, row => Assert.True(InGroup(ReferenceCatalog.WorkDepartments, row.Department, row.JobTitle, language), $"{row.Department} / {row.JobTitle}"));
            Assert.True(rows.Select(r => r.Department).Distinct().Count() > 5, "departments should still vary");
        }

        [Theory]
        [InlineData(NameLanguage.Arabic)]
        [InlineData(NameLanguage.Latin)]
        public void CourseName_BelongsToTheAcademicDepartmentProperty(NameLanguage language)
        {
            var rows = new Faker(7).Define<CoherentCourse>().AutoFill(new AutoFillOptions { Language = language }).Generate(300).Value!;

            Assert.All(rows, row => Assert.True(InGroup(ReferenceCatalog.AcademicDepartments, row.AcademicDepartment, row.CourseName, language), $"{row.AcademicDepartment} / {row.CourseName}"));
        }

        [Fact]
        public void CourseName_NextToABareDepartmentInAnAcademicClass_FollowsIt()
        {
            var rows = new Faker(7).Define<CoherentEnrollment>().AutoFill().Generate(300).Value!;

            Assert.All(rows, row => Assert.True(InGroup(ReferenceCatalog.AcademicDepartments, row.Department, row.CourseName, NameLanguage.Arabic), $"{row.Department} / {row.CourseName}"));
        }

        [Fact]
        public void JobTitle_FollowsADepartmentSetByAnExplicitRule()
        {
            ReferenceGroup group = ReferenceCatalog.WorkDepartments[3];

            var rows = new Faker(7).Define<CoherentEmployee>()
                .RuleFor(x => x.Department, _ => group.Group.Arabic)
                .AutoFill()
                .Generate(60).Value!;

            Assert.All(rows, row =>
            {
                Assert.Equal(group.Group.Arabic, row.Department);
                Assert.Contains(group.Items, item => item.Arabic == row.JobTitle);
            });
        }

        [Fact]
        public void JobTitle_WithAnUnknownDepartment_StillGetsARealJobTitle()
        {
            var rows = new Faker(7).Define<CoherentEmployee>()
                .RuleFor(x => x.Department, _ => "Not a department in the catalog")
                .AutoFill()
                .Generate(30).Value!;

            Assert.All(rows, row => Assert.Contains(ReferenceCatalog.WorkDepartments.SelectMany(g => g.Items), item => item.Arabic == row.JobTitle));
        }

        [Fact]
        public void JobTitle_WithoutADepartmentProperty_IsStillFilled()
        {
            var rows = new Faker(7).Define<LoneJobTitle>().AutoFill().Generate(30).Value!;

            Assert.All(rows, row => Assert.Contains(ReferenceCatalog.WorkDepartments.SelectMany(g => g.Items), item => item.Arabic == row.JobTitle));
        }

        [Fact]
        public void ExplicitJobTitleRule_WinsOverTheConvention()
        {
            var rows = new Faker(7).Define<CoherentEmployee>().RuleFor(x => x.JobTitle, _ => "Fixed").AutoFill().Generate(10).Value!;

            Assert.All(rows, row => Assert.Equal("Fixed", row.JobTitle));
        }

        [Fact]
        public void SameSeed_GivesTheSamePairs()
        {
            var first = new Faker(11).Define<CoherentEmployee>().AutoFill().Generate(50).Value!;
            var second = new Faker(11).Define<CoherentEmployee>().AutoFill().Generate(50).Value!;

            Assert.Equal(first.Select(r => (r.Department, r.JobTitle)), second.Select(r => (r.Department, r.JobTitle)));
        }
    }
}
