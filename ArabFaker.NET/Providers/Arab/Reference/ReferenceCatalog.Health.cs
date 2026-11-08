namespace ArabFaker.NET.Providers.Arab.Reference
{
    internal static partial class ReferenceCatalog
    {
        /// <summary>Medical specialties.</summary>
        internal static readonly ReferenceEntry[] MedicalSpecialties =
        [
            new("الباطنة", "Internal Medicine"),
            new("الجراحة العامة", "General Surgery"),
            new("طب الأطفال", "Pediatrics"),
            new("النساء والتوليد", "Obstetrics and Gynecology"),
            new("القلب والأوعية الدموية", "Cardiology"),
            new("العظام", "Orthopedics"),
            new("المخ والأعصاب", "Neurology"),
            new("جراحة المخ والأعصاب", "Neurosurgery"),
            new("الجلدية", "Dermatology"),
            new("العيون", "Ophthalmology"),
            new("الأنف والأذن والحنجرة", "Otolaryngology"),
            new("المسالك البولية", "Urology"),
            new("الأمراض الصدرية", "Pulmonology"),
            new("الجهاز الهضمي والكبد", "Gastroenterology and Hepatology"),
            new("الغدد الصماء والسكر", "Endocrinology and Diabetes"),
            new("الأمراض النفسية", "Psychiatry"),
            new("الأورام", "Oncology"),
            new("الكلى", "Nephrology"),
            new("الروماتيزم", "Rheumatology"),
            new("التخدير", "Anesthesiology"),
            new("الأشعة", "Radiology"),
            new("التحاليل الطبية", "Clinical Pathology"),
            new("الطب الطبيعي والتأهيل", "Physical Medicine and Rehabilitation"),
            new("جراحة التجميل", "Plastic Surgery"),
            new("الأمراض المعدية", "Infectious Diseases"),
            new("أمراض الدم", "Hematology"),
            new("طب الطوارئ", "Emergency Medicine"),
            new("طب الأسنان", "Dentistry"),
            new("جراحة الأوعية الدموية", "Vascular Surgery"),
            new("طب الأسرة", "Family Medicine"),
            new("التغذية العلاجية", "Clinical Nutrition"),
            new("حديثو الولادة", "Neonatology"),
        ];

        /// <summary>ABO and Rh blood groups.</summary>
        internal static readonly string[] BloodTypes = ["A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-"];
    }
}
