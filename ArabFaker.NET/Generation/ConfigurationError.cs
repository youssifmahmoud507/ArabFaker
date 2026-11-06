namespace ArabFaker.NET.Generation
{
    /// <summary>
    /// Describes a problem found while configuring a <see cref="FakerDefinition{T}"/>.
    /// </summary>
    /// <param name="Code">
    /// A stable, machine-readable code. See <see cref="ConfigurationErrorCodes"/>.
    /// </param>
    /// <param name="Message">A human-readable description of the problem.</param>
    public sealed record ConfigurationError(string Code, string Message);
}
