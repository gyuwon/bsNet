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
            return new Dictionary<string, object>() {};
        }
        [HttpPost]
        public IActionResult list() {
            var err = bs.valiResult();
            var rs = bs.dbSelect<List<Dictionary<String, String>>>(out err, "remote:contents/tree/list");
            if(err != null) {
                return Json(new { error = "트리 정보를 가져오지 못했습니다." });
            }
            //contree_rowid,parent_rowid,title,ord,regdate
            var r = new { };
            var pr = "0";
            foreach(var t in rs) {
                if(pr == t["parent_rowid"]) {

                }
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