using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace com.bsidesoft.cs {
    public partial class bs {
        private static readonly string pathSep = Path.DirectorySeparatorChar.ToString();
        private static readonly Regex pathNormal = new Regex(pathSep + pathSep + (pathSep == "\\" ? pathSep + pathSep : ""));
        private static string pathNormalize(bool isWeb, string[] v) {
            var r = path(isWeb) + pathSep + String.Join(pathSep, v);
            r = pathNormal.Replace(r, pathSep);
            return r;
        }
        private static string path(bool isWeb = false) {
            if(environment == null) {
                return Directory.GetCurrentDirectory() + pathSep + (isWeb ? "wwwroot" : "");
            } else {
                return isWeb ? environment.WebRootPath : environment.ContentRootPath;
            }
        }
        public static string path(bool isWeb, params string[] p) {
            return pathNormalize(isWeb, p);
        }
        public static FileInfo fi(bool isWeb, params string[] p) {
            return new FileInfo(pathNormalize(isWeb, p));
        }
        public static bool fd(bool isWeb, params string[] p) {
            return fd(new FileInfo(pathNormalize(isWeb, p)));
        }
        public static bool fd(FileInfo i) {
            if(!i.Exists) return false;
            i.Delete();
            return true;
        }
        public static T fr<T>(bool isWeb, params string[] p) {
            var path = pathNormalize(isWeb, p);
            var result = default(T);
            if(!File.Exists(path)) return result;
            switch(TYPES[typeof(T)]) {
            case "string": return to<T>(File.ReadAllText(path)));
            case "string[]": return to<T>(File.ReadAllLines(path));
            case "byte[]": return to<T>(File.ReadAllBytes(path));
            case "filestream": return to<T>(new FileStream(path, FileMode.Open));
            case "streamreader": return to<T>(new StreamReader(new FileStream(path, FileMode.Open)));
            case "jobject": return to<T>(JObject.Parse(File.ReadAllText(path)));
            default: log("fileRead:invalid T - " + typeof(T)); break;
            }
            return result;
        }
        public static void fw<T>(T v, bool isWeb, params string[] p) {
            var path = pathNormalize(isWeb, p);
            var f = new StreamWriter(new FileStream(path, FileMode.OpenOrCreate));
            switch(TYPES[typeof(T)]) {
            case "string": f.Write((string)((dynamic)v)); break;
            case "string[]":
                foreach(var k in (string[])((dynamic)v)) f.WriteLine(k);
                break;
            case "jobject": f.Write(((JObject)((dynamic)v)).ToString()); break;
            default: log("fileWrite:invalid T - " + typeof(T)); break;
            }
        }
    }
}