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
        public static Dictionary<string, T> opt<T>(string[] kv) {
            Dictionary<string, T> opt = null;
            if(kv.Length > 0) {
                if(kv.Length % 2 != 0) {
                    log("opt:invalid params(length needs even:...k,v)" + kv.Length);
                    return opt;
                }
                opt = new Dictionary<string, T>();
                for(var i = 0; i < kv.Length;) opt.Add(kv[i++], (dynamic)kv[i++]);
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
        /*
        public static Dictionary<string, string> opt(JObject obj) {
            
        }*/
    }
}
