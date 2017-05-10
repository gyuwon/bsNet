using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

namespace com.bsidesoft.cs {
    public partial class bs {
        private static Dictionary<string, string> conns = new Dictionary<string, string>();
        private static ConcurrentDictionary<string, Query> queries = new ConcurrentDictionary<string, Query>();
        private static void dbConn(string key, string conn) {
            if(!conns.ContainsKey(key)) conns[key] = conn;
        }
        private static SqlConnection dbConnGet(string key) {
            if(!conns.ContainsKey(key)) {
                log("dbConn:get:no exist key - " + key);
                return null;
            }
            return new SqlConnection(conns[key]);
        }
        public static void dbQuery(string path) {
            foreach(var k in JObject.Parse(File.ReadAllText(path))) {
                if(!dbQuery(k.Key, k.Value.Value<string>(), path)) {
                    log("dbQuery:fail to add in " + path);
                    break;
                }
            }
        }
        public static bool dbQuery(string db, string key, string sql) {
            if(queries.ContainsKey(key)) {
                log("dbQuery:exist key - " + key);
                return false;
            }
            var query = new Query(db, sql);
            return queries.TryAdd(key, query);
        }
        public static bool dbIsExistTable(string db, string tablename) {
            var query = db + ":is_exist_table";
            var conn = dbConnGet(db);
            var cmd = conn.CreateCommand();
            Query.get(query).prepare(query, cmd, new Dictionary<string, object>() { { "tablename", tablename } });
            var result = cmd.ExecuteScalar();
            return result == DBNull.Value ? false : (bs.to<int>(result) == 1 ? true : false);
        }
        private static void dbInit(IConfigurationRoot configuration) {
            var dbPath = configuration.GetSection("Databases")["Path"];
            var dbDir = pathNormalize(false, dbPath.Split('/')); //전체 Dir
            string[] dbConnDirList = Directory.GetDirectories(dbDir);
            foreach(var dbConnDir in dbConnDirList) {
                //DB 연결 
                var dbKey = Path.GetFileName(dbConnDir);
                var p = dbPath + "\\" + dbKey + "\\connection.txt";
                var dbConnValue = fr<string>(false, p.Split('\\'));
                if(dbConnValue == null) continue;
                dbConn(dbKey, dbConnValue);

                //Queries 문 등록
                var basePath = dbPath + "\\" + dbKey + "\\queries";
                var dir = pathNormalize(false, basePath.Split('\\')); //전체 Dir
                var baseDir = dir; //기본 Dir
                var sqls = new Dictionary<String, String>(); //SQL문을 저장 
                List<string> stack = new List<String>(); //하위 디렉토리내에 sql을 처리하기 위함임 
                while(true) {
                    //하위 디렉토리가 있으면 stack에다 넣고 예약처리 
                    string[] dirList = Directory.GetDirectories(dir);
                    if(dirList.Length > 0) foreach(var d in dirList) stack.Add(d);

                    //디렉토리내에 파일 경로를 찾는다. 
                    string[] filesPaths = Directory.GetFiles(dir);
                    foreach(var filePath in filesPaths) {
                        //파일 확장자가 .sql이어야 함 
                        var fileName = Path.GetFileName(filePath);
                        if(fileName.Length < 5 || fileName.Substring(fileName.Length - 4, 4) != ".sql") {
                            continue;
                        }
                        //기본 Dir에 대한 추가 경로 추출 
                        var appendPath = dir.Replace(baseDir, "");
                        if(appendPath.Length > 0 && appendPath[0] == '\\') appendPath = appendPath.Substring(1);
                        //기본 쿼리키 만들기(경로 및 sql 파일기반임)
                        var baseKey = fileName == "base.sql" ? "" : fileName.Substring(0, fileName.Length - 4);
                        if(baseKey.Length == 0) {
                            baseKey = appendPath.Length > 0 ? appendPath.Replace('\\', '/') + "/" : "";
                        } else {
                            baseKey = appendPath.Length > 0 ? appendPath.Replace('\\', '/') + "/" + baseKey + "/" : baseKey + "/";
                        }
                        //sql 파일 읽어옴 
                        var f = basePath + "\\" + (appendPath.Length == 0 ? "" : appendPath + "\\") + fileName;
                        var text = fr<string>(false, f.Split('\\'));
                        //한 줄씩 읽어서 퀴리 문을 sqls에 저장 
                        var lines = text.Split('\n');
                        var k = "";
                        var q = "";
                        foreach(var line in lines) {
                            //쿼리 시작점 확인 
                            if(line.Length > 1 && line[0] == '#') {
                                q = q.Trim();
                                if(k != "" && q != "") sqls.Add(baseKey + k, q); //퀴리 추가 
                                k = line.Substring(1);
                                var i = line.IndexOf(':'); //#add : 설명 
                                k = i == -1 ? k.Trim() : k.Substring(0, i - 1).Trim();
                                q = "";
                                continue;
                            }
                            q += line + " "; //쿼리문을 만듬 
                        }
                        //나머지 sql문 추가 
                        q = q.Trim();
                        if(k != "" && q != "") sqls.Add(baseKey + k, q);
                    }
                    if(stack.Count == 0) break;
                    dir = stack[stack.Count - 1];
                    stack.RemoveAt(stack.Count - 1);
                }
                //쿼리문을 등록한다.
                foreach(var q in sqls) dbQuery(dbKey, q.Key, q.Value);
                dbQuery(dbKey, "is_exist_table", "select count(*)from information_schema.tables where table_name=@tablename:string@");
            }
        }
        
        public static void db(bool isTransaction, string target, Func<SqlHandler, bool> f) {
            var conn = dbConnGet(target);
            var cmd = conn.CreateCommand();
            conn.Open();
            SqlTransaction ts = null;
            if(isTransaction) {
                ts = conn.BeginTransaction((Guid.NewGuid() + "").Substring(0, 32));
                cmd.Transaction = ts;
            }
            var result = f(new SqlHandler(cmd));
            if(isTransaction) {
                if(result) {
                    try {
                        ts.Commit();
                    } catch(Exception e0) {
                        log("db:fail to commit - " + e0);
                        try {
                            ts.Rollback();
                        } catch(Exception e1) {
                            log("db:fail to Rollback from commit - " + e1);
                        }
                    }
                } else {
                    try {
                        ts.Rollback();
                    } catch(Exception e) {
                        log("db:fail to rollback - " + e);
                    }
                }
            }
            conn.Close();
        }
        public static async Task dbAsync(bool isTransaction, string target, Func<SqlHandler, Task<bool>> f) {
            var conn = dbConnGet(target);
            var cmd = conn.CreateCommand();
            await conn.OpenAsync();
            SqlTransaction ts = null;
            if(isTransaction) {
                ts = conn.BeginTransaction((Guid.NewGuid() + "").Substring(0, 32));
                cmd.Transaction = ts;
            }
            var result = await f(new SqlHandler(cmd));
            if(isTransaction) {
                if(result) {
                    try {
                        ts.Commit();
                    } catch(Exception e0) {
                        log("dbAsync:fail to commit - " + e0);
                        try {
                            ts.Rollback();
                        } catch(Exception e1) {
                            log("dbAsync:fail to Rollback from commit - " + e1);
                        }
                    }
                } else {
                    try {
                        ts.Rollback();
                    } catch(Exception e) {
                        log("dbAsync:fail to rollback - " + e);
                    }
                }
            }
            conn.Close();            
        }
    }
}