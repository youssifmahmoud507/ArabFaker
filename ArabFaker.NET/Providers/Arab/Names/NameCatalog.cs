namespace ArabFaker.NET.Providers.Arab.Names
{
    /// <summary>
    /// Built-in name lists. Every entry has an Arabic spelling and a Latin (English letters)
    /// transliteration, so the same name can be produced in either language.
    /// </summary>
    /// <remarks>
    /// Keep Arabic and Latin spellings unique within a list. Adding or removing entries changes the
    /// names produced for a given seed, so treat changes as behaviour changes.
    /// </remarks>
    internal static class NameCatalog
    {
        internal static readonly NameEntry[] MaleFirstNames = [
            new("محمد", "Mohamed"), new("أحمد", "Ahmed"), new("محمود", "Mahmoud"), new("يوسف", "Youssef"),
            new("عمر", "Omar"), new("علي", "Ali"), new("إبراهيم", "Ibrahim"), new("خالد", "Khaled"),
            new("حسن", "Hassan"), new("حسين", "Hussein"), new("كريم", "Karim"), new("مصطفى", "Mostafa"),
            new("عمرو", "Amr"), new("طارق", "Tarek"), new("ياسين", "Yassin"), new("عبدالله", "Abdullah"),
            new("عبدالرحمن", "Abdelrahman"), new("عبدالعزيز", "Abdelaziz"), new("عبدالفتاح", "Abdelfattah"), new("عبدالمنعم", "Abdelmoneim"),
            new("عبدالحميد", "Abdelhamid"), new("عبدالرحيم", "Abdelrahim"), new("عبدالكريم", "Abdelkarim"), new("عبدالسلام", "Abdelsalam"),
            new("سيف", "Seif"), new("زياد", "Ziad"), new("آدم", "Adam"), new("مالك", "Malek"),
            new("حمزة", "Hamza"), new("بلال", "Belal"), new("أنس", "Anas"), new("أسامة", "Osama"),
            new("إسلام", "Islam"), new("إيهاب", "Ehab"), new("أيمن", "Ayman"), new("أشرف", "Ashraf"),
            new("أمير", "Amir"), new("أمين", "Amin"), new("باسم", "Basem"), new("باسل", "Basel"),
            new("بهاء", "Bahaa"), new("تامر", "Tamer"), new("ثروت", "Tharwat"), new("جمال", "Gamal"),
            new("جاد", "Gad"), new("جلال", "Galal"), new("حازم", "Hazem"), new("حاتم", "Hatem"),
            new("حامد", "Hamed"), new("حمدي", "Hamdy"), new("حمادة", "Hamada"), new("رامي", "Ramy"),
            new("رأفت", "Raafat"), new("رضا", "Reda"), new("رمضان", "Ramadan"), new("رؤوف", "Raouf"),
            new("زكريا", "Zakaria"), new("زين", "Zein"), new("سامح", "Sameh"), new("سامي", "Sami"),
            new("سعيد", "Said"), new("سعد", "Saad"), new("سلطان", "Sultan"), new("سليم", "Salim"),
            new("سليمان", "Soliman"), new("شادي", "Shady"), new("شريف", "Sherif"), new("شهاب", "Shehab"),
            new("صلاح", "Salah"), new("صبري", "Sabry"), new("طلعت", "Talaat"), new("طه", "Taha"),
            new("عادل", "Adel"), new("عاصم", "Assem"), new("عاطف", "Atef"), new("عثمان", "Osman"),
            new("عصام", "Essam"), new("علاء", "Alaa"), new("عماد", "Emad"), new("عمار", "Ammar"),
            new("عوض", "Awad"), new("غسان", "Ghassan"), new("فادي", "Fady"), new("فارس", "Fares"),
            new("فتحي", "Fathy"), new("فريد", "Farid"), new("فؤاد", "Fouad"), new("فيصل", "Faisal"),
            new("قاسم", "Kassem"), new("كمال", "Kamal"), new("لؤي", "Louay"), new("ماجد", "Maged"),
            new("مازن", "Mazen"), new("ماهر", "Maher"), new("مجدي", "Magdy"), new("محسن", "Mohsen"),
            new("مدحت", "Medhat"), new("مراد", "Mourad"), new("مروان", "Marwan"), new("مسعود", "Masoud"),
            new("معاذ", "Moaz"), new("منير", "Mounir"), new("مهند", "Mohanad"), new("مؤمن", "Moemen"),
            new("نادر", "Nader"), new("ناصر", "Nasser"), new("نبيل", "Nabil"), new("نصر", "Nasr"),
            new("نعمان", "Noaman"), new("هاني", "Hany"), new("هشام", "Hesham"), new("هيثم", "Haitham"),
            new("وائل", "Wael"), new("وليد", "Walid"), new("وسام", "Wesam"), new("ياسر", "Yasser"),
            new("يحيى", "Yehia"), new("يونس", "Younes"), new("يزيد", "Yazid"), new("أدهم", "Adham"),
            new("إياد", "Eyad"), new("إلياس", "Elias"), new("أيوب", "Ayoub")
        ];

        internal static readonly NameEntry[] FemaleFirstNames = [
            new("مريم", "Mariam"), new("فاطمة", "Fatma"), new("سارة", "Sara"), new("نور", "Nour"),
            new("ملك", "Malak"), new("آية", "Aya"), new("سلمى", "Salma"), new("فرح", "Farah"),
            new("ياسمين", "Yasmin"), new("ندى", "Nada"), new("حبيبة", "Habiba"), new("منة", "Menna"),
            new("رنا", "Rana"), new("ليلى", "Laila"), new("جنى", "Jana"), new("أسماء", "Asmaa"),
            new("أمل", "Amal"), new("أميرة", "Amira"), new("إيمان", "Eman"), new("إيناس", "Enas"),
            new("ابتسام", "Ebtesam"), new("أروى", "Arwa"), new("أسيل", "Aseel"), new("بسمة", "Basma"),
            new("بسنت", "Bassant"), new("بشرى", "Bushra"), new("بتول", "Batoul"), new("تسنيم", "Tasneem"),
            new("تقى", "Toka"), new("جميلة", "Gamila"), new("جيهان", "Gehan"), new("جهاد", "Gihad"),
            new("حنان", "Hanan"), new("حنين", "Haneen"), new("هالة", "Hala"), new("خديجة", "Khadija"),
            new("خلود", "Kholoud"), new("دعاء", "Doaa"), new("دينا", "Dina"), new("دانية", "Dania"),
            new("ديما", "Dima"), new("رحمة", "Rahma"), new("رشا", "Rasha"), new("رضوى", "Radwa"),
            new("ريم", "Reem"), new("ريهام", "Reham"), new("ريتاج", "Retaj"), new("روان", "Rawan"),
            new("روضة", "Rawda"), new("رنيم", "Raneem"), new("زينب", "Zeinab"), new("زهراء", "Zahraa"),
            new("زينة", "Zeina"), new("سمر", "Samar"), new("سمية", "Sumaya"), new("سهى", "Soha"),
            new("سناء", "Sanaa"), new("سهير", "Sohair"), new("سلوى", "Salwa"), new("سوسن", "Sawsan"),
            new("شيماء", "Shaimaa"), new("شروق", "Shorouk"), new("شهد", "Shahd"), new("شيرين", "Sherine"),
            new("صفاء", "Safaa"), new("صباح", "Sabah"), new("ضحى", "Doha"), new("عائشة", "Aisha"),
            new("عبير", "Abeer"), new("عزة", "Ezza"), new("علا", "Ola"), new("علياء", "Alyaa"),
            new("غادة", "Ghada"), new("غدير", "Ghadeer"), new("فاتن", "Faten"), new("فريدة", "Farida"),
            new("فيروز", "Fayrouz"), new("لمياء", "Lamiaa"), new("لينا", "Lina"), new("لجين", "Lujain"),
            new("لبنى", "Lobna"), new("مروة", "Marwa"), new("ميار", "Mayar"), new("مي", "May"),
            new("ميرفت", "Mervat"), new("منى", "Mona"), new("منال", "Manal"), new("مها", "Maha"),
            new("ميادة", "Mayada"), new("ماجدة", "Magda"), new("نهى", "Noha"), new("نجلاء", "Naglaa"),
            new("نادية", "Nadia"), new("ناهد", "Nahed"), new("نرمين", "Nermeen"), new("نسمة", "Nesma"),
            new("نشوى", "Nashwa"), new("نيرة", "Nayra"), new("هبة", "Heba"), new("هدى", "Hoda"),
            new("هنا", "Hana"), new("هند", "Hend"), new("هاجر", "Hagar"), new("هيام", "Hiam"),
            new("وفاء", "Wafaa"), new("ولاء", "Walaa"), new("يارا", "Yara"), new("يمنى", "Yomna"),
            new("رزان", "Razan"), new("ريما", "Rima"), new("جودي", "Judy"), new("جوري", "Jory"),
            new("تالا", "Tala"), new("لارا", "Lara"), new("ليان", "Lian"), new("ميرال", "Miral"),
            new("سلسبيل", "Salsabil"), new("أنجي", "Engy"), new("رغد", "Raghad"), new("عفاف", "Afaf"),
            new("نوران", "Nouran"), new("نانسي", "Nancy")
        ];

        internal static readonly NameEntry[] FamilyNames = [
            new("محمد", "Mohamed"), new("أحمد", "Ahmed"), new("محمود", "Mahmoud"), new("إبراهيم", "Ibrahim"),
            new("علي", "Ali"), new("حسن", "Hassan"), new("حسين", "Hussein"), new("السيد", "Elsayed"),
            new("عبدالله", "Abdullah"), new("سالم", "Salem"), new("مصطفى", "Mostafa"), new("منصور", "Mansour"),
            new("عمر", "Omar"), new("كمال", "Kamal"), new("عبدالرحمن", "Abdelrahman"), new("عبدالعزيز", "Abdelaziz"),
            new("عبدالفتاح", "Abdelfattah"), new("عبدالحميد", "Abdelhamid"), new("عبدالمنعم", "Abdelmoneim"), new("عبدالرازق", "Abdelrazek"),
            new("عبدالمجيد", "Abdelmagid"), new("عبدالجواد", "Abdelgawad"), new("عبدالنبي", "Abdelnaby"), new("عبدالهادي", "Abdelhady"),
            new("عبدالقادر", "Abdelkader"), new("عبدالسلام", "Abdelsalam"), new("إسماعيل", "Ismail"), new("أمين", "Amin"),
            new("أبوزيد", "Abouzeid"), new("أبوالعلا", "Abouelela"), new("بدوي", "Badawy"), new("بكر", "Bakr"),
            new("توفيق", "Tawfik"), new("تهامي", "Tohamy"), new("جابر", "Gaber"), new("جاد", "Gad"),
            new("جمعة", "Gomaa"), new("جمال", "Gamal"), new("حافظ", "Hafez"), new("حجازي", "Hegazy"),
            new("حمدان", "Hamdan"), new("حمزة", "Hamza"), new("حميدة", "Hamida"), new("خليل", "Khalil"),
            new("خليفة", "Khalifa"), new("درويش", "Darwish"), new("دياب", "Diab"), new("ذكي", "Zaki"),
            new("رجب", "Ragab"), new("رزق", "Rizk"), new("رشاد", "Rashad"), new("رشدي", "Roushdy"),
            new("رضوان", "Radwan"), new("زايد", "Zayed"), new("زيدان", "Zidan"), new("سعد", "Saad"),
            new("سعيد", "Said"), new("سلامة", "Salama"), new("سلطان", "Sultan"), new("سليمان", "Soliman"),
            new("شاهين", "Shaheen"), new("شحاتة", "Shehata"), new("شريف", "Sherif"), new("شعبان", "Shaaban"),
            new("صابر", "Saber"), new("صالح", "Saleh"), new("صبري", "Sabry"), new("صقر", "Saqr"),
            new("طاهر", "Taher"), new("طه", "Taha"), new("عامر", "Amer"), new("عباس", "Abbas"),
            new("عثمان", "Osman"), new("عطية", "Attia"), new("عطا", "Atta"), new("عفيفي", "Afifi"),
            new("عوض", "Awad"), new("عيد", "Eid"), new("غانم", "Ghanem"), new("غنيم", "Ghoneim"),
            new("فؤاد", "Fouad"), new("فاروق", "Farouk"), new("فرج", "Farag"), new("فهمي", "Fahmy"),
            new("قاسم", "Kassem"), new("قطب", "Qutb"), new("كامل", "Kamel"), new("لطفي", "Lotfy"),
            new("ماهر", "Maher"), new("متولي", "Metwally"), new("مجاهد", "Mogahed"), new("مرسي", "Morsy"),
            new("مرزوق", "Marzouk"), new("مسعود", "Masoud"), new("مصباح", "Mesbah"), new("معوض", "Moawad"),
            new("مغاوري", "Meghawry"), new("مهران", "Mehran"), new("موسى", "Mousa"), new("ناصر", "Nasser"),
            new("نجم", "Negm"), new("نصر", "Nasr"), new("نور", "Nour"), new("هاشم", "Hashem"),
            new("هلال", "Helal"), new("هندي", "Henedy"), new("همام", "Hammam"), new("وهبة", "Wahba"),
            new("ياسين", "Yassin"), new("يوسف", "Youssef"), new("يونس", "Younes"), new("الجمال", "Elgamal"),
            new("الباز", "Elbaz"), new("الدسوقي", "Eldessouky"), new("الشناوي", "Elshennawy"), new("النجار", "Elnaggar"),
            new("الحداد", "Elhaddad"), new("البنا", "Elbanna"), new("الصاوي", "Elsawy"), new("القاضي", "Elkady"),
            new("المصري", "Elmasry"), new("الطحان", "Eltahan"), new("الخولي", "Elkholy"), new("الشافعي", "Elshafei"),
            new("الفقي", "Elfeky"), new("العدوي", "Eladawy"), new("الزيات", "Elzayat"), new("الهواري", "Elhawary"),
            new("العطار", "Elattar"), new("الجندي", "Elgendy"), new("عزت", "Ezzat"), new("عزام", "Azzam"),
            new("عاشور", "Ashour"), new("عسكر", "Askar"), new("فوزي", "Fawzy"), new("قنديل", "Kandil"),
            new("كساب", "Kassab"), new("نبيه", "Nabih"), new("وجدي", "Wagdy")
        ];

        internal static readonly NameEntry[] AllFirstNames = [.. MaleFirstNames, .. FemaleFirstNames];
    }
}
