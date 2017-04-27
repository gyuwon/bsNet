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
        public Dictionary<string, string> _list(ActionExecutingContext c) {
            var rq = c.HttpContext.Request;
            var r = new StreamReader(rq.Body, Encoding.UTF8, true, 20000, true);
            var j = JObject.Parse(r.ReadToEnd());
            var k = "professor/contents/contents/list";
            //meta
            if (!bs.S<bool>(k)) {
                bs.S(k, true);
                bs.msg(k + "/contree_rowid", (value, rule, arg, safe) => "err1");
                bs.msg(k + "/key1", (value, rule, arg, safe) => "err2");
                bs.msg(k + "/key2", (value, rule, arg, safe) => "err3");
                bs.vali(k, "contree_rowid", "int:" + k + "/contree_rowid", "key1", "!string:"+ k + "/key1", "key2", "!string:" + k + "/key1");
                //bs.vali(k).setMsg(k);
            }
            var result = bs.valiResult();
            if (!bs.vali(k).check(out result, "contree_rowid", j.GetValue("contree_rowid") + "", "key1", j.GetValue("key1") + "", "key2", j.GetValue("key2") + "")){
                bs.s("valiError", result);
                return null;
            } else {
                return new Dictionary<string, string>() {
                    {"contree_rowid", result["contree_rowid"].value + ""}
                };
            }
        }
        [HttpPost]
        public IActionResult list() {
            var before = (Dictionary<string, string>)bs.before(this);
            if(null == before) {
                //return bs.beforeErr();
                if (null == bs.s("valiError")) {
                    return Json(new { error = "알 수 없는 에러 발생" });
                    } else {
                    return Json(new { error = "유효성 검사 에러 발생", vali = bs.s("valiError") });
                }
            }
            var err = bs.valiResult();
            var rs1 = bs.dbSelect<List<Dictionary<String,String>>>(out err, "remote:contents/cat_list");
            if(err != null) {
                return Json(new { error = "컨텐츠 종류 정보를 가져오지 못했습니다." });
            }
            var rs2 = bs.dbSelect<List<Object[]>>(out err, "remote:contents/list", before);
            if(err != null) {
                return Json(new { error = "컨텐츠 정보를 가져오지 못했습니다." });
            }
            var cnt = bs.dbSelect<int>(out err, "remote:contents/cnt");
            if(err != null) {
                return Json(new { error = "컨텐츠 정보를 수를 가져오지 못했습니다." });
            }
            var rowids = bs.dbSelect<List<int>>(out err, "remote:contents/cat_rowids");
            if (err != null) {
                return Json(new { error = "컨텐츠 종류 일련번호 리스트를 가져오지 못했음" });
            }
            return Json(new {cat = rs1, list = rs2, cnt = cnt, rowids = rowids });
        }
        public string _Index(ActionExecutingContext c) {
            
            var a = JObject.Parse("{}");
            return "test";
        }
        public IActionResult Index() {
            var err = bs.valiResult();
            var rs = bs.dbSelect<List<Object[]>>(out err, "remote:a", "title", "1PD시험a");
            return Json(new { data = rs, a = bs.before(this)});
        }
    }
}