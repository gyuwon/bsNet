using com.bsidesoft.cs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        public async Task<IActionResult> list() {
            List<Dictionary<string, string>> list = null;
            String errMsg = null;
            await bs.dbAsync(false, "remote", async (db) => {
                var rs = await db.selectAsync<List<Dictionary<string, string>>>("contents/tree/list");
                if(rs.valiError) {
                    errMsg = "유효성 검사 오류";
                    return false;
                }
                if(rs.noRecord) {
                    errMsg = "트리 리스트가 존재하지 않습니다.";
                    return false;
                }
                list = rs.result;
                return true;
            });
            //contree_rowid,parent_rowid,title,ord,regdate
            if(errMsg != null) bs.apiFail(errMsg);
            return bs.apiOk(new {
                success = 1,
                data = list
            });
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
                bs.s("valiError", result);
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
                if(bs.s("valiError") == null) {
                    return bs.apiFail("알 수 없는 에러 발생");
                } else {
                    return bs.apiFail((Dictionary<string, bs.ValiResult>)bs.s("valiError"));
                }
            }
            String errMsg = null;
            bs.db(false, "remote", (db) => {
                var pr = db.select<int>("contents/tree/view", before);
                if(pr.noRecord) {
                    errMsg = "트리의 부모 정보를 가져오지 못했습니다.";
                    return false;
                }
                before.Add("parent_rowid", pr.result);
                var rs = db.exec("contents/tree/add", before);
                if(rs.result != 1) {
                    errMsg = "트리를 등록하는데 실패했습니다.";
                    return false;
                }
                return true;
            });
            if(errMsg != null) return bs.apiFail(errMsg);
            return bs.apiOk(new { success = 1 });
        }/*
        public async Task<IActionResult> add() {
            var before = (Dictionary<string, object>)bs.before(this);
            if(before == null) {
                if(bs.s("valiError") == null) {
                    return bs.apiFail("알 수 없는 에러 발생");
                } else {
                    return bs.apiFail((Dictionary<string, bs.ValiResult>)bs.s("valiError"));
                }
            }
            String errMsg = null;
            await bs.dbAsync(false, "remote", async (db) => {
                var pr = await db.selectAsync<int>("contents/tree/view", before);
                if(pr.noRecord) {
                    errMsg = "트리의 부모 정보를 가져오지 못했습니다.";
                    return false;
                }
                before.Add("parent_rowid", pr.result);
                var rs = await db.execAsync("contents/tree/add", before);
                if(rs.result != 1) {
                    errMsg = "트리를 등록하는데 실패했습니다.";
                    return false;
                }
                return true;
            });
            if(errMsg != null) return bs.apiFail(errMsg);
            return bs.apiOk(new { success = 1 });
        }*/
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
                bs.s("valiError", result);
                return null;
            } else {
                return new Dictionary<string, object>() {
                    {"contree_rowid", result["r"].value },
                    { "title", result["title"].value}
                };
            }
        }
        [HttpPost]
        public async Task<IActionResult> edit() {
            var before = (Dictionary<string, object>)bs.before(this);
            if(before == null) {
                if(bs.s("valiError") == null) {
                    return bs.apiFail("알 수 없는 에러 발생");
                } else {
                    return bs.apiFail((Dictionary<string, bs.ValiResult>)bs.s("valiError"));
                }
            }
            String errMsg = null;
            await bs.dbAsync(false, "remote", async (db) => {
                var rs = await db.execAsync("contents/tree/edit", before);
                if(rs.result != 1) {
                    errMsg = "트리를 수정하는데 실패했습니다.";
                    return false;
                }
                return true;
            });
            if(errMsg != null) return bs.apiFail(errMsg);
            return bs.apiOk(new { success = 1 });
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
                bs.s("valiError", result);
                return null;
            } else {
                return new Dictionary<string, object>() {
                    {"contree_rowid", result["r"].value }
                };
            }
        }
        [HttpPost]
        public async Task<IActionResult> del() {
            var before = (Dictionary<string, object>)bs.before(this);
            if(before == null) {
                if(bs.s("valiError") == null) {
                    return bs.apiFail("알 수 없는 에러 발생");
                } else {
                    return bs.apiFail((Dictionary<string, bs.ValiResult>)bs.s("valiError"));
                }
            }
            String errMsg = null;
            await bs.dbAsync(false, "remote", async (db)=>{
                var rs = await db.execAsync("contents/tree/del", before);
                if(rs.result != 1) {
                    errMsg = "트리를 삭제하는데 실패했습니다.";
                    return false;
                }
                return true;
            });
            if(errMsg != null) return bs.apiFail(errMsg);
            return bs.apiOk(new { success = 1 });
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
                bs.s("valiError", result);
                return null;
            } else {
                return new Dictionary<string, object>() {
                    {"contree_rowid", result["r"].value }
                };
            }
        }
        [HttpPost]
        public async Task<IActionResult> up() {
            var before = (Dictionary<string, object>)bs.before(this);
            if(before == null) {
                if(bs.s("valiError") == null) {
                    return bs.apiFail("알 수 없는 에러 발생");
                } else {
                    return bs.apiFail((Dictionary<string, bs.ValiResult>)bs.s("valiError"));
                }
            }
            String errMsg = null;
            await bs.dbAsync(false, "remote", async (db) => {
                var rs = await db.selectAsync<List<Dictionary<String, String>>>("contents/tree/list2", before);
                if(rs.noRecord) {
                    errMsg = "트리 정보를 가져오지 못했습니다.";
                    return false;
                }
                var list = rs.result;
                if(true == bs.rsOrderChange(ref list, "contree_rowid", bs.to<int>(before["contree_rowid"]), true)) {
                    for(var i = 0; i < list.Count; i++) {
                        before = new Dictionary<string, object>() {
                            { "contree_rowid", list[i]["contree_rowid"] },
                            { "ord", i + 1 }
                        };
                        var r = await db.execAsync("contents/tree/ord", before);
                        if(r.result != 1) {
                            errMsg = "트리 위치를 변경하는데 실패했습니다.";
                            return false;
                        }
                    }
                }
                return true;
            });
            if(errMsg != null) return bs.apiFail(errMsg);
            return bs.apiOk(new { success = 1 });
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
                bs.s("valiError", result);
                return null;
            } else {
                return new Dictionary<string, object>() {
                    {"contree_rowid", result["r"].value }
                };
            }
        }
        [HttpPost]
        public async Task<IActionResult> down() {
            var before = (Dictionary<string, object>)bs.before(this);
            if(before == null) {
                if(bs.s("valiError") == null) {
                    return bs.apiFail("알 수 없는 에러 발생");
                } else {
                    return bs.apiFail((Dictionary<string, bs.ValiResult>)bs.s("valiError"));
                }
            }
            String errMsg = null;
            await bs.dbAsync(false, "remote", async (db) => {
                var rs = await db.selectAsync<List<Dictionary<String, String>>>("contents/tree/list2", before);
                if(rs.noRecord) {
                    errMsg = "트리 정보를 가져오지 못했습니다.";
                    return false;
                }
                var list = rs.result;
                if(true == bs.rsOrderChange(ref list, "contree_rowid", bs.to<int>(before["contree_rowid"]), false)) {
                    for(var i = 0; i < list.Count; i++) {
                        before = new Dictionary<string, object>() {
                            { "contree_rowid", list[i]["contree_rowid"] },
                            { "ord", i + 1 }
                        };
                        var r = await db.execAsync("contents/tree/ord", before);
                        if(r.result != 1) {
                            errMsg = "트리 위치를 변경하는데 실패했습니다.";
                            return false;
                        }
                    }
                }
                return true;
            });
            if(errMsg != null) return bs.apiFail(errMsg);
            return bs.apiOk(new { success = 1 });
        }
        //professor/contents/tree/depthadd
        public Dictionary<string, object> _depthadd(ActionExecutingContext c) {
            var j = bs.reqJson(c.HttpContext.Request); //{"r":3, "title":"트리 추가"}
            var k = bs.reqPath(c.HttpContext.Request); //professor/contents/tree/depthadd
            if(!bs.S<bool>(k)) {
                bs.S(k, true);
                bs.msg(k + "/r", (value, rule, arg, safe) => "정수값을 입력하세요.");
                bs.msg(k + "/title", (value, rule, arg, safe) => "문자열로 입력하세요.");
                bs.vali(k, "r", "int:" + k + "/r", "title", "string:" + k + "/title"); //contree_rowid, title
            }
            var result = bs.valiResult();
            if(!bs.vali(k).check(out result, bs.json2kv(j, "r", "title"))) {
                bs.s("valiError", result);
                return null;
            } else {
                return new Dictionary<string, object>() {
                    {"contree_rowid", result["r"].value},
                    {"title", result["title"].value}
                };
            }
        }
        [HttpPost]
        public async Task<IActionResult> depthadd() {
            var before = (Dictionary<string, object>)bs.before(this);
            if(before == null) {
                if(bs.s("valiError") == null) {
                    return bs.apiFail("알 수 없는 에러 발생");
                } else {
                    return bs.apiFail((Dictionary<string, bs.ValiResult>)bs.s("valiError"));
                }
            }
            String errMsg = null;
            await bs.dbAsync(false, "remote", async (db) => {
                before.Add("parent_rowid", before["contree_rowid"]);
                var rs = await db.execAsync("contents/tree/add", before);
                if(rs.result != 1) {
                    errMsg = "트리를 등록하는데 실패했습니다.";
                    return false;
                }
                return true;
            });
            if(errMsg != null) return bs.apiFail(errMsg);
            return bs.apiOk(new { success = 1 });
        }
    }
}