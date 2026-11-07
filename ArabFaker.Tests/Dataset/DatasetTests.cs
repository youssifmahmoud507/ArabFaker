using ArabFaker.NET;
using ArabFaker.NET.Dataset;
using ArabFaker.NET.Generation;

namespace ArabFaker.Tests.Dataset
{
    public class DatasetTests
    {
        private sealed record University(
            DatasetBuilder Dataset,
            FakerDefinition<DsStudent> StudentDefinition,
            DatasetTable<DsStudent, int> Students,
            DatasetTable<DsCourse, int> Courses,
            DatasetTable<DsEnrollment, int> Enrollments);

        private static University CreateUniversity(
            Faker faker,
            int studentCount,
            int courseCount,
            int enrollmentCount,
            bool unique = false)
        {
            var dataset = faker.CreateDataset();

            var studentDefinition = faker.Define<DsStudent>().AutoFill();

            var students = dataset.Table(studentDefinition, studentCount, x => x.Id);
            var courses = dataset.Table(faker.Define<DsCourse>().AutoFill(), courseCount, x => x.Id);
            var enrollments = dataset.Table(faker.Define<DsEnrollment>().AutoFill(), enrollmentCount, x => x.Id)
                .ForeignKey(x => x.StudentId, students)
                .ForeignKey(x => x.CourseId, courses);

            if (unique)
                enrollments.UniqueTogether(x => x.StudentId, x => x.CourseId);

            return new University(dataset, studentDefinition, students, courses, enrollments);
        }

        private static string Codes(IEnumerable<ConfigurationError> errors) =>
            string.Join(",", errors.Select(e => e.Code));

        // ---------- Foreign keys always point to existing rows ----------

        [Fact]
        public void Build_StudentEnrollmentCourse_ShouldNeverProduceOrphans()
        {
            var u = CreateUniversity(new Faker(2026), studentCount: 20, courseCount: 8, enrollmentCount: 300);

            var result = u.Dataset.Build();

            Assert.True(result.IsSuccess);

            var data = result.Value;
            var studentIds = data.Rows(u.Students).Select(x => x.Id).ToHashSet();
            var courseIds = data.Rows(u.Courses).Select(x => x.Id).ToHashSet();
            var enrollments = data.Rows(u.Enrollments);

            Assert.Equal(20, studentIds.Count);
            Assert.Equal(8, courseIds.Count);
            Assert.Equal(300, enrollments.Count);
            Assert.All(enrollments, e =>
            {
                Assert.Contains(e.StudentId, studentIds);
                Assert.Contains(e.CourseId, courseIds);
            });
        }

        [Fact]
        public void Build_ShouldSpreadReferencesAcrossParents()
        {
            var u = CreateUniversity(new Faker(2026), 10, 10, 300);

            var enrollments = u.Dataset.Build().Value.Rows(u.Enrollments);

            Assert.True(enrollments.Select(e => e.StudentId).Distinct().Count() > 5);
            Assert.True(enrollments.Select(e => e.CourseId).Distinct().Count() > 5);
        }

        [Fact]
        public void Build_ShouldOverwriteValuesTheDefinitionProducedForForeignKeys()
        {
            var faker = new Faker(2026);
            var dataset = faker.CreateDataset();

            var students = dataset.Table(faker.Define<DsStudent>().AutoFill(), 3, x => x.Id);
            var enrollments = dataset.Table(
                    faker.Define<DsEnrollment>().AutoFill().RuleFor(x => x.StudentId, _ => 999),
                    30,
                    x => x.Id)
                .ForeignKey(x => x.StudentId, students);

            var data = dataset.Build().Value;

            Assert.All(data.Rows(enrollments), e => Assert.InRange(e.StudentId, 1, 3));
        }

        [Fact]
        public void Build_SingleTableWithoutRelations_ShouldWork()
        {
            var faker = new Faker(2026);
            var dataset = faker.CreateDataset();
            var students = dataset.Table(faker.Define<DsStudent>().AutoFill(), 5, x => x.Id);

            var result = dataset.Build();

            Assert.True(result.IsSuccess);
            Assert.Equal(new[] { 1, 2, 3, 4, 5 }, result.Value.Rows(students).Select(x => x.Id));
        }

        [Fact]
        public void Build_EmptyDataset_ShouldSucceed()
        {
            Assert.True(new Faker(2026).CreateDataset().Build().IsSuccess);
        }

        [Fact]
        public void Build_ZeroRowTables_ShouldSucceedWithEmptyLists()
        {
            var u = CreateUniversity(new Faker(2026), 0, 0, 0);

            var data = u.Dataset.Build().Value;

            Assert.Empty(data.Rows(u.Students));
            Assert.Empty(data.Rows(u.Enrollments));
        }

        [Fact]
        public void Build_ThreeLevelChain_ShouldKeepEveryLevelConsistent()
        {
            var faker = new Faker(2026);
            var dataset = faker.CreateDataset();

            var students = dataset.Table(faker.Define<DsStudent>().AutoFill(), 5, x => x.Id);
            var courses = dataset.Table(faker.Define<DsCourse>().AutoFill(), 4, x => x.Id);
            var enrollments = dataset.Table(faker.Define<DsEnrollment>().AutoFill(), 40, x => x.Id)
                .ForeignKey(x => x.StudentId, students)
                .ForeignKey(x => x.CourseId, courses);
            var payments = dataset.Table(faker.Define<DsPayment>().AutoFill(), 100, x => x.Id)
                .ForeignKey(x => x.EnrollmentId, enrollments);

            var data = dataset.Build().Value;

            var enrollmentIds = data.Rows(enrollments).Select(x => x.Id).ToHashSet();

            Assert.All(data.Rows(payments), p => Assert.Contains(p.EnrollmentId, enrollmentIds));
        }

        [Fact]
        public void Build_TablesMayReferenceEachOther()
        {
            var faker = new Faker(2026);
            var dataset = faker.CreateDataset();

            var left = dataset.Table(faker.Define<DsLeft>().AutoFill(), 6, x => x.Id);
            var right = dataset.Table(faker.Define<DsRight>().AutoFill(), 4, x => x.Id);

            left.ForeignKey(x => x.RightId, right);
            right.ForeignKey(x => x.LeftId, left);

            var data = dataset.Build().Value;

            Assert.All(data.Rows(left), l => Assert.InRange(l.RightId, 1, 4));
            Assert.All(data.Rows(right), r => Assert.InRange(r.LeftId, 1, 6));
        }

        [Fact]
        public void Build_ShouldBeRepeatableAndEveryBuildStaysConsistent()
        {
            var u = CreateUniversity(new Faker(2026), 5, 5, 50);

            var first = u.Dataset.Build().Value;
            var second = u.Dataset.Build().Value;

            Assert.All(second.Rows(u.Enrollments), e => Assert.Contains(e.StudentId, second.Rows(u.Students).Select(s => s.Id)));
            Assert.NotEqual(first.Rows(u.Students).Select(s => s.Id), second.Rows(u.Students).Select(s => s.Id));
            Assert.Equal(50, first.Rows(u.Enrollments).Count);
        }

        // ---------- Key types ----------

        [Fact]
        public void Build_GuidKeys_ShouldReferenceExistingParentsAndAllowOptionalLinks()
        {
            var faker = new Faker(2026);
            var dataset = faker.CreateDataset();

            var parents = dataset.Table(faker.Define<DsGuidParent>().AutoFill(), 5, x => x.Id);
            var children = dataset.Table(faker.Define<DsGuidChild>().AutoFill(), 100, x => x.Id)
                .ForeignKey(x => x.ParentId, parents)
                .ForeignKey(x => x.OtherId, parents, nullChance: 0.5);

            var data = dataset.Build().Value;

            var ids = data.Rows(parents).Select(p => p.Id).ToHashSet();
            var rows = data.Rows(children);

            Assert.All(rows, c => Assert.Contains(c.ParentId, ids));
            Assert.Contains(rows, c => c.OtherId is null);
            Assert.Contains(rows, c => c.OtherId is not null);
            Assert.All(rows.Where(c => c.OtherId is not null), c => Assert.Contains(c.OtherId!.Value, ids));
        }

        [Fact]
        public void Build_StringKeys_ShouldReferenceExistingParents()
        {
            var faker = new Faker(2026);
            var dataset = faker.CreateDataset();

            var parents = dataset.Table(faker.Define<DsStringParent>().AutoFill(), 4, x => x.Id);
            var children = dataset.Table(faker.Define<DsStringChild>().AutoFill(), 40, x => x.Id)
                .ForeignKey(x => x.ParentId, parents);

            var data = dataset.Build().Value;

            var ids = data.Rows(parents).Select(p => p.Id).ToHashSet();

            Assert.All(data.Rows(children), c => Assert.Contains(c.ParentId, ids));
        }

        // ---------- Nullable foreign keys ----------

        [Fact]
        public void Build_NullChance_ShouldMixNullsAndValidReferences()
        {
            var faker = new Faker(2026);
            var dataset = faker.CreateDataset();

            var customers = dataset.Table(faker.Define<DsStudent>().AutoFill(), 5, x => x.Id);
            var orders = dataset.Table(faker.Define<DsOrder>().AutoFill(), 200, x => x.Id)
                .ForeignKey(x => x.CustomerId, customers)
                .ForeignKey(x => x.CouponId, customers, nullChance: 0.5);

            var rows = dataset.Build().Value.Rows(orders);

            Assert.Contains(rows, o => o.CouponId is null);
            Assert.Contains(rows, o => o.CouponId is not null);
            Assert.All(rows.Where(o => o.CouponId.HasValue), o => Assert.InRange(o.CouponId!.Value, 1, 5));
        }

        [Fact]
        public void Build_NullChanceOne_ShouldAllowEmptyParentTable()
        {
            var faker = new Faker(2026);
            var dataset = faker.CreateDataset();

            var customers = dataset.Table(faker.Define<DsStudent>().AutoFill(), 0, x => x.Id);
            var orders = dataset.Table(faker.Define<DsOrder>().AutoFill(), 5, x => x.Id)
                .ForeignKey(x => x.CouponId, customers, nullChance: 1);

            var result = dataset.Build();

            Assert.True(result.IsSuccess);
            Assert.All(result.Value.Rows(orders), o => Assert.Null(o.CouponId));
        }

        // ---------- Hierarchies ----------

        [Fact]
        public void Build_SelfReference_ShouldFormAForestWithoutCycles()
        {
            var faker = new Faker(2026);
            var dataset = faker.CreateDataset();

            var employees = dataset.Table(faker.Define<DsEmployee>().AutoFill(), 60, x => x.Id)
                .SelfReference(x => x.ManagerId);

            var rows = dataset.Build().Value.Rows(employees);

            var indexById = rows.Select((e, i) => (e.Id, i)).ToDictionary(p => p.Id, p => p.i);

            Assert.Null(rows[0].ManagerId);

            for (int i = 1; i < rows.Count; i++)
            {
                Assert.NotNull(rows[i].ManagerId);
                Assert.True(indexById[rows[i].ManagerId!.Value] < i);
            }
        }

        [Fact]
        public void Build_SelfReferenceWithRoots_ShouldLeaveExactlyThoseRowsWithoutParent()
        {
            var faker = new Faker(2026);
            var dataset = faker.CreateDataset();

            var employees = dataset.Table(faker.Define<DsEmployee>().AutoFill(), 30, x => x.Id)
                .SelfReference(x => x.ManagerId, roots: 3);

            var rows = dataset.Build().Value.Rows(employees);

            Assert.Equal(3, rows.Count(e => e.ManagerId is null));
            Assert.All(rows.Take(3), e => Assert.Null(e.ManagerId));
        }

        [Fact]
        public void Build_SelfReferenceWithFewerRowsThanRoots_ShouldLeaveAllNull()
        {
            var faker = new Faker(2026);
            var dataset = faker.CreateDataset();

            var employees = dataset.Table(faker.Define<DsEmployee>().AutoFill(), 2, x => x.Id)
                .SelfReference(x => x.ManagerId, roots: 5);

            Assert.All(dataset.Build().Value.Rows(employees), e => Assert.Null(e.ManagerId));
        }

        [Fact]
        public void SelfReference_NonNullableProperty_ShouldReportError()
        {
            var faker = new Faker(2026);
            var dataset = faker.CreateDataset();

            dataset.Table(faker.Define<DsNode>().AutoFill(), 5, x => x.Id).SelfReference(x => x.ParentId);

            Assert.Equal(ConfigurationErrorCodes.ForeignKeyNotNullable, Assert.Single(dataset.ConfigurationErrors).Code);
        }

        [Fact]
        public void SelfReference_InvalidRoots_ShouldReportError()
        {
            var faker = new Faker(2026);
            var dataset = faker.CreateDataset();

            dataset.Table(faker.Define<DsEmployee>().AutoFill(), 5, x => x.Id).SelfReference(x => x.ManagerId, roots: 0);

            Assert.Equal(ConfigurationErrorCodes.InvalidRoots, Assert.Single(dataset.ConfigurationErrors).Code);
        }

        // ---------- Unique combinations (many to many) ----------

        [Fact]
        public void Build_UniqueTogether_ShouldNeverRepeatAStudentCoursePair()
        {
            var u = CreateUniversity(new Faker(2026), 20, 15, 250, unique: true);

            var rows = u.Dataset.Build().Value.Rows(u.Enrollments);

            Assert.Equal(250, rows.Select(e => (e.StudentId, e.CourseId)).Distinct().Count());
        }

        [Fact]
        public void Build_UniqueTogether_ShouldFillEveryCombinationWhenRowsEqualCapacity()
        {
            for (int seed = 0; seed < 25; seed++)
            {
                var u = CreateUniversity(new Faker(seed), 3, 4, 12, unique: true);

                var rows = u.Dataset.Build().Value.Rows(u.Enrollments);

                Assert.Equal(12, rows.Select(e => (e.StudentId, e.CourseId)).Distinct().Count());
            }
        }

        [Fact]
        public void Build_UniqueTogether_TooManyRows_ShouldFailBeforeGeneratingAnything()
        {
            var u = CreateUniversity(new Faker(2026), 3, 4, 13, unique: true);

            var result = u.Dataset.Build();

            Assert.False(result.IsSuccess);
            Assert.Equal(GenerationErrorCodes.UniqueConstraintUnsatisfiable, Assert.Single(result.Errors).Code);
            Assert.Equal(1, u.StudentDefinition.Generate().Value!.Id);
        }

        [Fact]
        public void Build_UniqueTogetherOfThree_ShouldBeUnique()
        {
            var faker = new Faker(2026);
            var dataset = faker.CreateDataset();

            var a = dataset.Table(faker.Define<DsStudent>().AutoFill(), 3, x => x.Id);
            var b = dataset.Table(faker.Define<DsCourse>().AutoFill(), 3, x => x.Id);
            var c = dataset.Table(faker.Define<DsPayment>().AutoFill(), 3, x => x.Id);
            var link = dataset.Table(faker.Define<TripleLink>().AutoFill(), 27, x => x.Id)
                .ForeignKey(x => x.A, a)
                .ForeignKey(x => x.B, b)
                .ForeignKey(x => x.C, c)
                .UniqueTogether(x => x.A, x => x.B, x => x.C);

            var rows = dataset.Build().Value.Rows(link);

            Assert.Equal(27, rows.Select(r => (r.A, r.B, r.C)).Distinct().Count());
        }

        [Fact]
        public void UniqueTogether_InvalidGroups_ShouldReportErrors()
        {
            var faker = new Faker(2026);
            var dataset = faker.CreateDataset();

            var students = dataset.Table(faker.Define<DsStudent>().AutoFill(), 3, x => x.Id);
            var orders = dataset.Table(faker.Define<DsOrder>().AutoFill(), 3, x => x.Id)
                .ForeignKey(x => x.CouponId, students, nullChance: 0.5);

            // CustomerId has no relation yet; CouponId can be null.
            orders.UniqueTogether(x => x.CustomerId, x => x.CouponId);

            Assert.Equal(ConfigurationErrorCodes.InvalidUniqueGroup, Assert.Single(dataset.ConfigurationErrors).Code);
        }

        [Fact]
        public void UniqueTogether_SamePropertyTwice_ShouldReportError()
        {
            var u = CreateUniversity(new Faker(2026), 3, 3, 3);

            u.Enrollments.UniqueTogether(x => x.StudentId, x => x.StudentId);

            Assert.Equal(ConfigurationErrorCodes.InvalidUniqueGroup, Assert.Single(u.Dataset.ConfigurationErrors).Code);
        }

        [Fact]
        public void UniqueTogether_PropertyInTwoGroups_ShouldReportError()
        {
            var u = CreateUniversity(new Faker(2026), 3, 3, 3, unique: true);

            u.Enrollments.UniqueTogether(x => x.StudentId, x => x.CourseId);

            Assert.Equal(ConfigurationErrorCodes.InvalidUniqueGroup, Assert.Single(u.Dataset.ConfigurationErrors).Code);
        }

        [Fact]
        public void UniqueTogether_SelfReference_ShouldReportError()
        {
            var faker = new Faker(2026);
            var dataset = faker.CreateDataset();

            var table = dataset.Table(faker.Define<DsEmployee>().AutoFill(), 5, x => x.Id)
                .SelfReference(x => x.ManagerId);

            table.UniqueTogether(x => x.ManagerId, x => x.Id);

            Assert.Contains(ConfigurationErrorCodes.InvalidUniqueGroup, Codes(dataset.ConfigurationErrors));
        }

        // ---------- Failures at build time ----------

        [Fact]
        public void Build_ChildRowsWithEmptyParent_ShouldFailWithEmptyParentTable()
        {
            var u = CreateUniversity(new Faker(2026), 0, 5, 10);

            var result = u.Dataset.Build();

            Assert.False(result.IsSuccess);
            Assert.Null(result.Value);
            Assert.Equal(GenerationErrorCodes.EmptyParentTable, Assert.Single(result.Errors).Code);
        }

        [Fact]
        public void Build_ChildWithZeroRowsAndEmptyParent_ShouldSucceed()
        {
            Assert.True(CreateUniversity(new Faker(2026), 0, 0, 0).Dataset.Build().IsSuccess);
        }

        [Fact]
        public void Build_DuplicateKeys_ShouldFail()
        {
            var faker = new Faker(2026);
            var dataset = faker.CreateDataset();

            dataset.Table(faker.Define<DsStudent>(), 3, x => x.Id); // Id stays 0 for every row.

            var result = dataset.Build();

            Assert.False(result.IsSuccess);
            Assert.Equal(GenerationErrorCodes.DuplicateKey, Assert.Single(result.Errors).Code);
        }

        [Fact]
        public void Build_NullKey_ShouldFail()
        {
            var faker = new Faker(2026);
            var dataset = faker.CreateDataset();

            dataset.Table(faker.Define<DsStringParent>(), 2, x => x.Id);

            var result = dataset.Build();

            Assert.Equal(GenerationErrorCodes.NullKey, Assert.Single(result.Errors).Code);
        }

        [Fact]
        public void Build_InvalidDefinition_ShouldFailWithDefinitionErrors()
        {
            var faker = new Faker(2026);
            var dataset = faker.CreateDataset();

            dataset.Table(faker.Define<DsNoKeyRule>().RuleFor(x => x.Code, _ => "x"), 2, x => x.Id);

            var result = dataset.Build();

            Assert.False(result.IsSuccess);
            Assert.Equal(GenerationErrorCodes.InvalidDefinition, result.Errors[0].Code);
        }

        // ---------- Configuration errors ----------

        [Fact]
        public void Table_NullDefinition_ShouldReportErrorAndFailBuild()
        {
            var dataset = new Faker(2026).CreateDataset();

            var table = dataset.Table<DsStudent, int>(null!, 3, x => x.Id);

            Assert.Equal(ConfigurationErrorCodes.NullDefinition, Assert.Single(dataset.ConfigurationErrors).Code);
            Assert.False(dataset.IsValid);
            Assert.Equal(GenerationErrorCodes.InvalidDefinition, dataset.Build().Errors[0].Code);
            Assert.False(table.RowCount > 0);
        }

        [Fact]
        public void Table_NegativeRowCount_ShouldReportError()
        {
            var faker = new Faker(2026);
            var dataset = faker.CreateDataset();

            dataset.Table(faker.Define<DsStudent>(), -1, x => x.Id);

            Assert.Equal(ConfigurationErrorCodes.InvalidRowCount, Assert.Single(dataset.ConfigurationErrors).Code);
        }

        [Fact]
        public void Table_InvalidKeySelector_ShouldReportError()
        {
            var faker = new Faker(2026);
            var dataset = faker.CreateDataset();

            dataset.Table(faker.Define<DsNoKeyRule>(), 3, x => x.Code);

            Assert.Equal(ConfigurationErrorCodes.FieldNotSupported, Assert.Single(dataset.ConfigurationErrors).Code);
        }

        [Fact]
        public void Table_SameDefinitionTwice_ShouldReportError()
        {
            var faker = new Faker(2026);
            var dataset = faker.CreateDataset();
            var definition = faker.Define<DsStudent>().AutoFill();

            dataset.Table(definition, 3, x => x.Id);
            dataset.Table(definition, 3, x => x.Id);

            Assert.Equal(ConfigurationErrorCodes.DuplicateTable, Assert.Single(dataset.ConfigurationErrors).Code);
        }

        [Fact]
        public void ForeignKey_TypeMismatch_ShouldReportError()
        {
            var u = CreateUniversity(new Faker(2026), 3, 3, 3);

            u.Enrollments.ForeignKey(x => x.WrongTypeId, u.Students);

            Assert.Equal(ConfigurationErrorCodes.ForeignKeyTypeMismatch, Assert.Single(u.Dataset.ConfigurationErrors).Code);
            Assert.False(u.Dataset.Build().IsSuccess);
        }

        [Fact]
        public void ForeignKey_ToTableOfAnotherDataset_ShouldReportError()
        {
            var faker = new Faker(2026);

            var other = faker.CreateDataset();
            var foreignStudents = other.Table(faker.Define<DsStudent>().AutoFill(), 3, x => x.Id);

            var dataset = faker.CreateDataset();
            var orders = dataset.Table(faker.Define<DsOrder>().AutoFill(), 3, x => x.Id);

            orders.ForeignKey(x => x.CustomerId, foreignStudents);

            Assert.Equal(ConfigurationErrorCodes.ForeignKeyTargetInvalid, Assert.Single(dataset.ConfigurationErrors).Code);
        }

        [Fact]
        public void ForeignKey_ToItself_ShouldPointToSelfReference()
        {
            var faker = new Faker(2026);
            var dataset = faker.CreateDataset();

            var employees = dataset.Table(faker.Define<DsEmployee>().AutoFill(), 3, x => x.Id);

            employees.ForeignKey(x => x.ManagerId, employees);

            var error = Assert.Single(dataset.ConfigurationErrors);

            Assert.Equal(ConfigurationErrorCodes.ForeignKeyTargetInvalid, error.Code);
            Assert.Contains("SelfReference", error.Message);
        }

        [Fact]
        public void ForeignKey_OnTheKeyProperty_ShouldReportConflict()
        {
            var faker = new Faker(2026);
            var dataset = faker.CreateDataset();

            var students = dataset.Table(faker.Define<DsStudent>().AutoFill(), 3, x => x.Id);
            var courses = dataset.Table(faker.Define<DsCourse>().AutoFill(), 3, x => x.Id);

            courses.ForeignKey(x => x.Id, students);

            Assert.Equal(ConfigurationErrorCodes.ForeignKeyPropertyConflict, Assert.Single(dataset.ConfigurationErrors).Code);
        }

        [Fact]
        public void ForeignKey_SamePropertyTwice_ShouldReportConflict()
        {
            var u = CreateUniversity(new Faker(2026), 3, 3, 3);

            u.Enrollments.ForeignKey(x => x.StudentId, u.Students);

            Assert.Equal(ConfigurationErrorCodes.ForeignKeyPropertyConflict, Assert.Single(u.Dataset.ConfigurationErrors).Code);
        }

        [Fact]
        public void ForeignKey_NullChanceOnNonNullableProperty_ShouldReportError()
        {
            var faker = new Faker(2026);
            var dataset = faker.CreateDataset();

            var students = dataset.Table(faker.Define<DsStudent>().AutoFill(), 3, x => x.Id);
            dataset.Table(faker.Define<DsOrder>().AutoFill(), 3, x => x.Id)
                .ForeignKey(x => x.CustomerId, students, nullChance: 0.3);

            Assert.Equal(ConfigurationErrorCodes.ForeignKeyNotNullable, Assert.Single(dataset.ConfigurationErrors).Code);
        }

        [Theory]
        [InlineData(-0.1)]
        [InlineData(1.5)]
        [InlineData(double.NaN)]
        public void ForeignKey_InvalidNullChance_ShouldReportError(double nullChance)
        {
            var faker = new Faker(2026);
            var dataset = faker.CreateDataset();

            var students = dataset.Table(faker.Define<DsStudent>().AutoFill(), 3, x => x.Id);
            dataset.Table(faker.Define<DsOrder>().AutoFill(), 3, x => x.Id)
                .ForeignKey(x => x.CouponId, students, nullChance);

            Assert.Equal(ConfigurationErrorCodes.InvalidNullChance, Assert.Single(dataset.ConfigurationErrors).Code);
        }

        [Fact]
        public void ForeignKey_InvalidSelector_ShouldReportAnalyzerError()
        {
            var faker = new Faker(2026);
            var dataset = faker.CreateDataset();

            var students = dataset.Table(faker.Define<DsStudent>().AutoFill(), 3, x => x.Id);
            var noKey = dataset.Table(faker.Define<DsNoKeyRule>().AutoFill(), 3, x => x.Id);

            noKey.ForeignKey(x => x.Code, students);

            Assert.Contains(ConfigurationErrorCodes.FieldNotSupported, Codes(dataset.ConfigurationErrors));
        }

        [Fact]
        public void ForeignKey_OnInvalidTable_ShouldNotThrowAndStayQuiet()
        {
            var faker = new Faker(2026);
            var dataset = faker.CreateDataset();

            var broken = dataset.Table<DsStudent, int>(null!, 3, x => x.Id);
            var orders = dataset.Table(faker.Define<DsOrder>().AutoFill(), 3, x => x.Id);

            orders.ForeignKey(x => x.CustomerId, broken);
            broken.ForeignKey(x => x.Id, broken);

            Assert.Equal(ConfigurationErrorCodes.NullDefinition, Assert.Single(dataset.ConfigurationErrors).Code);
        }

        // ---------- Determinism ----------

        [Fact]
        public void Build_SameSeedAndSameSetup_ShouldProduceIdenticalRelations()
        {
            var first = CreateUniversity(new Faker(5), 10, 10, 100, unique: true);
            var second = CreateUniversity(new Faker(5), 10, 10, 100, unique: true);

            var a = first.Dataset.Build().Value.Rows(first.Enrollments).Select(e => (e.StudentId, e.CourseId));
            var b = second.Dataset.Build().Value.Rows(second.Enrollments).Select(e => (e.StudentId, e.CourseId));

            Assert.Equal(a, b);
        }

        [Fact]
        public void Build_DifferentSeeds_ShouldProduceDifferentRelations()
        {
            var first = CreateUniversity(new Faker(1), 10, 10, 100);
            var second = CreateUniversity(new Faker(2), 10, 10, 100);

            var a = first.Dataset.Build().Value.Rows(first.Enrollments).Select(e => (e.StudentId, e.CourseId));
            var b = second.Dataset.Build().Value.Rows(second.Enrollments).Select(e => (e.StudentId, e.CourseId));

            Assert.NotEqual(a, b);
        }

        // ---------- GeneratedDataset ----------

        [Fact]
        public void Rows_ForTableOfAnotherDataset_ShouldReturnEmpty()
        {
            var u = CreateUniversity(new Faker(2026), 3, 3, 3);
            var other = CreateUniversity(new Faker(2026), 3, 3, 3);

            var data = u.Dataset.Build().Value;

            Assert.Empty(data.Rows(other.Students));
            Assert.Empty(data.Rows<DsStudent, int>(null!));
        }

        [Fact]
        public void ConfigurationErrors_ShouldBeASnapshot()
        {
            var faker = new Faker(2026);
            var dataset = faker.CreateDataset();

            var snapshot = dataset.ConfigurationErrors;

            dataset.Table(faker.Define<DsStudent>(), -1, x => x.Id);

            Assert.Empty(snapshot);
            Assert.Single(dataset.ConfigurationErrors);
        }
    }

    internal sealed class TripleLink
    {
        public int Id { get; set; }

        public int A { get; set; }

        public int B { get; set; }

        public int C { get; set; }
    }
}
