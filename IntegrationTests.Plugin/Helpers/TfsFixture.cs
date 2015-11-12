using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTests.Plugin
{
    public class TfsFixture : IDisposable
    {
        protected string SQLInstance { get; private set; }
        protected string SQLDataDir { get; private set; }
        protected string Collection { get; private set; }
        protected string Project { get; private set; }
        protected string TfsConfig { get; private set; }
        protected string TfsServiceControl { get; private set; }

        public TfsFixture()
        {
            this.SQLInstance = string.Format(@"{0}\SQLEXPRESS", Environment.MachineName);
            this.Collection = "TfsAggregator2IntegrationTest";
            this.Project = "ScrumTfvc";
            this.SQLDataDir = @"C:\Program Files\Microsoft SQL Server\MSSQL12.SQLEXPRESS\MSSQL\DATA";

            this.DetectTeamFoundationSever();
            this.RestoreCollectionDatabase();
        }

        protected void DetectTeamFoundationSever()
        {
            using (var tfsBaseKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\TeamFoundationServer"))
            {
                var versionKeys = tfsBaseKey.GetSubKeyNames();
                double dummy;
                double maxVersion = versionKeys.Max(x => double.TryParse(x, out dummy) ? dummy : 0.0);
                var latestVersionKey = maxVersion.ToString("#.0");
                using (var tfsKey = tfsBaseKey.OpenSubKey(latestVersionKey))
                {
                    string tfsInstallPath = tfsKey.GetValue("InstallPath").ToString();
                    this.TfsConfig = Path.Combine(tfsInstallPath, @"Tools\TfsConfig.exe");
                    this.TfsServiceControl = Path.Combine(tfsInstallPath, @"Tools\TfsServiceControl.exe");
                }
            }
        }

        private void Run(string programPath, string programArgs)
        {
            using (var proc = new Process())
            {
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.FileName = programPath;
                proc.StartInfo.Arguments = programArgs;
                proc.StartInfo.CreateNoWindow = true;
                proc.Start();
                string stdout = proc.StandardOutput.ReadToEnd();
                string stderr = proc.StandardError.ReadToEnd();
                Debug.WriteLine(stdout);
                Debug.WriteLine(stderr);
                proc.WaitForExit(5000);
            }
        }

        private void ExecSql(string command)
        {
            string connString = string.Format(@"Data Source={0};Initial Catalog=master;Integrated Security=True", this.SQLInstance);
            using (var conn = new SqlConnection(connString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = command;
                    cmd.CommandTimeout = 5000;
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        protected void RestoreCollectionDatabase()
        {
            Run(this.TfsServiceControl, "unquiesce");

            Run(this.TfsConfig, string.Format("collection /delete /collectionName:{0} /noprompt", this.Collection));
            ExecSql(string.Format(@"ALTER DATABASE [Tfs_{0}] SET  SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [Tfs_{0}]", this.Collection));

            File.Delete(Path.Combine(this.SQLDataDir, string.Format("Tfs_{0}.mdf", this.Collection)));
            File.Delete(Path.Combine(this.SQLDataDir, string.Format("Tfs_{0}_log.ldf", this.Collection)));
            ZipFile.ExtractToDirectory(this.Collection + ".zip", this.SQLDataDir);

            ExecSql(string.Format(@"CREATE DATABASE [Tfs_{0}] ON (FILENAME = N'{1}\Tfs_{0}.mdf'), (FILENAME = N'{1}\Tfs_{0}_log.ldf') FOR ATTACH", this.Collection, this.SQLDataDir));
            Run(this.TfsServiceControl, "unquiesce");
            Run(this.TfsConfig, string.Format(@"collection /attach /collectionDb:{1};Tfs_{0} /noprompt", this.Collection, this.SQLInstance));
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~TfsFixture() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}
