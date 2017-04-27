using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;

namespace com.bsidesoft.cs {
    public partial class bs {
        public static String reqPath(HttpRequest r) {
            return r.Path;
        }
        public static JObject reqJson(HttpRequest r) {
            return JObject.Parse(new StreamReader(r.Body, Encoding.UTF8, true, 20000, true).ReadToEnd());
        }
    }
}
