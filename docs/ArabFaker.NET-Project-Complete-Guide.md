# ArabFaker.NET — الدليل الشامل للمشروع

> **وثيقة تعريف وتصميم وتنفيذ (Project Vision, Product Specification & Technical Architecture)**  
> آخر تجميع للمعلومات: **5 نوفمبر 2026**  
> الحالة: **المشروع قيد التطوير — أول 10 أجزاء موجودة فعليًا في الريبو (تمت مراجعتها يدويًا في 6 نوفمبر 2026). ملفات الجزء 11 (`FakerDefinition.cs`) غير موجودة في الريبو بعد، والجزء 11 هو المرحلة الحالية.**

---

## فهرس المحتويات

1. [ملخص المشروع](#1-ملخص-المشروع)
2. [المشكلة التي يحلها المشروع](#2-المشكلة-التي-يحلها-المشروع)
3. [الرؤية والأهداف](#3-الرؤية-والأهداف)
4. [نطاق المشروع وما لا يهدف إلى تقديمه](#4-نطاق-المشروع-وما-لا-يهدف-إلى-تقديمه)
5. [حالات الاستخدام](#5-حالات-الاستخدام)
6. [المستخدم المستهدف](#6-المستخدم-المستهدف)
7. [المبادئ والقرارات التصميمية](#7-المبادئ-والقرارات-التصميمية)
8. [التقنيات وبيئة التطوير](#8-التقنيات-وبيئة-التطوير)
9. [الهيكل الفعلي للـ Solution](#9-الهيكل-الفعلي-للـ-solution)
10. [الصورة المعمارية العامة](#10-الصورة-المعمارية-العامة)
11. [محرك العشوائية Randomization Engine](#11-محرك-العشوائية-randomization-engine)
12. [Seed وRandom Streams](#12-seed-وrandom-streams)
13. [Generation Context](#13-generation-context)
14. [شرح الـ Providers الحالية](#14-شرح-الـ-providers-الحالية)
15. [بيانات المواقع الجغرافية المصرية](#15-بيانات-المواقع-الجغرافية-المصرية)
16. [واجهة المستخدم العامة Public API](#16-واجهة-المستخدم-العامة-public-api)
17. [Define<T> وRuleFor](#17-definet-وrulefor)
18. [التوليد وResult Handling — التصميم المستهدف](#18-التوليد-وresult-handling--التصميم-المستهدف)
19. [Determinism وReproducibility](#19-determinism-وreproducibility)
20. [Validation وسياسة الأخطاء](#20-validation-وسياسة-الأخطاء)
21. [Thread Safety والتزامن](#21-thread-safety-والتزامن)
22. [الاختبارات وضمان الجودة](#22-الاختبارات-وضمان-الجودة)
23. [الأداء واستهلاك الذاكرة](#23-الأداء-واستهلاك-الذاكرة)
24. [الأمان والخصوصية وحدود الاستخدام](#24-الأمان-والخصوصية-وحدود-الاستخدام)
25. [الترخيص وحقوق البيانات](#25-الترخيص-وحقوق-البيانات)
26. [التوثيق وتجربة المطور](#26-التوثيق-وتجربة-المطور)
27. [خطة التنفيذ الكاملة](#27-خطة-التنفيذ-الكاملة)
28. [حالة التنفيذ الحالية](#28-حالة-التنفيذ-الحالية)
29. [قرارات مؤجلة ومخاطر يجب مراجعتها](#29-قرارات-مؤجلة-ومخاطر-يجب-مراجعتها)
30. [معايير اعتبار المشروع جاهزًا للنشر](#30-معايير-اعتبار-المشروع-جاهزًا-للنشر)
31. [قاموس المصطلحات](#31-قاموس-المصطلحات)
32. [مراجع وروابط](#32-مراجع-وروابط)
33. [تعليمات استكمال المشروع مع أي مساعد أو مساهم](#33-تعليمات-استكمال-المشروع-مع-أي-مساعد-أو-مساهم)

---

# 1. ملخص المشروع

**ArabFaker.NET** مكتبة مفتوحة المصدر مكتوبة بـ C# على منصة .NET، هدفها توليد بيانات وهمية (Fake/Synthetic Data) قابلة للاستخدام في الاختبارات والتطوير والتجارب المحلية، مع تركيز واضح على البيانات العربية والإقليمية، وبداية عملية ببيانات مصر.

الفكرة الأساسية ليست توفير مجموعة Classes جاهزة مثل `FakeStudent` أو `FakeCustomer` أو توليد سيناريوهات أعمال محددة. الفكرة أن المطور يملك أي Class في مشروعه، ويستخدم ArabFaker لتعريف كيفية إنشاء نسخة وهمية منه، وتحديد قيمة كل Property من خلال قواعد Fluent.

مثال تصوري للواجهة النهائية:

```csharp
var faker = new Faker(seed: 2026);

var studentFaker = faker.Define<Student>()
    .RuleFor(x => x.FirstName, context =>
        context.Names.TryFirstName(out var name)
            ? name
            : "Unknown")
    .RuleFor(x => x.Email, context =>
        context.Internet.TryEmail(out var email)
            ? email
            : string.Empty)
    .RuleFor(x => x.Age, context =>
        context.RandomData.TryInt(18, 40, out var age)
            ? age
            : 18);

// شكل Generate() توضيحي للواجهة المستهدفة، وليس ادعاءً بأن Part 12 اكتمل.
var result = studentFaker.Generate();
```

المكتبة توفر Providers مستقلة لتوليد القيم الشائعة، وتسمح للمستخدم بتركيب هذه القيم كما يريد داخل أي Class يملكه.

**الخلاصة:** ArabFaker.NET هي مكتبة توليد بيانات عامة وقابلة للتخصيص، وليست نظامًا جاهزًا لتوليد بيانات مجال أعمال بعينه.

# 2. المشكلة التي يحلها المشروع

أثناء تطوير تطبيقات .NET، يحتاج المطور باستمرار إلى بيانات غير حقيقية من أجل:

- اختبار الـ Business Logic دون إدخال بيانات حقيقية.
- اختبار REST APIs والـ DTOs والـ Validation.
- تجهيز بيانات أولية محلية للتجربة أو Demo.
- اختبار Pagination وFiltering وSorting مع عدد كبير من السجلات.
- اختبار الأداء والذاكرة مع أحجام بيانات مختلفة.
- إعداد Unit Tests وIntegration Tests.
- إنشاء بيانات في بيئات التطوير والـ QA.
- إعادة إنتاج Bug باستخدام نفس مجموعة البيانات في كل تشغيل.

إنشاء البيانات يدويًا مرهق، واستخدام بيانات عشوائية غير منظمة قد ينتج قيمًا غير واقعية أو غير متناسقة. كما أن البيانات الجغرافية والأسماء العربية تحتاج مصادر مناسبة للسياق العربي، ولا يكفي الاعتماد على قوائم أجنبية عامة.

ArabFaker.NET تحاول حل هذه المشكلة بتوفير:

1. **Providers جاهزة** للقيم العامة والعربية.
2. **Fluent Mapping API** يحدد به المطور طريقة ملء أي Class.
3. **Seeded Randomness** لإعادة إنتاج نفس النتائج عند الحاجة.
4. **Independent Random Streams** حتى لا يؤثر استهلاك Provider على Provider آخر.
5. **Offline operation** دون الحاجة إلى APIs خارجية أو خدمات مدفوعة وقت التشغيل.

# 3. الرؤية والأهداف

## 3.1 الرؤية

أن تصبح ArabFaker.NET مكتبة .NET موثوقة لتوليد بيانات اختبارية عامة، مع دعم عربي قوي وقابل للتوسع إلى دول عربية أخرى، وواجهة سهلة تشبه أسلوب Fluent APIs المعروف في منظومة .NET.

## 3.2 الأهداف الأساسية

- دعم أسماء عربية مع تمثيل عربي وLatin/English.
- دعم المواقع الإدارية العربية، مع البدء بمصر.
- توليد Email وUsername وهميين آمنين للاختبارات.
- توليد أرقام وقيم عشوائية وتواريخ ونصوص وIDs.
- السماح للمطور بإنشاء Generator لأي Class يملكه.
- دعم توليد Object واحد أو مجموعة Objects أو توليد كسول عند اكتمال Generation API.
- دعم Seed للتحكم في قابلية إعادة الإنتاج.
- عدم فرض ORM أو Database أو Web Framework على المستخدم.
- العمل Offline بعد تثبيت الحزمة.
- عدم فرض خدمات خارجية أو اشتراكات أو API Keys.
- الحفاظ على API عامة مستقرة، مع إخفاء تفاصيل التنفيذ الداخلي.
- توفير اختبارات موثوقة وتوثيق وأمثلة وBenchmarks قبل الإصدار العام.

## 3.3 معايير النجاح

يُعتبر المشروع ناجحًا عندما يستطيع مطور .NET:

1. تثبيت الحزمة دون إعدادات خارجية معقدة.
2. إنشاء `Faker` مع Seed أو بدونه.
3. تعريف أي Class مناسب عبر `Define<T>()`.
4. تحديد Properties باستخدام `RuleFor()`.
5. استخدام Providers عربية وعامة داخل قواعد التوليد.
6. توليد كائنات ومجموعات بكفاءة.
7. إعادة إنتاج النتائج باستخدام Seed ثابت.
8. فهم أخطاء الإعداد والتوليد من خلال Result واضح.
9. قراءة Documentation وأمثلة قابلة للتشغيل.

# 4. نطاق المشروع وما لا يهدف إلى تقديمه

تحديد النطاق مهم حتى لا تتحول المكتبة إلى نظام ضخم خارج غرضها.

## 4.1 ما يدخل في النطاق

- بيانات وهمية عامة وقيم أولية قابلة للتخصيص.
- أسماء وأماكن عربية وإقليمية.
- نصوص وأرقام وتواريخ وIDs وبيانات Internet وهمية.
- Mapping لأي Class يقدمه المستخدم.
- اختيار البيانات من Collections يمررها المستخدم.
- توليد Object واحد أو عدة Objects أو Lazy generation.
- التحكم في Seed وStreams.
- Data Catalogs مضمّنة داخل المكتبة عند الحاجة.
- دعم أكثر من لغة لحقول البيانات التي تملك ترجمة، مثل Arabic/English.

## 4.2 ما لا يدخل في النطاق حاليًا

- **لا توجد Domain Classes جاهزة:** لن توفر المكتبة `Student`, `Order`, `Product`, `Employee` كـ Models إجبارية.
- **لا توجد Business Scenarios:** لن تكون هناك سيناريوهات Ecommerce أو School أو Banking جاهزة.
- **لا توجد Relational Dataset Engine:** المكتبة لا تدير علاقات الكيانات بين بعضها كمنظومة قواعد بيانات متكاملة.
- **لا توجد Database Seeding Engine:** لا تتصل المكتبة بـ SQL Server أو PostgreSQL ولا تنفذ Insert أو Migrations.
- **لا توجد ORM dependency:** لا تعتمد على EF Core أو Dapper.
- **لا توجد خدمات خارجية وقت التوليد:** لا تحتاج API أو Internet connection.
- **لا توجد خدمات AI/Ollama:** ليست جزءًا من الفكرة الحالية.
- **لا توجد بيانات شخصية حقيقية:** المكتبة لا تستخدم سجلات أفراد حقيقيين لتوليد هويات.
- **لا تستهدف توليد بيانات اعتماد أمنية حقيقية:** مثل Passwords أو Cryptographic Tokens.
- **لا تستهدف توليد أرقام قومية حقيقية أو أرقام هواتف صالحة للوصول إلى أشخاص** ضمن النطاق الحالي.
- **لا تضمن uniqueness تلقائيًا:** Email وUsername وGUIDs قد تتكرر نظريًا عبر نطاق كبير؛ uniqueness داخل قاعدة بيانات المستخدم ليست مسؤولية المكتبة حاليًا.

## 4.3 مبدأ الفصل

ArabFaker تولّد Objects في الذاكرة. المستخدم هو الذي يقرر هل سيستخدمها في Unit Test أو API Test أو يرسلها إلى Database من خلال مشروعه هو. هذا الفصل أساسي في هوية المكتبة.

# 5. حالات الاستخدام

## 5.1 Unit Testing

إنشاء بيانات متغيرة لتغطية حالات متعددة داخل الاختبارات، بدل تكرار إنشاء Objects يدويًا.

## 5.2 Integration Testing

تجهيز DTOs أو Request Models وهمية لإرسالها إلى API في بيئة الاختبار.

## 5.3 API Testing

توليد عدد كبير من Requests لاختبار Validation وPagination وFiltering والـ Error Handling.

## 5.4 Local Development

تجهيز بيانات للـ Frontend أو Demo محلي دون استخدام بيانات مستخدمين حقيقيين.

## 5.5 Performance Testing

توليد حجم كبير من Objects في الذاكرة لاختبار Serialization أو Processing أو Mapping أو أجزاء أخرى من تطبيق المستخدم. المكتبة نفسها ليست بديلًا عن أدوات Load Testing المتخصصة.

## 5.6 Database Seeding من جهة المستخدم

يمكن للمستخدم توليد Objects ثم حفظها بنفسه باستخدام EF Core أو أي Data Access Technology. ArabFaker لا تنفذ عملية الحفظ.

# 6. المستخدم المستهدف

- مطورو C# و.NET.
- مطورو Backend APIs.
- مطورو Libraries وأدوات الاختبار.
- فرق QA وAutomation التي تعمل على .NET.
- طلاب ومطورون يحتاجون بيانات تجريبية لمشاريعهم.
- مطورو تطبيقات تستفيد من أسماء أو بيانات مواقع عربية.

# 7. المبادئ والقرارات التصميمية

هذه المبادئ تحكم كل قرار تقني جديد.

## 7.1 Generic First

المكتبة لا تعرف Domain المستخدم مسبقًا. المطور يحدد الـ Class والـ Properties بنفسه.

## 7.2 Arabic-Focused, Not Egypt-Locked

التركيز الحالي عربي، وأول Dataset للمواقع هي مصر. التصميم لا ينبغي أن يفترض أن كل دولة لها نفس التقسيمات الإدارية أو نفس أسماء المستويات. دعم دول عربية أخرى يحتاج Catalog/Locale configuration واضحًا، لا مجرد تبديل أسماء.

## 7.3 Provider Separation

كل مجال توليد له Provider مستقل، مثل Names وDates وInternet وLocations وText. هذا يمنع تضخم `RandomGenerator` ويحسن الاختبار والصيانة.

## 7.4 Public Facade / Internal Implementation

المستخدم يتعامل مع Public Facades مثل `NameGenerator` و`TextGenerator`. التنفيذ مثل `NamesProvider` و`NameCatalog` و`RandomGenerator` يظل Internal قدر الإمكان.

## 7.5 No Throw-Based Validation

صاحب المشروع لا يريد استخدام `throw` كآلية أساسية للتحقق من المدخلات أو التحكم في مسار العمل. المطلوب تفضيل:

- `Try...` Methods مع `bool` و`out` للقيم الداخلية البسيطة.
- `GenerationResult<T>` للأخطاء الخاصة بعملية التوليد العامة.
- `ConfigurationErrors` لأخطاء إعداد Definition.
- `TryParse` و`TryGetValue` وما شابهها عند التعامل مع قيم قابلة للفشل.

هذا لا يعني أن المكتبة تستطيع منع كل Exceptions التي قد تنتج من CLR أو من Delegates كتبها المستخدم. لا ينبغي إخفاء Exceptions غير المتوقعة داخل `catch (Exception)` شامل، ولا ابتلاع أخطاء المستخدم بصمت. يجب تحديد سياسة واضحة لأخطاء User Factories وRule Factories قبل إصدار Part 12.

## 7.6 Reproducibility

نفس Seed، ونفس Definition Scope وترتيب العمليات، ونفس إصدارات البيانات والقواعد ينبغي أن ينتجوا نفس Sequence من القيم. أي تغيير في Dataset أو Algorithm قد يغيّر النتائج بين إصدارات المكتبة؛ لذلك يجب توثيق حدود ضمان الاستقرار عبر الإصدارات.

## 7.7 Offline by Default

كل Data Catalog مطلوب وقت التوليد يكون مضمّنًا في الـ Assembly أو متاحًا محليًا بطريقة موثقة. لا توجد اتصالات شبكية أثناء Generate.

## 7.8 No Forced Dependencies

مشروع المكتبة يفضل أن يعتمد على .NET BCL فقط. حزم الاختبارات والتوثيق والأدوات المساعدة تخص مشاريعها، ولا تتحول تلقائيًا إلى Dependencies للمستخدم النهائي.

# 8. التقنيات وبيئة التطوير

## 8.1 التقنية الأساسية

| البند | الاختيار الحالي |
|---|---|
| Language | C# |
| Target Framework | `net10.0` |
| .NET SDK المستخدم | 10.0.403 حسب آخر بيئة معلومة |
| C# Language Version | C# 14 ضمن بيئة .NET 10 الحالية |
| Library Type | .NET Class Library |
| Test Framework | xUnit v2 (`xunit` 2.9.3 + `xunit.runner.visualstudio` 3.1.4 — الحالي في الريبو) |
| IDE | Visual Studio Community / أدوات تحرير أخرى |
| Data Storage داخل المكتبة | Embedded JSON Resources للمواقع حاليًا |
| Network dependency | لا توجد وقت التشغيل |
| Database dependency | لا توجد |
| ORM dependency | لا توجد |

## 8.2 بيئة الجهاز التي استخدمت في التطوير

- Windows 10 build 19045.
- SDKs ظهرت في `dotnet --list-sdks`: 9.0.320 و10.0.301 و10.0.403.
- الـ SDK النشط في آخر فحص: 10.0.403.
- MSBuild 18.9.15.
- المشروع يستهدف `net10.0`.

هذه معلومات بيئة التطوير وقت تسجيلها، وليست شرطًا على مستخدم المكتبة النهائي سوى متطلبات Target Framework المنشورة.

## 8.3 Dependencies

- لا تضف Dependency إلى مشروع `ArabFaker.NET` دون حاجة واضحة ومبرر معماري.
- xUnit v3 موجود في مشروع الاختبارات فقط.
- لا توجد حاجة إلى AutoMapper أو EF Core أو Newtonsoft.Json في المكتبة لمجرد تنفيذ الفكرة؛ استخدام `System.Text.Json` وBCL كافٍ للمسار الحالي.

# 9. الهيكل الفعلي للـ Solution

**مهم جدًا:** الهيكل الفعلي الذي أكده صاحب المشروع لا يحتوي على مجلد `src`، واسم مشروع المكتبة هو `ArabFaker.NET` وليس `ArabFaker`.

المسار المحلي المعروف للجذر:

```text
D:\My .net Projects Full version\My Projects\ArabFaker
```

شكل الـ Solution:

```text
ArabFaker/
│
├── ArabFaker.slnx
│
├── ArabFaker.NET/
│   ├── ArabFaker.NET.csproj
│   ├── Faker.cs
│   │
│   ├── Generation/
│   │   ├── GenerationContext.cs
│   │   ├── IdGenerator.cs
│   │   ├── FakerContext.cs                 (Public Facade Context)
│   │   ├── FakerDefinition.cs              (Part 11)
│   │   ├── GenerationError.cs              (planned for Part 12)
│   │   ├── GenerationResult.cs             (planned for Part 12)
│   │   │
│   │   ├── Randomization/
│   │   │   ├── IRandomSource.cs
│   │   │   ├── SplitMix64.cs
│   │   │   ├── Xoshiro256StarStarRandomSource.cs
│   │   │   ├── RandomGenerator.cs
│   │   │   └── RandomStreamFactory.cs
│   │   │
│   │   └── Seed/
│   │       ├── StableStringHash64.cs
│   │       └── SeedDeriver.cs
│   │
│   ├── Providers/
│   │   ├── Arab/
│   │   │   ├── Names/
│   │   │   │   ├── NameEntry.cs
│   │   │   │   ├── NameLanguage.cs
│   │   │   │   ├── NameGender.cs
│   │   │   │   ├── NameCatalog.cs
│   │   │   │   ├── NamesProvider.cs
│   │   │   │   └── NameGenerator.cs         (Public Facade)
│   │   │   │
│   │   │   └── Locations/
│   │   │       ├── LocationLevel.cs
│   │   │       ├── LocationLanguage.cs
│   │   │       ├── LocationEntry.cs
│   │   │       ├── LocationCatalog.cs
│   │   │       ├── LocationsProvider.cs
│   │   │       ├── LocationInfo.cs          (Public DTO)
│   │   │       └── LocationsGenerator.cs    (Public Facade)
│   │   │
│   │   └── Common/
│   │       ├── Internet/
│   │       │   ├── InternetProvider.cs
│   │       │   └── InternetGenerator.cs
│   │       ├── Dates/
│   │       │   ├── DatesProvider.cs
│   │       │   └── DatesGenerator.cs
│   │       ├── Text/
│   │       │   ├── TextCharacterSet.cs
│   │       │   ├── TextProvider.cs
│   │       │   └── TextGenerator.cs
│   │       ├── Random/
│   │       │   └── RandomDataGenerator.cs
│   │       └── Identifiers/
│   │           └── IdentifierGenerator.cs
│   │
│   └── Data/
│       └── Locations/
│           └── Egypt/
│               ├── all-governorate.json
│               ├── all-district.json
│               └── all-shiyakha.json
│
└── ArabFaker.Tests/
    ├── ArabFaker.Tests.csproj
    ├── Generation/
    ├── Providers/
    ├── FakerTests.cs
    └── ...
```

هذه خريطة مرجعية وليست أمرًا بإعادة تسمية ملفات موجودة. أسماء بعض ملفات الاختبارات قد تختلف حسب ما أنشأه المطور محليًا. لا تنشئ `src/` ولا مجلد `ArabFaker/` جديدًا للمكتبة.

## 9.1 Namespace مقابل Project Name

- اسم المشروع: `ArabFaker.NET`.
- اسم الـ Solution: `ArabFaker.slnx` (صيغة .slnx الجديدة، وليس `.sln`).
- اسم مشروع الاختبارات: `ArabFaker.Tests`.
- الـ Namespace الأساسي **الفعلي** المستخدم في كل ملفات الكود: `ArabFaker.NET` (مثل `ArabFaker.NET.Generation`, `ArabFaker.NET.Providers.Arab.Names`). أما الاختبارات فتستخدم `ArabFaker.Tests`. ملاحظة: الـ LogicalName للموارد المضمّنة يبدأ بـ `ArabFaker.Data...` (وهذا مقصود ومطابق للثابت `ResourcePrefix` في `LocationCatalog`).

الـ Namespace الحالي في الكود هو `ArabFaker.NET`، فلا تغيّره إلى `ArabFaker` دون قرار صريح ومراجعة لكل الملفات والاختبارات والـ Package ID.

## 9.2 Embedded Resources للمواقع

في `ArabFaker.NET.csproj` تم حل مشكلة عدم العثور على الموارد باستخدام أسماء LogicalName صريحة. الشكل المعتمد:

```xml
<ItemGroup>
  <EmbeddedResource Include="Data\Locations\Egypt\all-governorate.json"
                    LogicalName="ArabFaker.Data.Locations.Egypt.all-governorate.json" />

  <EmbeddedResource Include="Data\Locations\Egypt\all-district.json"
                    LogicalName="ArabFaker.Data.Locations.Egypt.all-district.json" />

  <EmbeddedResource Include="Data\Locations\Egypt\all-shiyakha.json"
                    LogicalName="ArabFaker.Data.Locations.Egypt.all-shiyakha.json" />
</ItemGroup>
```

استخدام أسماء صريحة تم بعد فشل اختبارات `LocationCatalog` بسبب عدم مطابقة أسماء الـ Embedded Resources لما يبحث عنه الكود. بعد هذا التعديل أكد المستخدم أن الاختبارات نجحت.

# 10. الصورة المعمارية العامة

التصميم مقسوم إلى طبقتين واضحتين:

1. **Public API:** الواجهة التي يستخدمها مطور المكتبة.
2. **Internal Engine:** محرك التوليد والـ Providers والـ Catalogs وخوارزميات الـ Random.

التصور:

```text
Consumer Application
        │
        ▼
      Faker
        │
        ├── Define<T>()
        │       │
        │       ▼
        │   FakerDefinition<T>
        │       │
        │       └── RuleFor(...)
        │
        └── Generation Infrastructure
                │
                ├── SeedDeriver
                ├── RandomStreamFactory
                └── GenerationContext
                        │
                        ├── RandomGenerator
                        ├── IdGenerator
                        ├── NamesProvider
                        ├── InternetProvider
                        ├── DatesProvider
                        ├── LocationsProvider
                        └── TextProvider
```

والـ Public Facades تربط المستخدم بالتنفيذ الداخلي:

```text
FakerContext (Public)
│
├── Text          -> TextProvider
├── Names         -> NamesProvider
├── Internet      -> InternetProvider
├── Dates         -> DatesProvider
├── Locations     -> LocationsProvider
├── RandomData    -> RandomGenerator
└── Identifiers   -> IdGenerator
```

الفكرة أن Public Facade لا تكشف الـ Provider الداخلي ولا تفاصيل الـ Random Engine.

# 11. محرك العشوائية Randomization Engine

هذا المحرك هو الأساس الذي تعتمد عليه كل الـ Providers.

## 11.1 IRandomSource

واجهة داخلية مسؤولة عن العمليات منخفضة المستوى، ومن بين العمليات التي تم اعتمادها:

```csharp
internal interface IRandomSource
{
    ulong NextUInt64();
    bool TryNextInt32(int minInclusive, int maxExclusive, out int value);
    double NextDouble();
    void NextBytes(Span<byte> destination);
}
```

لا يجب أن يتعامل مستخدم NuGet معها مباشرة.

## 11.2 SplitMix64

خوارزمية مساعدة تُستخدم لاشتقاق/تهيئة الحالة العشوائية. هي جزء من Randomization internals، وليست API عامة.

## 11.3 Xoshiro256**

مصدر العشوائية الأساسي في التصميم الحالي هو `Xoshiro256StarStarRandomSource`.

خصائص التصميم المطلوبة:

- سريع ومناسب لتوليد بيانات اختبارية.
- قابل لإعادة الإنتاج عند استخدام نفس الحالة الابتدائية.
- لا يعتمد على `System.Random` لكل Provider بشكل مشترك.
- ليس Cryptographically Secure.

**مهم:** لا تستخدمه لتوليد Passwords حقيقية أو Reset Tokens أو مفاتيح تشفير أو بيانات اعتماد أمنية.

## 11.4 RandomGenerator

طبقة أعلى من `IRandomSource` تقدم عمليات عامة، مثل:

- `TryNextInt`.
- `TryNextInt64`.
- `TryNextDecimal`.
- `NextDouble`.
- `TryNextBoolean`.
- `TryPick<T>`.
- `TryPickMany<T>`.

التفاصيل الدقيقة للحدود يجب أن تكون موثقة:

- `minInclusive`: الحد الأدنى مشمول.
- `maxExclusive`: الحد الأعلى غير مشمول في دوال Integer الحالية.
- `TryNextDecimal` يستخدم نطاقًا عشريًا بحدود معلنة، ويجب أن يظل فيه تدقيق خاص لحالات decimal القصوى.

## 11.5 Rejection Sampling

عند تحويل قيمة عشوائية كبيرة إلى Range أصغر، يتم استخدام Rejection Sampling بدل الاعتماد على Modulo فقط؛ لتجنب الانحياز الإحصائي الناتج عن اختلاف قابلية القسمة.

## 11.6 TryPickMany

- عند `allowDuplicates = true` يمكن اختيار نفس القيمة أكثر من مرة.
- عند `allowDuplicates = false` لا تتكرر العناصر المختارة من المصدر وفق منطق السحب الجزئي/Partial Fisher–Yates.
- يجب عدم تعديل الـ Collection التي مررها المستخدم.
- يجب اختبار `count = 0` وCollection فارغة وCount أكبر من عدد العناصر عند منع التكرار.

# 12. Seed وRandom Streams

## 12.1 ما هو Seed؟

Seed قيمة تحدد بداية تسلسل التوليد. مثال:

```csharp
var faker = new Faker(seed: 2026);
```

استخدام نفس Seed، مع نفس ترتيب التعريفات والعمليات ونفس إصدار قواعد البيانات، يتيح إعادة إنتاج نفس القيم.

عند عدم تمرير Seed، يتم إنشاء Seed تلقائيًا. آخر تصميم قدم `Random.Shared.NextInt64()` لتوليد هذه القيمة الافتراضية. هذه وسيلة لاختيار Seed، وليست ضمانًا أمنيًا.

## 12.2 Root Seed

الـ `Faker` يحتفظ بالـ Root Seed. داخليًا يتم تحويل القيمة إلى `ulong` باستخدام تحويل غير checked عند تمريرها إلى `RandomStreamFactory`؛ لذلك يمكن قبول Seeds سالبة أيضًا.

## 12.3 StableStringHash64

تستخدم مفاتيح الـ Streams لتحديد Streams مختلفة. لذلك توجد Hash ثابتة على النص، وليست Hash عشوائية تعتمد على Process runtime.

## 12.4 SeedDeriver

يشتق Seed فرعي من:

- Root Seed.
- Stream Key.
- Item Index.

والهدف هو جعل كل Provider وكل عنصر له سلسلة عشوائية مستقلة منطقيًا.

## 12.5 Independent Streams

أمثلة على المفاتيح الداخلية:

```text
{scopeKey}/random
{scopeKey}/ids
{scopeKey}/names
{scopeKey}/internet
{scopeKey}/dates
{scopeKey}/locations
{scopeKey}/text
```

بالتالي زيادة عدد مرات استخدام Names لا ينبغي أن تغيّر نتيجة Internet في نفس Generation Context.

## 12.6 Definition Scope وItem Index

في مرحلة `Define<T>()`، لكل Definition Scope مستقل. وعند توليد كل Object، يتم استخدام Item Index مستقل.

**النوع المعتمد لـ `itemIndex` هو `ulong`، وليس `long`.** تم تصحيح هذا النوع في كود المشروع المحلي. يجب الالتزام به في كل Methods والاختبارات والأمثلة اللاحقة.

## 12.7 حدود Reproducibility

تتأثر النتائج بـ:

- Seed.
- ترتيب إنشاء Definitions إن كان الـ Scope ID تسلسليًا.
- ترتيب طلب Generate إن كان Item Index تسلسليًا.
- أسماء Stream Keys.
- خوارزمية Seed Derivation والـ PRNG.
- إصدار الـ Dataset.
- ترتيب البيانات داخل الـ Catalog.
- ترتيب تنفيذ الـ Rules إذا كانت Rules تعتمد على Side Effects أو تشارك حالة.

لذلك لا ينبغي الوعد بأن نفس Seed سيعطي نفس البيانات عبر كل الإصدارات المستقبلية دون سياسة Versioning للـ Algorithm والـ Data.

# 13. Generation Context

`GenerationContext` كائن داخلي يمثل سياق عملية توليد محددة.

مسؤولياته:

- حمل `RandomGenerator`.
- حمل `IdGenerator`.
- حمل Providers الخاصة بالتوليد.
- الاحتفاظ بـ `ItemIndex` من النوع `ulong`.
- الاحتفاظ بالـ Stream Factory والـ Scope Key.
- إنشاء Child Contexts/Streams عند الحاجة.

كل Generation Context يجب أن يكون مربوطًا بـ Item محدد، وليس Random واحدًا مشتركًا بين كل عمليات التوليد.

`FakerContext` هو Public Facade Context الذي يعرض Generators للمستخدم داخل Delegate الـ `RuleFor`.

# 14. شرح الـ Providers الحالية

## 14.1 Names Provider

المسار:

```text
Providers/Arab/Names/
```

المكونات:

- `NameEntry`: يمثل الاسم بلغتين.
- `NameLanguage`: اختيار Arabic أو Latin، وهو Public عندما يحتاجه المستهلك.
- `NameGender`: Any أو Male أو Female، وهو Public عندما يحتاجه المستهلك.
- `NameCatalog`: قوائم الأسماء.
- `NamesProvider`: منطق الاختيار الداخلي.
- `NameGenerator`: Public Facade.

الوظائف الحالية التي تم بناؤها:

- `TryFirstName`.
- `TryMiddleName`.
- `TryLastName`.
- `TryFullName`.

ملاحظات:

- الـ Catalog الحالي يحتوي على عينات أسماء ثابتة داخل الكود.
- هذه العينات ليست Dataset إحصائية ممثلة لكل السكان أو لكل الدول العربية.
- `TryMiddleName` كان يستخدم نفس قائمة First Names في التنفيذ الأولي؛ هذا قرار مؤقت يحتاج تحسينًا في Data Catalog لاحقًا.
- يجب مراجعة التهجئة العربية والـ Latin transliteration والتوازن بين Gender categories قبل الإصدار.
- يجب عدم الادعاء بأن الاسم يعكس توزيعًا ديموغرافيًا حقيقيًا دون مصدر بيانات مناسب.

## 14.2 Internet Provider

المسار:

```text
Providers/Common/Internet/
```

الوظائف الحالية:

- `TryUsername`.
- `TryEmail`.

السلوك الأولي:

- Username يبدأ بـ `user`، ويتبعه 8 محارف صغيرة/أرقام.
- الطول المتوقع للـ Username الحالي 12 محرفًا.
- البريد الافتراضي يستخدم `example.test`.
- يمكن تمرير Domain مخصص، لكن التحقق الحالي يشترط أن ينتهي النطاق بـ `.test`.
- يتم التحقق من Labels المسموح بها وطولها وعدم وجود Hyphen في أول أو آخر Label.
- لا يتم إرسال بريد حقيقي.
- لا يوجد ضمان أن Username أو Email فريد.

الغرض: بيانات اختبارية لا تستخدم للتواصل الحقيقي.

## 14.3 Random Data Provider

التنفيذ منخفض المستوى في `RandomGenerator` والواجهة العامة المقترحة/المنشأة في `RandomDataGenerator`.

الوظائف:

- Integer داخل Range.
- Int64 داخل Range.
- Decimal داخل Range بدقة عشرية محددة.
- Double بين 0 شاملًا و1 غير شامل.
- Boolean مع True Probability.
- اختيار عنصر من Collection.
- اختيار عدة عناصر مع أو بدون تكرار.

ملاحظات مهمة:

- دوال Integer الحالية تستخدم Upper Bound Exclusive.
- `TryBoolean` يجب أن يرفض Probability غير المحددة finite أو خارج `[0, 1]`.
- `TryPickMany` يجب ألا يعدل Collection المصدر.
- `TryNextDecimal` في التنفيذ الأولي يعتمد على طريقة حساب مبنية على `NextDouble` مع Rounding؛ هذا ليس بالضرورة Uniform بعد التقريب، ويحتاج تدقيقًا لحالات `decimal.MinValue` و`decimal.MaxValue` لتجنب Overflow.
- يجب توثيق حدود الأداء عند تمرير Collections كبيرة، لأن منع التكرار قد ينسخ المجموعة قبل تنفيذ Partial Shuffle.

## 14.4 Dates Provider

المسار:

```text
Providers/Common/Dates/
```

الوظائف:

- `TryDateOnlyBetween(startInclusive, endInclusive, out value)`.
- `TryDateTimeBetween(startInclusive, endInclusive, out value)`.
- `TryDateOfBirth(asOfDate, minAge, maxAge, out dateOfBirth)`.

السلوك:

- DateOnly Range شامل للطرفين.
- DateTime Range شامل للطرفين في التنفيذ الحالي.
- DateTime المدخلان يجب أن يكون لهما نفس `DateTimeKind`.
- توليد تاريخ الميلاد يعتمد على تاريخ مرجعي صريح `asOfDate`، وليس وقت الجهاز الحالي، لدعم Reproducibility.
- يجب اختبار الحدود الدنيا/العليا للتقويم وسنوات الكبيسة ويوم 29 فبراير.
- طريقة حساب العمر يجب أن تتفق مع قاعدة `AddYears` المستخدمة في .NET، وتوثق هذه السياسة.
- يجب ألا ينتج عن بناء الحد الأدنى أو الأعلى تاريخ خارج مجال DateOnly.

## 14.5 Locations Provider

المسار:

```text
Providers/Arab/Locations/
```

الطبقات:

- `LocationLevel`: Governorate / District / Shiyakha.
- `LocationLanguage`: Arabic / English.
- `LocationEntry`: الشكل الداخلي الموحد للسجل.
- `LocationCatalog`: تحميل البيانات وبناء Indexes.
- `LocationsProvider`: اختيار المواقع والحفاظ على Parent/Child relationship.
- `LocationInfo`: Public DTO.
- `LocationsGenerator`: Public Facade.

الوظائف:

- اختيار محافظة.
- اختيار District/مركز تابع لمحافظة ID محددة.
- اختيار Shiyakha تابعة لـ District ID محدد.
- اختيار اسم عشوائي بمستوى ولغة محددين.
- تمرير إحداثيات Latitude/Longitude إذا كانت متاحة وصالحة.

النقطة الأساسية: اختيار الموقع التابع يجب أن يعتمد على Parent ID، وليس اختيار كل مستوى بشكل مستقل؛ لتجنب إنتاج علاقة جغرافية غير صحيحة.

## 14.6 Text Provider

المسار:

```text
Providers/Common/Text/
```

الـ Character Sets الحالية:

- Lowercase Letters.
- Uppercase Letters.
- Digits.
- Letters.
- Alphanumeric Lowercase.
- Alphanumeric.
- Hexadecimal.

الوظائف:

- `TryRandomString(length, characterSet, out value)` (داخلي فقط في `TextProvider` لأن `TextCharacterSet` Internal؛ الـ Facade العام يعرض الدوال المسماة فقط، ولا يعرض `AlphaNumericLowercase`).
- `TryDigits`.
- `TryLowercaseLetters`.
- `TryUppercaseLetters`.
- `TryLetters`.
- `TryAlphaNumeric`.
- `TryHexadecimal`.

السلوك:

- Length سالب => Failure.
- Length يساوي صفر => Success مع String فارغة.
- Character Set غير معروف => Failure.
- الحروف الحالية ASCII/Latin، وليست مولّد نصوص عربية طبيعية.
- لا تستخدم Text Provider لتوليد أسرار أمنية حقيقية.

## 14.7 Identifier Generator

التنفيذ الداخلي:

```csharp
internal sealed class IdGenerator
{
    private readonly IRandomSource _randomSource;

    internal IdGenerator(IRandomSource randomSource)
    {
        _randomSource = randomSource;
    }

    internal Guid NextGuid()
    {
        Span<byte> bytes = stackalloc byte[16];

        _randomSource.NextBytes(bytes);
        bytes[6] = (byte)((bytes[6] & 0x0F) | 0x40);
        bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80);

        return new Guid(bytes, bigEndian: true);
    }
}
```

مهم جدًا:

- الاسم الفعلي للـ Method الداخلية هو `NextGuid()`.
- لا توجد `TryGuid()` في `IdGenerator`.
- الـ Public Facade المناسب يستخدم أسماء مباشرة مثل `GenerateGuid()` و`GenerateGuidString()`؛ لا تستخدم `TryGuid` إلا إذا تغير التصميم الداخلي فعليًا.
- يتم ضبط Version bits إلى UUID v4 والـ Variant bits المتوافقة مع RFC.
- الناتج مناسب كـ Fake Identifier للاختبار، وليس Cryptographic Token.
- بسبب Seeded PRNG، يمكن إعادة إنتاج GUIDs في نفس السياق والـ Seed.

# 15. بيانات المواقع الجغرافية المصرية

## 15.1 المصدر

المصدر المستخدم:

[Open Admin Data — Egypt Administrative Divisions](https://github.com/open-admin-data/egypt-administrative-divisions)

وفق صفحة المستودع التي تمت مراجعتها في 5 نوفمبر 2026، يعرض المصدر:

| المستوى | العدد المعلن |
|---|---:|
| Governorates | 27 |
| Districts | 365 |
| Shiyakhas | 5,716 |
| الإجمالي | 6,108 |

البيانات ثنائية اللغة Arabic/English، وتضم إحداثيات جغرافية وملفات JSON/NDJSON/CSV، والمستودع يذكر CC BY 4.0. يجب مراجعة صفحة المصدر والترخيص مرة أخرى قبل كل Release؛ لأن Dataset قد تتغير.

## 15.2 الملفات المستخدمة

```text
Data/Locations/Egypt/
├── all-governorate.json
├── all-district.json
└── all-shiyakha.json
```

الأحجام التي ظهرت أثناء تنزيل الملفات في المحادثة كانت تقريبًا:

| الملف | الحجم التقريبي وقت الفحص |
|---|---:|
| all-governorate.json | 18.2 KB |
| all-district.json | 308 KB |
| all-shiyakha.json | 5.74 MB |
| الإجمالي التقريبي | حوالي 6.1 MB |

الأحجام تتغير إذا تغيرت Dataset أو تنسيقها؛ لا تعتمد على الأرقام كقيم ثابتة.

## 15.3 شكل السجل في المصدر

السجل في ملفات المصدر لا يقتصر على الاسم والـ ID فقط. الحقول التي يعرضها الـ Schema تشمل:

- `id`: معرف السجل.
- `level`: المستوى الإداري (1 أو 2 أو 3).
- `level_name`: اسم المستوى بالعربي والإنجليزي.
- `name.local`: الاسم باللغة المحلية.
- `name.en`: الاسم باللغة الإنجليزية.
- `name.slug`: Slug.
- `parent`: مرجع الأب أو null للمحافظة.
- `ancestors`: قائمة الأجداد.
- `children_count`: عدد الأبناء حسب المستوى.
- `zip_codes`: الرموز البريدية المتاحة عند وجودها.
- `geo.lat` و`geo.lon`: الإحداثيات كسلاسل نصية في ملفات JSON الحالية.
- `metadata` و`source`: بيانات وصفية ومصدر السجل.

`LocationCatalog` الحالي يحوّل الجزء الذي تحتاجه المكتبة إلى `LocationEntry` موحد، ويتجاهل الحقول غير المستخدمة حاليًا. إذا احتجنا لاحقًا Postal Codes أو Slugs أو Ancestors يمكن توسيع النموذج بعد تحديد API عامة مناسبة.

## 15.4 تحميل البيانات

- البيانات تُضمّن كـ Embedded Resources.
- لا يتم تنزيلها وقت تشغيل المكتبة.
- `LocationCatalog` يستخدم Lazy loading حتى لا يقرأ الملفات عند تحميل المكتبة كلها.
- يتم بناء Index حسب Location Level.
- يتم بناء Index للأبناء حسب Parent ID + Level لتجنب البحث الكامل كل مرة.
- يتم التحقق من Hierarchy قبل اعتماد البيانات.
- عند وجود JSON غير صالح أو Resource مفقود أو Hierarchy غير مكتملة، يجب أن يفشل التحميل بطريقة يمكن تشخيصها دون ابتلاع كل أنواع الاستثناءات.

## 15.5 التوسع لدول عربية أخرى

لا ينبغي وضع اسم مصر داخل كل منطق الاختيار. التصميم المستقبلي الأفضل:

```text
Location Country / Locale
        │
        ├── Country Code
        ├── Dataset Catalog
        ├── Level Definitions
        └── Locale-specific hierarchy
```

لكن لا تفترض أن كل دولة لديها Governorate/District/Shiyakha. أسماء ومستويات التقسيم الإداري يجب أن تكون قابلة للتمثيل حسب الدولة. يمكن البدء بـ `CountryCode` وCatalog abstractions بعد استقرار API مصر، دون تعقيد مبكر غير مطلوب.

# 16. واجهة المستخدم العامة Public API

## 16.1 Faker

`Faker` هي Entry Point:

```csharp
var faker = new Faker(seed: 2026);
var randomSeedFaker = new Faker();
```

المسؤوليات:

- الاحتفاظ بالـ Root Seed.
- إنشاء Random Stream Factory.
- إنشاء Definitions عبر `Define<T>()`.
- تخصيص Definition Scope مستقل.
- عدم كشف Random engine الداخلي للمستخدم.

## 16.2 FakerContext

كائن Public يعرض Generators أثناء تنفيذ الـ Rule Factories. إنشاؤه الداخلي يتم من `GenerationContext` المرتبط بالـ Item الحالي.

الـ Properties المستهدفة/الموجودة حسب خطوات Part 10:

```text
FakerContext
├── Text
├── Names
├── Internet
├── Dates
├── Locations
├── RandomData
└── Identifiers
```

الإنشاء المباشر لـ `FakerContext` أو `CreatePublicContext` يظل Internal في التصميم الحالي؛ المستخدم يحصل عليه من الـ `context` parameter داخل `RuleFor`.

## 16.3 Public Facades

| Public Facade | الوظيفة |
|---|---|
| `TextGenerator` | توليد نصوص ومجموعات حروف |
| `NameGenerator` | توليد First/Middle/Last/Full Name |
| `InternetGenerator` | توليد Username وEmail اختباري |
| `DatesGenerator` | توليد DateOnly/DateTime/Date of Birth |
| `LocationsGenerator` | توليد مواقع مصرية حسب مستوى وعلاقة الأب |
| `RandomDataGenerator` | قيم رقمية وBoolean واختيار Collection items |
| `IdentifierGenerator` | توليد Guid وGuid string |

## 16.4 قاعدة مهمة

كل Public Method يجب أن يكون موثقًا بـ XML Documentation، وتكون حدودها مفهومة: Inclusive/Exclusive، Nullable behavior، Seed behavior، والـ Failure behavior.

# 17. Define<T> وRuleFor

هذا هو جوهر التخصيص في المكتبة.

## 17.1 الفكرة

المستخدم هو الذي يملك الـ Model ويحدد خصائصه، مثل:

```csharp
public sealed class Student
{
    public string FirstName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
    public DateOnly BirthDate { get; set; }
}
```

المكتبة لا تعرّف `Student` بنفسها. المستخدم يعرّف قواعد التوليد:

```csharp
var studentFaker = faker.Define<Student>()
    .RuleFor(x => x.FirstName, context => ...)
    .RuleFor(x => x.Email, context => ...)
    .RuleFor(x => x.Age, context => ...)
    .RuleFor(x => x.BirthDate, context => ...);
```

## 17.2 Define<T>()

مسؤولياته:

- تحديد نوع الـ Object.
- اختيار Factory لإنشائه.
- إنشاء Definition Scope خاص به.
- تجهيز Definition لاستقبال Rules.
- تسجيل أخطاء الإعداد دون بدء التوليد.

هناك مساران:

```csharp
faker.Define<Student>();
```

وهذا يحتاج `class` مع `public parameterless constructor` في النسخة الحالية.

```csharp
faker.Define<Student>(() => new Student("Default"));
```

وهذا يتيح دعم Constructors ذات Parameters.

## 17.3 Definition Scope

كل Definition يحصل على Scope Key مستقل حتى لو كان لنفس Type:

```csharp
var first = faker.Define<Student>();
var second = faker.Define<Student>();
```

هذا يمنع مشاركة Random Streams داخليًا بين التعريفين. الـ Definition IDs من النوع `ulong`، ويتم تخصيصها داخل Lock لحماية عملية التخصيص.

## 17.4 RuleFor

التوقيع المقترح/المستخدم في التنفيذ الحالي:

```csharp
public FakerDefinition<T> RuleFor<TProperty>(
    Expression<Func<T, TProperty>> propertySelector,
    Func<FakerContext, TProperty> valueFactory);
```

الجزء الأول يحدد Property، والجزء الثاني يحدد طريقة إنتاج القيمة وقت توليد كل Object.

مثال:

```csharp
.RuleFor(x => x.Name, context =>
{
    bool success = context.Names.TryFirstName(out var name);
    return success ? name : "Unknown";
})
```

لا يتم تنفيذ `valueFactory` عند تسجيل Rule؛ بل يتم حفظها حتى عملية Generation.

## 17.5 Expression Trees

لماذا `Expression<Func<T, TProperty>>` بدل `string propertyName`؟

- Compile-time property selection.
- IntelliSense.
- Refactoring safety.
- اكتشاف نوع الـ Property.
- فحص أن التعبير يمثل Property مباشرة.
- إنشاء Setter strongly typed مرة عند تسجيل Rule بدل `PropertyInfo.SetValue()` المتكرر لكل Object.

العملية الداخلية المتوقعة:

1. فحص أن Expression لها Parameter واحد.
2. فحص أن Body هي MemberExpression.
3. التأكد أن Member هي PropertyInfo.
4. التأكد أن الـ Property مباشرة على Parameter، وليس Nested.
5. التحقق من Public instance setter.
6. رفض Indexers.
7. رفض init-only في التصميم الحالي.
8. إنشاء Setter Delegate من Expression.
9. تخزين Setter مع Value Factory.

## 17.6 حدود Mapping في الإصدار الأول

مدعوم مبدئيًا:

```csharp
.RuleFor(x => x.Name, context => "Youssef")
.RuleFor(x => x.Age, context => 25)
```

غير مدعوم مبدئيًا:

```csharp
.RuleFor(x => x.Address.City, context => "Cairo") // Nested
.RuleFor(x => x.Name.Length, context => 10)        // Computed
.RuleFor(x => x[0], context => "Value")           // Indexer
```

وكذلك properties ذات `private set` أو بدون Setter أو `init-only` لا تدخل في النطاق الأولي.

## 17.7 Duplicate Rules

لو تم تعريف نفس Property أكثر من مرة:

```csharp
.RuleFor(x => x.Name, _ => "First")
.RuleFor(x => x.Name, _ => "Second")
```

السلوك الذي تم الاتفاق عليه في التنفيذ المبدئي: **آخر Rule تكسب**.

يجب توثيق هذا السلوك واختباره. يجب أيضًا عدم الاعتماد على ترتيب Dictionary غير الموثق لتحديد ترتيب تنفيذ Rules ذات Side Effects؛ الأفضل أن يحتفظ التنفيذ بقائمة مرتبة للقواعد المسجلة مع Dictionary لتحديد/استبدال Property، أو أن يوضح أن Rule Factories يجب ألا تعتمد على بعضها.

## 17.8 Custom Factory

Custom Factory تستخدم لإنشاء Object يحتاج Constructor Parameters أو تهيئة خاصة:

```csharp
var definition = faker.Define<Student>(
    () => new Student("Default Name"));
```

بعدها تطبق Rules على الـ instance الذي أرجعته الـ Factory.

## 17.9 ConfigurationErrors

الهدف هو كشف أخطاء Definition قبل التوليد، مثل:

- Factory غير موجودة.
- Selector غير صالح.
- اختيار Field بدل Property.
- Nested property غير مدعومة.
- Private/No setter.
- Indexer.
- Init-only.
- Null Selector/Value Factory.
- استنفاد Definition ID space، وهو احتمال نظري.

يجب تقديم Error Codes منظمة بدل الاعتماد على نص الرسالة فقط عندما يتم تصميم Result/Configuration Error Model النهائي.

# 18. التوليد وResult Handling — التصميم المستهدف

**هذا القسم يصف المرحلة التالية، وليس جزءًا مكتملًا.**

## 18.1 Generate()

الهدف توليد Object واحد:

```csharp
GenerationResult<Student> result = studentFaker.Generate();
```

العملية المقترحة:

1. التحقق من Definition.
2. تخصيص `itemIndex` من نوع `ulong`.
3. إنشاء instance بواسطة Factory.
4. رفض null factory result.
5. إنشاء `GenerationContext` خاص بهذا Item.
6. إنشاء Public `FakerContext` فوقه.
7. تنفيذ كل Rules.
8. إرجاع Success مع Object أو Failure مع Error details.

لا يجب إعادة Object جزئي تم تعديل بعض خصائصه ثم فشلت Rule أخرى، إلا إذا تم تبني سياسة صريحة لذلك. السياسة الأكثر أمانًا هي عدم إرجاع Partial Object للمستخدم عند فشل العملية.

## 18.2 GenerationError

يمثل خطأ واحدًا:

```csharp
public sealed record GenerationError(
    string Code,
    string Message);
```

أمثلة Error Codes مقترحة:

- `Faker.InvalidDefinition`
- `Faker.FactoryReturnedNull`
- `Faker.RuleApplicationFailed`
- `Faker.ItemIndexExhausted`
- `Faker.InvalidCount`
- `Faker.CatalogUnavailable`

القائمة ليست نهائية، ويجب توحيد Naming convention قبل الإصدار.

## 18.3 GenerationResult<T>

النتيجة تمثل Success أو Failure، وقد تحتوي على Value أو مجموعة Errors.

شكل استهلاك API المقترح:

```csharp
var result = studentFaker.Generate();

if (result.IsSuccess)
{
    Student student = result.Value!;
}
else
{
    foreach (GenerationError error in result.Errors)
    {
        Console.WriteLine($"{error.Code}: {error.Message}");
    }
}
```

ينبغي أن تكون Errors Read-only، وألا يسمح للمستخدم بتغيير الحالة الداخلية للـ Result.

## 18.4 Generate(count)

الهدف توليد N Objects:

```csharp
var result = studentFaker.Generate(100);
```

ما زال يلزم اتخاذ قرار نهائي حول Return Type، مثل:

- `GenerationResult<IReadOnlyList<T>>`.
- أو `GenerationResult<T[]>`.

يجب حسم سياسة الفشل:

- **All-or-nothing:** إذا فشل عنصر واحد لا يتم إرجاع المجموعة كنجاح.
- **Partial result:** إرجاع العناصر الناجحة مع أخطاء العناصر الفاشلة.

للبداية، All-or-nothing أسهل في الفهم، لكن قد يكون Partial result مفيدًا لأحجام كبيرة. لا تعتمد أي سياسة قبل مناقشتها واختبارها.

يجب التحقق من Count السالب أو غير الصالح قبل البدء، وعدم استهلاك Item Indices عند فشل Validation مبكرًا إن أمكن.

## 18.5 GenerateLazy(count)

الهدف توليد العناصر واحدًا تلو الآخر عند Enumeration بدل إنشاء مجموعة كاملة مسبقًا:

```csharp
foreach (var result in studentFaker.GenerateLazy(1000))
{
    // Process each result.
}
```

يجب حسم هل `GenerateLazy` يعيد:

- `IEnumerable<GenerationResult<T>>` بحيث كل عنصر له نتيجة مستقلة.
- أو نوع Lazy Result آخر.

ويجب توثيق أن التوليد يبدأ عند Enumeration، وأن إعادة Enumeration قد تولد Sequence جديدة أو تعيد تشغيل العملية حسب العقد المختار.

## 18.6 Result لا يلغي Exceptions من العالم الخارجي

`GenerationResult<T>` مخصص للأخطاء المتوقعة التي تتحكم المكتبة في اكتشافها. لو المستخدم كتب Factory أو Rule ترمي Exception، يجب تحديد السياسة بوضوح:

- عدم استخدام `catch (Exception)` وابتلاع الخطأ.
- إمّا ترك Exception غير المتوقعة تظهر للمطور، أو تحويل أنواع محددة متفق عليها فقط إلى GenerationError.
- عدم استخدام `throw` صراحة كآلية Validation داخل المكتبة.

# 19. Determinism وReproducibility

## 19.1 مثال

```csharp
var firstFaker = new Faker(2026);
var secondFaker = new Faker(2026);
```

إذا أنشأ الاثنان نفس Definition بالترتيب نفسه، وطبق المستخدم نفس Rules، وطلب التوليد بنفس الترتيب، فيجب أن تتطابق النتائج.

## 19.2 ما الذي قد يكسر Reproducibility؟

- تغيير ترتيب إنشاء Definitions.
- تغيير Item Index allocation أو استدعاءات Generate.
- تغيير أسماء Stream Keys.
- تغيير Algorithm أو Hash.
- تغيير ترتيب Dataset.
- تغيير قائمة Names.
- استخدام Rule تعتمد على الوقت الحقيقي أو Random خارجي غير Seeded.
- استخدام static mutable state داخل Rule.
- تشغيل متزامن إذا رُبطت النتائج بترتيب Scheduling بدل Index صريح.

## 19.3 توصية مستقبلية

عند الحاجة لضمان ربط كل نتيجة بـ Index محدد حتى في Parallel Generation، يمكن التفكير في API مثل `GenerateAt(ulong itemIndex)` أو Batch generation ذات Indexes ثابتة. لا تضفها قبل إثبات الحاجة، لكن لا تدّعِ أن الـ lock وحده يضمن ترتيب النتائج في كل سيناريو Parallel.

# 20. Validation وسياسة الأخطاء

## 20.1 استخدام Try Pattern

استخدم `Try...` عندما تكون هناك مدخلات قابلة للرفض أو عملية قد تفشل بشكل متوقع:

```csharp
bool success = provider.TryPick(..., out var value);
```

## 20.2 Configuration Validation

أخطاء إعداد الـ Definition تظهر في `ConfigurationErrors` أو نتيجة Validation مخصصة.

## 20.3 Generation Validation

أخطاء التوليد تظهر في `GenerationResult<T>` عند اعتماد هذا العقد.

## 20.4 Nullability

يجب تفعيل Nullable Reference Types في المشروع ومراجعة الـ Public API:

- هل `TProperty` يسمح بـ null؟
- هل Factory يمكن أن ترجع null؟
- هل `GenerationResult<T>.Value` nullable عند Failure فقط؟
- هل APIs تتعامل مع Collections nullable؟

يجب أن تكون هذه الأمور متسقة في XML docs والاختبارات.

## 20.5 لا تستخدم Exceptions كمنطق عادي

لا تكتب Validation مبنيًا على `throw new ArgumentException` أو `throw new InvalidOperationException`؛ استخدم Failure واضحًا. ولا تضع `catch` عام حول كل الكود حتى لا تخفي Bugs.

# 21. Thread Safety والتزامن

يجب التمييز بين:

- Thread-safe Definition ID allocation.
- Thread-safe Catalog caching.
- Thread-safe generation operations.
- Thread-safe user-provided Factories/Rules.

وجود Lock لتخصيص Index لا يجعل كل أجزاء Definition آمنة تلقائيًا. يجب دراسة ما يلي:

- هل `Generate()` يسمح باستدعاءات متزامنة على نفس Definition؟
- هل `_rules` لا تتغير بعد بدء التوليد؟
- هل يمكن للمستخدم تعديل Factory/Rules أثناء Generation؟
- هل Value Factories نفسها آمنة للتزامن؟
- هل Random Sources تُنشأ لكل Item بدل مشاركة State؟

التوجه المفضل: بعد اكتمال Configuration، تصبح Definition ثابتة وغير قابلة للتعديل أثناء Generation، أو يتم توفير Freeze/Build step واضح قبل الاستخدام المتزامن.

# 22. الاختبارات وضمان الجودة

## 22.1 Test Project

```text
ArabFaker.Tests/
```

يستخدم xUnit 2.9.3 (وليس v3 كما كان مكتوبًا سابقًا)، ويحتوي على Tests لكل Randomization Engine وProvider وPublic Facade وBuilder.

## 22.2 فئات الاختبارات

1. **Unit Tests:** كل Method في عزل.
2. **Boundary Tests:** القيم الدنيا والعليا وEmpty Inputs.
3. **Validation Tests:** مدخلات غير صالحة وUnknown Enums وInvalid Ranges.
4. **Reproducibility Tests:** نفس Seed ونفس العمليات ينتجان نفس النتائج.
5. **Isolation Tests:** تغيير استهلاك Provider لا يغير Streams مستقلة.
6. **Hierarchy Tests:** كل Location child تابع فعلًا للأب المطلوب.
7. **Public API Tests:** المستخدم يستطيع التعامل مع Public Facades دون الوصول إلى Internals.
8. **Integration Tests:** Definition كامل يملأ Class من خلال أكثر من Provider.
9. **Performance Tests:** عند تجهيز Benchmarks.
10. **Packaging Tests:** تجربة تثبيت NuGet package في مشروع مستقل.

## 22.3 قواعد الاختبارات

- لا تعتمد الاختبارات على ترتيب غير موثق في Dataset.
- لا تستخدم أسماء أو IDs ثابتة إلا عندما يكون الاختبار تحديدًا عن ذلك الـ ID وبعد التحقق من المصدر.
- لا تكتفِ باختبار `Assert.True(success)`؛ افحص القيمة والعلاقة والحدود.
- لا تخلط Tests للـ Internal Provider مع Tests للـ Public Facade دون تمييز.
- اجعل كل Test يوضح Behavior واحدًا قدر الإمكان.
- لا تستخدم نتائج عشوائية متوقعة Hardcoded إلا إذا كانت Golden Vector موثقة من مرجع موثوق أو من Specification واضحة.
- شغّل Build Release وTest suite كاملة بعد كل جزء.

## 22.4 الأوامر الأساسية

من جذر الـ Solution:

```powershell
dotnet build --configuration Release
dotnet test
```

# 23. الأداء واستهلاك الذاكرة

## 23.1 أهداف عامة

- لا تعِد قراءة JSON لكل Location operation.
- لا تنشئ Character Sets ثابتة في كل عملية Text generation.
- لا تستخدم Reflection في كل Property assignment إذا كان Setter Delegate قد تم تجهيزه عند تعريف Rule.
- لا تنشئ Random Source مشتركًا بين كل العناصر إذا كان ذلك يضر العزل وإعادة الإنتاج.
- لا تحتفظ بنتائج توليد كبيرة في الذاكرة عند استخدام Lazy generation.

## 23.2 Lazy Catalog

تحميل Location Catalog مرة واحدة باستخدام Lazy caching يقلل وقت كل عملية لاحقة. يجب أن تكون حالة فشل التحميل قابلة للتشخيص، وأن لا تخفي مشاكل غير متوقعة.

## 23.3 Generate(count) مقابل GenerateLazy(count)

- Generate(count): مريحة عندما يحتاج المستخدم Collection كاملة، لكنها تستهلك ذاكرة بحجم المجموعة.
- GenerateLazy(count): مناسبة للمعالجة التدريجية عندما لا يحتاج المستخدم كل العناصر في الذاكرة دفعة واحدة.

## 23.4 Benchmarks

قبل النشر، قارن على الأقل:

- توليد 1 Object.
- 1,000 Objects.
- 100,000 Objects حسب إمكانات الجهاز.
- أسماء فقط مقابل Object متعدد الخصائص.
- `Generate(count)` مقابل `GenerateLazy(count)`.
- Location lookup بعد أول Catalog load.
- تكلفة Rule Setter compiled مقارنة بآلية Reflection عند الحاجة.

يجب نشر أرقام حقيقية مع الجهاز والإصدار والإعدادات، لا تقديرات تسويقية.

# 24. الأمان والخصوصية وحدود الاستخدام

## 24.1 Fake Data ليست Security Data

Xoshiro256** وSeeded Random Streams مخصصة للبيانات الاختبارية. لا تستخدم المكتبة لتوليد:

- Passwords حقيقية.
- Access/Refresh Tokens.
- API Keys.
- Session IDs أمنية.
- Encryption keys.
- OTPs حقيقية لتوثيق المستخدمين.

## 24.2 Email

استخدام `.test` يمنع تقديم العناوين على أنها بريد حقيقي. لا تنفذ المكتبة SMTP أو تحقق ملكية البريد.

## 24.3 Phone Numbers وNational IDs

النطاق الحالي يتجنب توليد أرقام هواتف حقيقية أو أرقام قومية قابلة للاستخدام. أي دعم مستقبلي يحتاج قواعد واضحة لمنع التصادم مع بيانات أشخاص حقيقيين والالتزام بالقوانين.

## 24.4 Synthetic Datasets

الأسماء والعينات الاصطناعية لا يجب وصفها بأنها تمثل التوزيع السكاني الحقيقي إلا إذا كان هناك مصدر بيانات مرخص وممثل إحصائيًا.

## 24.5 User Callbacks

المستخدم يكتب Factory وRule callbacks. المكتبة لا تتحكم في Side Effects التي تنفذها هذه الدوال، مثل الكتابة في File أو Database أو قراءة الوقت أو استخدام Random خارجي.

# 25. الترخيص وحقوق البيانات

## 25.1 ترخيص الكود

الترخيص المقترح سابقًا للمكتبة هو MIT، لكن يجب تأكيد الاختيار النهائي قبل النشر وإضافة ملف `LICENSE` إلى Root repository. لا تعتبر الترخيص نهائيًا حتى يُضاف للريبو ويُراجع.

## 25.2 ترخيص Dataset مصر

المصدر الحالي يذكر أن Dataset متاحة تحت **Creative Commons Attribution 4.0 International (CC BY 4.0)**.

رابط المصدر:

- https://github.com/open-admin-data/egypt-administrative-divisions
- https://creativecommons.org/licenses/by/4.0/

يجب:

- ذكر اسم Dataset ومصدرها.
- توفير رابط الترخيص.
- توضيح أي تغييرات على البيانات إن وجدت.
- عدم الإيحاء بأن المصدر يؤيد ArabFaker.NET أو يرعاها.
- مراجعة `CITATION.cff` وأي `NOTICE`/Attribution يحدده المصدر.
- الاحتفاظ بملف `THIRD-PARTY-NOTICES.md` منفصل عن ترخيص كود المكتبة.
- مراجعة ترخيص البيانات الفعلي قبل كل إصدار؛ فقد يتغير المصدر أو بياناته.

## 25.3 فصل License الكود عن License البيانات

قد يكون كود المكتبة MIT بينما Dataset لها CC BY 4.0. لا تضع كل المحتويات تحت ترخيص واحد دون مراجعة حقوق كل أصل مستخدم.

## 25.4 Attribution مبدئي مقترح

```markdown
## Egypt Administrative Divisions Dataset

Source: https://github.com/open-admin-data/egypt-administrative-divisions
License: Creative Commons Attribution 4.0 International (CC BY 4.0)
License URL: https://creativecommons.org/licenses/by/4.0/

The dataset is embedded in ArabFaker.NET as application data.
Any transformations or modifications should be described here.
The dataset provider does not endorse ArabFaker.NET.
```

يجب تعديل وصف Changes حسب ما يحدث فعليًا في الملفات؛ لا تكتب أنها غير معدلة إذا تم تحويلها أو تعديلها.

# 26. التوثيق وتجربة المطور

المكتبة تحتاج Documentation تجعل المطور يبدأ بسرعة ويفهم حدود كل Provider.

## 26.1 README.md

يحتوي على:

- تعريف قصير بالمشروع.
- Problem/Solution.
- المزايا الأساسية.
- Target Framework.
- Installation command بعد نشر NuGet.
- Quick Start.
- Seed example.
- Define/RuleFor example بعد اكتمال Part 11/12.
- قائمة Providers.
- مثال توليد Collection.
- أمثلة عربي/إنجليزي.
- حدود الاستخدام الأمني.
- License وThird-party data notices.
- حالة المشروع وروابط Contribution.

## 26.2 XML Documentation

كل Public Type وPublic Method وPublic Property يجب أن يحتوي على XML Documentation، بما في ذلك:

- حدود Range.
- معنى `maxExclusive`.
- هل Null مسموح؟
- Failure behavior.
- هل القيمة قابلة لإعادة الإنتاج؟
- هل البيانات Offline؟
- هل هناك حدود Locale؟

## 26.3 أمثلة منفصلة

مجلد `samples/` أو مشروع أمثلة لاحق يمكن أن يحتوي على:

- Basic values.
- Arabic names.
- Egyptian locations.
- Student-like user-defined model (في Example فقط، وليس Domain Class داخل المكتبة).
- Batch generation.
- Custom factory.
- Reproducibility with seed.

# 27. خطة التنفيذ الكاملة

الخطة الأساسية التي بُني عليها العمل تتكون من 13 جزءًا:

| Part | العنوان | النتيجة المطلوبة | الحالة حسب آخر تحديث |
|---:|---|---|---|
| 1 | Randomization Engine | PRNG وواجهات العشوائية الأساسية | مكتمل حسب تأكيد المستخدم |
| 2 | Seed & Random Streams | اشتقاق Seeds مستقرة وStreams مستقلة | مكتمل حسب تأكيد المستخدم |
| 3 | Generation Context & ID Generator | Context وUUID generation | مكتمل حسب تأكيد المستخدم |
| 4 | Names Provider | أسماء عربية وLatin | مكتمل مبدئيًا حسب تأكيد المستخدم |
| 5 | Internet Provider | Username وEmail اختباري | مكتمل حسب تأكيد المستخدم |
| 6 | Random Data Provider | الأرقام والاختيار وBoolean | مكتمل حسب تأكيد المستخدم |
| 7 | Dates Provider | DateOnly/DateTime/Date of Birth | مكتمل حسب تأكيد المستخدم |
| 8 | Locations Provider | بيانات مصر وعلاقاتها | مكتمل؛ تم إصلاح Embedded Resources والاختبارات نجحت |
| 9 | Text Provider | توليد نصوص ومجموعات أحرف | مكتمل حسب تأكيد المستخدم |
| 10 | Main Builder & Public API | Faker وPublic Generators | مكتمل حسب تأكيد المستخدم |
| 11 | Define<T> & RuleFor | تعريف أي Class وقواعد Properties | المرحلة الحالية؛ يلزم تثبيت Step 1 ومراجعة Step 2 |
| 12 | Generate / Generate(count) / GenerateLazy & Result Handling | تنفيذ التوليد وإرجاع النتائج | لم يبدأ حسب آخر تصحيح من المستخدم |
| 13 | Docs, Benchmarks, NuGet & Release | توثيق وقياس ونشر | لم يبدأ |

## 27.1 Part 11 — Define<T> & RuleFor

ينبغي تنفيذه بالترتيب:

### Step 1 — Define<T>()

- `Faker.Define<T>()` مع `where T : class, new()`.
- `Faker.Define<T>(Func<T> factory)` لدعم Constructor مخصص.
- `FakerDefinition<T>`.
- Definition Scope مستقل لكل Definition.
- Definition IDs من نوع `ulong`.
- Error handling لإعداد Definition.
- اختبار Factory وInstance creation والـ Scope.

### Step 2 — RuleFor()

- Expression-based property selector.
- `Func<FakerContext, TProperty>` لتوليد القيمة.
- التحقق من Direct public writable property.
- إنشاء Compiled Setter Delegate.
- تخزين Rules وعدم تنفيذ Value Factory وقت الإعداد.
- Duplicate Rule behavior.
- رفض Nested/Computed/Indexer في النسخة الأولى.
- الاختبارات.

### Step 3 — Advanced Mapping / Final Validation

- مراجعة Nullability.
- مراجعة نوع الـ Properties.
- التحقق من ترتيب القواعد واستبدال Duplicate Rules.
- توضيح دعم/عدم دعم `init` وprivate setters.
- مراجعة custom Factory behavior.
- تجميد Definition قبل الاستخدام أو منع تغييرها أثناء التوليد.
- قرار هل يدعم Fields أو Nested Objects لاحقًا.
- إتمام الاختبارات وتوثيق القيود.

**لا تدمج Part 12 في Part 11.** Part 11 مسؤول عن Definition وMapping فقط.

## 27.2 Part 12 — Generation Engine

### Step 1 — GenerationResult وGenerationError

- Result model.
- Error codes.
- Nullability.
- Read-only errors.
- Success/Failure factories داخلية.

### Step 2 — Generate()

- تخصيص Item Index (`ulong`).
- إنشاء instance.
- إنشاء Per-Item Context.
- تطبيق Rules.
- إرجاع Result.
- سياسة واضحة لفشل Factory أو Rule.

### Step 3 — Generate(count)

- التحقق من Count.
- توليد عدد ثابت من Objects.
- تحديد نوع Return.
- تحديد All-or-nothing أو Partial result.
- اختبارات الأحجام والحدود والفشل.

### Step 4 — GenerateLazy(count)

- Lazy enumeration.
- عدم تخزين Collection كاملة.
- تحديد سلوك إعادة Enumeration.
- تحديد سلوك Errors أثناء Enumeration.
- قياس الذاكرة.

### Step 5 — Determinism & Concurrency Review

- ثبات Index allocation.
- استقلال Streams.
- Thread-safety.
- لا تقدم ضمانات Parallel Determinism غير مختبرة.

## 27.3 Part 13 — Docs, Benchmarks, NuGet & Release

1. مراجعة Public API.
2. مراجعة Naming وXML docs.
3. README كامل.
4. Quick-start/sample projects.
5. License وThird-party notices.
6. Changelog وVersioning.
7. Benchmarks.
8. NuGet package metadata.
9. Pack وتجربة تثبيت محلية.
10. CI build/test.
11. مراجعة Dependencies وPackage contents.
12. نشر Preview/Release بعد موافقة صاحب المشروع.

# 28. حالة التنفيذ الحالية

## 28.1 ما تم تأكيد نجاحه

حسب تأكيدات المستخدم في جلسات العمل:

- Randomization Engine.
- Seed & Random Streams.
- Generation Context وID Generator.
- Names Provider.
- Internet Provider.
- Random Data Provider.
- Dates Provider.
- Locations Provider بعد إصلاح إعدادات Embedded Resources في `.csproj`.
- Text Provider.
- Main Faker/Public Generators، بما يشمل Facades الخاصة بـ Text وNames وInternet وDates وLocations وRandomData وIdentifiers.
- اختبار Locations الشامل أصبح ناجحًا بعد استخدام `LogicalName` صريح لكل ملف JSON.

## 28.2 حالة Part 11

صاحب المشروع أكد أن أول 10 Parts انتهت، وأن نقطة الاستكمال هي:

```text
Part 11 — Define<T>() & RuleFor()
```

تم تقديم أكثر من نسخة/مراجعة للكود الخاص بـ `Define<T>()` و`FakerDefinition<T>`، كما تم تقديم كود `RuleFor()` باستخدام Expressions. لكن ملاحظة مراجعة 6 نوفمبر 2026: لا يوجد `FakerDefinition.cs` ولا `Define<T>()` في الريبو (`Faker.cs` يحتوي فقط على Seed وCreateContext وCreatePublicContext). لا توجد في آخر تحديث إفادة صريحة تؤكد أن اختبارات Part 11 الحالية نجحت بالكامل أو أن الكود الموجود محليًا يطابق آخر نسخة.

لذلك الحالة الآمنة هي:

- لا تعتبر Part 11 مكتملًا.
- افحص ملفات المستخدم الحالية قبل إرسال Replacement كامل.
- لا تفترض أن Step 1 أو Step 2 تم دمجهما بنجاح لمجرد أنهما أُرسلا في المحادثة.
- ابدأ من الملفات الموجودة فعليًا، وحدد ما ينقصها فقط.

## 28.3 Part 12

لم يبدأ بعد وفق تصحيح صاحب المشروع. لا تعتبر أي كود GenerationResult/Generate تمت مشاركته في رد سابق جزءًا معتمدًا من المشروع إلا إذا أكد المستخدم أنه طبقه واختبره. البداية الرسمية تكون بعد إغلاق Part 11.

# 29. قرارات مؤجلة ومخاطر يجب مراجعتها

هذه النقاط لا تعني أن المشروع فاشل؛ لكنها أمور مهمة قبل نشر مكتبة عامة.

## 29.1 Decimal Range Overflow

طريقة `TryNextDecimal` الأولية التي تعتمد على interpolation باستخدام `NextDouble` قد تواجه Overflow أو سلوكًا غير دقيق عند القيم القصوى. يجب كتابة Tests تشمل:

- `decimal.MinValue`.
- `decimal.MaxValue`.
- نطاق يعبر الصفر.
- نطاق صغير جدًا.
- Decimal Places من 0 إلى 28.
- Rounding boundaries.

يجب اختيار Algorithm موثوق وتوثيق هل التوزيع Uniform تقريبًا أم لا.

## 29.2 Exception from User Delegates

`Factory` و`RuleFor` يشغّلان كود المستخدم. يجب اتخاذ قرار نهائي: هل Exceptions التي يكتبها المستخدم تمر كما هي، أم يتم تحويل قائمة محددة منها إلى GenerationError؟ تجنب catch عام.

## 29.3 Thread Safety of Definitions

الـ Definition يحتوي على Rules وFactories. ينبغي تحديد نقطة يصبح بعدها Immutable/Ready for Generation، كي لا يحدث تعديل بالتزامن مع Generate.

## 29.4 Stable Rule Order

لا تجعل صحة التوليد تعتمد على آثار جانبية بين Value Factories. إذا كان ترتيب التنفيذ جزءًا من API، احتفظ بقائمة مرتبة للقواعد، مع فهرس لتفادي تكرار Property.

## 29.5 Reproducibility Across Versions

تغيير dataset أو ترتيبها أو PRNG أو Hash derivation قد يغير النتائج. يجب أن تكون سياسة Versioning واضحة: هل نضمن reproducibility داخل نفس إصدار الحزمة فقط، أم بين إصدارات معينة؟ لا تعد بضمان غير مختبر.

## 29.6 Native AOT وTrimming

`Expression.Compile()` مناسب لمسار .NET العادي، لكن إذا أراد المشروع دعم Native AOT أو Trimming بشكل قوي، يجب اختبار ذلك وتقييم البدائل. لا تعلن دعم AOT دون Tests.

## 29.7 Names Dataset Quality

القوائم الأولية قصيرة نسبيًا. قبل إصدار عام، يجب تحسين التنوع والتهجئة والترجمة والتوثيق وحقوق أي بيانات خارجية.

## 29.8 Multiple Arab Locales

تصميم أسماء المواقع الإدارية يختلف بين الدول. لا تجعل `Governorate/District/Shiyakha` Schema إجباريًا لكل الدول. Egypt adapter يمكنه استخدام هذه المستويات، والبلدان الأخرى قد تحتاج Labels/Levels مختلفة.

## 29.9 NuGet Identity

اسم المشروع الحالي `ArabFaker.NET`، لكن يجب التحقق من توفر Package ID على NuGet، ومن الاسم التجاري/التعارضات قبل النشر. لا تعتبر توفر الاسم مؤكدًا حتى يتم فحصه مباشرة.

## 29.10 Project/Assembly/Namespace Alignment

لا تغيّر أسماء Project أو Solution أو Namespaces لمجرد توحيد شكلي. يجب معرفة أثر ذلك على Assembly name وPackage ID والـ Namespaces والـ Test references أولًا.

# 30. معايير اعتبار المشروع جاهزًا للنشر

## Code Quality

- Build نظيف في Release.
- Tests كاملة ناجحة.
- Nullable warnings مراجعة.
- XML documentation للمكونات العامة.
- لا يوجد TODO غير موثق في Critical path.
- لا يوجد استخدام مقصود لـ `throw` كـ Validation flow.
- لا يوجد catch عام يبتلع الأخطاء.
- Public API صغيرة وواضحة ومستقرة.

## Functional Quality

- كل Provider موثق ومختبر.
- Seed behavior موثق.
- Range semantics موثقة.
- Null semantics موثقة.
- Locations hierarchy سليمة.
- Data attribution موجودة.
- Generate single/batch/lazy مكتملة.

## Package Quality

- Package ID محقق منه.
- Version محدد.
- License وReadme وRepository URL وDescription وTags موجودة.
- NuGet package لا يحتوي على Test artifacts أو ملفات غير مقصودة.
- Embedded Resources موجودة في Package.
- تجربة Install في Console App نظيف.
- Package dependencies سليمة.

## Documentation Quality

- README يوضح الهدف وما لا يفعله المشروع.
- أمثلة كاملة قابلة للنسخ والتشغيل.
- توضيح أن البيانات Fake وليست Security-grade.
- توضيح Attribution وLicensing.
- توثيق قيود الـ Mapping.

# 31. قاموس المصطلحات

| المصطلح | المعنى داخل المشروع |
|---|---|
| Fake Data | بيانات مصطنعة للاختبار والتطوير |
| Synthetic Data | بيانات مولّدة وليست سجلات أفراد حقيقيين |
| Provider | كلاس داخلي متخصص في توليد نوع محدد من البيانات |
| Generator | Public Facade يتيح للمستخدم الوصول إلى قدرات Provider |
| Faker | نقطة الدخول الرئيسية للمكتبة |
| FakerContext | واجهة الـ Generators المتاحة أثناء توليد Object |
| GenerationContext | السياق الداخلي المرتبط بـ Seed/Streams/Item Index |
| Seed | قيمة بداية تسلسل العشوائية |
| Random Stream | تسلسل عشوائي مستقل لمجال أو Item معين |
| PRNG | Pseudo-Random Number Generator |
| Reproducibility | إمكانية إعادة إنتاج نفس النتائج |
| Definition | قواعد إنشاء Type معين |
| RuleFor | ربط Property بدالة توليد القيمة |
| Expression Tree | تمثيل قابل للفحص لتعبير C# |
| Factory | دالة تنشئ Object |
| Setter Delegate | دالة معيّنة لتحديث Property |
| Embedded Resource | ملف مضمّن داخل Assembly |
| Catalog | مصدر بيانات منظم داخل المكتبة |
| DTO | كائن لنقل بيانات عامة دون كشف Internal model |
| Result Pattern | تمثيل نجاح/فشل بقيمة وErrors بدل Exceptions كمسار عادي |
| Lazy Generation | توليد العناصر عند طلبها بدل تخزينها كلها مسبقًا |
| Item Index | رقم العنصر داخل Definition، نوعه `ulong` |
| Definition Scope | معرّف Stream مستقل لكل Definition |

# 32. مراجع وروابط

## المشروع ومصدر البيانات

- Open Admin Data — Egypt Administrative Divisions: https://github.com/open-admin-data/egypt-administrative-divisions
- Dataset overview: https://openadmindata.org/eg/
- Creative Commons Attribution 4.0: https://creativecommons.org/licenses/by/4.0/

## روابط تطوير عامة

- .NET documentation: https://learn.microsoft.com/dotnet/
- C# documentation: https://learn.microsoft.com/dotnet/csharp/
- System.Linq.Expressions: https://learn.microsoft.com/dotnet/api/system.linq.expressions
- System.Text.Json: https://learn.microsoft.com/dotnet/standard/serialization/system-text-json/overview
- NuGet documentation: https://learn.microsoft.com/nuget/
- xUnit: https://xunit.net/

> يجب التأكد من المصادر والتراخيص والإصدارات مرة أخرى عند تجهيز Release؛ هذه الوثيقة ليست بديلًا عن مراجعة ملفات المصدر والترخيص الأصلية.

# 33. تعليمات استكمال المشروع مع أي مساعد أو مساهم

هذا القسم مهم حتى لا تضيع قرارات المشروع عند الانتقال إلى محادثة جديدة أو العمل مع مساعد AI آخر.

## 33.1 طريقة العمل المتفق عليها

التنفيذ يكون خطوة صغيرة في كل مرة:

1. شرح الهدف من الخطوة ولماذا نحتاجها.
2. توضيح المسار الفعلي للملف داخل `ArabFaker.NET` أو `ArabFaker.Tests`.
3. تقديم الكود الكامل للملف أو التعديل المحدد بوضوح.
4. شرح القرارات المهمة، لا مجرد إرسال كود.
5. إضافة Unit Tests مناسبة.
6. المستخدم يشغّل `dotnet build --configuration Release` و`dotnet test`.
7. لا ننتقل للخطوة التالية حتى يرسل المستخدم نتيجة التنفيذ أو يطلب ذلك صراحة.

لا ترسل تنفيذ عدة أجزاء كبيرة دفعة واحدة. الهدف أن يفهم صاحب المشروع كل قرار ويكتب/يراجع الكود بنفسه، وليس نسخ مكتبة كاملة دون استيعابها.

## 33.2 حقائق لا يجوز نسيانها

- المشروع اسمه `ArabFaker.NET`.
- مشروع الاختبارات اسمه `ArabFaker.Tests`.
- الـ Solution اسمه `ArabFaker.slnx`.
- لا يوجد مجلد `src` في الهيكل الفعلي الحالي.
- Namespace الأساسي الفعلي في الكود هو `ArabFaker.NET` (تم التحقق من الريبو).
- `itemIndex` نوعه `ulong`، وليس `long`.
- `IdGenerator` الداخلي يحتوي على `NextGuid()`، وليس `TryGuid()`.
- Names وLocations data providers تظل Internal، والـ Public Facades منفصلة.
- Location JSON resources تحتاج LogicalName صريحًا كما في `ArabFaker.NET.csproj`.
- اختبارات الـ Locations كانت تفشل بسبب تضمين الموارد، وتم حلها بتعديل `.csproj`، وبعده أكد المستخدم نجاح الاختبارات.
- المستخدم لا يريد استخدام `throw` بأي شكل كآلية عادية للتحقق أو التحكم.
- لا تفترض أسماء Methods أو Types قبل مراجعة الكود الذي أرسله المستخدم.
- لا تستبدل ملفًا كاملًا دون توضيح ما إذا كان ذلك سيحذف تغييرات سابقة.
- لا تعتبر خطوة مكتملة إلا بعد تأكيد المستخدم أن الاختبارات نجحت.

## 33.3 نقطة الاستكمال الحالية

النقطة الحالية رسميًا:

```text
Part 11 — Define<T>() & RuleFor()
```

أول 10 Parts اعتبرها صاحب المشروع مكتملة.

العمل المطلوب حاليًا:

1. مراجعة الكود المحلي الحالي لـ `Faker.cs` و`FakerDefinition.cs` قبل أي تعديل جديد.
2. التأكد من اكتمال Step 1: `Define<T>()` وDefault/Custom Factory وScope.
3. إكمال Step 2: `RuleFor()` وExpression Tree validation.
4. تنفيذ/مراجعة Step 3: mapping restrictions وconfiguration validation وtest coverage.
5. إغلاق Part 11 بعد نجاح الاختبارات.
6. بعد ذلك فقط الانتقال إلى Part 12: `Generate()` ثم `Generate(count)` ثم `GenerateLazy()` وResult Handling.

لا تبدأ Part 12 على أساس أن Part 11 اكتمل تلقائيًا.

## 33.4 قواعد هندسية للمساهمين

- حافظ على حدود Public/Internal.
- لا تحوّل كل الـ Providers إلى Public لمجرد تسهيل الاستخدام.
- لا تستخدم Domain classes جاهزة داخل المكتبة.
- لا تضف قاعدة بيانات أو ORM.
- لا تضف Network calls وقت التوليد.
- لا تضف Dependencies دون سبب واضح.
- لا تستخدم PRNG غير مناسب وتصفه بأنه Cryptographically Secure.
- لا تستخدم catch عام لابتلاع الأخطاء.
- اختبر الحدود وInvalid Inputs وSeed reproducibility.
- افصل License البيانات عن License الكود.
- أي تغيير في الـ Public API يحتاج مراجعة أثره على التوافق والإصدارات.

---

## ملحوظة ختامية

هذه الوثيقة تجمع الرؤية والقرارات والتصميم والخطة وحالة التنفيذ وفق المعلومات المتاحة حتى **5 نوفمبر 2026**. في أي تعارض بين هذه الوثيقة والكود الفعلي الموجود على جهاز صاحب المشروع، يجب فحص الكود والاختبارات أولًا، ثم تحديث الوثيقة لتعكس التنفيذ الحقيقي. الوثيقة مرجع حي (Living Document)، وليست بديلًا عن مراجعة الـ Source Code أو الـ Dataset الأصلية.
