using com.bsidesoft.cs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApplication2.Controllers {
    public class ProfessorController:Controller {
        private bs bs;
        public ProfessorController(bs b) {
            bs = b;
        }
        public Dictionary<string, object> _add(ActionExecutingContext c) {
            var j = bs.reqJson(c.HttpContext.Request); //{"cmps_r":3, "username":"김교수"}
            var k = bs.reqPath(c.HttpContext.Request); //professor/add
            if(!bs.S<bool>(k)) {
                bs.S(k, true);
                bs.msg(k + "/cmps_r", (value, rule, arg, safe) => "정수값을 입력하세요.");
                bs.msg(k + "/username", (value, rule, arg, safe) => "잘못된 형식의 이름입니다.");
                bs.vali(k, "cmps_r", "int:" + k + "/cmps_r", "username", "min[1]|max[10]");
            }
            var result = bs.valiResult();
            if(!bs.vali(k).check(out result, bs.json2kv(j, "cmps_r", "username"))) {
                bs.s("valiError", result);
                return null;
            } else {
                return new Dictionary<string, object>() {
                    {"cmps_rowid", result["cmps_r"].value},
                    {"username", result["username"].value}
                };
            }
        }
        [HttpPost]
        public async Task<IActionResult> add() {
            var before = (Dictionary<string, object>)bs.before(this);
            if(null == before) {
                //return bs.beforeErr();
                if(null == bs.s("valiError")) {
                    return bs.apiFail("알 수 없는 에러 발생");
                } else {
                    return bs.apiFail((dynamic)bs.s("valiError"));
                }
            }
            Dictionary<String, String> teacher = null;
            int teacher_rowid = 0;
            String errMsg = null;
            await bs.dbAsync(true, "remote", async (db) => {
                var rs0 = await db.selectAsync<Dictionary<String, String>>("teacher/view_from_cmps_rowid", before);
                if(rs0.valiError) {
                    errMsg = "유효성 검사 오류";
                    return false;
                }
                if(!rs0.noRecord) {
                    errMsg = "이미 존재하는 교수입니다.";
                    return false;
                }

                var rs1 = await db.execAsync("teacher/add", before);
                if(rs1.result != 1) {
                    errMsg = "교수를 추가하지 못했습니다.";
                    return false;
                }
                teacher_rowid = rs1.insertId;

                var rs2 = await db.selectAsync<Dictionary<String, String>>("teacher/view_from_rowid", "teacher_rowid", teacher_rowid + "");
                if(rs2.noRecord) {
                    errMsg = "교수 정보를 가져오지 못했습니다.";
                    return false;
                }
                teacher = rs2.result;
                return true;
            });
            if(errMsg != null) {
                return bs.apiFail(errMsg);
            }
            return bs.apiOk(new {
                success = 1,
                professor = new {
                    r = teacher["teacher_rowid"],
                    cmps_r = teacher["cmps_rowid"],
                    username = teacher["username"],
                    regdate = teacher["regdate"]
                }
            });
        }
        public Dictionary<string, object> _view1(ActionExecutingContext c) {
            var j = bs.reqJson(c.HttpContext.Request); //{"cmps_r":3}
            var k = bs.reqPath(c.HttpContext.Request); //professor/view1
            if(!bs.S<bool>(k)) {
                bs.S(k, true);
                bs.msg(k + "/cmps_r", (value, rule, arg, safe) => "정수값을 입력하세요.");
                bs.vali(k, "cmps_r", "int:" + k + "/cmps_r");
            }
            var result = bs.valiResult();
            if(!bs.vali(k).check(out result, bs.json2kv(j, "cmps_r"))) {
                bs.s("valiError", result);
                return null;
            } else {
                return new Dictionary<string, object>() {
                    {"cmps_rowid", result["cmps_r"].value}
                };
            }
        }
        [HttpPost]
        public async Task<IActionResult> view1() {
            var before = (Dictionary<string, object>)bs.before(this);
            if(null == before) {
                //return bs.beforeErr();
                if(null == bs.s("valiError")) {
                    return bs.apiFail("알 수 없는 에러 발생");
                } else {
                    return bs.apiFail((dynamic)bs.s("valiError"));
                }
            }
            String errMsg = null;
            Dictionary<string, string> professor = null;
            await bs.dbAsync(false, "remote", async (db) => {
                var rs = await db.selectAsync<Dictionary<string, string>>("teacher/view_from_cmps_rowid", before);
                if(rs.noRecord) {
                    errMsg = "컨텐츠 종류 정보를 가져오지 못했습니다";
                    return false;
                }
                professor = rs.result;
                return true;
            });
            if(errMsg != null) {
                return bs.apiFail(errMsg);
            }
            return bs.apiOk(new {
                professor = new {
                    r = professor["teacher_rowid"],
                    cmps_r = professor["cmps_rowid"],
                    username = professor["username"],
                    regdate = professor["regdate"]
                }
            });
        }

        public Dictionary<string, object> _view2(ActionExecutingContext c) {
            var j = bs.reqJson(c.HttpContext.Request); //{"r":3}
            var k = bs.reqPath(c.HttpContext.Request); //professor/view2
            if(!bs.S<bool>(k)) {
                bs.S(k, true);
                bs.msg(k + "/r", (value, rule, arg, safe) => "정수값을 입력하세요.");
                bs.vali(k, "r", "int:" + k + "/r");
            }
            var result = bs.valiResult();
            if(!bs.vali(k).check(out result, bs.json2kv(j, "r"))) {
                bs.s("valiError", result);
                return null;
            } else {
                return new Dictionary<string, object>() {
                    {"teacher_rowid", result["r"].value}
                };
            }
        }
        [HttpPost]
        public async Task<IActionResult> view2() {
            var before = (Dictionary<string, object>)bs.before(this);
            if(null == before) {
                //return bs.beforeErr();
                if(null == bs.s("valiError")) {
                    return bs.apiFail("알 수 없는 에러 발생");
                } else {
                    return bs.apiFail((dynamic)bs.s("valiError"));
                }
            }
            String errMsg = null;
            Dictionary<string, string> professor = null;
            await bs.dbAsync(false, "remote", async (db) => {
                var rs = await db.selectAsync<Dictionary<string, string>>("teacher/view_from_rowid", before);
                if(rs.noRecord) {
                    errMsg = "컨텐츠 종류 정보를 가져오지 못했습니다";
                    return false;
                }
                professor = rs.result;
                return true;
            });
            if(errMsg != null) {
                return bs.apiFail(errMsg);
            }
            return bs.apiOk(new {
                professor = new {
                    r = professor["teacher_rowid"],
                    cmps_r = professor["cmps_rowid"],
                    username = professor["username"],
                    regdate = professor["regdate"]
                }
            });
        }

        public Dictionary<string, object> _edit(ActionExecutingContext c) {
            var j = bs.reqJson(c.HttpContext.Request); //{"cmps_r":3, "username":"김교수"}
            var k = bs.reqPath(c.HttpContext.Request); //professor/edit
            if(!bs.S<bool>(k)) {
                bs.S(k, true);
                bs.msg(k + "/cmps_r", (value, rule, arg, safe) => "정수값을 입력하세요.");
                bs.msg(k + "/username", (value, rule, arg, safe) => "잘못된 형식의 이름입니다.");
                bs.vali(k, "cmps_r", "int:" + k + "/cmps_r", "username", "min[1]|max[10]");
            }
            var result = bs.valiResult();
            if(!bs.vali(k).check(out result, bs.json2kv(j, "cmps_r", "username"))) {
                bs.s("valiError", result);
                return null;
            } else {
                return new Dictionary<string, object>() {
                    {"cmps_rowid", result["cmps_r"].value},
                    {"username", result["username"].value}
                };
            }
        }
        [HttpPost]
        public async Task<IActionResult> edit() {
            var before = (Dictionary<string, object>)bs.before(this);
            if(null == before) {
                if(null == bs.s("valiError")) {
                    return bs.apiFail("알 수 없는 에러 발생");
                } else {
                    return bs.apiFail((dynamic)bs.s("valiError"));
                }
            }
            String errMsg = null;
            Dictionary<string, string> professor = null;
            await bs.dbAsync(true, "remote", async (db) => {
                var rs0 = await db.selectAsync<Dictionary<string, string>>("teacher/view_from_cmps_rowid", before);
                if(rs0.noRecord) {
                    errMsg = "교수가 존재하지 않습니다.";
                    return false;
                }
                var rs1 = await db.execAsync("teacher/edit", before);
                if(rs1.result != 1) {
                    errMsg = "교수를 수정하지 못했습니다.";
                    return false;
                }
                rs0 = await db.selectAsync<Dictionary<string, string>>("teacher/view_from_cmps_rowid", before);
                if(rs0.noRecord) {
                    errMsg = "교수가 존재하지 않습니다.";
                    return false;
                }
                professor = rs0.result;
                return true;
            });
            if(errMsg != null) {
                return bs.apiFail(errMsg);
            }
            return bs.apiOk(new {
                professor = new {
                    r = professor["teacher_rowid"],
                    cmps_r = professor["cmps_rowid"],
                    username = professor["username"],
                    regdate = professor["regdate"]
                }
            });
        }
       
    }
}