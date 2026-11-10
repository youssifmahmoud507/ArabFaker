using ArabFaker.NET;
using ArabFaker.NET.Export;
using ArabFaker.NET.Generation;
using ArabFaker.NET.Providers.Arab.Names;
using System.Text;

// Usage: dotnet run --project samples/ArabFaker.QuickStart -- [--latin] [--dialect sqlserver|standard|mysql] [--out <folder>]
//   --latin     names in English letters (Mohamed Ahmed) instead of Arabic script
//   --dialect   SQL flavour of dataset.sql (default: sqlserver)
//   --out       where to write the files (default: ./quickstart-output)

Console.OutputEncoding = Encoding.UTF8;

var language = NameLanguage.Arabic;
var dialect = SqlDialect.SqlServer;
string outputFolder = Path.Combine(Directory.GetCurrentDirectory(), "quickstart-output");

for (int i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--latin":
            language = NameLanguage.Latin;
            break;

        case "--dialect" when i + 1 < args.Length:
            string requested = args[++i];

            if (requested.Equals("sqlserver", StringComparison.OrdinalIgnoreCase))
                dialect = SqlDialect.SqlServer;
            else if (requested.Equals("standard", StringComparison.OrdinalIgnoreCase))
                dialect = SqlDialect.Standard;
            else if (requested.Equals("mysql", StringComparison.OrdinalIgnoreCase))
                dialect = SqlDialect.MySql;
            else
                return Fail($"Unknown dialect '{requested}'. Use sqlserver, standard or mysql.");

            break;

        case "--out" when i + 1 < args.Length:
            outputFolder = Path.GetFullPath(args[++i]);
            break;

        default:
            return Fail($"Unknown argument '{args[i]}'.");
    }
}

var autoFill = new AutoFillOptions { Language = language };
var faker = new Faker(seed: 2026);

// 1) One class, no relations: define, fill by convention, generate.
var people = faker.Define<Person>().AutoFill(autoFill).Generate(10);

if (!people.IsSuccess)
    return FailAll(people.Errors);

// 2) Related tables: every foreign key points to a row that exists.
var dataset = faker.CreateDataset();

var students = dataset.Table(faker.Define<Student>().AutoFill(autoFill), 8, x => x.Id);

var courses = dataset.Table(
    faker.Define<Course>()
        .AutoFill(autoFill)
        .RuleFor(x => x.Title, c => c.Reference.TryCourse(out string title, language) ? title : "Course"),
    5,
    x => x.Id);

dataset.Table(faker.Define<Enrollment>().AutoFill(autoFill), 20, x => x.Id)
    .ForeignKey(x => x.StudentId, students)
    .ForeignKey(x => x.CourseId, courses)
    .UniqueTogether(x => x.StudentId, x => x.CourseId);

var built = dataset.Build();

if (!built.IsSuccess)
    return FailAll(built.Errors);

// 3) Write everything as UTF-8 files, so the console font and code page do not matter.
Directory.CreateDirectory(outputFolder);

var utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
var utf8WithBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true); // Excel needs the BOM for Arabic CSV.

var files = new List<(string Name, GenerationResult<string> Content, Encoding Encoding)>
{
    ("people.csv", RowExporter.ToCsv(people.Value), utf8WithBom),
    ("people.json", RowExporter.ToJson(people.Value), utf8),
    ("dataset.json", built.Value.ToJson(), utf8),
    ("dataset.sql", built.Value.ToSql(new SqlExportOptions { Dialect = dialect }), utf8)
};

foreach (var (name, content, encoding) in files)
{
    if (!content.IsSuccess)
        return FailAll(content.Errors);

    string path = Path.Combine(outputFolder, name);

    File.WriteAllText(path, content.Value, encoding);
    Console.WriteLine(path);
}

Console.WriteLine();
Console.WriteLine($"Language: {language}, SQL dialect: {dialect}. Open the files in an editor that supports UTF-8 (for example VS Code).");

return 0;

static int Fail(string message)
{
    Console.Error.WriteLine(message);

    return 1;
}

static int FailAll(IEnumerable<GenerationError> errors)
{
    foreach (var error in errors)
        Console.Error.WriteLine($"{error.Code}: {error.Message}");

    return 1;
}

internal sealed class Person
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public DateOnly BirthDate { get; set; }
}

internal sealed class Student
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;
}

internal sealed class Course
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;
}

internal sealed class Enrollment
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public int CourseId { get; set; }
}
