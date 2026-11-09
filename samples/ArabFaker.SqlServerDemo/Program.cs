// A real SQL Server check of ArabFaker.NET.
//
// It builds a small company and a small university with ArabFaker, creates a database with real
// constraints (primary keys, foreign keys, UNIQUE, CHECK), loads the generated SQL Server INSERT script
// (N'..' Arabic text, [identifiers]) into it, then asks SQL Server itself whether the data is sound.
//
// Usage:
//   dotnet run --project samples/ArabFaker.SqlServerDemo -- [--server <name>] [--connection <string>]
//                                                          [--database ArabFakerDemo] [--latin] [--seed 2026]
//
//   --server      server or instance, for example localhost or .\SQLEXPRESS or (localdb)\MSSQLLocalDB.
//                 Uses Windows authentication. Default: localhost
//   --connection  a full connection string instead of --server
//   --database    name of the demo database. It is DROPPED and recreated, so the name must start with
//                 "ArabFaker". Default: ArabFakerDemo

using ArabFaker.NET;
using ArabFaker.NET.Export;
using ArabFaker.NET.Generation;
using ArabFaker.NET.Providers.Arab.Names;
using Microsoft.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;

Console.OutputEncoding = Encoding.UTF8;

long seed = 2026;
var language = NameLanguage.Arabic;
string server = "localhost";
string? fullConnection = null;
string database = "ArabFakerDemo";

for (int i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--latin":
            language = NameLanguage.Latin;
            break;

        case "--seed" when i + 1 < args.Length && long.TryParse(args[i + 1], out long parsed):
            seed = parsed;
            i++;
            break;

        case "--server" when i + 1 < args.Length:
            server = args[++i];
            break;

        case "--connection" when i + 1 < args.Length:
            fullConnection = args[++i];
            break;

        case "--database" when i + 1 < args.Length:
            database = args[++i];
            break;

        default:
            Console.Error.WriteLine($"Unknown argument '{args[i]}'.");

            return 2;
    }
}

// The database name goes into DROP/CREATE statements, so keep it strict and recognisable as a demo.
if (!Regex.IsMatch(database, "^ArabFaker[A-Za-z0-9_]*$"))
{
    Console.Error.WriteLine("The demo database is dropped and recreated, so its name must start with 'ArabFaker' and contain only letters, digits and underscores.");

    return 2;
}

var builder = fullConnection is null
    ? new SqlConnectionStringBuilder
    {
        DataSource = server,
        IntegratedSecurity = true,
        TrustServerCertificate = true,
        ConnectTimeout = 15
    }
    : new SqlConnectionStringBuilder(fullConnection);

builder.InitialCatalog = "master";

string masterConnection = builder.ConnectionString;

builder.InitialCatalog = database;

string demoConnection = builder.ConnectionString;

// 1) Generate. The same seed must give the same script.
var (demo, errors) = BuildDemo(seed, language);

if (demo is null)
{
    foreach (var error in errors)
        Console.Error.WriteLine($"{error.Code}: {error.Message}");

    return 1;
}

var (again, _) = BuildDemo(seed, language);

// 2) Create a fresh database.
try
{
    using var master = new SqlConnection(masterConnection);

    master.Open();

    using var create = master.CreateCommand();

    create.CommandText = $"""
        IF DB_ID(N'{database}') IS NOT NULL
        BEGIN
            ALTER DATABASE [{database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
            DROP DATABASE [{database}];
        END;
        CREATE DATABASE [{database}];
        """;
    create.ExecuteNonQuery();
}
catch (SqlException exception)
{
    Console.Error.WriteLine($"Could not create the database on '{builder.DataSource}': {exception.Message}");
    Console.Error.WriteLine("Check that SQL Server is running and pass --server <name> (for example .\\SQLEXPRESS) or --connection <string>.");

    return 1;
}

// A script you can also open in SSMS. UTF-8 with a BOM so SSMS shows the Arabic correctly.
string scriptPath = Path.GetFullPath("arabfaker-sqlserver-demo.sql");

File.WriteAllText(
    scriptPath,
    $"USE [{database}];\nGO\n{DemoSchema.Sql}\nGO\n{demo.Sql}",
    new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

using var connection = new SqlConnection(demoConnection);

connection.Open();

Execute(DemoSchema.Sql);

// 3) Load the generated script in one transaction.
using (var transaction = connection.BeginTransaction())
{
    try
    {
        using var command = connection.CreateCommand();

        command.Transaction = transaction;
        command.CommandText = demo.Sql;
        command.ExecuteNonQuery();

        transaction.Commit();
    }
    catch (SqlException exception)
    {
        transaction.Rollback();
        Console.Error.WriteLine($"SQL Server rejected the generated script: {exception.Message}");

        return 1;
    }
}

// 4) Ask SQL Server.
var checks = new List<(string Name, bool Passed)>
{
    ("Same seed gives the same script", again is not null && again.Sql == demo.Sql),
    ("Employee rows loaded", Scalar("SELECT COUNT(*) FROM Employee") == demo.EmployeeCount),
    ("Student rows loaded", Scalar("SELECT COUNT(*) FROM Student") == demo.StudentCount),
    ("Course rows loaded", Scalar("SELECT COUNT(*) FROM Course") == demo.CourseCount),
    ("Enrollment rows loaded", Scalar("SELECT COUNT(*) FROM Enrollment") == demo.EnrollmentCount),
    ("Foreign keys are enabled and trusted", Scalar("SELECT COUNT(*) FROM sys.foreign_keys WHERE is_disabled = 1 OR is_not_trusted = 1") == 0),
    ("No constraint violations (DBCC CHECKCONSTRAINTS)", Rows("DBCC CHECKCONSTRAINTS WITH ALL_CONSTRAINTS").Count == 0),
    ("Every employee manager exists", Scalar("SELECT COUNT(*) FROM Employee e WHERE e.ManagerId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Employee m WHERE m.Id = e.ManagerId)") == 0),
    ("Some employees are top-level", Scalar("SELECT COUNT(*) FROM Employee WHERE ManagerId IS NULL") == demo.EmployeeRoots),
    ("National IDs are 14 digits", Scalar("SELECT COUNT(*) FROM Employee WHERE NationalId LIKE REPLICATE('[0-9]', 14)") == demo.EmployeeCount),
    ("No student enrolled twice in a course", Scalar("SELECT COUNT(*) FROM (SELECT StudentId, CourseId FROM Enrollment GROUP BY StudentId, CourseId HAVING COUNT(*) > 1) d") == 0),
    ("Unicode text survives the round trip", RoundTrips(demo.FirstStudent)),
    ("No empty text values", Scalar("SELECT COUNT(*) FROM Employee WHERE LTRIM(RTRIM(FullName)) = '' OR LTRIM(RTRIM(Department)) = '' OR LTRIM(RTRIM(JobTitle)) = ''") == 0),
};

// 5) Report.
Console.WriteLine($"Server:   {builder.DataSource}");
Console.WriteLine($"Database: {database}");
Console.WriteLine($"Script:   {scriptPath}");
Console.WriteLine($"Language: {language}, seed: {seed}");
Console.WriteLine();

Console.WriteLine("Employees and their managers:");

foreach (var row in Rows("SELECT TOP 6 e.FullName, e.JobTitle, e.Department, COALESCE(m.FullName, N'-') FROM Employee e LEFT JOIN Employee m ON m.Id = e.ManagerId ORDER BY e.Id"))
    Console.WriteLine($"  {row[0]} | {row[1]} | {row[2]} | manager: {row[3]}");

Console.WriteLine();
Console.WriteLine("Most popular courses:");

foreach (var row in Rows("SELECT TOP 5 c.Title, c.Department, COUNT(*) FROM Enrollment e JOIN Course c ON c.Id = e.CourseId GROUP BY c.Id, c.Title, c.Department ORDER BY COUNT(*) DESC, c.Id"))
    Console.WriteLine($"  {row[0]} ({row[1]}): {row[2]} students");

Console.WriteLine();
Console.WriteLine("Checks:");

foreach (var (name, passed) in checks)
    Console.WriteLine($"  [{(passed ? "PASS" : "FAIL")}] {name}");

int failed = checks.Count(x => !x.Passed);

Console.WriteLine();
Console.WriteLine(failed == 0 ? "All checks passed." : $"{failed} check(s) failed.");

return failed == 0 ? 0 : 1;

// ---------- helpers ----------

void Execute(string sql)
{
    using var command = connection.CreateCommand();

    command.CommandText = sql;
    command.ExecuteNonQuery();
}

long Scalar(string sql)
{
    using var command = connection.CreateCommand();

    command.CommandText = sql;

    return Convert.ToInt64(command.ExecuteScalar());
}

List<object?[]> Rows(string sql)
{
    var result = new List<object?[]>();

    using var command = connection.CreateCommand();

    command.CommandText = sql;

    using var reader = command.ExecuteReader();

    while (reader.Read())
    {
        var values = new object?[reader.FieldCount];

        for (int i = 0; i < values.Length; i++)
            values[i] = reader.IsDBNull(i) ? null : reader.GetValue(i);

        result.Add(values);
    }

    return result;
}

bool RoundTrips(Student student)
{
    using var command = connection.CreateCommand();

    command.CommandText = "SELECT FirstName, LastName, Email, University, Faculty FROM Student WHERE Id = @id";
    command.Parameters.AddWithValue("@id", student.Id);

    using var reader = command.ExecuteReader();

    return reader.Read()
        && reader.GetString(0) == student.FirstName
        && reader.GetString(1) == student.LastName
        && reader.GetString(2) == student.Email
        && reader.GetString(3) == student.University
        && reader.GetString(4) == student.Faculty;
}

static (DemoData? Data, IReadOnlyList<GenerationError> Errors) BuildDemo(long seed, NameLanguage language)
{
    var faker = new Faker(seed);
    var options = new AutoFillOptions { Language = language };
    var dataset = faker.CreateDataset();

    const int employeeCount = 60;
    const int employeeRoots = 3;

    // The job title must belong to the department written on the same row.
    dataset.Table(
        faker.Define<Employee>()
            .AutoFill(options)
            .RuleFor(
                x => x.JobTitle,
                (x, c) => c.Reference.TryJobTitleInDepartment(x.Department, out string title, language) ? title : string.Empty),
        employeeCount,
        x => x.Id)
        .SelfReference(x => x.ManagerId, roots: employeeRoots);

    var students = dataset.Table(faker.Define<Student>().AutoFill(options), 40, x => x.Id);

    // The course title must belong to the department written on the same row.
    var courses = dataset.Table(
        faker.Define<Course>()
            .AutoFill(options)
            .RuleFor(
                x => x.Title,
                (x, c) => c.Reference.TryCourseInDepartment(x.Department, out string title, language) ? title : string.Empty),
        12,
        x => x.Id);

    dataset.Table(
        faker.Define<Enrollment>()
            .AutoFill(options)
            .RuleFor(x => x.GradeRating, c => c.Reference.TryGradeRating(out string grade, language) ? grade : string.Empty),
        150,
        x => x.Id)
        .ForeignKey(x => x.StudentId, students)
        .ForeignKey(x => x.CourseId, courses)
        .UniqueTogether(x => x.StudentId, x => x.CourseId);

    var built = dataset.Build();

    if (!built.IsSuccess)
        return (null, built.Errors);

    var sql = built.Value!.ToSql(new SqlExportOptions { Dialect = SqlDialect.SqlServer });

    if (!sql.IsSuccess)
        return (null, sql.Errors);

    var data = new DemoData(
        sql.Value!,
        employeeCount,
        employeeRoots,
        40,
        12,
        150,
        built.Value.Rows(students)[0]);

    return (data, Array.Empty<GenerationError>());
}

// The tables are named after the row classes below, and the columns after their properties,
// because that is how ArabFaker names them in the INSERT script.
// Foreign keys, UNIQUE and CHECK are enforced by SQL Server itself.
internal static class DemoSchema
{
    internal const string Sql = """
        CREATE TABLE Employee (
            Id          INT           NOT NULL PRIMARY KEY,
            FullName    NVARCHAR(200) NOT NULL,
            Email       NVARCHAR(200) NOT NULL,
            NationalId  CHAR(14)      NOT NULL UNIQUE CHECK (NationalId NOT LIKE '%[^0-9]%'),
            Department  NVARCHAR(100) NOT NULL,
            JobTitle    NVARCHAR(150) NOT NULL,
            Salary      DECIMAL(18,2) NOT NULL CHECK (Salary > 0),
            HireDate    DATE          NOT NULL,
            ManagerId   INT           NULL REFERENCES Employee (Id)
        );

        CREATE TABLE Student (
            Id          INT           NOT NULL PRIMARY KEY,
            FirstName   NVARCHAR(100) NOT NULL,
            LastName    NVARCHAR(100) NOT NULL,
            Email       NVARCHAR(200) NOT NULL,
            NationalId  CHAR(14)      NOT NULL UNIQUE CHECK (NationalId NOT LIKE '%[^0-9]%'),
            BirthDate   DATE          NOT NULL,
            University  NVARCHAR(150) NOT NULL,
            Faculty     NVARCHAR(150) NOT NULL
        );

        CREATE TABLE Course (
            Id          INT           NOT NULL PRIMARY KEY,
            Title       NVARCHAR(150) NOT NULL,
            Department  NVARCHAR(100) NOT NULL
        );

        CREATE TABLE Enrollment (
            Id           INT          NOT NULL PRIMARY KEY,
            StudentId    INT          NOT NULL REFERENCES Student (Id),
            CourseId     INT          NOT NULL REFERENCES Course (Id),
            GradeRating  NVARCHAR(30) NOT NULL,
            CONSTRAINT UQ_Enrollment_StudentCourse UNIQUE (StudentId, CourseId)
        );
        """;
}

internal sealed record DemoData(
    string Sql,
    int EmployeeCount,
    int EmployeeRoots,
    int StudentCount,
    int CourseCount,
    int EnrollmentCount,
    Student FirstStudent);

internal sealed class Employee
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string NationalId { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public string JobTitle { get; set; } = string.Empty;

    public decimal Salary { get; set; }

    public DateOnly HireDate { get; set; }

    public int? ManagerId { get; set; }
}

internal sealed class Student
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string NationalId { get; set; } = string.Empty;

    public DateOnly BirthDate { get; set; }

    public string University { get; set; } = string.Empty;

    public string Faculty { get; set; } = string.Empty;
}

internal sealed class Course
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;
}

internal sealed class Enrollment
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public int CourseId { get; set; }

    public string GradeRating { get; set; } = string.Empty;
}
