using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DbUpgrade.Core;
using System.IO;
using Microsoft.SqlServer.Dac;

namespace DbUpgrade.Tests
{
    [TestClass]
    public class CoreTests
    {

        private static string userName = "sa";
        private static string password = "abc123$%";
        private string modelConnection = $"Server=tcp:127.0.0.1,1433;Persist Security Info=False;User ID={userName};Password={password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;";
        private string anotherServerConnection = $"Server=tcp:127.0.0.1,1433;Persist Security Info=False;User ID={userName};Password={password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;";

        [TestMethod]
        public void GenerateDacPacTest()
        {
            var dbConnection = DbConnection.From(modelConnection, "unemployment");
            var service = new DbUpgradeService(dbConnection);
            var result = service.GenerateDacPac();

            var fileName = $"{Guid.NewGuid()}.dacpac";
            File.WriteAllBytes(fileName, result);

            Assert.IsNotNull(result);
            Assert.IsTrue(File.Exists(fileName));
        }

        [TestMethod]
        public void GenerateScriptTest()
        {
            var dbConnection = DbConnection.From(modelConnection, "unemployment");
            var service = new DbUpgradeService(dbConnection);
            var dacpac = service.GenerateDacPac();

            var script = service.GenerateUpgradeScript(dacpac, DbUpgradeOptions.From(ObjectType.Users, ObjectType.Views, ObjectType.RoleMembership, ObjectType.Permissions, ObjectType.ExtendedProperties, ObjectType.StoredProcedures, ObjectType.Logins, ObjectType.DatabaseTriggers, ObjectType.ServerTriggers), "newdb");

            Assert.IsNotNull(script);
        }

        [TestMethod]
        public void GenerateScriptToAnotherServerTest()
        {
            var dbConnection = DbConnection.From(modelConnection, "unemployment");
            var service = new DbUpgradeService(dbConnection);
            var dacpac = service.GenerateDacPac();

            var anotherServer = DbConnection.From(anotherServerConnection, "newdb");
            var script = service.GenerateUpgradeScript(dacpac, anotherServer, DbUpgradeOptions.From(ObjectType.Users, ObjectType.Views, ObjectType.RoleMembership, ObjectType.Permissions, ObjectType.ExtendedProperties, ObjectType.StoredProcedures, ObjectType.Logins, ObjectType.DatabaseTriggers, ObjectType.ServerTriggers));

            Assert.IsNotNull(script);
        }
    }
}
