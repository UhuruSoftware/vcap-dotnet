using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using SevenZip;
using System.Xml;

namespace netzip
{
    class Program
    {
        static void Main(string[] args)
        {
            CmdArguments arguments = new CmdArguments(args);
            if (arguments.HasParam("?"))
            {
                Console.WriteLine(@"
-s sourceFolder
-d destination.zip
-xml vocabulary.xml
");
                return;
            }

            string sourceDirectory = arguments["s"];
            string destination = arguments["d"];
            string xmlFile = arguments["xml"];

            List<string> bannedFiles = GetFiles(xmlFile);
            List<string> bannedFolders = GetFolders(xmlFile);
            string tempFolder = CopyFolderToTemp(sourceDirectory, bannedFiles, bannedFolders);

            if (IntPtr.Size == 8)
            {
                SevenZipCompressor.SetLibraryPath(Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(SevenZipCompressor)).Location), @"lib\7z64.dll"));
            }
            else
            {
                SevenZipCompressor.SetLibraryPath(Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(SevenZipCompressor)).Location), @"lib\7z86.dll"));
            }

            SevenZipCompressor compressor = new SevenZipCompressor();
            compressor.ArchiveFormat = OutArchiveFormat.Zip;
            compressor.CompressDirectory(tempFolder, destination);
        }

        private static List<string> GetFolders(string fileName)
        {
            List<string> folders = new List<string>();

            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);

            XmlNode node = doc.SelectSingleNode("//vocab/folders/text()");
            foreach (string str in node.Value.Split('|'))
            {
                folders.Add(str);
            }

            return folders;
        }

        private static List<string> GetFiles(string fileName)
        {
            List<string> files = new List<string>();

            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);

            XmlNode node = doc.SelectSingleNode("//vocab/files/text()");
            foreach (string str in node.Value.Split('|'))
            {
                files.Add(str);
            }

            return files;
        }

        private static string CopyFolderToTemp(string folder, List<string> files, List<string> folders)
        {
            string tempFolder = Path.GetTempPath();
            string targetPath = Path.Combine(tempFolder, Guid.NewGuid().ToString());

            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }

            if (Directory.Exists(folder))
            {
                DirectoryInfo source = new DirectoryInfo(folder);
                DirectoryInfo target = new DirectoryInfo(targetPath);

                CopyAll(source, target, files, folders);
            }

            return targetPath;
        }

        private static void CopyAll(DirectoryInfo source, DirectoryInfo target, List<string> files, List<string> folders)
        {
            if (Directory.Exists(target.FullName) == false)
            {
                Directory.CreateDirectory(target.FullName);
            }
            foreach (FileInfo fi in source.GetFiles())
            {
                if (!files.Contains(fi.Name))
                {
                    fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
                }
            }
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                if (!folders.Contains(diSourceSubDir.Name))
                {
                    DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                    CopyAll(diSourceSubDir, nextTargetSubDir, files, folders);
                }
            }
        }
    }
}
