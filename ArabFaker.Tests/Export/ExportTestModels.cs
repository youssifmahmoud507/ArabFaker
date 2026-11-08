namespace ArabFaker.Tests.Export
{
    internal enum ExpColor
    {
        Red = 1,
        Green = 2
    }

    internal class ExpBase
    {
        public int BaseId { get; set; }
    }

    internal sealed class ExpRow : ExpBase
    {
        public string Name { get; set; } = string.Empty;

        public bool Active { get; set; }

        public decimal Price { get; set; }

        public double Ratio { get; set; }

        public Guid Token { get; set; }

        public DateOnly Day { get; set; }

        public DateTime Stamp { get; set; }

        public ExpColor Color { get; set; }

        public int? Maybe { get; set; }

        public string? Notes { get; set; }

        // Never exported: read-only, navigation and collection.
        public string Computed => Name + "!";

        public ExpRow? Parent { get; set; }

        public List<ExpRow> Children { get; set; } = [];
    }

    internal sealed class ExpNoColumns
    {
        public ExpNoColumns? Next { get; set; }

        public List<int> Numbers { get; set; } = [];

        public string ReadOnly => "x";
    }
}
