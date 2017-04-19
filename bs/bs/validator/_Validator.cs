using System.Collections.Generic;

namespace com.bsidesoft.cs {
    public partial class bs {
        public class ValiResult {
            public object result { get; internal set; }
            public string msg { get; internal set; }
            public object value { get; internal set; }
        }
        public static Dictionary<string, ValiResult> valiResult() { return null; }
        public static Vali vali(string key) {
            Vali vali;
            if(!Vali.VALI.TryGetValue(key, out vali)) {
                log("vali:fail to get - " + key);
                return null;
            }
            return vali;
        }
        public static Vali vali(string key, params string[] kv) {
            return vali(key, opt(kv));
        }
        public static Vali vali(string key, Dictionary<string, string> opt) {
            Vali v = vali(key);
            if(v == null) {
                v = new Vali();
                if(!Vali.VALI.TryAdd(key, v)) {
                    log("vali:fail to add - " + key);
                    return null;
                }
            }
            if(opt != null) foreach(var k in opt) v.add(k.Key, new RuleSet(k.Value));
            return v;
        }
    }
}