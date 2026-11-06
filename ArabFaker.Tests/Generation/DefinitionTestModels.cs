namespace ArabFaker.Tests.Generation
{
    internal sealed class DefinitionAddress
    {
        public string City { get; set; } = string.Empty;
    }

    /// <summary>
    /// A user-owned model that exposes every member shape the selector analyzer must handle.
    /// </summary>
    internal sealed class DefinitionStudent
    {
        public static int Counter { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public int Age { get; set; }

        public DateOnly BirthDate { get; set; }

        public string? Nickname { get; set; }

        public int? Score { get; set; }

        public DefinitionAddress Address { get; set; } = new();

        public string Display => FirstName;

        public string Secret { get; private set; } = string.Empty;

        public string Created { get; } = string.Empty;

        public string Tag { get; init; } = string.Empty;

        public string Code = string.Empty;

        public string this[int index]
        {
            get => FirstName;
            set => FirstName = value;
        }

        public string GetName() => FirstName;
    }

    internal sealed class DefinitionStudentWithConstructor
    {
        public DefinitionStudentWithConstructor(string defaultName)
        {
            DefaultName = defaultName;
        }

        public string DefaultName { get; }

        public string FirstName { get; set; } = string.Empty;
    }

    internal class DefinitionBase
    {
        public string BaseValue { get; set; } = string.Empty;
    }

    internal sealed class DefinitionDerived : DefinitionBase
    {
        public int DerivedValue { get; set; }
    }
}
