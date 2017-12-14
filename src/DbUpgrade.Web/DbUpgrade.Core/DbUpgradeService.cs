using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Dac;
using System.IO;

namespace DbUpgrade.Core
{
    public class DbConnection
    {
        public string DatabaseName { get; set; }
        public string ConnectionString { get; set; }

        public static DbConnection From(string connectionString, string databaseName)
        {
            return new DbConnection { ConnectionString = connectionString, DatabaseName = databaseName };
        }
    }

    public class DbUpgradeService
    {
        private readonly DacServices dacServices;
        private readonly DbConnection sourceConnection;
        private readonly Version version;
        private readonly string applicationName;

        public DbUpgradeService(DbConnection sourceConnection)
        {
            this.sourceConnection = sourceConnection;
            dacServices = new DacServices(this.sourceConnection.ConnectionString);
            version = new Version();
            applicationName = "DbUpgrade";
        }

        public byte[] GenerateDacPac()
        {
            return GenerateDacPac(sourceConnection.DatabaseName);
        }

        private byte[] GenerateDacPac(string databaseName)
        {
            using (var stream = new MemoryStream())
            {
                dacServices.Extract(stream, databaseName, applicationName, version);
                stream.Seek(0, SeekOrigin.Begin);
                return stream.ToArray();
            }
        }

        public string GenerateUpgradeScript(byte[] dacpac, DbUpgradeOptions options, string databaseName = "")
        {
            return GenerateUpgradeScript(sourceConnection, dacpac, options, databaseName);
        }

        public string GenerateUpgradeScript(byte[] dacpac, DbConnection target, DbUpgradeOptions options)
        {
            return GenerateUpgradeScript(target, dacpac, options);
        }

        private string GenerateUpgradeScript(DbConnection dbConnection, byte[] dacpac, DbUpgradeOptions options, string databaseNameOverride = "")
        {
            var dacServices = new DacServices(dbConnection.ConnectionString);
            var databaseName = dbConnection.DatabaseName;

            if (!String.IsNullOrEmpty(databaseNameOverride))
                databaseName = databaseNameOverride;

            using (var stream = new MemoryStream(dacpac))
            {
                var publishResult = dacServices.Script(DacPackage.Load(stream, DacSchemaModelStorageType.Memory), dbConnection.DatabaseName, new PublishOptions
                {
                    GenerateDeploymentScript = true,
                    DeployOptions = new DacDeployOptions
                    {
                        AllowIncompatiblePlatform = true,
                        IgnoreNotForReplication = options.IgnoreNotForReplication,
                        DropConstraintsNotInSource = options.DropConstraintsNotInSource,
                        DropIndexesNotInSource = options.DropIndexesNotInSource,
                        VerifyDeployment = options.VerifyDeployment,
                        ExcludeObjectTypes = options.IgnoreObjectTypes// new ObjectType[] { ObjectType.Users, ObjectType.Views, ObjectType.RoleMembership, ObjectType.Permissions, ObjectType.ExtendedProperties, ObjectType.StoredProcedures, ObjectType.Logins, ObjectType.DatabaseTriggers, ObjectType.ServerTriggers }
                    }
                });

                return publishResult.DatabaseScript;
            }
        }
    }
}
