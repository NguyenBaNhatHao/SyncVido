using Microsoft.Synchronization;
using Microsoft.Synchronization.Data;
using Microsoft.Synchronization.Data.SqlServer;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncVido
{
    internal class Program
    {
        static string sServerConnection =
    @"Server=localhost;Database=Company;User Id=sincollmm;Password=zxcZXCV123";

        static string sClientConnection =
            @"Server=(localdb)\Local2;Database=Company;User Id=sincill;Password=zxcZXCV123";

        static string sScope = "EmployeeScope";
        public static void ProvisionServer()
        {
            SqlConnection serverConn = new SqlConnection(sServerConnection);

            DbSyncScopeDescription scopeDesc = new DbSyncScopeDescription(sScope);

            DbSyncTableDescription tableDesc = SqlSyncDescriptionBuilder.GetDescriptionForTable("Employee", serverConn);

            scopeDesc.Tables.Add(tableDesc);

            SqlSyncScopeProvisioning serverProvision = new SqlSyncScopeProvisioning(serverConn, scopeDesc);
            serverProvision.SetCreateTableDefault(DbSyncCreationOption.Skip);

            serverProvision.Apply();
        }
        public static void ProvisionClient()
        {
            SqlConnection serverConn = new SqlConnection(sServerConnection);

            SqlConnection clientConn = new SqlConnection(sClientConnection);

            DbSyncScopeDescription scopeDesc = SqlSyncDescriptionBuilder.GetDescriptionForScope(sScope, serverConn);

            SqlSyncScopeProvisioning clientProvision = new SqlSyncScopeProvisioning(clientConn, scopeDesc);

            clientProvision.Apply();
        }
        private static void Sync()
        {
            SqlConnection serverConn = new SqlConnection(sServerConnection);

            SqlConnection clientConn = new SqlConnection(sClientConnection);

            SyncOrchestrator syncOrchestrator = new SyncOrchestrator();

            syncOrchestrator.LocalProvider = new SqlSyncProvider(sScope, clientConn);

            syncOrchestrator.RemoteProvider = new SqlSyncProvider(sScope, serverConn);

            syncOrchestrator.Direction = SyncDirectionOrder.Download;
            ((SqlSyncProvider)syncOrchestrator.LocalProvider).ApplyChangeFailed += new EventHandler<DbApplyChangeFailedEventArgs>(Program_ApplyChangeFailed);

            SyncOperationStatistics syncStats = syncOrchestrator.Synchronize();

            Console.WriteLine("Start Time: " + syncStats.SyncStartTime);

            Console.WriteLine("Total Changes Uploaded: " + syncStats.UploadChangesTotal);

            Console.WriteLine("Total Changes Downloaded: " + syncStats.DownloadChangesTotal);

            Console.WriteLine("Complete Time: " + syncStats.SyncEndTime);

            Console.WriteLine(String.Empty);

            Console.ReadLine();

        }
        static void Program_ApplyChangeFailed(object sender, DbApplyChangeFailedEventArgs e)
        {
            Console.WriteLine(e.Conflict.Type);
            Console.WriteLine(e.Error);
        }
        static void Main(string[] args)
        {
            ProvisionServer();
            ProvisionClient();
            Sync();
        }
    }
}
