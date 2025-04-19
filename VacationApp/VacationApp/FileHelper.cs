using System;
using System.IO;

namespace VacationApp
{
    public static class FileHelper
    {
        // method to try multiple path formats
        public static bool TryFindFile(string filePath, out string validPath)
        {
            validPath = filePath;

            // try original path
            if (File.Exists(filePath))
            {
                return true;
            }

            // try fixing common issues with quotes and whitespace
            string trimmedPath = filePath.Trim('"', ' ');
            if (File.Exists(trimmedPath))
            {
                validPath = trimmedPath;
                return true;
            }

            // try replacing backslashes with forward slashes
            string forwardSlashPath = filePath.Replace('\\', '/');
            if (File.Exists(forwardSlashPath))
            {
                validPath = forwardSlashPath;
                return true;
            }

            // try replacing double backslashes with single
            string fixedBackslashPath = filePath.Replace("\\\\", "\\");
            if (File.Exists(fixedBackslashPath))
            {
                validPath = fixedBackslashPath;
                return true;
            }

            // try to check if it's just a filename in the current directory
            string currentDirPath = Path.Combine(Directory.GetCurrentDirectory(), Path.GetFileName(filePath));
            if (File.Exists(currentDirPath))
            {
                validPath = currentDirPath;
                return true;
            }

            // file not found with any attempt
            return false;
        }
    }
}