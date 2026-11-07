namespace ArabFaker.Tests.Dataset
{
    internal sealed class DsStudent
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = string.Empty;
    }

    internal sealed class DsCourse
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;
    }

    internal sealed class DsEnrollment
    {
        public int Id { get; set; }

        public int StudentId { get; set; }

        public int CourseId { get; set; }

        public long WrongTypeId { get; set; }
    }

    internal sealed class DsEmployee
    {
        public int Id { get; set; }

        public int? ManagerId { get; set; }
    }

    internal sealed class DsNode
    {
        public int Id { get; set; }

        public int ParentId { get; set; }
    }

    internal sealed class DsOrder
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public int? CouponId { get; set; }
    }

    internal sealed class DsGuidParent
    {
        public Guid Id { get; set; }
    }

    internal sealed class DsGuidChild
    {
        public Guid Id { get; set; }

        public Guid ParentId { get; set; }

        public Guid? OtherId { get; set; }
    }

    internal sealed class DsStringParent
    {
        public string Id { get; set; } = null!;
    }

    internal sealed class DsStringChild
    {
        public int Id { get; set; }

        public string ParentId { get; set; } = string.Empty;
    }

    internal sealed class DsLeft
    {
        public int Id { get; set; }

        public int RightId { get; set; }
    }

    internal sealed class DsRight
    {
        public int Id { get; set; }

        public int LeftId { get; set; }
    }

    internal sealed class DsPayment
    {
        public int Id { get; set; }

        public int EnrollmentId { get; set; }
    }

    internal sealed class DsNoKeyRule
    {
        public int Id { get; set; }

        public string Code = string.Empty;
    }
}
