using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace com.bsidesoft.cs {
    public partial class bs {
        private static ConcurrentDictionary<string, bool> upfileIsInit = new ConcurrentDictionary<string, bool>();
        public static object upfileCopy(string db, string catname, string filePath) {
            //파일 복사 
            return new { };
        }
        public static object upfileAdd(string db, string catname, IFormFile file) {
            if(file.Length == 0) return null;
            //기본정보 가져오기 
            var info = upfileReady(db, catname);

            //파일명
            var filename = upfileName(file);

            //확장자 검사 
            var ext = upfileExt(filename);
            string[] exts = (dynamic)info["exts"];
            bool find = false;
            for(var i = 0; i < exts.Length; i++) {
                if(exts[i] == ext) {
                    find = true;
                    break;
                }
            }
            if(!find) {
                log("Wrong file extension");
                return null;
            }

            //파일크기 검사 
            if(file.Length > (dynamic)info["maxsize"]) {
                log("Exceeded size");
                return null;
            }

            var savename = Guid.NewGuid() + "." + ext;
            if("" != (dynamic)info["filterName"]) {
                //TODO 파일 필터링 
                //임시로 파일을 메모리에 적재한 뒤, 필터로 밀어줄 것, 성공시 파일저장도 해야 함 
            } else {
                var st = new FileStream(info["fullPath"] + "\\" + savename, FileMode.Create);
                file.CopyToAsync(st);
            }

            //파일정보 저장 
            var upfile_r = upfileInsert(db, (dynamic)info["upfilecat_rowid"], ext, filename, info["subPath"] + "\\" + savename);
            if(0 == upfile_r) {
                //TODO 로컬에 저장된 파일을 삭제할 것 
                log("File information save failed");
                return null;
            }

            //TODO 결과 반환 
            return upfileResult(db, upfile_r);
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
        private static void upfileInit(string db) {
            if(!upfileIsInit.ContainsKey(db)) {
                upfileIsInit[db] = true;
                dbQuery(db, "upfile/get_cat", "select t0.upfilecat_rowid,t0.title,t0.intype,t0.inkey,t0.infilter,t0.basepath,t0.maxsize,STUFF((select ',' + title from upfilecatext where upfilecat_rowid = t0.upfilecat_rowid FOR XML PATH('')), 1, 1, '')exts from upfilecat t0 where t0.title=@title:string@");
                dbQuery(db, "upfile/add", "insert into upfile(upfilecatext_rowid,originname,upfile)values((select upfilecatext_rowid from upfilecatext where upfilecat_rowid=@upfilecat_rowid:int@ and title=@ext:string@),@originname:upfile.originname@,@upfile:upfile.upfile@)");
                dbQuery(db, "upfile/get", "select t0.upfile_rowid,t0.originname,t0.upfile,t2.title cat,t1.title ext,t0.regdate from upfile t0 left join upfilecatext t1 on t0.upfilecatext_rowid=t1.upfilecatext_rowid left join upfilecat t2 on t1.upfilecat_rowid=t2.upfilecat_rowid where upfile_rowid=@upfile_rowid:int@");
            }
        }
        private static Dictionary<string, object> upfileReady(string db, string catname) {
            upfileInit(db);

            //TODO 아래 정보는 cache 처리 할 것!
            var err = valiResult();
            var cat = dbSelect<Dictionary<String, String>>(out err, db + ":upfile/get_cat", "title", catname);
            if(null == cat) {
                log("A category name is not valid. name = " + catname);
                return null;
            }
            var exts = cat["exts"].Split(',');
            var basePath = cat["basepath"];
            var rootPath = path(false, basePath.Split('/'));
            if(!Directory.Exists(rootPath)) {
                log("Not exist dir : " + rootPath);
                return null;
            }
            DateTime dt = DateTime.Today;
            var y = String.Format("{0:yyyy}", dt);
            var m = String.Format("{0:MM}", dt);
            var d = String.Format("{0:dd}", dt);
            var subPath = y + "\\" + m + "\\" + d;
            var fullPath = rootPath + "\\" + subPath;
            if(!Directory.Exists(fullPath)) {
                try {
                    Directory.CreateDirectory(fullPath);
                } catch(Exception e) {
                    log("Cannot make directory. path= " + fullPath);
                    return null;
                }
            }
            return new Dictionary<string, object> {
                {"fullPath", fullPath},
                {"subPath", subPath},
                {"exts", exts},
                {"maxsize", to<int>(cat["maxsize"]) * 1000000},
                {"upfilecat_rowid", to<int>(cat["upfilecat_rowid"])},
                {"inkey", cat["inkey"]},
                {"intype", cat["intype"]},
                {"filterName", cat["infilter"]}
            };
        }
        private static int upfileInsert(string db, int upfilecat_r, string ext, string originname, string upfile) {
            var err = valiResult();
            int insertId;
            if(1 != dbExec(out err, out insertId, db + ":upfile/add", "upfilecat_rowid", upfilecat_r + "", "ext", ext, "originname", originname, "upfile", upfile)) {
                return 0;
            }
            return insertId;
        }
        private static Dictionary<string, object> upfileResult(string db, int upfile_r) {
            var err = valiResult();
            var rs = dbSelect<Dictionary<String, String>>(out err, db + ":upfile/get", "upfile_rowid", upfile_r + "");
            if(null == rs) {
                return null;
            }
            return new Dictionary<string, object>() {
                {"r", to<int>(rs["upfile_rowid"]) },
                {"originname", rs["originname"] },
                {"upfile", rs["upfile"] },
                {"cat", rs["cat"] },
                {"ext", rs["ext"] },
                {"regdate", rs["regdate"] } //TODO 날짜 형태를 맞출 것 
            };
        }
        private static string upfileName(IFormFile file) {
            var s = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim();
            return (new Regex("[\"]")).Replace(s, m => "");
        }
        private static string upfileExt(string filename) {
            var i = filename.LastIndexOf('.');
            if(-1 == i) return "";
            return filename.Substring(i + 1).ToLower();
        }
    }
}
