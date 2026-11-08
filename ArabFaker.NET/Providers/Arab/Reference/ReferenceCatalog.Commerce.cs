namespace ArabFaker.NET.Providers.Arab.Reference
{
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
        ];

        /// <summary>ISO 4217 currency codes.</summary>
        internal static readonly string[] CurrencyCodes = ["EGP", "USD", "EUR", "GBP", "SAR", "AED", "KWD", "QAR", "BHD", "OMR", "JOD", "LYD", "TND", "MAD", "CHF", "JPY", "CNY", "CAD", "AUD", "TRY"];
    }
}
