using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace com.bsidesoft.cs {
    public partial class bs {
        class Rule {
            private static RegularExpressionAttribute I = new RegularExpressionAttribute("^-?[0-9]+$");
            private static RegularExpressionAttribute F = new RegularExpressionAttribute("^-?[0-9.]+$");
            static Rule() {
                //extra
                add("creditCard", new RuleVali(new DataTypeAttribute(DataType.CreditCard)));
                add("html", new RuleVali(new DataTypeAttribute(DataType.Html)));
                add("image", new RuleVali(new DataTypeAttribute(DataType.ImageUrl)));
                //base
                add("ip", new RuleVali(new RegularExpressionAttribute("^((([0-9])|(1[0-9]{1,2})|(2[0-4][0-9])|(25[0-5]))[.]){3}(([0-9])|([1-9][0-9]{1,2})|(2[0-4][0-9])|(25[0-5]))$")));
                add("url", new RuleVali(new DataTypeAttribute(DataType.Url)));
                add("email", new RuleVali(new DataTypeAttribute(DataType.EmailAddress)));
                add("korean", new RuleVali(new RegularExpressionAttribute("^[ㄱ-힣]+$")));
                add("japanese", new RuleVali(new RegularExpressionAttribute("^[ぁ-んァ-ヶー一-龠！-ﾟ・～「」“”‘’｛｝〜−]+$")));
                add("alpha", new RuleVali(new RegularExpressionAttribute("^[a-z]+$")));
                add("ALPHA", new RuleVali(new RegularExpressionAttribute("^[A-Z]+$")));
                add("num", new RuleVali(F));
                add("alphanum", new RuleVali(new RegularExpressionAttribute("^[a-z0-9]+$")));
                add("1alpha", new RuleVali(new RegularExpressionAttribute("^[a-z]")));
                add("1ALPHA", new RuleVali(new RegularExpressionAttribute("^[A-Z]")));
                //type
                add("int", new Rule((value, arg, safe) => {
                    if(value is string) {
                        if(!I.IsValid(value)) return FAIL;
                    }
                    return toI(value);
                }));
                add("float", new Rule((value, arg, safe) => {
                    if(value is string) {
                        if(!F.IsValid(value)) return FAIL;
                    }
                    return toF(value);
                }));
                add("equalto", new Rule((value, arg, safe) => {
                    var s = safe[arg[0]];
                    if(value is string) return (string)value == (string)s ? value : FAIL;
                    if(value is int) return (int)value == toI(s) ? value : FAIL;
                    if(value is float) return (float)value == toF(s) ? value : FAIL;
                    if(value is double) return (double)value == toD(s) ? value : FAIL;
                    if(value is bool) return (bool)value == toB(s) ? value : FAIL;
                    return FAIL;
                }));
                add("max", new Rule((value, arg, safe) => {
                    var l = toF(arg[0]);
                    if(value is string) return ((string)value).Length < l ? value : FAIL;
                    else return toF(value) < l ? value : FAIL;
                }));
                add("min", new Rule((value, arg, safe) => {
                    var l = toF(arg[0]);
                    if(value is string) return ((string)value).Length > l ? value : FAIL;
                    else return toF(value) > l ? value : FAIL;
                }));
                add("length", new Rule((value, arg, safe) => {
                    var l = toF(arg[0]);
                    if(value is string) return ((string)value).Length == l ? value : FAIL;
                    else return toF(value) == l ? value : FAIL;
                }));
                add("range", new Rule((value, arg, safe) => {
                    float l = toF(arg[0]), m = toF(arg[1]);
                    var v = value is string ? ((string)value).Length : toF(value);
                    return l <= v && v <= m ? value : FAIL;
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
        class RuleVali:Rule {
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
