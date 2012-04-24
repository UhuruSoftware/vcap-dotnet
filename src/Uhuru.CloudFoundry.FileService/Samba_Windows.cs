using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Uhuru.CloudFoundry.FileService
{
    class Samba_Windows
    {
        private void mount(string RemoteDirectory, string TargetMachine, string Username, string Password, string LocalUser, string LocalPath)
        {
            //mklink creates the directory if not exist
            //if (!Directory.Exists(LocalPath)) { Directory.CreateDirectory(LocalPath);}
            try
            {
                Process.Start("runas /user:administrator password mklink /d " + LocalPath + " \\\\" + TargetMachine + "\\" + RemoteDirectory);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void unmount(string LocalPath)
        {
            try
            {
                Process.Start("rmdir /q " + LocalPath);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        static public void CopyFolderRecursively(string source, string destination)
        {
            if (!Directory.Exists(destination)) Directory.CreateDirectory(destination);

            string[] files = Directory.GetFiles(source);

            foreach (string file in files)
            {
                string name = Path.GetFileName(file);
                string dest = Path.Combine(destination, name);
                File.Copy(file, dest, false);
            }

            string[] folders = Directory.GetDirectories(source);

            foreach (string folder in folders)
            {
                string name = Path.GetFileName(folder);
                string dest = Path.Combine(destination, name);
                CopyFolderRecursively(folder, dest);
            }
        }

        private void Link(string InstancePath, string PersistentItem, string MountPath)
        {
            string item=string.Empty;
            if (Directory.Exists(MountPath + "\\" + PersistentItem)) 
                {
                    Directory.CreateDirectory(InstancePath + "\\" + PersistentItem);
                    CopyFolderRecursively(InstancePath + "\\" + PersistentItem, MountPath + "\\"+PersistentItem);
                    Directory.Delete(InstancePath + "\\" + PersistentItem);
                    try
                    {
                        Process.Start("mklink /d " + InstancePath + "\\" + PersistentItem + " " + MountPath + "\\" + PersistentItem);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                };

            if (File.Exists(MountPath + "\\" + PersistentItem)) 
                {
                    string[] dirs = PersistentItem.Split('\\');
                    string dirname=dirs[dirs.Length - 1];

                    Directory.CreateDirectory(InstancePath + "\\" + dirname);
                    File.Copy(InstancePath + "\\" + PersistentItem, MountPath + "\\" + PersistentItem);
                    File.Delete(InstancePath + "\\" + PersistentItem);
                    try
                    {
                        Process.Start("mklink /d "+ InstancePath + "\\" + PersistentItem + " " + MountPath + "\\" + PersistentItem);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                };
            if (item == string.Empty) { throw new ArgumentException("The resource couldnt be persisted. No such file or directory."); };
        }
    }
}
