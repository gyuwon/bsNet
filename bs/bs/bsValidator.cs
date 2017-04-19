using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;

namespace com.bsidesoft.cs {
    public partial class bs {

        public bs(IConfigurationRoot configuration) {
            dbInit(configuration);
        }
        
        public class Rule {
            private static ConcurrentDictionary<string, Rule> RULES = new ConcurrentDictionary<string, Rule>();
            public static Rule get(string key) {
                Rule rule;
                if(!RULES.TryGetValue(key, out rule)) {
                    log("Rule.get:fail to get - " + key);
                    return null;
                }
                return rule;
            }
            public static void add(string key, Rule rule) {
                if(!RULES.TryAdd(key, rule)) log("bsRule.add:fail to add - " + key);
            }
            private Func<object, string[], Dictionary<string, object>, object> isvalid;
            protected Rule() { }
            public Rule(Func<object, string[], Dictionary<string, object>, object> v) {
                isvalid = v;
            }
            virtual public object isValid(object value, string[] arg, Dictionary<string, object> safe) {
                return isvalid(value, arg, safe);
            }
            static Rule() {
                //extra
                add("creditCard", new RuleVali(new DataTypeAttribute(DataType.CreditCard)));
                add("html", new RuleVali(new DataTypeAttribute(DataType.Html)));
                add("image", new RuleVali(new DataTypeAttribute(DataType.ImageUrl)));
                //base
                add("ip", new RuleVali(new RegularExpressionAttribute("^((([0-9])|(1[0-9]{1,2})|(2[0-4][0-9])|(25[0-5]))[.]){3}(([0-9])|(1[0-9]{1,2})|(2[0-4][0-9])|(25[0-5]))$")));
                add("url", new RuleVali(new DataTypeAttribute(DataType.Url)));
                add("email", new RuleVali(new DataTypeAttribute(DataType.EmailAddress)));
                add("korean", new RuleVali(new RegularExpressionAttribute("^[ㄱ-힣]+$")));
                add("japanese", new RuleVali(new RegularExpressionAttribute("^[ぁ-んァ-ヶー一-龠！-ﾟ・～「」“”‘’｛｝〜−]+$")));
                add("alpha", new RuleVali(new RegularExpressionAttribute("^[a - z] +$")));
                add("ALPHA", new RuleVali(new RegularExpressionAttribute("^[A-Z]+$")));
                add("num", new RuleVali(new RegularExpressionAttribute("^-?[0-9.]+$")));
                add("alphanum", new RuleVali(new RegularExpressionAttribute("^[a-z0-9]+$")));
                add("1alpha", new RuleVali(new RegularExpressionAttribute("^[a-z]")));
                add("1ALPHA", new RuleVali(new RegularExpressionAttribute("^[A-Z]")));
                //type
                add("int", new Rule((value, arg, safe)=>{
                    if(value is string) {
                        if(!new RegularExpressionAttribute("^-?[0-9]+$").IsValid(value)) return FAIL;
                    }
                    return toI(value);
                }));
                add("float", new Rule((value, arg, safe) => {
                    if(value is string) {
                        if(!new RegularExpressionAttribute("^-?[0-9.]+$").IsValid(value)) return FAIL;
                    }
                    return toF(value);
                }));
            }
        }
        public class RuleVali:Rule {
            private ValidationAttribute validator;
            public RuleVali(ValidationAttribute v) {
                validator = v;
            }
            override public object isValid(object value, string[] arg, Dictionary<string, object> safe) {
                return validator.IsValid(value) ? value : FAIL;
            }
        }
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
        public class ValiResult {
            public object result { get; internal set; }
            public string msg { get; internal set; }
            public object value { get; internal set; }
        }
        class RuleSet {
            class Item {
                internal string rule, msg;
                internal string[] arg;
            }
            private static Item OR = new Item(), AND = new Item();
            private List<Item> rules = new List<Item>();
            private string baseMsg;
            internal RuleSet(string rules) {
                parse(rules);
            }
            internal void setMsg(string key) {
                baseMsg = key;
            }
            internal ValiResult check(object value, Dictionary<string, object> safe) {
                object isOk = OK;
                var logic = AND;
                var r = new ValiResult() { msg = "", result = OK };
                for(var i = 0; i < rules.Count;) {
                    var item = rules[i++];
                    var temp = Rule.get(item.rule).isValid(value, item.arg, safe);
                    if(logic == AND) {
                        if(temp == FAIL) { 
                            var m = item.msg;
                            if(m == "") m = baseMsg;
                            var message = Msg.get(m);
                            if(message == null) r.msg = "error : " + value;
                            else r.msg = message.msg(value, item.rule, item.arg, safe);
                            r.result = FAIL;
                            break;
                        } else value = temp;
                    } else if(temp != FAIL) value = temp;
                }
                r.value = value;
                return r;
            }
            private void parse(string rule) {
                bool isToken = true;
                rules.Clear();
                rule = rule.Trim();
                if(rule.Length == 0) return;
                foreach(var token in rule.Split('|')) {
                    int i;
                    string ruleKey = token, msg;
                    string[] arg;
                    if(!isToken) {
                        isToken = true;
                        if(ruleKey == "or") {
                            rules.Add(OR);
                            continue;
                        } else rules.Add(AND);
                    }
                    if(ruleKey.Contains(":")) {
                        i = ruleKey.IndexOf(':');
                        msg = ruleKey.Substring(i + 1);
                        ruleKey = ruleKey.Substring(0, i);
                    } else msg = "";
                    if(ruleKey.Contains("[")) {
                        i = ruleKey.IndexOf('[');
                        arg = ruleKey.Substring(i + 1, ruleKey.Length - i - 2).Split(',');
                        ruleKey = ruleKey.Substring(0, i);
                    } else arg = null;
                    rules.Add(new Item() { rule = ruleKey, arg = arg, msg = msg });
                    isToken = false;
                }
            }
        }
        public static Dictionary<string, ValiResult> valiResult() { return null; }
        public class Vali {
            private static ConcurrentDictionary<string, Vali> VALI = new ConcurrentDictionary<string, Vali>();
            public static Vali get(string key) {
                Vali vali;
                if(!VALI.TryGetValue(key, out vali)) {
                    log("Vali.get:fail to get - " + key);
                    return null;
                }
                return vali;
            }
            public static void add(string key, params string[] kv) {
                add(key, opt(kv));
            }
            public static void add(string key, Dictionary<string, string> opt) {
                if(VALI.ContainsKey(key)) {
                    log("Vali.add:exist key - " + key);
                    return;
                }
                if(opt == null) return;
                Vali vali = new Vali();
                foreach(var k in opt) vali.add(k.Key, new RuleSet(k.Value));
                if(!VALI.TryAdd(key, vali)) log("Vali.add:fail to add - " + key);
            }
            private Dictionary<string, RuleSet> ruleSets = new Dictionary<string, RuleSet>();
            private string msg;
            private void add(string key, RuleSet r) {
                ruleSets.Add(key, r);
            }
            internal void add(string key, string r) {
                ruleSets.Add(key, new RuleSet(r));
            }
            public bool check(out Dictionary<string, ValiResult> result, params string[] kv) {
                return check(out result, opt(kv));
            }
            public bool check(out Dictionary<string, ValiResult> result, Dictionary<string,string> opt) {
                bool r = true;
                var safe = new Dictionary<string, object>();
                result = new Dictionary<string, ValiResult>();
                foreach(var rule in ruleSets) {
                    var v = opt[rule.Key];
                    if(v == null) {
                        r = false;
                        log("check:fail to get opt - " + rule.Key);
                        break;
                    }
                    rule.Value.setMsg(msg);
                    var vr = rule.Value.check(v, safe);
                    result.Add(rule.Key, vr);
                    if(vr.result == FAIL) r = false;
                    else safe.Add(rule.Key, vr.value);
                }
                return r;
            }

            internal void setMsg(string q) {
                msg = q;
            }
        }
    }
}