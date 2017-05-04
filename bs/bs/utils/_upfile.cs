using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace com.bsidesoft.cs {
    public partial class bs {
        public class UpfileResult {
            public int r { get; internal set; }
            public string originname { get; internal set; }
            public string upfile { get; internal set; }
            public string cat { get; internal set; }
            public string ext { get; internal set; }
            public string regdate { get; internal set; }
        }
        private static ConcurrentDictionary<string, bool> upfileIsInit = new ConcurrentDictionary<string, bool>();
        private static ConcurrentDictionary<string, Dictionary<string, object>> upfileCatInfo = new ConcurrentDictionary<string, Dictionary<string, object>>();
        private static ConcurrentDictionary<string, Func<Stream, string, Stream>> upfileFilters = new ConcurrentDictionary<string, Func<Stream, string, Stream>>();
        public static void upfileFilterAdd(string filterName, Func<Stream, string, Stream> filter) { //data, ext, out data
            if(!upfileFilters.ContainsKey(filterName)) {
                upfileFilters[filterName] = filter;
            }
        }
        public static UpfileResult upfileAdd(string db, string catname, IFormFile file) {
            if(file.Length == 0) return null;
            //기본정보 가져오기 
            var info = upfileReady(db, catname);
            if(null == info) {
                return null;
            }

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

            var newfilename = Guid.NewGuid() + "." + ext;
            var savePath = info["fullPath"] + "\\" + newfilename;
            var filterName = (string)info["filterName"];
            if("" != filterName) {
                //TODO 파일 필터링 
                //임시로 파일을 메모리에 적재한 뒤, 필터로 밀어줄 것, 성공시 파일저장도 해야 함
                if(!upfileFilters.ContainsKey(filterName)){
                    log("Cannot found upfile filter. name=" + info["filterName"]);
                    return null;
                }
                var filter = upfileFilters[filterName];
                Stream st;
                st = file.OpenReadStream();
                var newSt = filter(st, ext);
                if(newSt != st) st.Dispose();
                if(null == newSt) {
                    log("Wrong file");
                    return null;
                } else {
                    var fst = new FileStream(savePath, FileMode.Create);
                    newSt.CopyTo(fst);
                    newSt.Dispose();
                    fst.Dispose();
                }
            } else {
                var fst = new FileStream(savePath, FileMode.Create);
                file.CopyTo(fst);
                fst.Dispose();
            }

            //파일정보 저장 
            var upfile_r = upfileInsert(db, (dynamic)info["upfilecat_rowid"], ext, filename, info["subPath"] + "\\" + newfilename);
            if(0 == upfile_r) {
                log("File information save failed");
                var fi = new FileInfo(savePath);
                try {
                    fi.Delete();
                } catch(IOException e) {
                    log(e.Message);
                }
                return null;
            }

            //결과 반환 
            return upfileResult(db, upfile_r);
        }
        private static void upfileInit(string db) {
            if(!upfileIsInit.ContainsKey(db)) {
                upfileIsInit[db] = true;
                dbQuery(db, "upfile/get_cat", "select t0.upfilecat_rowid,t0.title,t0.intype,t0.inkey,t0.infilter,t0.basepath,t0.maxsize,STUFF((select ',' + title from upfilecatext where upfilecat_rowid = t0.upfilecat_rowid FOR XML PATH('')), 1, 1, '')exts from upfilecat t0 where t0.title=@title:string@");
                dbQuery(db, "upfile/add", "insert into upfile(upfilecatext_rowid,originname,upfile)values((select upfilecatext_rowid from upfilecatext where upfilecat_rowid=@upfilecat_rowid:int@ and title=@ext:string@),@originname:upfile.originname@,@upfile:upfile.upfile@)");
                dbQuery(db, "upfile/get", "select t0.upfile_rowid,t0.originname,t0.upfile,t2.title cat,t2.basepath,t1.title ext,t0.regdate from upfile t0 left join upfilecatext t1 on t0.upfilecatext_rowid=t1.upfilecatext_rowid left join upfilecat t2 on t1.upfilecat_rowid=t2.upfilecat_rowid where upfile_rowid=@upfile_rowid:int@");
            }
        }
        private static Dictionary<string, object> upfileReady(string db, string catname) {
            var key = db + ":" + catname;
            Dictionary<string, object> result = null;
            if(upfileCatInfo.ContainsKey(key)) {
                result = upfileCatInfo[key];
            } else {
                upfileInit(db);
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
                result = new Dictionary<string, object> {
                    {"fullPath", fullPath},
                    {"subPath", subPath},
                    {"exts", exts},
                    {"maxsize", to<int>(cat["maxsize"]) * 1000000},
                    {"upfilecat_rowid", to<int>(cat["upfilecat_rowid"])},
                    {"inkey", cat["inkey"]},
                    {"intype", cat["intype"]},
                    {"filterName", cat["infilter"]}
                };
                upfileCatInfo[key] = result;
            }
            return result;
        }
        private static int upfileInsert(string db, int upfilecat_r, string ext, string originname, string upfile) {
            var err = valiResult();
            int insertId;
            if(1 != dbExec(out err, out insertId, db + ":upfile/add", "upfilecat_rowid", upfilecat_r + "", "ext", ext, "originname", originname, "upfile", upfile)) {
                return 0;
            }
            return insertId;
        }
        private static UpfileResult upfileResult(string db, int upfile_r) {
            var err = valiResult();
            var rs = dbSelect<Dictionary<String, String>>(out err, db + ":upfile/get", "upfile_rowid", upfile_r + "");
            if(null == rs) {
                return null;
            }
            return new UpfileResult() {
                r = to<int>(rs["upfile_rowid"]),
                originname = rs["originname"],
                upfile = rs["upfile"],
                cat = rs["cat"],
                ext = rs["ext"],
                regdate = rs["regdate"] //TODO 날짜 형태를 맞출 것 
            };
        }
        public static string upfileName(IFormFile file) {
            var s = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim();
            return (new Regex("[\"]")).Replace(s, m => "");
        }
        public static string upfileExt(IFormFile file) {
            return upfileExt(upfileName(file));
        }
        public static string upfileExt(string filename) {
            var i = filename.LastIndexOf('.');
            if(-1 == i) return "";
            return filename.Substring(i + 1).ToLower();
        }
    }
}
