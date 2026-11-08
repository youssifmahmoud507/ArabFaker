namespace ArabFaker.Tests.Dataset
{
    internal sealed class NvStudent
    {
        public int Id { get; set; }

        public List<NvEnrollment> Enrollments { get; set; } = [];
    }

    internal sealed class NvCourse
    {
        public int Id { get; set; }

        public ICollection<NvEnrollment> Enrollments { get; set; } = new List<NvEnrollment>();

        public HashSet<NvEnrollment> UnsupportedSet { get; set; } = [];
    }

    internal sealed class NvEnrollment
    {
        public int Id { get; set; }

        public int StudentId { get; set; }

        public int CourseId { get; set; }

        public NvStudent? Student { get; set; }

        public NvCourse? Course { get; set; }

        public NvCourse? WrongTypeNavigation { get; set; }
    }

    internal sealed class NvEmployee
    {
        public int Id { get; set; }

        public int? ManagerId { get; set; }

        public NvEmployee? Manager { get; set; }

        public IReadOnlyList<NvEmployee> Reports { get; set; } = [];
    }

    internal sealed class NvOrder
    {
        public int Id { get; set; }

        public int? CouponId { get; set; }

        public NvStudent? Coupon { get; set; }
    }

    internal sealed class NvGuidParent
    {
        public Guid Id { get; set; }

        public List<NvGuidChild> Children { get; set; } = [];
    }

    internal sealed class NvGuidChild
    {
        public Guid Id { get; set; }

        public Guid ParentId { get; set; }

        public NvGuidParent? Parent { get; set; }
    }
}
