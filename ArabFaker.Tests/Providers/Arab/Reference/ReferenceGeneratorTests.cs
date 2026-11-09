using ArabFaker.NET;
using ArabFaker.NET.Generation;
using ArabFaker.NET.Providers.Arab.Names;
using ArabFaker.NET.Providers.Arab.Reference;
using System.Reflection;

namespace ArabFaker.Tests.Providers.Arab.Reference
{
    public class ReferenceGeneratorTests
    {
        private static bool HasArabic(string text) => text.Any(c => c >= '؀' && c <= 'ۿ');

        private static bool HasLatin(string text) => text.Any(c => c is >= 'A' and <= 'Z' or >= 'a' and <= 'z');

        private static FakerContext CreateContext(ulong itemIndex = 0)
        {
            var factory = new ArabFaker.NET.Generation.Randomization.RandomStreamFactory(2026);

            return new FakerContext(new GenerationContext(factory, "reference-tests", itemIndex));
        }

        private static IEnumerable<MethodInfo> TryMethods() =>
            typeof(ReferenceGenerator).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.Name.StartsWith("Try", StringComparison.Ordinal));

        private static bool IsLanguageMethod(MethodInfo method)
        {
            var parameters = method.GetParameters();

            return parameters.Length == 2
                && parameters[0].ParameterType == typeof(string).MakeByRefType()
                && parameters[1].ParameterType == typeof(NameLanguage);
        }

        private static bool IsNeutralMethod(MethodInfo method)
        {
            var parameters = method.GetParameters();

            return parameters.Length == 1 && parameters[0].ParameterType == typeof(string).MakeByRefType();
        }

        // Every "TryX(out string value, NameLanguage language)" method, found automatically.
        public static TheoryData<string> LanguageMethods
        {
            get
            {
                var data = new TheoryData<string>();

                foreach (var method in TryMethods().Where(IsLanguageMethod))
                    data.Add(method.Name);

                return data;
            }
        }

        // Every "TryX(out string value)" method (blood types, currency codes...), found automatically.
        public static TheoryData<string> NeutralMethods
        {
            get
            {
                var data = new TheoryData<string>();

                foreach (var method in TryMethods().Where(IsNeutralMethod))
                    data.Add(method.Name);

                return data;
            }
        }

        private static bool Call(string method, ReferenceGenerator reference, NameLanguage language, out string value)
        {
            object?[] args = [null, language];

            bool success = (bool)typeof(ReferenceGenerator).GetMethod(method)!.Invoke(reference, args)!;

            value = (string)args[0]!;

            return success;
        }

        private static bool CallNeutral(string method, ReferenceGenerator reference, out string value)
        {
            object?[] args = [null];

            bool success = (bool)typeof(ReferenceGenerator).GetMethod(method)!.Invoke(reference, args)!;

            value = (string)args[0]!;

            return success;
        }

        [Fact]
        public void Generator_ShouldExposeManyMethods()
        {
            Assert.True(LanguageMethods.Count() >= 40);
            Assert.True(NeutralMethods.Count() >= 5);
        }

        [Theory]
        [MemberData(nameof(LanguageMethods))]
        public void LanguageMethod_ShouldReturnArabicOrLatinText(string method)
        {
            var reference = CreateContext().Reference;

            for (int i = 0; i < 30; i++)
            {
                Assert.True(Call(method, reference, NameLanguage.Arabic, out string arabic));
                Assert.True(HasArabic(arabic) && !HasLatin(arabic), $"{method}: '{arabic}'");

                Assert.True(Call(method, reference, NameLanguage.Latin, out string latin));
                Assert.True(HasLatin(latin) && !HasArabic(latin), $"{method}: '{latin}'");
            }
        }

        [Theory]
        [MemberData(nameof(LanguageMethods))]
        public void LanguageMethod_InvalidLanguage_ShouldReturnFalseAndEmptyValue(string method)
        {
            var reference = CreateContext().Reference;

            Assert.False(Call(method, reference, (NameLanguage)99, out string value));
            Assert.Empty(value);
        }

        [Theory]
        [MemberData(nameof(NeutralMethods))]
        public void NeutralMethod_ShouldReturnANonEmptyValue(string method)
        {
            var reference = CreateContext().Reference;

            for (int i = 0; i < 30; i++)
            {
                Assert.True(CallNeutral(method, reference, out string value));
                Assert.False(string.IsNullOrWhiteSpace(value));
            }
        }

        [Fact]
        public void Method_ShouldEventuallyReturnManyDifferentValues()
        {
            var reference = CreateContext().Reference;
            var seen = new HashSet<string>();

            for (int i = 0; i < 2000; i++)
            {
                Assert.True(reference.TryCourse(out string course));
                seen.Add(course);
            }

            Assert.True(seen.Count > 150, $"Only {seen.Count} different courses were produced");
        }

        [Fact]
        public void CodeMethods_ShouldReturnKnownCodes()
        {
            var reference = CreateContext().Reference;

            Assert.True(reference.TryBloodType(out string blood));
            Assert.Contains(blood, ReferenceCatalog.BloodTypes);

            Assert.True(reference.TryCurrencyCode(out string currency));
            Assert.Contains(currency, ReferenceCatalog.CurrencyCodes);
        }

        [Theory]
        [InlineData(NameLanguage.Arabic)]
        [InlineData(NameLanguage.Latin)]
        public void TryCourseWithDepartment_ShouldReturnACourseOfThatDepartment(NameLanguage language)
        {
            var reference = CreateContext().Reference;

            for (int i = 0; i < 300; i++)
            {
                Assert.True(reference.TryCourseWithDepartment(out string course, out string department, language));

                var group = ReferenceCatalog.AcademicDepartments.Single(x => x.Group.GetValue(language) == department);

                Assert.Contains(group.Items, x => x.GetValue(language) == course);
            }
        }

        [Theory]
        [InlineData(NameLanguage.Arabic)]
        [InlineData(NameLanguage.Latin)]
        public void TryJobTitleWithDepartment_ShouldReturnATitleOfThatDepartment(NameLanguage language)
        {
            var reference = CreateContext().Reference;

            for (int i = 0; i < 300; i++)
            {
                Assert.True(reference.TryJobTitleWithDepartment(out string title, out string department, language));

                var group = ReferenceCatalog.WorkDepartments.Single(x => x.Group.GetValue(language) == department);

                Assert.Contains(group.Items, x => x.GetValue(language) == title);
            }
        }

        [Theory]
        [InlineData("Law")]
        [InlineData("law")]
        [InlineData("  Law  ")]
        [InlineData("القانون")]
        public void TryCourseInDepartment_ShouldFindTheDepartmentByEitherSpelling(string department)
        {
            var reference = CreateContext().Reference;
            var law = ReferenceCatalog.AcademicDepartments.Single(x => x.Group.English == "Law");

            for (int i = 0; i < 100; i++)
            {
                Assert.True(reference.TryCourseInDepartment(department, out string course, NameLanguage.Latin));
                Assert.Contains(law.Items, x => x.English == course);
            }
        }

        [Theory]
        [InlineData("Accounting")]
        [InlineData("المحاسبة")]
        public void TryJobTitleInDepartment_ShouldFindTheDepartment(string department)
        {
            var reference = CreateContext().Reference;

            // "Accounting" is academic only, "Finance" is the work department.
            Assert.False(reference.TryJobTitleInDepartment(department, out string missing));
            Assert.Empty(missing);

            Assert.True(reference.TryJobTitleInDepartment("Finance", out string title, NameLanguage.Latin));

            var finance = ReferenceCatalog.WorkDepartments.Single(x => x.Group.English == "Finance");

            Assert.Contains(finance.Items, x => x.English == title);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("No Such Department")]
        public void TryCourseInDepartment_UnknownDepartment_ShouldReturnFalse(string? department)
        {
            var reference = CreateContext().Reference;

            Assert.False(reference.TryCourseInDepartment(department!, out string course));
            Assert.Empty(course);
        }

        [Fact]
        public void Reference_ShouldBeDeterministicForTheSameSeedAndIndex()
        {
            var first = CreateContext(5).Reference;
            var second = CreateContext(5).Reference;

            for (int i = 0; i < 50; i++)
            {
                first.TryCourse(out string a);
                second.TryCourse(out string b);

                Assert.Equal(a, b);
            }
        }

        [Fact]
        public void Reference_ShouldNotChangeTheValuesOfOtherProviders()
        {
            // Reference uses its own random stream, so using it must not shift the names.
            var withReference = CreateContext(3);
            var without = CreateContext(3);

            for (int i = 0; i < 10; i++)
                withReference.Reference.TryCourse(out _);

            withReference.Names.TryFullName(out string a);
            without.Names.TryFullName(out string b);

            Assert.Equal(a, b);
        }
    }
}
