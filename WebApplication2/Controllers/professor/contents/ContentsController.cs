using com.bsidesoft.cs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace WebApplication2.Controllers {
    public class ContentsController:Controller {
        private bs bs;
        public ContentsController(bs b) {
            bs = b;
            //문서 파일 
            string con10CheckKey = "ContentsController/upfileFilter/con10Check";
            if(bs.S(con10CheckKey) == null) {
                bs.S(con10CheckKey, true);
                bs.upfileFilterAdd("con10Check", (Stream data, string ext) => {
                    return data;
                });
            }

            //동영상 파일 

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
                bs.s("valiError", result);
                return null;
            } else {
                return new Dictionary<string, object>() {
                    {"contree_rowid", result["r"].value}
                };
            }
        }
        [HttpPost]
        public async Task<IActionResult> list() {
            var before = (Dictionary<string, object>)bs.before(this);
            if(null == before) {
                if(null == bs.s("valiError")) {
                    return bs.apiFail("알 수 없는 에러 발생");
                } else {
                    return bs.apiFail((Dictionary<string,bs.ValiResult>)bs.s("valiError"));
                }
            }
            List<Dictionary<String, String>> catList = null;
            List<Dictionary<String, String>> contentsList = null;
            string errorMsg = null;
            await bs.dbAsync(false, "remote", async (db) => {
                var rs = await db.selectAsync<List<Dictionary<String, String>>>("contents/cat_list");
                if(rs.noRecord || rs.valiError) {
                    errorMsg = "컨텐츠 종류 정보를 가져오지 못했습니다.";
                    return false;
                }
                catList = rs.result;
                rs = await db.selectAsync<List<Dictionary<String, String>>>("contents/list", before);
                if(rs.noRecord || rs.valiError) {
                    errorMsg = "컨텐츠 정보를 가져오지 못했습니다.";
                    return false;
                }
                contentsList = rs.result;
                return true;
            });
            if(errorMsg != null) {
                return bs.apiFail(errorMsg);
            }
            return bs.apiOk(new { cat = catList, list = contentsList });
        }
        [HttpPost]
        public async Task<IActionResult> add(IFormFile upfile) {
            var result = await bs.upfileAdd("remote", "con10", upfile);
            return bs.apiOk(new { upfile = result });
        }
    }
}