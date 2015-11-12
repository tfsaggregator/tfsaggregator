using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTests.Plugin
{
    public class TfsFixture : IDisposable
    {
        // configuration variables
        protected string SQLInstance { get; private set; }
        protected string SQLDataDir { get; private set; }
        protected string CollectionName { get; private set; }
        protected string ProjectName { get; private set; }
        // computed values
        protected string TfsPluginsFolder { get; private set; }
        protected string TfsConfigExePath { get; private set; }
        protected string TfsServiceControlExePath { get; private set; }

        public TfsFixture()
        {
            this.SQLInstance = string.Format(@"{0}\SQLEXPRESS", Environment.MachineName);
            this.CollectionName = "TfsAggregator2IntegrationTest";
            this.ProjectName = "ScrumTfvc";
            this.SQLDataDir = @"C:\Program Files\Microsoft SQL Server\MSSQL12.SQLEXPRESS\MSSQL\DATA";

            this.DetectTeamFoundationSever();
            this.DropCollectionDatabase();
            this.PushAggregator();
            this.RestoreCollectionDatabase();

            var tfsUrl = new Uri("http://localhost:8080/tfs/" + this.CollectionName);
            this.Collection = new TfsTeamProjectCollection(tfsUrl);
            this.WorkItemStore = (WorkItemStore)this.Collection.GetService(typeof(WorkItemStore));
            this.Project = this.WorkItemStore.Projects[this.ProjectName];
        }

        public TfsTeamProjectCollection Collection { get; private set; }
        public WorkItemStore WorkItemStore { get; private set; }
        public Project Project { get; private set; }

        public void PushPolicies(string policyFilename)
        {
            File.Copy(
                string.Format(
                    @"..\..\ConfigurationsForTests\{0}.policies",
                    policyFilename),
                Path.Combine(
                    this.TfsPluginsFolder,
                    "TFSAggregator2.ServerPlugin.policies"),
                true);
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
                    this.TfsPluginsFolder = Path.Combine(tfsInstallPath, @"Application Tier\Web Services\bin\Plugins");
                    this.TfsConfigExePath = Path.Combine(tfsInstallPath, @"Tools\TfsConfig.exe");
                    this.TfsServiceControlExePath = Path.Combine(tfsInstallPath, @"Tools\TfsServiceControl.exe");
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

        [SuppressMessage("Microsoft.Security", "CA2100")]
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

        private void DropCollectionDatabase()
        {
            Run(this.TfsServiceControlExePath, "unquiesce");

            Run(this.TfsConfigExePath, string.Format("collection /delete /collectionName:{0} /noprompt", this.CollectionName));
            ExecSql(string.Format(@"ALTER DATABASE [Tfs_{0}] SET  SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [Tfs_{0}]", this.CollectionName));

            File.Delete(Path.Combine(this.SQLDataDir, string.Format("Tfs_{0}.mdf", this.CollectionName)));
            File.Delete(Path.Combine(this.SQLDataDir, string.Format("Tfs_{0}_log.ldf", this.CollectionName)));
        }

        private void PushAggregator()
        {
            //TODO: for now it is a post-build event
        }

        protected void RestoreCollectionDatabase()
        {
            ZipFile.ExtractToDirectory(
                string.Format(
                    @"..\..\{0}.zip",
                    this.CollectionName),
                this.SQLDataDir);

            ExecSql(string.Format(@"CREATE DATABASE [Tfs_{0}] ON (FILENAME = N'{1}\Tfs_{0}.mdf'), (FILENAME = N'{1}\Tfs_{0}_log.ldf') FOR ATTACH", this.CollectionName, this.SQLDataDir));
            Run(this.TfsServiceControlExePath, "unquiesce");
            Run(this.TfsConfigExePath, string.Format(@"collection /attach /collectionDb:{1};Tfs_{0} /noprompt", this.CollectionName, this.SQLInstance));
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects).
                    this.Collection.Dispose();
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
