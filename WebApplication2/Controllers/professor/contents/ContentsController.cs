using com.bsidesoft.cs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WebApplication2.Controllers {
    public class ContentsController:Controller {
        private bs bs;
        public ContentsController(bs b) {
            bs = b;
        }
        public Dictionary<string, object> _list(ActionExecutingContext c) {
            var j = bs.reqJson(c.HttpContext.Request); //{"r":3}
            var k = bs.reqPath(c.HttpContext.Request); //professor/contents/contents/list
            if(!bs.S<bool>(k)) {
                bs.S(k, true);
                bs.msg(k + "/r", (value, rule, arg, safe) => "정수값을 입력하세요.");
                bs.vali(k, "r", "int:" + k + "/r"); //contree_rowid
            }
            var result = bs.valiResult();
            if(!bs.vali(k).check(out result, bs.json2kv(j, "r"))) {
                bs.s("valiError", bs.toDicValiResult(result));
                return null;
            } else {
                return new Dictionary<string, object>() {
                    {"contree_rowid", result["r"].value}
                };
            }
        }
        [HttpPost]
        public IActionResult list() {
            var before = (Dictionary<string, object>)bs.before(this);
            if(null == before) {
                //return bs.beforeErr();
                if(null == bs.s("valiError")) {
                    return Json(new { error = "알 수 없는 에러 발생" });
                } else {
                    return Json(new { error = "유효성 검사 에러 발생", vali = bs.s("valiError") });
                }
            }
            var err = bs.valiResult();
            var rs1 = bs.dbSelect<List<Dictionary<String, String>>>(out err, "remote:contents/cat_list");
            if(err != null) {
                return Json(new { error = "컨텐츠 종류 정보를 가져오지 못했습니다." });
            }
            var rs2 = bs.dbSelect<List<Dictionary<String, String>>>(out err, "remote:contents/list", before);
            if(err != null) {
                return Json(new { error = "컨텐츠 정보를 가져오지 못했습니다." });
            }
            return Json(new { cat = rs1, list = rs2 });
        }
        public string _Index(ActionExecutingContext c) {

            var a = JObject.Parse("{}");
            return "test";
        }
        public IActionResult Index() {
            var err = bs.valiResult();
            var rs = bs.dbSelect<List<Object[]>>(out err, "remote:a", "title", "1PD시험a");
            return Json(new { data = rs, a = bs.before(this) });
        }
    }
}