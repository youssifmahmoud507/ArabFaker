namespace ArabFaker.Tests.Generation
{
    internal enum AutoFillStatus
    {
        Active,
        Suspended,
        Closed
    }

    internal sealed class AutoFillPerson
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string MiddleName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string Governorate { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public int Age { get; set; }

        public DateOnly BirthDate { get; set; }

        public bool IsActive { get; set; }

        public decimal Salary { get; set; }

        public DateTime CreatedAt { get; set; }

        public AutoFillStatus Status { get; set; }

        public AutoFillStatus? OptionalStatus { get; set; }

        public Guid Token { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;
    }

    internal sealed class AutoFillOrder
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public bool Paid { get; set; }
    }

    internal sealed class AutoFillGuidEntity
    {
        public Guid Id { get; set; }

        public Guid OwnerId { get; set; }
    }

    internal sealed class AutoFillStringKeyEntity
    {
        public string Id { get; set; } = string.Empty;

        public string ParentId { get; set; } = string.Empty;
    }

    internal sealed class AutoFillLongKeyEntity
    {
        public long Id { get; set; }

        public long? ParentId { get; set; }
    }

    internal enum AutoFillEmptyEnum
    {
    }

    internal sealed class AutoFillEmptyEnumEntity
    {
        public AutoFillEmptyEnum Kind { get; set; }
    }
}
