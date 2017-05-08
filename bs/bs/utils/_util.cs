using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace com.bsidesoft.cs {
    public partial class bs {
        public static readonly object OK = new { Name = "OK" }, FAIL = new { Name = "FAIL" };
        public static bool isOK(object v) {
            return v == OK;
        }
        public static bool isFAIL(object v) {
            return v == FAIL;
        }
        public static Dictionary<string, T> opt<T>(object[] kv) {
            if(kv.Length == 0) {
                log("opt:no params");
                return null;
            }
            if(kv.Length % 2 != 0) {
                log("opt:length is odd - " + kv.Length);
                return null;
            }
            var opt = new Dictionary<string, T>();
            var type = typeof(T);
            for(var i = 0; i < kv.Length;) {
                object k = kv[i++], v = kv[i++];
                if(v is T) opt.Add(k + "", (T)Convert.ChangeType(v, type));
                else {
                    log("opt:invalid typeT - " + typeof(T) + " - " + v);
                    return null;
                }
            }
            return opt;
        }   

        public static Dictionary<string, object> json2kv(JObject j, params String[] keys) {
            var result = new Dictionary<string, object>();
            foreach (var k in keys) {
                JToken v;
                if(j.TryGetValue(k, out v)) {
                    result.Add(k, v + "");
                } else {
                    result.Add(k, FAIL);
                }
            }
            return result;
        }
    }
}
