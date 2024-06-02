using HiEndsCore.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HiEndsCore.Helper
{
    public static class Utilities
    {
        public static List<TEnum> GetEnumList<TEnum>() where TEnum : Enum
            => ((TEnum[])Enum.GetValues(typeof(TEnum))).ToList();

        public static bool IsAbsolutePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            return Path.IsPathRooted(path) && !string.IsNullOrEmpty(Path.GetPathRoot(path).Trim('\\', '/'));
        }

        public static void CreateFoldersIfNotExists(string? path)
        {
            try
            {
                if (!string.IsNullOrEmpty(path))
                {
                    // Use Directory.CreateDirectory to create directories
                    Directory.CreateDirectory(path);

                    // Recursively create parent directories
                    while (!Directory.Exists(path))
                    {
                        DirectoryInfo info = Directory.GetParent(path);
                        Directory.CreateDirectory(info.FullName);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }

        public static string ConvertAbsolutePathToRelative(string absolutePath, string directoryPath)
        {
            try
            {
                // Ensure both paths are absolute
                if (!Path.IsPathFullyQualified(absolutePath) || !Path.IsPathFullyQualified(directoryPath))
                {
                    throw new ArgumentException("Both paths must be absolute.");
                }

                // Get the relative path
                string relativePath = Path.GetRelativePath(directoryPath, absolutePath);
                return relativePath;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }

        public static bool ContainsWildcard(string input, string pattern)
        {
            // Escape special characters in the pattern and replace * with .*
            string regexPattern = Regex.Escape(pattern).Replace("\\*", ".*");

            // Allow the regex to span across multiple lines
            regexPattern = "(?s)" + regexPattern;

            // Check if the input matches the pattern
            return Regex.IsMatch(input, regexPattern);
        }
    }
}
