﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.Primitives;

namespace com.bsidesoft.cs {
    public partial class bs {
        public class FilterAction:Attribute, IFilterFactory, IActionFilter {
            public bool IsReusable => true;
            public IFilterMetadata CreateInstance(IServiceProvider serviceProvider) {
                return new FilterAction();
            }
            private static ConcurrentDictionary<string, MethodInfo> methods = new ConcurrentDictionary<string, MethodInfo>();

            public void OnActionExecuting(ActionExecutingContext c) {
                //var v = invoke(c) ?? invokeJson(c);
                if(invoke(c)) {
                    invokeJson(c);
                }

                
                //인증요청이 있다면 인증처리
                //밸리요청이 있다면 리퀘밸리처리
                //디비에 넘길값이 있다면 값처리
                //c.HttpContext.Response.Redirect();
            }
            public void OnActionExecuted(ActionExecutedContext c) {
                //head일반처리
                //c.HttpContext.Response.Headers.Add("Content-Type", "application/json");
            }
            private bool invokeJson(ActionExecutingContext c) {
                var json = fr<JObject>(false, "Controllers", c.RouteData.Values["controller"] + "", c.RouteData.Values["action"] + ".json");
                if(json == null) return false;
                ((Controller)c.Controller).ViewBag.bsBefore = json;
                return true;
            }
            private bool invoke(ActionExecutingContext c) {
                var action = "_" + c.RouteData.Values["action"];
                var key = c.RouteData.Values["controller"] + action;
                var controller = (Controller)c.Controller;
                if(!methods.ContainsKey(key)) {
                    var type = controller.GetType();
                    if(type == null) {
                        log("FilterAction.getMethod:fail to get Type - " + key);
                        return false;
                    } else {
                        var method = type.GetMethod(action);
                        if(method == null) {
                            log("FilterAction.getMethod:fail to get method - " + key);
                            return false;
                        } else if(!methods.TryAdd(key, method)) {
                            log("FilterAction.getMethod:fail to add method - " + key);
                            return false;
                        }
                    }
                }
                MethodInfo result;
                if(!methods.TryGetValue(key, out result)) {
                    log("FilterAction.getMethod:fail to get method - " + key);
                    return false;
                }
                controller.ViewBag.bsBefore = result.Invoke(controller, new object[]{c});
                return true;
            }
        }
    }
}
