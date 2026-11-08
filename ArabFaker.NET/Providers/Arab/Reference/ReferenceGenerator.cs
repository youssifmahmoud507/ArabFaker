using ArabFaker.NET.Providers.Arab.Names;

namespace ArabFaker.NET.Providers.Arab.Reference
{
    /// <summary>
    /// Provides values from closed, well-known lists: courses, departments, job titles,
    /// universities, banks, medical specialties and similar.
    /// </summary>
    /// <remarks>
    /// Names are available in Arabic or Latin (English) letters, like <see cref="NameGenerator"/>.
    /// The lists are built in, so results do not depend on anything outside the library.
    /// Changing a list changes the values produced for a given seed.
    /// </remarks>
    public sealed class ReferenceGenerator
    {
        private readonly ReferenceProvider _provider;

        internal ReferenceGenerator(ReferenceProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Generates a faculty found in Egyptian universities, such as Faculty of Engineering.
        /// </summary>
        /// <param name="language">The script of the result. Defaults to Arabic.</param>
        public bool TryFaculty(out string value, NameLanguage language = NameLanguage.Arabic)
        {
            return _provider.TryEntry(ReferenceCatalog.Faculties, language, out value);
        }

        /// <summary>
        /// Generates an Egyptian university.
        /// </summary>
        /// <param name="language">The script of the result. Defaults to Arabic.</param>
        public bool TryUniversity(out string value, NameLanguage language = NameLanguage.Arabic)
        {
            return _provider.TryEntry(ReferenceCatalog.Universities, language, out value);
        }

        /// <summary>
        /// Generates an education level, such as Secondary or Bachelor's Degree.
        /// </summary>
        /// <param name="language">The script of the result. Defaults to Arabic.</param>
        public bool TryEducationLevel(out string value, NameLanguage language = NameLanguage.Arabic)
        {
            return _provider.TryEntry(ReferenceCatalog.EducationLevels, language, out value);
        }

        /// <summary>
        /// Generates a year of study, such as First Year.
        /// </summary>
        /// <param name="language">The script of the result. Defaults to Arabic.</param>
        public bool TryAcademicYear(out string value, NameLanguage language = NameLanguage.Arabic)
        {
            return _provider.TryEntry(ReferenceCatalog.AcademicYears, language, out value);
        }

        /// <summary>
        /// Generates an industry a company can work in.
        /// </summary>
        /// <param name="language">The script of the result. Defaults to Arabic.</param>
        public bool TryIndustry(out string value, NameLanguage language = NameLanguage.Arabic)
        {
            return _provider.TryEntry(ReferenceCatalog.Industries, language, out value);
        }

        /// <summary>
        /// Generates a product category of a general store, such as Electronics.
        /// </summary>
        /// <param name="language">The script of the result. Defaults to Arabic.</param>
        public bool TryProductCategory(out string value, NameLanguage language = NameLanguage.Arabic)
        {
            return _provider.TryEntry(ReferenceCatalog.ProductCategories, language, out value);
        }

        /// <summary>
        /// Generates a payment method used in Egypt, such as Cash on Delivery.
        /// </summary>
        /// <param name="language">The script of the result. Defaults to Arabic.</param>
        public bool TryPaymentMethod(out string value, NameLanguage language = NameLanguage.Arabic)
        {
            return _provider.TryEntry(ReferenceCatalog.PaymentMethods, language, out value);
        }

        /// <summary>
        /// Generates a bank operating in Egypt.
        /// </summary>
        /// <param name="language">The script of the result. Defaults to Arabic.</param>
        public bool TryBank(out string value, NameLanguage language = NameLanguage.Arabic)
        {
            return _provider.TryEntry(ReferenceCatalog.Banks, language, out value);
        }

        /// <summary>
        /// Generates a medical specialty, such as Cardiology.
        /// </summary>
        /// <param name="language">The script of the result. Defaults to Arabic.</param>
        public bool TryMedicalSpecialty(out string value, NameLanguage language = NameLanguage.Arabic)
        {
            return _provider.TryEntry(ReferenceCatalog.MedicalSpecialties, language, out value);
        }

        /// <summary>
        /// Generates a marital status.
        /// </summary>
        /// <param name="language">The script of the result. Defaults to Arabic.</param>
        public bool TryMaritalStatus(out string value, NameLanguage language = NameLanguage.Arabic)
        {
            return _provider.TryEntry(ReferenceCatalog.MaritalStatuses, language, out value);
        }

        /// <summary>
        /// Generates a country.
        /// </summary>
        /// <param name="language">The script of the result. Defaults to Arabic.</param>
        public bool TryCountry(out string value, NameLanguage language = NameLanguage.Arabic)
        {
            return _provider.TryEntry(ReferenceCatalog.Countries, language, out value);
        }

        /// <summary>
        /// Generates a spoken language.
        /// </summary>
        /// <param name="language">The script of the result. Defaults to Arabic.</param>
        public bool TryLanguage(out string value, NameLanguage language = NameLanguage.Arabic)
        {
            return _provider.TryEntry(ReferenceCatalog.Languages, language, out value);
        }

        /// <summary>
        /// Generates a color name.
        /// </summary>
        /// <param name="language">The script of the result. Defaults to Arabic.</param>
        public bool TryColor(out string value, NameLanguage language = NameLanguage.Arabic)
        {
            return _provider.TryEntry(ReferenceCatalog.Colors, language, out value);
        }

        /// <summary>
        /// Generates the name of a day of the week.
        /// </summary>
        /// <param name="language">The script of the result. Defaults to Arabic.</param>
        public bool TryDayName(out string value, NameLanguage language = NameLanguage.Arabic)
        {
            return _provider.TryEntry(ReferenceCatalog.Weekdays, language, out value);
        }

        /// <summary>
        /// Generates the name of a Gregorian month as used in Egypt.
        /// </summary>
        /// <param name="language">The script of the result. Defaults to Arabic.</param>
        public bool TryMonthName(out string value, NameLanguage language = NameLanguage.Arabic)
        {
            return _provider.TryEntry(ReferenceCatalog.Months, language, out value);
        }

        /// <summary>
        /// Generates an academic department, such as Computer Science or Law.
        /// </summary>
        /// <param name="language">The script of the result. Defaults to Arabic.</param>
        public bool TryAcademicDepartment(out string value, NameLanguage language = NameLanguage.Arabic)
        {
            return _provider.TryAcademicDepartment(language, out value);
        }

        /// <summary>
        /// Generates a university course, such as Data Structures or Civil Law.
        /// </summary>
        /// <param name="language">The script of the result. Defaults to Arabic.</param>
        public bool TryCourse(out string value, NameLanguage language = NameLanguage.Arabic)
        {
            return _provider.TryCourse(language, out value);
        }

        /// <summary>
        /// Generates a course together with the academic department that teaches it.
        /// </summary>
        /// <remarks>Use this when the course and the department must agree with each other.</remarks>
        /// <param name="course">The course.</param>
        /// <param name="department">The department the course belongs to.</param>
        /// <param name="language">The script of the results. Defaults to Arabic.</param>
        public bool TryCourseWithDepartment(out string course, out string department, NameLanguage language = NameLanguage.Arabic)
        {
            return _provider.TryCourseWithDepartment(language, out course, out department);
        }

        /// <summary>
        /// Generates a course taught by a given academic department.
        /// </summary>
        /// <param name="department">The department, spelled as this library spells it, in Arabic or English.</param>
        /// <param name="course">The course, or an empty string when the department is unknown.</param>
        /// <param name="language">The script of the result. Defaults to Arabic.</param>
        /// <returns><see langword="false"/> when the department is not in the built-in list.</returns>
        public bool TryCourseInDepartment(string department, out string course, NameLanguage language = NameLanguage.Arabic)
        {
            return _provider.TryCourseInDepartment(department, language, out course);
        }

        /// <summary>
        /// Generates a company department, such as Human Resources or Finance.
        /// </summary>
        /// <param name="language">The script of the result. Defaults to Arabic.</param>
        public bool TryDepartment(out string value, NameLanguage language = NameLanguage.Arabic)
        {
            return _provider.TryWorkDepartment(language, out value);
        }

        /// <summary>
        /// Generates a job title, such as Accountant or Software Developer.
        /// </summary>
        /// <param name="language">The script of the result. Defaults to Arabic.</param>
        public bool TryJobTitle(out string value, NameLanguage language = NameLanguage.Arabic)
        {
            return _provider.TryJobTitle(language, out value);
        }

        /// <summary>
        /// Generates a job title together with the company department it belongs to.
        /// </summary>
        /// <remarks>Use this when the job title and the department must agree with each other.</remarks>
        /// <param name="jobTitle">The job title.</param>
        /// <param name="department">The department the job title belongs to.</param>
        /// <param name="language">The script of the results. Defaults to Arabic.</param>
        public bool TryJobTitleWithDepartment(out string jobTitle, out string department, NameLanguage language = NameLanguage.Arabic)
        {
            return _provider.TryJobTitleWithDepartment(language, out jobTitle, out department);
        }

        /// <summary>
        /// Generates a job title found in a given company department.
        /// </summary>
        /// <param name="department">The department, spelled as this library spells it, in Arabic or English.</param>
        /// <param name="jobTitle">The job title, or an empty string when the department is unknown.</param>
        /// <param name="language">The script of the result. Defaults to Arabic.</param>
        /// <returns><see langword="false"/> when the department is not in the built-in list.</returns>
        public bool TryJobTitleInDepartment(string department, out string jobTitle, NameLanguage language = NameLanguage.Arabic)
        {
            return _provider.TryJobTitleInDepartment(department, language, out jobTitle);
        }

        /// <summary>
        /// Generates a blood group, such as O+ or AB-.
        /// </summary>
        public bool TryBloodType(out string value)
        {
            return _provider.TryCode(ReferenceCatalog.BloodTypes, out value);
        }

        /// <summary>
        /// Generates an ISO 4217 currency code, such as EGP or USD.
        /// </summary>
        public bool TryCurrencyCode(out string value)
        {
            return _provider.TryCode(ReferenceCatalog.CurrencyCodes, out value);
        }
    }
}
