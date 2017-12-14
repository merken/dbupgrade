using Microsoft.SqlServer.Dac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbUpgrade.Core
{
    public class DbUpgradeOptions
    {
        public ObjectType[] IgnoreObjectTypes { get; set; }
        public bool IgnoreNotForReplication { get; set; }
        public bool DropConstraintsNotInSource { get; set; }
        public bool DropIndexesNotInSource { get; set; }
        public bool VerifyDeployment { get; set; }

        public static DbUpgradeOptions From(params ObjectType[] ignoreObjectTypes)
        {
            return new DbUpgradeOptions
            {
                IgnoreObjectTypes = ignoreObjectTypes
            };
        }
    }
}
