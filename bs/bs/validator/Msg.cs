using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace com.bsidesoft.cs {
    public partial class bs {
        public class Msg {
            internal static ConcurrentDictionary<string, Msg> MSGS = new ConcurrentDictionary<string, Msg>();
            private static Func<object, string, string[], Dictionary<string, object>, string> F = (v, r, arg, safe) => "error";
            internal Msg(Func<object, string, string[], Dictionary<string, object>, string> func) {
                f = func;
            }
            private Func<object, string, string[], Dictionary<string, object>, string> f = F;
            virtual public string msg(object value, string rule, string[] arg, Dictionary<string, object> safe) {
                return f(value, rule, arg, safe);
            }
        }
    }
}