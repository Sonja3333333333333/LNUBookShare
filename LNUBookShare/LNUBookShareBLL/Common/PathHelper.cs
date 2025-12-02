namespace LNUBookShareBLL.Common
{
    public static class PathHelper
    {
        private static readonly string _baseDir = AppDomain.CurrentDomain.BaseDirectory;

        public static string? ConvertToAbsolutePath(string? dbPath)
        {
            if (string.IsNullOrEmpty(dbPath))
            {
                return null;
            }


            if (dbPath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                dbPath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return dbPath;
            }

            return Path.Combine(_baseDir, dbPath);
        }

        public static string? ConvertToRelativePath(string? absolutePath)
        {
            if (string.IsNullOrEmpty(absolutePath))
            {
                return null;
            }

            if (absolutePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                absolutePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return absolutePath;
            }

            var relativePath = Path.GetRelativePath(_baseDir, absolutePath);

            return relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }
    }
}