namespace ArabFaker.NET.Dataset
{
    /// <summary>
    /// Orders dataset tables so a parent table always comes before the tables that reference it
    /// (used by the SQL export so INSERT scripts never break a foreign key).
    /// </summary>
    internal static class TableOrdering
    {
        /// <summary>
        /// Orders tables so that referenced tables come before the tables that reference them.
        /// Ties keep registration order. Tables that reference each other have no valid order and
        /// are appended in registration order. Self references are ignored (their rows are already
        /// generated parents first).
        /// </summary>
        internal static List<ITableRuntime> ParentsFirst(IReadOnlyList<ITableRuntime> tables)
        {
            var remaining = tables.ToList();

            var ordered = new List<ITableRuntime>(tables.Count);

            Dictionary<ITableRuntime, HashSet<ITableRuntime>> dependencies = tables.ToDictionary(
                table => table,
                table => table.GetForeignKeys()
                    .Where(plan => !plan.IsSelfReference)
                    .Select(plan => plan.Parent)
                    .ToHashSet());

            while (remaining.Count > 0)
            {
                ITableRuntime? next = remaining.FirstOrDefault(
                    table => dependencies[table].All(parent => !remaining.Contains(parent) || parent == table));

                if (next is null)
                {
                    ordered.AddRange(remaining);

                    break;
                }

                ordered.Add(next);
                remaining.Remove(next);
            }

            return ordered;
        }
    }
}
