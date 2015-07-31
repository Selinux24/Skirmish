using System.Collections.Generic;
using System;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace Engine.Content
{
    public static class Compression
    {
        public static string[] ReadIndex(string file)
        {
            List<string> files = new List<string>();

            using (ZipArchive archive = ZipFile.OpenRead(file))
            {
                foreach (var compressedFile in archive.Entries)
                {
                    files.Add(compressedFile.Name);
                }
            }

            return files.ToArray();
        }

        public static bool Contains(string file, string name)
        {
            string[] entries = ReadIndex(file);

            return Array.Exists(entries, e => e.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public static MemoryStream GetFile(string file, string name)
        {
            using (ZipArchive archive = ZipFile.OpenRead(file))
            {
                ZipArchiveEntry entry = archive.GetEntry(name);

                using (var stream = entry.Open())
                {
                    MemoryStream ms = new MemoryStream();

                    stream.CopyTo(ms);

                    ms.Position = 0;

                    return ms;
                }
            }
        }

        public static MemoryStream[] GetFiles(string file, string mask)
        {
            List<MemoryStream> res = new List<MemoryStream>();

            string regexMask = Regex.Escape(mask).Replace(@"\*", ".*").Replace(@"\?", ".");

            using (ZipArchive archive = ZipFile.OpenRead(file))
            {
                for (int i = 0; i < archive.Entries.Count; i++)
                {
                    ZipArchiveEntry entry = archive.Entries[i];

                    Match match = Regex.Match(entry.Name, regexMask);
                    if (match.Success)
                    {
                        using (var stream = entry.Open())
                        {
                            MemoryStream ms = new MemoryStream();

                            stream.CopyTo(ms);

                            ms.Position = 0;

                            res.Add(ms);
                        }
                    }
                }
            }

            return res.ToArray();
        }

        public static MemoryStream[] GetFiles(string file, string[] names)
        {
            MemoryStream[] res = new MemoryStream[names.Length];

            using (ZipArchive archive = ZipFile.OpenRead(file))
            {
                for (int i = 0; i < names.Length; i++)
                {
                    ZipArchiveEntry entry = archive.GetEntry(names[i]);

                    using (var stream = entry.Open())
                    {
                        MemoryStream ms = new MemoryStream();

                        stream.CopyTo(ms);

                        ms.Position = 0;

                        res[i] = ms;
                    }
                }
            }

            return res;
        }
    }
}
