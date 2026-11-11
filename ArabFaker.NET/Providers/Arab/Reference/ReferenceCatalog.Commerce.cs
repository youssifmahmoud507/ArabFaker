namespace ArabFaker.NET.Providers.Arab.Reference
{
    /// <summary>
    /// Commerce reference lists: product categories, payment methods, banks and currency codes.
    /// The values are test data only; changing a list changes the output for the same seed.
    /// </summary>
    internal static partial class ReferenceCatalog
    {
        /// <summary>Product categories of a general store.</summary>
        internal static readonly ReferenceEntry[] ProductCategories =
        [
            new("إلكترونيات", "Electronics"),
            new("هواتف محمولة", "Mobile Phones"),
            new("أجهزة كمبيوتر", "Computers"),
            new("أجهزة منزلية", "Home Appliances"),
            new("أثاث", "Furniture"),
            new("ملابس رجالية", "Men's Clothing"),
            new("ملابس نسائية", "Women's Clothing"),
            new("ملابس أطفال", "Children's Clothing"),
            new("أحذية", "Shoes"),
            new("حقائب", "Bags"),
            new("ساعات", "Watches"),
            new("مجوهرات", "Jewelry"),
            new("مستحضرات تجميل", "Cosmetics"),
            new("عطور", "Perfumes"),
            new("العناية الشخصية", "Personal Care"),
            new("مواد غذائية", "Groceries"),
            new("مشروبات", "Beverages"),
            new("خضروات وفاكهة", "Fruits and Vegetables"),
            new("لحوم ودواجن", "Meat and Poultry"),
            new("مخبوزات وحلويات", "Bakery and Sweets"),
            new("ألعاب أطفال", "Toys"),
            new("كتب", "Books"),
            new("قرطاسية", "Stationery"),
            new("معدات رياضية", "Sports Equipment"),
            new("أدوات مطبخ", "Kitchenware"),
            new("أدوات منزلية", "Household Items"),
            new("مستلزمات حيوانات أليفة", "Pet Supplies"),
            new("قطع غيار سيارات", "Auto Parts"),
            new("مواد بناء", "Building Materials"),
            new("عدد وأدوات", "Tools and Hardware"),
            new("مستلزمات طبية", "Medical Supplies"),
            new("نباتات وحدائق", "Garden and Plants"),
            new("ألعاب فيديو", "Video Games"),
            new("كاميرات", "Cameras"),
            new("أجهزة لوحية", "Tablets"),
            new("سماعات وإكسسوارات", "Audio and Accessories"),
            new("شاشات وتلفزيونات", "TVs and Displays"),
            new("مستلزمات المكاتب", "Office Supplies"),
            new("أجهزة تكييف", "Air Conditioners"),
            new("مفروشات", "Home Textiles"),
            new("إضاءة", "Lighting"),
            new("حقائب سفر", "Luggage"),
            new("نظارات", "Eyewear"),
            new("مستلزمات الأطفال والرضع", "Baby Products"),
            new("أجهزة رياضية", "Fitness Equipment"),
            new("دراجات", "Bicycles"),
            new("زيوت وإطارات السيارات", "Car Oils and Tires"),
            new("أدوات الرسم والفنون", "Art Supplies"),
            new("مكملات غذائية", "Dietary Supplements"),
            new("بقالة جافة", "Dry Goods"),
            new("منتجات الألبان", "Dairy Products"),
            new("مأكولات بحرية", "Seafood"),
            new("قهوة وشاي", "Coffee and Tea"),
            new("مستلزمات الحفلات", "Party Supplies"),
        ];

        /// <summary>Payment methods used in Egypt.</summary>
        internal static readonly ReferenceEntry[] PaymentMethods =
        [
            new("الدفع عند الاستلام", "Cash on Delivery"),
            new("نقدًا", "Cash"),
            new("بطاقة ائتمان", "Credit Card"),
            new("بطاقة خصم", "Debit Card"),
            new("تحويل بنكي", "Bank Transfer"),
            new("فوري", "Fawry"),
            new("إنستاباي", "InstaPay"),
            new("محفظة إلكترونية", "Mobile Wallet"),
            new("شيك", "Cheque"),
            new("تقسيط", "Installments"),
            new("بطاقة هدايا", "Gift Card"),
            new("تحويل عبر الهاتف المحمول", "Mobile Transfer"),
            new("دفع آجل", "Pay Later"),
            new("خصم من الراتب", "Salary Deduction"),
        ];

        /// <summary>Banks operating in Egypt.</summary>
        internal static readonly ReferenceEntry[] Banks =
        [
            new("البنك الأهلي المصري", "National Bank of Egypt"),
            new("بنك مصر", "Banque Misr"),
            new("بنك القاهرة", "Banque du Caire"),
            new("البنك التجاري الدولي", "Commercial International Bank"),
            new("بنك الإسكندرية", "Bank of Alexandria"),
            new("بنك قطر الوطني مصر", "QNB Egypt"),
            new("بنك فيصل الإسلامي المصري", "Faisal Islamic Bank of Egypt"),
            new("بنك التعمير والإسكان", "Housing and Development Bank"),
            new("البنك العربي الأفريقي الدولي", "Arab African International Bank"),
            new("بنك أبوظبي الأول مصر", "First Abu Dhabi Bank Egypt"),
            new("بنك الإمارات دبي الوطني مصر", "Emirates NBD Egypt"),
            new("بنك المشرق", "Mashreq Bank"),
            new("إتش إس بي سي مصر", "HSBC Egypt"),
            new("كريدي أجريكول مصر", "Crédit Agricole Egypt"),
            new("المصرف المتحد", "United Bank"),
            new("بنك الكويت الوطني مصر", "National Bank of Kuwait Egypt"),
            new("البنك الزراعي المصري", "Agricultural Bank of Egypt"),
            new("بنك قناة السويس", "Suez Canal Bank"),
            new("بنك البركة مصر", "Al Baraka Bank Egypt"),
            new("مصرف أبوظبي الإسلامي مصر", "Abu Dhabi Islamic Bank Egypt"),
            new("البنك العقاري المصري العربي", "Egyptian Arab Land Bank"),
            new("بنك الاستثمار العربي", "Arab Investment Bank"),
            new("بنك المؤسسة العربية المصرفية مصر", "Arab Banking Corporation Egypt"),
            new("البنك العربي الدولي", "Arab International Bank"),
            new("التجاري وفا بنك مصر", "Attijariwafa Bank Egypt"),
            new("البنك المصري الخليجي", "Egyptian Gulf Bank"),
            new("البنك المصري لتنمية الصادرات", "Export Development Bank of Egypt"),
        ];

        /// <summary>ISO 4217 currency codes.</summary>
        internal static readonly string[] CurrencyCodes = ["EGP", "USD", "EUR", "GBP", "SAR", "AED", "KWD", "QAR", "BHD", "OMR", "JOD", "LYD", "TND", "MAD", "CHF", "JPY", "CNY", "CAD", "AUD", "TRY", "DZD", "IQD", "LBP", "SYP", "YER", "SDG", "MRU", "SOS", "DJF", "KMF", "INR", "PKR", "BDT", "IDR", "MYR", "SGD", "HKD", "KRW", "THB", "VND", "PHP", "RUB", "UAH", "PLN", "CZK", "HUF", "RON", "SEK", "NOK", "DKK", "ISK", "NZD", "ZAR", "NGN", "KES", "ETB", "GHS", "TZS", "UGX", "BRL", "ARS", "MXN", "CLP", "COP", "PEN"];
    }
}
