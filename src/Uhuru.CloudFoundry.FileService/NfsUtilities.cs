// -----------------------------------------------------------------------
// <copyright file="NfsUtilities.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

using System;


namespace Uhuru.CloudFoundry.FileService
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Diagnostics;

    /// <summary>
    /// This utilities class contains helper functions for managing NFS shared directories
    /// </summary>

    class NfsUtilities
    {
        public static int CreateNfsShare(string name, string directory, string user)
        {
            //FIXME: define user access
            try
            {
                Process.Start("nfsshare", "-o root rw " + name + "=" + directory);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }
            return 0;
        }

        public static int DeleteNfsShare(string name)
        {
            try
            {
                Process.Start("nfsshare", name + "/delete");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }
            return 0;
        }
    }
}
