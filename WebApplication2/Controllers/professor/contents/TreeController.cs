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
        //professor/contents/tree/list
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
        //professor/contents/tree/add
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
        //professor/contents/tree/edit
        public Dictionary<string, object> _edit(ActionExecutingContext c) {
            var j = bs.reqJson(c.HttpContext.Request); //{"r":3, "title":"트리 수정"}
            var k = bs.reqPath(c.HttpContext.Request); //professor/contents/tree/edit
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
        public IActionResult edit() {
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
            var r = bs.dbExec(out err, "remote:contents/tree/edit", before);
            if(r != 1) {
                return Json(new { error = "트리 정보를 수정하는데 실패했습니다." });
            }
            return Json(new { data = new { success = r } });
        }
        //professor/contents/tree/del
        public Dictionary<string, object> _del(ActionExecutingContext c) {
            var j = bs.reqJson(c.HttpContext.Request); //{"r":3}
            var k = bs.reqPath(c.HttpContext.Request); //professor/contents/tree/del
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
                    {"contree_rowid", result["r"].value }
                };
            }
        }
        [HttpPost]
        public IActionResult del() {
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
            var r = bs.dbExec(out err, "remote:contents/tree/del", before);
            if(r != 1) {
                return Json(new { error = "트리 정보를 삭제하는데 실패했습니다." });
            }
            return Json(new { data = new { success = r } });
        }
        //professor/contents/tree/up
        public Dictionary<string, object> _up(ActionExecutingContext c) {
            var j = bs.reqJson(c.HttpContext.Request); //{"r":3}
            var k = bs.reqPath(c.HttpContext.Request); //professor/contents/tree/up
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
                    {"contree_rowid", result["r"].value }
                };
            }
        }
        [HttpPost]
        public IActionResult up() {
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
            var rs = bs.dbSelect<List<Dictionary<String, String>>>(out err, "remote:contents/tree/list2", before);
            if(err != null) {
                return Json(new { error = "트리 정보를 가져오지 못했습니다." });
            }
            if(true == bs.rsOrderChange(ref rs, "contree_rowid", bs.to<int>(before["contree_rowid"]), true)) {
                for(var i = 0; i < rs.Count; i++) {
                    before = new Dictionary<string, object>() {
                        { "contree_rowid", rs[i]["contree_rowid"] },
                        { "ord", i + 1 }
                    };
                    var r = bs.dbExec(out err, "remote:contents/tree/ord", before);
                    if(r != 1) {
                        return Json(new { error = "트리 위치를 새로 설정하는데 실패했습니다." });
                    }
                }
            }
            return Json(new { data = new { success = 1 } });
        }
        //professor/contents/tree/down
        public Dictionary<string, object> _down(ActionExecutingContext c) {
            var j = bs.reqJson(c.HttpContext.Request); //{"r":3}
            var k = bs.reqPath(c.HttpContext.Request); //professor/contents/tree/up
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
                    {"contree_rowid", result["r"].value }
                };
            }
        }
        [HttpPost]
        public IActionResult down() {
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
            var rs = bs.dbSelect<List<Dictionary<String, String>>>(out err, "remote:contents/tree/list2", before);
            if(err != null) {
                return Json(new { error = "트리 정보를 가져오지 못했습니다." });
            }
            if(true == bs.rsOrderChange(ref rs, "contree_rowid", bs.to<int>(before["contree_rowid"]), false)) {
                for(var i = 0; i < rs.Count; i++) {
                    before = new Dictionary<string, object>() {
                        { "contree_rowid", rs[i]["contree_rowid"] },
                        { "ord", i + 1 }
                    };
                    var r = bs.dbExec(out err, "remote:contents/tree/ord", before);
                    if(r != 1) {
                        return Json(new { error = "트리 위치를 새로 설정하는데 실패했습니다." });
                    }
                }
            }
            return Json(new { data = new { success = 1 } });
        }
        //professor/contents/tree/depthadd
        public Dictionary<string, object> _depthadd(ActionExecutingContext c) {
            var j = bs.reqJson(c.HttpContext.Request); //{"r":3}
            var k = bs.reqPath(c.HttpContext.Request); //professor/contents/tree/del
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
                    {"contree_rowid", result["r"].value }
                };
            }
        }
        [HttpPost]
        public IActionResult depthadd() {
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
            var r = bs.dbExec(out err, "remote:contents/tree/del", before);
            if(r != 1) {
                return Json(new { error = "트리 정보를 삭제하는데 실패했습니다." });
            }
            return Json(new { data = new { success = r } });
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