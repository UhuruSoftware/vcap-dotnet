﻿// -----------------------------------------------------------------------
// <copyright file="Helper.cs" company="Uhuru Software">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.IO;
using System.Xml;

namespace CloudFoundry.Net.Test.Automation
{
    static class Helper
    {
        public static void UpdateWebConfigKey(string fileName, string key, string newValue)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(fileName);
            XmlNode appSettingsNode = xmlDoc.SelectSingleNode("configuration/appSettings");

            // Attempt to locate the requested setting.
            foreach (XmlNode childNode in appSettingsNode)
            {
                if (childNode.Attributes["key"].Value == key)
                {
                    childNode.Attributes["value"].Value = newValue;
                    break;
                }
            }
            xmlDoc.Save(fileName);
        }

        public static string CopyFolderToTemp(string folder)
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

                CopyAll(source, target);
            }

            return targetPath;
        }

        private static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            if (Directory.Exists(target.FullName) == false)
            {
                Directory.CreateDirectory(target.FullName);
            }
            foreach (FileInfo fi in source.GetFiles())
            {
                fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
            }
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }
    }
}