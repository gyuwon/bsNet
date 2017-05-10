using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace com.bsidesoft.cs {
    public partial class bs {
        public static object before(Controller c) {
            return c.ViewBag.bsBefore;
        }

        private static Dictionary<string, string> CONTENT_TYPE = new Dictionary<string, string>(){
            {"xslx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"}
        };

        public class ApiResult:IActionResult {
            private static ConcurrentStack<ApiResult> pool = new ConcurrentStack<ApiResult>();
            internal static ApiResult get(object d, object e) {
                ApiResult r;
                if(pool.Count == 0) {
                    r = new ApiResult();
                } else {
                    if(!pool.TryPop(out r)) {
                        r = new ApiResult();
                    }
                }
                return r.init(d, e);
            }

            private object data = null;
            private object error = null;
            private ApiResult() { }
            private ApiResult init(object d, object e) {
                data = d;
                error = e;
                return this;
            }
            
            public async Task ExecuteResultAsync(ActionContext context) {
                context.HttpContext.Response.Headers.Add("Content-Type", "application/json");
                object obj;
                if(error == null) {
                    obj = new { data = data };
                } else {
                    obj = new { error = error };
                }
                var result = new ObjectResult(obj);
                await result.ExecuteResultAsync(context);
                data = null;
                error = null;
                pool.Push(this);
            }
        }
        public static ApiResult apiFail(int code = 0) {
            return ApiResult.get(null, new { code = code, msg = "Unknown error" });
        }
        public static ApiResult apiFail(string msg = null) {
            return ApiResult.get(null, new { code = 0, msg = msg });
        }
        public static ApiResult apiFail(Dictionary<string, ValiResult> vali) {
            return ApiResult.get(null, new { code = 0, msg = "유효하지 ", vali = toDicValiResult(vali) });
        }
        public static ApiResult apiOk(Object data) {
            return ApiResult.get(data, null);
        }
       

        public PhysicalFileResult downXlsx(FileInfo i) {
            return down("xslx", i.FullName);
        }
        public PhysicalFileResult downXlsx(bool isWeb, params string[] p) {
            return down("xslx", pathNormalize(isWeb, p));
        }
        private PhysicalFileResult down(string type, string path) {
            return new PhysicalFileResult(path, CONTENT_TYPE[type]);
        }
    }
}