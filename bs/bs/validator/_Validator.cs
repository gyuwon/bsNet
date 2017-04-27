using System;
using System.Collections.Generic;

namespace com.bsidesoft.cs {
    public partial class bs {
        public class ValiResult {
            public object result { get; internal set; }
            public string msg { get; internal set; }
            public object value { get; internal set; }
            public Dictionary<string, Object> toDic() {
                return new Dictionary<string, Object>() {
                    {"result", result == OK ? true : false},
                    {"msg", msg},
                    {"value", value}
                };
            }
        }
        public static Dictionary<string, Dictionary<string, Object>> toDicValiResult(Dictionary<string, ValiResult> valiResult) {
            var result = new Dictionary<string, Dictionary<string, Object>>();
            foreach (var vali in valiResult) {
                result.Add(vali.Key, vali.Value.toDic());
            }
            return result;
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
        public static Vali vali(string key, Dictionary<string, object> opt) {
            Vali v = vali(key);
            if(v == null) {
                v = new Vali();
                if(!Vali.VALI.TryAdd(key, v)) {
                    log("vali:fail to add - " + key);
                    return null;
                }
            }
            if(opt != null) foreach(var k in opt) v.add(k.Key, new RuleSet(k.Value + ""));
            return v;
        }
        public static Msg msg(string key) {
            Msg m;
            if(!Msg.MSGS.TryGetValue(key, out m)) {
                log("msg:fail to get - " + key);
                return null;
            }
            return m;
        }
        public static Msg msg(string key, Func<object, string, string[], Dictionary<string, object>, string> f) {
            Msg v = msg(key);
            if(v == null) {
                v = new Msg(f);
                if(!Msg.MSGS.TryAdd(key, v)) {
                    log("msg:fail to add - " + key);
                    return null;
                }
            }
            return v;
        }
    }
}