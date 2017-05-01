using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.IO;

namespace com.bsidesoft.cs {
    public partial class bs {
        public static object before(Controller c) {
            return c.ViewBag.bsBefore;
        }

        private static Dictionary<string, string> CONTENT_TYPE = new Dictionary<string, string>(){
            {"xslx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"}
        };

        public PhysicalFileResult downXlsx(FileInfo i) {
            return down("xslx", i.FullName);
        }
        public PhysicalFileResult downXlsx(bool isWeb, params string[] p) {
            return down("xslx", pathNormalize(isWeb, p));
        }
        private PhysicalFileResult down(string type, string path) {
            return new PhysicalFileResult(path, CONTENT_TYPE[type]);
        }
    }
}