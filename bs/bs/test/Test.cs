using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace com.bsidesoft.cs {
    public partial class bs {
        public class Test {
            protected bs bs;
            public Test(params string[] arg) {
                string path = null;
                foreach(var k in arg) {
                    if(Directory.Exists(k)) {
                        path = k + "\\appsettings.json";
                        break;
                    }
                };
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(path, optional: false, reloadOnChange: true)
                    .AddEnvironmentVariables();
                var bs = new bs(null, null);
                bs.service(builder.Build(), null);
            }
        }
    }
}