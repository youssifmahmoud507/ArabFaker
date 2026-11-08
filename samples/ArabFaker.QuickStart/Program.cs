using ArabFaker.NET;
using ArabFaker.NET.Export;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

var faker = new Faker(seed: 2026);

// 1) One class, no relations: define, fill by convention, generate.
var people = faker.Define<Person>().AutoFill().Generate(3);

if (!people.IsSuccess)
{
    foreach (var error in people.Errors)
        Console.Error.WriteLine($"{error.Code}: {error.Message}");

    return 1;
}

Console.WriteLine("People (CSV):");
Console.WriteLine(RowExporter.ToCsv(people.Value).Value);

// 2) Related tables: every foreign key points to a row that exists.
var dataset = faker.CreateDataset();

var students = dataset.Table(faker.Define<Student>().AutoFill(), 5, x => x.Id);

var courses = dataset.Table(
    faker.Define<Course>()
        .AutoFill()
        .RuleFor(x => x.Title, c => c.RandomData.TryPick(new[] { "Algebra", "Physics", "Databases" }, out var title) ? title : "Course"),
    3,
    x => x.Id);

dataset.Table(faker.Define<Enrollment>().AutoFill(), 8, x => x.Id)
    .ForeignKey(x => x.StudentId, students)
    .ForeignKey(x => x.CourseId, courses)
    .UniqueTogether(x => x.StudentId, x => x.CourseId);

var built = dataset.Build();

if (!built.IsSuccess)
{
    foreach (var error in built.Errors)
        Console.Error.WriteLine($"{error.Code}: {error.Message}");

    return 1;
}

Console.WriteLine("Dataset (SQL Server INSERT script, referenced tables first):");
Console.WriteLine(built.Value.ToSql(new SqlExportOptions { Dialect = SqlDialect.SqlServer }).Value);

return 0;

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
