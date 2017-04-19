using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.IO;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace com.bsidesoft.cs {
    public partial class bs{
        public static readonly object OK = new { }, FAIL = new { };
        public static bool isOK(object v) {
            return v == OK;
        }
        public static bool isFAIL(object v) {
            return v == FAIL;
        }

        private static ILogger logger;
        public static void setLogger(ILogger l) {
            logger = l;
        }
        public static void log(params string[] args) {
            logger.LogInformation(String.Join(", ", args));
        }

        public static int toI(object v) {
            if(v is string) return Convert.ToInt32((string)v);
            return (int)v;
        }
        public static float toF(object v) {
            if(v is string) return Convert.ToSingle((string)v);
            return (float)v;
        }
        public static double toD(object v) {
            if(v is string) return Convert.ToDouble((string)v);
            return (double)v;
        }
        public static string toS(object v) {
            if(v is string) return (string)v;
            return v + "";
        }
        public static Dictionary<string, string> opt(string[] kv) {
            Dictionary<string, string> opt = null;
            if(kv.Length > 0) {
                if(kv.Length % 2 != 0) {
                    log("opt:invalid params(length needs even:...k,v)" + kv.Length);
                    return opt;
                }
                opt = new Dictionary<string, string>();
                for(var i = 0; i < kv.Length;) opt.Add(kv[i++], kv[i++]);
            }
            return opt;
        }

        public bs() {
        }
        private Dictionary<string, object> s = new Dictionary<string, object>();
        public object S(params object[] kv) {
            object v = null;
            for(var i = 0; i < kv.Length;) {
                string k = (string)kv[i++];
                if(i == kv.Length) return s[k];
                v = kv[i++];
                if(v == null) s.Remove(k);
                s.Add(k, v);
            }
            return v;
        }
    }
}
