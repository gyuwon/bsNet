using System.Collections.Concurrent;
using System.Collections.Generic;

namespace com.bsidesoft.cs {
    public partial class bs {
        public class Vali {
            internal static ConcurrentDictionary<string, Vali> VALI = new ConcurrentDictionary<string, Vali>();
            private Dictionary<string, RuleSet> ruleSets = new Dictionary<string, RuleSet>();
            private string msg;
            internal void add(string key, RuleSet r) {
                ruleSets.Add(key, r);
            }
            internal void add(string key, string r) {
                ruleSets.Add(key, new RuleSet(r));
            }
            public bool check(out Dictionary<string, ValiResult> result, params string[] kv) {
                return check(out result, opt(kv));
            }
            public bool check(out Dictionary<string, ValiResult> result, Dictionary<string, string> opt) {
                bool r = true;
                ValiResult vr = null;
                var safe = new Dictionary<string, object>();
                result = new Dictionary<string, ValiResult>();
                foreach(var ruleSet in ruleSets) {
                    var k = ruleSet.Key;
                    var v = opt.ContainsKey(k) ? opt[k] : FAIL;
                    ruleSet.Value.setMsg(msg);
                    vr = ruleSet.Value.check(v, safe);
                    result.Add(k, vr);
                    if(vr.result == FAIL) r = false;
                    else safe.Add(k, vr.value);
                }
                return r;
            }
            public void setMsg(string q) {
                msg = q;
            }
        }
    }
}