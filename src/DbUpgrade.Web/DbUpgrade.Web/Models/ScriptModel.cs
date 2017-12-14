using Microsoft.SqlServer.Dac;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DbUpgrade.Web.Models
{
    public class ScriptModel : DacPacModel
    {
        [DataType(DataType.Upload)]
        public HttpPostedFileBase DacPac { get; set; }

        public ObjectType IgnoreObjectType { get; set; }
        public ObjectType[] IgnoreObjectTypes { get; set; }
        public ObjectType ObjectTypeFlags => IgnoreObjectTypes?.Aggregate((a, e) => a | e) ?? 0;

        public bool IgnoreNotForReplication { get; set; }
        public bool DropConstraintsNotInSource { get; set; }
        public bool DropIndexesNotInSource { get; set; }
        public bool VerifyDeployment { get; set; }

        public SelectList ToSelectList()
        {
            SelectList selectList = null;

            // Cast the enum values to strings then linq them into a key value pair we can use for the select list.
            var selectListItemList = Enum.GetValues(typeof(ObjectType)).Cast<ObjectType>().Select(v => new SelectListItem
            {
                Text = v.ToString(),
                Value = ((int)v).ToString()
            }).ToList();

            // Build the select list from it.
            selectList = new SelectList(selectListItemList);

            return selectList;
        }
    }
}