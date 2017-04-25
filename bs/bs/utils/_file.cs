using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace com.bsidesoft.cs {
    public partial class bs {
        private static readonly string pathSep = Path.DirectorySeparatorChar.ToString();
        private static readonly Regex pathNormal = new Regex(pathSep + pathSep + (pathSep == "\\" ? pathSep + pathSep : ""));

        private static string pathNormalize(bool isWeb, string[] v) {
            var r = path(isWeb) + String.Join(pathSep, v);
            r = pathNormal.Replace(r, pathSep);
            return r;
        }
        public static T file<T>(bool isWeb, params string[] p) {
            var path = pathNormalize(isWeb, p);
            var result = default(T);
            if(!File.Exists(path)) return result;
            switch(TYPES[typeof(T)]) {
            case "string": return (dynamic)File.ReadAllText(path);
            case "string[]": return (dynamic)File.ReadAllLines(path);
            case "filestream": return (dynamic)new FileStream(path, FileMode.Open);
            case "streamreader": return (dynamic)new StreamReader(new FileStream(path, FileMode.Open));
            case "jobject": return (dynamic)JObject.Parse(File.ReadAllText(path));
            }
            return result;
        }
    }
}