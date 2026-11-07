namespace ArabFaker.NET.Generation
{
    /// <summary>
    /// Describes why a generation call failed.
    /// </summary>
    /// <param name="Code">
    /// A stable, machine-readable code. See <see cref="GenerationErrorCodes"/>.
    /// </param>
    /// <param name="Message">A human-readable description of the problem.</param>
    public sealed record GenerationError(string Code, string Message);
}
