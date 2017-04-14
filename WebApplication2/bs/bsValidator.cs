using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace com.bsidesoft.cs {
    public partial class bs {
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
                /*
                add("int", new Rule((value, arg, safe)=>{

                });
                */
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
            private string key;
            private List<Item> rules = new List<Item>();
            internal RuleSet(string k, string rules) {
                key = k;
                parse(rules);
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
                            r.value = value;
                            r.msg = Msg.get(item.msg).msg(value, item.rule, item.arg, safe);
                            r.result = FAIL;
                            break;
                        } else value = temp;
                    } else if(temp != FAIL) value = temp;
                }
                return r;
            }
            private void parse(string rule) {
                bool isToken = true;
                rules.Clear();
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
        class Vali {
            private static ConcurrentDictionary<string, Vali> VALI = new ConcurrentDictionary<string, Vali>();
            static Vali get(string key) {
                Vali vali;
                if(!VALI.TryGetValue(key, out vali)) {
                    log("Vali.get:fail to get - " + key);
                    return null;
                }
                return vali;
            }
            public static void add(string key, params string[] kv) {
                if(VALI.ContainsKey(key)) {
                    log("Vali.add:exist key - " + key);
                    return;
                }
                Vali vali = new Vali();
                for(var i = 0; i < kv.Length;) {
                    string k = kv[i++], v = kv[i++];
                    vali.add(k, new RuleSet(k, v));
                }
                if(!VALI.TryAdd(key, vali)) log("Vali.add:fail to add - " + key);
            }
            private Dictionary<string, RuleSet> ruleSets = new Dictionary<string, RuleSet>();
            private void add(string key, RuleSet r) {
                ruleSets.Add(key, r);
            }
            public bool check(out Dictionary<string, ValiResult> result, params string[] kv) {
                bool r = true;
                var safe = new Dictionary<string, object>();
                result = new Dictionary<string, ValiResult>();
                for(var i = 0; i < kv.Length;) {
                    string k = kv[i++], v = kv[i++];
                    var vr = ruleSets[k].check(v, safe);
                    result.Add(k, vr);
                    if(vr.result == FAIL) r = false;
                    else safe.Add(k, vr.value);
                }
                return r;
            }
        }
    }
}