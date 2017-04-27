using Newtonsoft.Json.Linq;
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
        /*
        public static Dictionary<string, string> opt(JObject obj) {
            
        }*/
    }
}
