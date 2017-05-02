using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace com.bsidesoft.cs {
    public partial class bs {
       public static object upfileAdd(string catname, IFormFile file) {
            if(file.Length == 0) return null;
            var s = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim();
            s = Guid.NewGuid() + "." + (new Regex("[\"]")).Replace(s, m => "").Split('.')[1];
            var st = new FileStream(bs.path(true, "upfile", s), FileMode.Create);
            file.CopyToAsync(st);
            return new { };
            /*
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
             */

        }
    }
}
