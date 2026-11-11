using System.Text;

namespace ArabFaker.Tests
{
    /// <summary>
    /// Compares text with a file stored in <c>ArabFaker.Tests/Golden</c>. Line endings are ignored, so Windows and Linux agree.
    /// To (re)write the files after an intended change, set the environment variable <c>ARABFAKER_UPDATE_GOLDEN=1</c> and run the tests.
    /// </summary>
    internal static class GoldenFile
    {
        private const string UpdateVariable = "ARABFAKER_UPDATE_GOLDEN";

        public static void Verify(string fileName, string actual)
        {
            string path = Path.Combine(FindGoldenFolder(), fileName);
            string normalizedActual = Normalize(actual);

            if (Environment.GetEnvironmentVariable(UpdateVariable) == "1")
            {
                File.WriteAllText(path, normalizedActual, new UTF8Encoding(false));
                return;
            }

            if (!File.Exists(path))
            {
                Assert.Fail($"Golden file '{fileName}' does not exist. Create it by running the tests once with {UpdateVariable}=1 " +
                            "(or run the 'Update golden files' workflow), review it and commit it.");
            }

            Assert.Equal(Normalize(File.ReadAllText(path, Encoding.UTF8)), normalizedActual);
        }

        private static string Normalize(string text)
        {
            return text.Replace("\r\n", "\n", StringComparison.Ordinal).TrimEnd('\n') + "\n";
        }

        private static string FindGoldenFolder()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);

            while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "ArabFaker.slnx")))
                directory = directory.Parent;

            if (directory is null)
                throw new InvalidOperationException("Could not find ArabFaker.slnx above " + AppContext.BaseDirectory);

            string folder = Path.Combine(directory.FullName, "ArabFaker.Tests", "Golden");
            Directory.CreateDirectory(folder);

            return folder;
        }
    }
}
