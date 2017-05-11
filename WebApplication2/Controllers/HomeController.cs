using com.bsidesoft.cs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebApplication2.Controllers {
    
    public class HomeController:Controller {
        private bs bs;
        public HomeController(bs b) {
            bs = b;
        }

        public string _Index(ActionExecutingContext c) {
            var a = JObject.Parse("{}");
            return "test";
        }
        [HttpPost]
        public string testText(string a, string b) {
            return a + "::" + b;
        }
        public async Task<IActionResult> query0() {
            bs.dbQuery("test", "hika0", "select * from hika00 where title in(@title:hika00.title@,@title:hika00.title@)");
            var r = bs.dbResult<List<object[]>>();
            await bs.dbAsync(false, "test", async(db)=>{
                r = await db.selectAsync<List<object[]>>("hika0", "title", "hika");
                return true;
            });
            return Json(new {r=r});
        }
        public async Task<IActionResult> Index() {
            var result = bs.dbResult<List<Object[]>>();
            await bs.dbAsync(false, "test", async (db) => {
                result = await db.selectAsync<List<Object[]>>("a", "title", "1PD시험a");
                return true;
            }); 
            return Json(new { data = result.result, a = bs.before(this), b = bs.fr<string>(true, "test.html")});
        }
        public async Task<IActionResult> insert() {
            bs.dbQuery("test", "hika0", "insert into hika00(title)values(@title:hika00.title@)");
            bs.dbQuery("test", "hika1", "select id, title from hika00");
            var result = bs.dbResult<List<Object[]>>();
            await bs.dbAsync(false, "test", async (db) => {
                await db.execAsync("hika0", "title", Guid.NewGuid() + "");
                result = await db.selectAsync<List<Object[]>>("hika1");
                return true;
            });
            if(result.noRecord) {
                if(result.valiError) {
                    //result.vali
                } else if(result.castFail){

                }
            } else {
                //정상
            }
            return Json(new { data = result.result});
        }
        public IActionResult insert2() {
            bs.dbQuery("test", "hika0", "insert into hika00(title)values(@title:hika00.title@)");
            bs.dbQuery("test", "hika1", "select id, title from hika00");
            var r = bs.valiResult();
            var result = bs.dbResult<List<Object[]>>();
            bs.db(false, "test", (db) => {
                db.exec("hika0", "title", Guid.NewGuid() + "");
                result = db.select<List<Object[]>>("hika1");
                return true;
            });
            return Json(new { data = result.result });
        }
        public async Task<IActionResult> upload(List<IFormFile> upfile) {
            var result = new List<string>();
            bs.log("upload:" + upfile.Count);
            foreach(var f in upfile) {
                if(f.Length == 0) continue;
                //한가득 정책(확장자검사, 용량검사..)
                var s = ContentDispositionHeaderValue.Parse(f.ContentDisposition).FileName.Trim();
                s = Guid.NewGuid() + "." + (new Regex("[\"]")).Replace(s, m => "").Split('.')[1];
                result.Add("/upfile/" + s);
                var st = new FileStream(bs.path(true, "upfile", s), FileMode.Create);
                await f.CopyToAsync(st);
            }
            return Json(result);
        }
        public IActionResult excel() {
            var sFileName = @"demo.xlsx";
            var URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, sFileName);
            var fi = bs.fi(true, sFileName);
            if(bs.fd(fi)) fi = bs.fi(true, sFileName);
            var package = new ExcelPackage(fi);
            var worksheet = package.Workbook.Worksheets.Add("Employee");
            worksheet.Cells[1, 1].Value = "ID";
            worksheet.Cells[1, 2].Value = "Name";
            worksheet.Cells[1, 3].Value = "Gender";
            worksheet.Cells[1, 4].Value = "Salary (in $)";

            worksheet.Cells["A2"].Value = 1000;
            worksheet.Cells["B2"].Value = "Jon";
            worksheet.Cells["C2"].Value = "M";
            worksheet.Cells["D2"].Value = 5000;

            worksheet.Cells["A3"].Value = 1001;
            worksheet.Cells["B3"].Value = "Graham";
            worksheet.Cells["C3"].Value = "M";
            worksheet.Cells["D3"].Value = 10000;

            worksheet.Cells["A4"].Value = 1002;
            worksheet.Cells["B4"].Value = "Jenny";
            worksheet.Cells["C4"].Value = "F";
            worksheet.Cells["D4"].Value = 5000;

            package.Save();
            return bs.downXlsx(fi);
        }

        [HttpPost]
        public string httppost0(string a, string b) {
            return a + "::" + b;
        }
    }
}