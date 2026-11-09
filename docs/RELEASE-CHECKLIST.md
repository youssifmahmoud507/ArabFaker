# قائمة مراجعة الإصدار — ArabFaker.NET

هذه القائمة لصاحب المشروع. كل بند يحتاج تشغيلًا فعليًا على جهازك أو على GitHub، لأن بيئة الكتابة لا تستطيع تشغيل `dotnet`. **لا تنشر شيئًا على NuGet قبل إتمام كل البنود.**

## 1. الجودة التقنية

- [ ] `dotnet build -c Release` ينجح. راجع التحذيرات (Nullable وXML docs `CS1591` و`CS1574`) وأصلح ما يخص الكود العام.
- [ ] `dotnet test -c Release` ينجح بالكامل (على Windows وLinux عبر CI).
- [ ] تبويب Actions في GitHub: الـ Workflow `CI` أخضر على آخر `master`.
- [ ] `dotnet run --project samples/ArabFaker.QuickStart` يطبع CSV وسكريبت SQL ويخرج بكود 0.

## 2. الحزمة (Package)

```
dotnet pack ArabFaker.NET/ArabFaker.NET.csproj -c Release -o artifacts
```

- [ ] داخل `artifacts/*.nupkg` (افتحه كـ zip): `README.md` و`LICENSE.txt` و`THIRD-PARTY-NOTICES.md` و`lib/net10.0/ArabFaker.NET.dll` و`lib/net10.0/ArabFaker.NET.xml`، ولا شيء من الاختبارات.
- [ ] ملفات الـ JSON للمواقع مضمنة داخل الـ DLL كـ Embedded Resources (جرّب الخطوة التالية للتأكد).
- [ ] **تجربة تثبيت في مشروع نظيف:**

```
mkdir PackageTest && cd PackageTest
dotnet new console -n PackageTest && cd PackageTest
dotnet add package ArabFaker.NET --version 0.1.0-preview.1 --source ../../artifacts
```

  ثم استخدم `new Faker(1).Locations` وولّد محافظة للتأكد أن البيانات المضمنة تعمل من الحزمة.
- [ ] لا توجد Dependencies غير مقصودة في `.nuspec` (المكتبة بلا اعتماديات خارجية).

## 3. الهوية والتراخيص

- [ ] **اسم الحزمة `ArabFaker.NET` متاح على nuget.org** (ابحث عنه، ولا تفترض). إن لم يكن متاحًا غيّر `PackageId` فقط، ولا تغيّر Namespaces.
- [ ] `Authors` و`Description` و`PackageTags` و`RepositoryUrl` في `ArabFaker.NET.csproj` صحيحة.
- [ ] الترخيص MIT للكود، والبيانات المضمنة CC BY 4.0 مع ذكر المصدر في `THIRD-PARTY-NOTICES.md` (موجود). تحقق من ترخيص مصدر البيانات وعدد السجلات (27 محافظة، 365 مركز/قسم، 5716 شياخة) قبل كل إصدار.
- [ ] (اختياري) إرسال رسالة شكر لمالك بيانات Open Admin Data عبر قناة تواصل عامة (Issue في الريبو) — ليس شرطًا قانونيًا لكن الإسناد (Attribution) شرط.

## 4. الـ API والتوثيق

- [ ] راجع `docs/PublicAPI.md` وقارنه بالإصدار السابق قبل كل نشر.
- [ ] `README.md` و`CHANGELOG.md` محدثان، ورقم الإصدار في `ArabFaker.NET.csproj` يطابق `CHANGELOG.md`.
- [ ] كل Public type موثق بـ XML (تم فحصه نصيًا: لا نقص في الأنواع العامة؛ الأنواع الداخلية خارج الفحص).

## 4.5 اختبار قاعدة بيانات حقيقية

- [ ] `dotnet run --project samples/ArabFaker.SqlServerDemo -- --server <your-server>`: كل الفحوص PASS بالعربية.
- [ ] نفس الأمر مع `--latin --seed 7`: كل الفحوص PASS.
- [ ] افتح `arabfaker-sqlserver-demo.sql` في SSMS وتأكد أن العربية تظهر صحيحة.
- [ ] `dotnet run --project samples/ArabFaker.DatabaseDemo` (SQLite، يعمل في CI أيضًا): كل الفحوص PASS.

## 5. النشر (بعد موافقتك فقط)

1. أنشئ Tag: `git tag v0.1.0-preview.1 && git push origin v0.1.0-preview.1`.
2. أنشئ API key من nuget.org (Scope: Push، لحزمة `ArabFaker.NET` فقط).
3. `dotnet nuget push artifacts/ArabFaker.NET.0.1.0-preview.1.nupkg --api-key <KEY> --source https://api.nuget.org/v3/index.json`
4. أنشئ GitHub Release من نفس الـ Tag وارفق ملاحظات `CHANGELOG.md`.
5. بعد النشر: ثبّت الحزمة من nuget.org في مشروع نظيف وأعد تجربة البند 2.

> لا تضع مفتاح NuGet في الكود أو في ملفات الريبو. استخدم GitHub Secrets إن أضفت Workflow للنشر لاحقًا.
