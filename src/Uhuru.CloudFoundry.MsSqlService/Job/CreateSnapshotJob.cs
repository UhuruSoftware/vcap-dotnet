// -----------------------------------------------------------------------
// <copyright file="CreateSnapshotJob.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.CloudFoundry.MSSqlService.Job
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Uhuru.CloudFoundry.ServiceBase.Worker.AsyncJob;
    using Uhuru.CloudFoundry.ServiceBase.Objects;
    using Uhuru.Configuration;
    using System.Configuration;
    using Uhuru.CloudFoundry.ServiceBase.Worker.Objects;
    using Newtonsoft.Json;
    using Uhuru.Utilities;
    using System.IO;
    using System.Transactions;
    using System.Globalization;
    using System.Data.SqlClient;
    using Uhuru.CloudFoundry.ServiceBase;
    using SevenZip;
    using System.Threading;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class CreateSnapshotJob : BaseCreateSnapshotJob
    {
        SqlCommand cmdBackup;
        SevenZipCompressor compressor;
        bool cancel;
        bool compressionStopped;

        /// <summary>
        /// Gets the connection string used to connect to the SQL Server.
        /// </summary>
        private string ConnectionString
        {
            get
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.UserID = this.config.MSSql.User;
                builder.Password = this.config.MSSql.Password;
                builder.DataSource = string.Format(CultureInfo.InvariantCulture, "{0},{1}", this.config.MSSql.Host, this.config.MSSql.Port);
                builder.Pooling = false;
                builder.MultipleActiveResultSets = true;

                return builder.ConnectionString;
            }
        }

        public CreateSnapshotJob() : base() 
        {
            this.cancel = false;
        }

        public void Perform(string args)
        {
            List<object> objects = JsonConvert.DeserializeObject<List<object>>(args);
            this.JobId = objects[0].ToString();
            CreateSnapshotRequest request = JsonConvert.DeserializeObject<CreateSnapshotRequest>(objects[1].ToString());
            base.Perform(request);
        }

        protected override SnapshotObject Execute()
        {
            string dumpPath;
            SqlConnection connection = new SqlConnection(this.ConnectionString);

            try
            {
                dumpPath = GetDumpPath(this.Name, this.SnapshotId);

                if (!Directory.Exists(dumpPath))
                {
                    Directory.CreateDirectory(dumpPath);
                }

                string backupFileName = string.Concat(this.SnapshotId.ToString(), ".bak");
                string dumpFileName = Path.Combine(dumpPath, backupFileName);

                string sqlCommand = string.Format(CultureInfo.InvariantCulture, "BACKUP DATABASE {0} TO DISK = '{1}'", this.Name, dumpFileName);

                DateTime t1 = DateTime.Now;
                connection.Open();

                try
                {
                    using (cmdBackup = new SqlCommand(sqlCommand, connection))
                    {
                        cmdBackup.ExecuteNonQuery();
                    }
                }
                catch (SqlException ex)
                {
                    if (this.cancel)
                    {
                        return null;
                    }
                    else
                    {
                        throw ex;
                    }
                }

                string fileName = string.Concat(this.SnapshotId.ToString(), ".bak.gz");
                string finalPath = Path.Combine(dumpPath, fileName);
                ZipUtilities.SetupZlib();
                compressor = new SevenZipCompressor();
                compressor.ArchiveFormat = OutArchiveFormat.GZip;
                compressor.BeginCompressFiles(finalPath, new string[] { dumpFileName });
                compressor.Compressing += new EventHandler<ProgressEventArgs>(compressor_Compressing);
                compressor.FileCompressionStarted += new EventHandler<FileNameEventArgs>(compressor_FileCompressionStarted);
                compressor.FileCompressionFinished += new EventHandler<EventArgs>(compressor_FileCompressionFinished);
                while (!this.compressionStopped)
                {
                    Thread.Sleep(200);
                }
                if (this.cancel)
                {
                    return null;
                }

                File.Delete(dumpFileName);

                DateTime t2 = DateTime.Now;
                Logger.Info("Backup for db {0} completed in {1} seconds", this.SnapshotId.ToString(), (t2 - t1).Seconds);

                SnapshotObject response = new SnapshotObject();
                response.Files = new string[] { finalPath };
                response.Size = new FileInfo(finalPath).Length;
                response.SnapshotId = this.SnapshotId;
                response.Manifest = new Manifest() { Version = 1 };

                return response;
            }
            finally
            {
                connection.Close();
            }
        }

        void compressor_FileCompressionFinished(object sender, EventArgs e)
        {
            compressionStopped = true;
        }

        void compressor_FileCompressionStarted(object sender, FileNameEventArgs e)
        {
            if (this.cancel)
            {
                e.Cancel = true;
                compressionStopped = true;
            }
        }

        void compressor_Compressing(object sender, ProgressEventArgs e)
        {
            if (this.cancel)
            {
                e.Cancel = true;
            }
        }

        protected override void Cancel()
        {
            this.cancel = true;
            if (cmdBackup != null)
            {
                cmdBackup.Cancel();
            }
        }
    }
}
