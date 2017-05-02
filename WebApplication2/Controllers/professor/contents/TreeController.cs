using com.bsidesoft.cs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace WebApplication2.Controllers {
    public class TreeController:Controller {
        private bs bs;
        public TreeController(bs b) {
            bs = b;
        }
        public Dictionary<string, object> _list(ActionExecutingContext c) {
            var k = bs.reqPath(c.HttpContext.Request); //professor/contents/tree/list
            return new Dictionary<string, object>() { };
        }
        [HttpPost]
        public IActionResult list() {
            var err = bs.valiResult();
            var rs = bs.dbSelect<List<Dictionary<String, String>>>(out err, "remote:contents/tree/list");
            if(err != null) {
                return Json(new { error = "트리 정보를 가져오지 못했습니다." });
            }
            //contree_rowid,parent_rowid,title,ord,regdate
            return Json(new { data = rs });
        }
        public Dictionary<string, object> _add(ActionExecutingContext c) {
            var j = bs.reqJson(c.HttpContext.Request); //{"r":3, "title":"트리 추가"}
            var k = bs.reqPath(c.HttpContext.Request); //professor/contents/tree/add
            if(!bs.S<bool>(k)) {
                bs.S(k, true);
                bs.msg(k + "/r", (value, rule, arg, safe) => "정수값을 입력하세요.");
                bs.msg(k + "/title", (value, rule, arg, safe) => "문자열로 입력하세요.");
                bs.vali(k, "r", "int:" + k + "/r", "title", "string:" + k + "/title"); //contree_rowid, title
            }
            var result = bs.valiResult();
            if(!bs.vali(k).check(out result, bs.json2kv(j, "r", "title"))) {
                bs.s("valiError", bs.toDicValiResult(result));
                return null;
            } else {
                return new Dictionary<string, object>() {
                    {"contree_rowid", result["r"].value },
                    { "title", result["title"].value}
                };
            }
        }
        [HttpPost]
        public IActionResult add() {
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
            var pr = bs.dbSelect<int>(out err, "remote:contents/tree/view", before);
            if(err != null) {
                return Json(new { error = "트리의 부모 정보를 가져오지 못했습니다." });
            }
            
            before.Add("parent_rowid", pr);
            int insertId;
            var r = bs.dbExec(out err, out insertId, "remote:contents/tree/add", before);
            if(err != null) {
                return Json(new { error = "트리 정보를 가져오지 못했습니다." });
            }
            if(r != 1) {
                return Json(new { error = "트리 정보를 등록하는데 실패했습니다." });
            }
            
            return Json(new { data = r });
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