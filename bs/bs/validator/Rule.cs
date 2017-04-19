using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace com.bsidesoft.cs {
    public partial class bs {
        public class Rule {
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
                add("int", new Rule((value, arg, safe) => {
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
    }
}
