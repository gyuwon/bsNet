﻿using com.bsidesoft.cs;
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
        public IActionResult Index(int id) {
            var r = bs.valiResult();
            var rs = bs.dbSelect<List<Object[]>>(out r, "test:a", "title", "1PD시험a");
            return Json(new { data = rs, a = bs.before(this), b = bs.fr<string>(true, "test.html")});
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
    }
}