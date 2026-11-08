using ArabFaker.NET;
using ArabFaker.NET.Dataset;
using ArabFaker.NET.Generation;

namespace ArabFaker.Tests.Dataset
{
    public class NavigationTests
    {
        private sealed record School(
            DatasetBuilder Dataset,
            DatasetTable<NvStudent, int> Students,
            DatasetTable<NvCourse, int> Courses,
            DatasetTable<NvEnrollment, int> Enrollments);

        private static School CreateSchool(Faker faker, int students, int courses, int enrollments, bool navigations = true)
        {
            var dataset = faker.CreateDataset();

            var studentTable = dataset.Table(faker.Define<NvStudent>().AutoFill(), students, x => x.Id);
            var courseTable = dataset.Table(faker.Define<NvCourse>().AutoFill(), courses, x => x.Id);
            var enrollmentTable = dataset.Table(faker.Define<NvEnrollment>().AutoFill(), enrollments, x => x.Id)
                .ForeignKey(x => x.StudentId, studentTable)
                .ForeignKey(x => x.CourseId, courseTable);

            if (navigations)
            {
                enrollmentTable
                    .Navigation(x => x.StudentId, x => x.Student)
                    .Navigation(x => x.CourseId, x => x.Course);

                studentTable.Collection(x => x.Enrollments, enrollmentTable, e => e.StudentId);
                courseTable.Collection(x => x.Enrollments, enrollmentTable, e => e.CourseId);
            }

            return new School(dataset, studentTable, courseTable, enrollmentTable);
        }

        private static string Codes(DatasetBuilder dataset) =>
            string.Join(",", dataset.ConfigurationErrors.Select(e => e.Code));

        // ---------- Reference navigations ----------

        [Fact]
        public void Navigation_ShouldSetTheSameParentInstanceTheForeignKeyPointsTo()
        {
            var school = CreateSchool(new Faker(2026), 10, 5, 80);

            var data = school.Dataset.Build().Value!;

            var students = data.Rows(school.Students);
            var courses = data.Rows(school.Courses);

            Assert.All(data.Rows(school.Enrollments), e =>
            {
                Assert.NotNull(e.Student);
                Assert.NotNull(e.Course);
                Assert.Equal(e.StudentId, e.Student.Id);
                Assert.Equal(e.CourseId, e.Course.Id);
                Assert.Contains(e.Student, students);
                Assert.Contains(e.Course, courses);
            });
        }

        [Fact]
        public void Navigation_NullForeignKey_ShouldGiveNullNavigation()
        {
            var faker = new Faker(2026);
            var dataset = faker.CreateDataset();

            var students = dataset.Table(faker.Define<NvStudent>().AutoFill(), 3, x => x.Id);
            var orders = dataset.Table(faker.Define<NvOrder>().AutoFill(), 100, x => x.Id)
                .ForeignKey(x => x.CouponId, students, nullChance: 0.5)
                .Navigation(x => x.CouponId, x => x.Coupon);

            var rows = dataset.Build().Value!.Rows(orders);

            Assert.Contains(rows, o => o.CouponId is null);
            Assert.All(rows.Where(o => o.CouponId is null), o => Assert.Null(o.Coupon));
            Assert.All(rows.Where(o => o.CouponId is not null), o => Assert.Equal(o.CouponId, o.Coupon!.Id));
        }

        // ---------- Collection navigations ----------

        [Fact]
        public void Collection_ShouldContainExactlyTheRowsThatReferenceTheParent()
        {
            var school = CreateSchool(new Faker(2026), 10, 5, 80);

            var data = school.Dataset.Build().Value!;

            var enrollments = data.Rows(school.Enrollments);

            foreach (var student in data.Rows(school.Students))
            {
                var expected = enrollments.Where(e => e.StudentId == student.Id).ToList();

                Assert.Equal(expected, student.Enrollments);
            }

            foreach (var course in data.Rows(school.Courses))
            {
                Assert.Equal(enrollments.Where(e => e.CourseId == course.Id), course.Enrollments);
            }

            Assert.Equal(80, data.Rows(school.Students).Sum(s => s.Enrollments.Count));
        }

        [Fact]
        public void Collection_ParentWithoutChildren_ShouldGetAnEmptyListNotNull()
        {
            var school = CreateSchool(new Faker(2026), 20, 3, 2);

            var students = school.Dataset.Build().Value!.Rows(school.Students);

            Assert.All(students, s => Assert.NotNull(s.Enrollments));
            Assert.Contains(students, s => s.Enrollments.Count == 0);
        }

        [Fact]
        public void Navigations_BothDirections_ShouldAgree()
        {
            var school = CreateSchool(new Faker(2026), 6, 4, 40);

            var students = school.Dataset.Build().Value!.Rows(school.Students);

            Assert.All(students, student =>
                Assert.All(student.Enrollments, e => Assert.Same(student, e.Student)));
        }

        // ---------- Hierarchies ----------

        [Fact]
        public void SelfReference_ManagerAndReports_ShouldBeConsistent()
        {
            var faker = new Faker(2026);
            var dataset = faker.CreateDataset();

            var employees = dataset.Table(faker.Define<NvEmployee>().AutoFill(), 40, x => x.Id)
                .SelfReference(x => x.ManagerId)
                .Navigation(x => x.ManagerId, x => x.Manager);

            employees.Collection(x => x.Reports, employees, x => x.ManagerId);

            var rows = dataset.Build().Value!.Rows(employees);

            Assert.Null(rows[0].Manager);
            Assert.All(rows.Skip(1), e => Assert.Equal(e.ManagerId, e.Manager!.Id));
            Assert.All(rows, manager => Assert.All(manager.Reports, report => Assert.Same(manager, report.Manager)));
            Assert.Equal(rows.Count - 1, rows.Sum(e => e.Reports.Count));
        }

        // ---------- Key types ----------

        [Fact]
        public void Navigations_WithGuidKeys_ShouldWork()
        {
            var faker = new Faker(2026);
            var dataset = faker.CreateDataset();

            var parents = dataset.Table(faker.Define<NvGuidParent>().AutoFill(), 4, x => x.Id);
            var children = dataset.Table(faker.Define<NvGuidChild>().AutoFill(), 30, x => x.Id)
                .ForeignKey(x => x.ParentId, parents)
                .Navigation(x => x.ParentId, x => x.Parent);

            parents.Collection(x => x.Children, children, c => c.ParentId);

            var data = dataset.Build().Value!;

            Assert.All(data.Rows(children), c => Assert.Equal(c.ParentId, c.Parent!.Id));
            Assert.Equal(30, data.Rows(parents).Sum(p => p.Children.Count));
        }

        // ---------- Navigations do not change the relations ----------

        [Fact]
        public void Navigations_ShouldNotChangeForeignKeyValues()
        {
            var with = CreateSchool(new Faker(9), 10, 10, 60, navigations: true);
            var without = CreateSchool(new Faker(9), 10, 10, 60, navigations: false);

            var a = with.Dataset.Build().Value!.Rows(with.Enrollments).Select(e => (e.StudentId, e.CourseId));
            var b = without.Dataset.Build().Value!.Rows(without.Enrollments).Select(e => (e.StudentId, e.CourseId));

            Assert.Equal(a, b);
        }

        [Fact]
        public void Navigations_WithoutNavigationSetup_ShouldLeavePropertiesAlone()
        {
            var school = CreateSchool(new Faker(2026), 3, 3, 5, navigations: false);

            var data = school.Dataset.Build().Value!;

            Assert.All(data.Rows(school.Enrollments), e => Assert.Null(e.Student));
            Assert.All(data.Rows(school.Students), s => Assert.Empty(s.Enrollments));
        }

        // ---------- Configuration errors ----------

        [Fact]
        public void Navigation_BeforeForeignKey_ShouldReportError()
        {
            var faker = new Faker(2026);
            var dataset = faker.CreateDataset();

            dataset.Table(faker.Define<NvEnrollment>().AutoFill(), 3, x => x.Id)
                .Navigation(x => x.StudentId, x => x.Student);

            Assert.Equal(ConfigurationErrorCodes.NavigationForeignKeyMissing, Assert.Single(dataset.ConfigurationErrors).Code);
            Assert.False(dataset.Build().IsSuccess);
        }

        [Fact]
        public void Navigation_WrongPropertyType_ShouldReportError()
        {
            var school = CreateSchool(new Faker(2026), 3, 3, 3, navigations: false);

            school.Enrollments.Navigation(x => x.StudentId, x => x.WrongTypeNavigation);

            Assert.Equal(ConfigurationErrorCodes.NavigationTypeMismatch, Assert.Single(school.Dataset.ConfigurationErrors).Code);
        }

        [Fact]
        public void Navigation_SameForeignKeyTwice_ShouldReportConflict()
        {
            var school = CreateSchool(new Faker(2026), 3, 3, 3, navigations: false);

            school.Enrollments
                .Navigation(x => x.StudentId, x => x.Student)
                .Navigation(x => x.StudentId, x => x.Student);

            Assert.Contains(ConfigurationErrorCodes.NavigationPropertyConflict, Codes(school.Dataset));
        }

        [Fact]
        public void Navigation_OnAForeignKeyProperty_ShouldReportConflict()
        {
            var school = CreateSchool(new Faker(2026), 3, 3, 3, navigations: false);

            // StudentId is already a foreign key, so it cannot also be the navigation property.
            school.Enrollments.Navigation(x => x.CourseId, x => x.StudentId);

            Assert.NotEmpty(school.Dataset.ConfigurationErrors);
        }

        [Fact]
        public void Collection_UnsupportedCollectionType_ShouldReportError()
        {
            var school = CreateSchool(new Faker(2026), 3, 3, 3, navigations: false);

            school.Courses.Collection(x => x.UnsupportedSet, school.Enrollments, e => e.CourseId);

            Assert.Equal(ConfigurationErrorCodes.NavigationTypeMismatch, Assert.Single(school.Dataset.ConfigurationErrors).Code);
        }

        [Fact]
        public void Collection_ForeignKeyPointingToAnotherTable_ShouldReportError()
        {
            var school = CreateSchool(new Faker(2026), 3, 3, 3, navigations: false);

            // Enrollment.StudentId references students, not courses.
            school.Courses.Collection(x => x.Enrollments, school.Enrollments, e => e.StudentId);

            Assert.Equal(ConfigurationErrorCodes.NavigationTargetInvalid, Assert.Single(school.Dataset.ConfigurationErrors).Code);
        }

        [Fact]
        public void Collection_ForeignKeyNotRegistered_ShouldReportError()
        {
            var faker = new Faker(2026);
            var dataset = faker.CreateDataset();

            var students = dataset.Table(faker.Define<NvStudent>().AutoFill(), 3, x => x.Id);
            var enrollments = dataset.Table(faker.Define<NvEnrollment>().AutoFill(), 3, x => x.Id);

            students.Collection(x => x.Enrollments, enrollments, e => e.StudentId);

            Assert.Equal(ConfigurationErrorCodes.NavigationForeignKeyMissing, Assert.Single(dataset.ConfigurationErrors).Code);
        }

        [Fact]
        public void Collection_ChildFromAnotherDataset_ShouldReportError()
        {
            var school = CreateSchool(new Faker(2026), 3, 3, 3, navigations: false);
            var other = CreateSchool(new Faker(2026), 3, 3, 3, navigations: false);

            school.Students.Collection(x => x.Enrollments, other.Enrollments, e => e.StudentId);

            Assert.Contains(ConfigurationErrorCodes.NavigationTargetInvalid, Codes(school.Dataset));
        }

        [Fact]
        public void Collection_SamePropertyTwice_ShouldReportConflict()
        {
            var school = CreateSchool(new Faker(2026), 3, 3, 3, navigations: false);

            school.Students
                .Collection(x => x.Enrollments, school.Enrollments, e => e.StudentId)
                .Collection(x => x.Enrollments, school.Enrollments, e => e.StudentId);

            Assert.Equal(ConfigurationErrorCodes.NavigationPropertyConflict, Assert.Single(school.Dataset.ConfigurationErrors).Code);
        }

        [Fact]
        public void Collection_OnInvalidChildTable_ShouldStayQuiet()
        {
            var faker = new Faker(2026);
            var dataset = faker.CreateDataset();

            var students = dataset.Table(faker.Define<NvStudent>().AutoFill(), 3, x => x.Id);
            var broken = dataset.Table<NvEnrollment, int>(null!, 3, x => x.Id);

            students.Collection(x => x.Enrollments, broken, e => e.StudentId);

            Assert.Equal(ConfigurationErrorCodes.NullDefinition, Assert.Single(dataset.ConfigurationErrors).Code);
        }
    }
}
