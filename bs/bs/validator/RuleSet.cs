using System.Collections.Generic;

namespace com.bsidesoft.cs {
    public partial class bs {
        internal class RuleSet {
            class Item {
                internal string rule, msg;
                internal string[] arg;
            }
            private static Item OR = new Item(), AND = new Item();
            private List<Item> rules = new List<Item>();
            private string baseMsg;
            private bool optional;

            internal RuleSet(string rules) {
                parse(rules);
            }
            internal void setMsg(string key) {
                baseMsg = key;
            }
            internal ValiResult check(object value, Dictionary<string, object> safe) {
                var r = new ValiResult() { msg = "", result = OK };
                if(value == FAIL) {
                    if (!optional) {
                        r.result = FAIL;
                        r.msg = "Cannot found key";
                        r.value = null;
                    }
                } else for (var i = 0; i < rules.Count;) {
                    var item = rules[i++];
                    var rule = Rule.get(item.rule);
                    if(rule == null) {
                        r.msg = "Cannot found rule. key name is '" + item.rule + "'";
                        r.result = FAIL;
                        break;
                    }
                    var temp = rule.isValid(value, item.arg, safe);
                    Item logic = AND;
                    if (i < rules.Count) logic = rules[i++];
                    if(temp == FAIL) {
                        if(logic == AND) {
                            var m = item.msg;
                            if(m == "") m = baseMsg;
                            var message = msg(m);
                            if(message == null) r.msg = "error : " + value;
                            else r.msg = message.msg(value, item.rule, item.arg, safe);
                            r.result = FAIL;
                            break;
                        }
                    } else {
                        value = temp;
                        if(logic == OR) break;
                    }
                }
                r.value = value;
                return r;
            }
            private void parse(string rule) {
                bool isToken = true;
                rules.Clear();
                rule = rule.Trim();
                if(rule.Length == 0) return;
                if(rule[0] == '!') {
                    optional = true;
                    rule = rule.Substring(1);
                }
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
    }
}
