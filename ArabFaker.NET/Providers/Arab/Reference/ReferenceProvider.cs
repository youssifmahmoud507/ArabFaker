using ArabFaker.NET.Generation.Randomization;
using ArabFaker.NET.Providers.Arab.Names;

namespace ArabFaker.NET.Providers.Arab.Reference
{
    internal sealed class ReferenceProvider
    {
        private static readonly ReferenceEntry[] AllCourses = Flatten(ReferenceCatalog.AcademicDepartments);

        private static readonly ReferenceEntry[] AllJobTitles = Flatten(ReferenceCatalog.WorkDepartments);

        private readonly RandomGenerator _random;

        internal ReferenceProvider(RandomGenerator random)
        {
            _random = random;
        }

        internal bool TryEntry(ReferenceEntry[] entries, NameLanguage language, out string value)
        {
            value = string.Empty;

            if (!Enum.IsDefined(language) || !_random.TryPick(entries, out ReferenceEntry entry))
                return false;

            value = entry.GetValue(language);

            return true;
        }

        internal bool TryCode(string[] codes, out string value)
        {
            value = string.Empty;

            if (!_random.TryPick(codes, out string code))
                return false;

            value = code;

            return true;
        }

        internal bool TryAcademicDepartment(NameLanguage language, out string value)
        {
            return TryGroupName(ReferenceCatalog.AcademicDepartments, language, out value);
        }

        internal bool TryWorkDepartment(NameLanguage language, out string value)
        {
            return TryGroupName(ReferenceCatalog.WorkDepartments, language, out value);
        }

        internal bool TryCourse(NameLanguage language, out string value)
        {
            return TryEntry(AllCourses, language, out value);
        }

        internal bool TryJobTitle(NameLanguage language, out string value)
        {
            return TryEntry(AllJobTitles, language, out value);
        }

        internal bool TryCourseWithDepartment(NameLanguage language, out string course, out string department)
        {
            return TryPair(ReferenceCatalog.AcademicDepartments, language, out course, out department);
        }

        internal bool TryJobTitleWithDepartment(NameLanguage language, out string jobTitle, out string department)
        {
            return TryPair(ReferenceCatalog.WorkDepartments, language, out jobTitle, out department);
        }

        internal bool TryCourseInDepartment(string department, NameLanguage language, out string course)
        {
            return TryInGroup(ReferenceCatalog.AcademicDepartments, department, language, out course);
        }

        internal bool TryJobTitleInDepartment(string department, NameLanguage language, out string jobTitle)
        {
            return TryInGroup(ReferenceCatalog.WorkDepartments, department, language, out jobTitle);
        }

        private bool TryGroupName(ReferenceGroup[] groups, NameLanguage language, out string value)
        {
            value = string.Empty;

            if (!Enum.IsDefined(language) || !_random.TryPick(groups, out ReferenceGroup group))
                return false;

            value = group.Group.GetValue(language);

            return true;
        }

        private bool TryPair(ReferenceGroup[] groups, NameLanguage language, out string item, out string group)
        {
            item = string.Empty;
            group = string.Empty;

            if (!Enum.IsDefined(language) || !_random.TryPick(groups, out ReferenceGroup picked))
                return false;

            if (!_random.TryPick(picked.Items, out ReferenceEntry entry))
                return false;

            item = entry.GetValue(language);
            group = picked.Group.GetValue(language);

            return true;
        }

        private bool TryInGroup(ReferenceGroup[] groups, string groupName, NameLanguage language, out string item)
        {
            item = string.Empty;

            if (!Enum.IsDefined(language) || string.IsNullOrWhiteSpace(groupName))
                return false;

            string wanted = groupName.Trim();

            foreach (ReferenceGroup group in groups)
            {
                if (!group.Group.Matches(wanted))
                    continue;

                if (!_random.TryPick(group.Items, out ReferenceEntry entry))
                    return false;

                item = entry.GetValue(language);

                return true;
            }

            return false;
        }

        private static ReferenceEntry[] Flatten(ReferenceGroup[] groups)
        {
            var items = new List<ReferenceEntry>();

            foreach (ReferenceGroup group in groups)
            {
                items.AddRange(group.Items);
            }

            return items.ToArray();
        }
    }
}
