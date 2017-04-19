using System.Collections.Concurrent;
using System.Collections.Generic;

namespace com.bsidesoft.cs {
    public partial class bs {
        public class Msg {
            private static ConcurrentDictionary<string, Msg> MSGS = new ConcurrentDictionary<string, Msg>();
            public static Msg get(string key) {
                Msg m;
                if(!MSGS.TryGetValue(key, out m)) {
                    log("Msg.get:fail to get - " + key);
                    return null;
                }
                return m;
            }
            public static void add(string key, Msg m) {
                if(!MSGS.TryAdd(key, m)) log("Msg.add:fail to add - " + key);
            }
            virtual public string msg(object value, string rule, string[] arg, Dictionary<string, object> safe) {
                return "error";
            }
        }
    }
}
