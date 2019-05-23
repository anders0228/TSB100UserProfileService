namespace TSB100UserProfileService
{
    using Newtonsoft.Json;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;

    public class FileReader
    {
        public static FileReader SingletonReader { get; private set; }

        public static void MakeSingletonReader()
        {
            SingletonReader = new FileReader();
        }

        public IEnumerable<string> GetTextFileLines(string pathAndFileName)
        {
            try
            {
                var fs = new FileStream(pathAndFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var sr = new StreamReader(fs);
                var lst = new List<string>();
                while (!sr.EndOfStream)
                {
                    lst.Add(sr.ReadLine());
                }
                return lst;
            }
            catch (Exception ex)
            when (ex is ArgumentException
            || ex is IOException
            || ex is UnauthorizedAccessException
            || ex is NotSupportedException)
            {
                // Catches all exceptions related to File.ReadAllText() and ONLY those.
                Log.Error($"In FileReader.GetTextFileLines(): Unable to read file. Exception of type {ex.GetType().Name} was thrown. Exception: {JsonConvert.SerializeObject(ex)}");
                return new Collection<string>();
            }
        }
        public IEnumerable<string> GetDirectory(string path)
        {
            IEnumerable<string> dirs;
            try
            {
                dirs = Directory.GetFiles(path, "WcfLog-*");
                return dirs;
            }
            catch (Exception ex)
            when (ex is IOException
            || ex is UnauthorizedAccessException
            || ex is ArgumentException
            || ex is ArgumentNullException
            || ex is PathTooLongException
            || ex is DirectoryNotFoundException)
            {
                // Catches all exceptions related to Directory.GetFiles() and ONLY those.
                Log.Error($"In FileReader.GetFilePathsAndNames(): Unable to read directory. Exception of type {ex.GetType().Name} was thrown. Exception: {JsonConvert.SerializeObject(ex)}");
                return new Collection<string>();
            }

        }
    }
}
