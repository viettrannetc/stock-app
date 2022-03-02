using DotNetCoreSqlDb.Common.ArrayExtensions;
using DotNetCoreSqlDb.Models;
using DotNetCoreSqlDb.Models.Business;
using DotNetCoreSqlDb.Models.Business.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotNetCoreSqlDb.Common
{
    public static class ReportModellExtension
    {
        public static List<ReportFormularModel> Plus(this List<ReportFormularModel> formulars, ReportFormularModel newObject)
        {
            if (newObject != null && newObject.IsActive)
                formulars.Add(newObject);

            return formulars;
        }

    }

}
