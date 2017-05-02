using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;

namespace com.bsidesoft.cs {
    public partial class bs {
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
            Query q;
            var query = db + ":is_exist_table"; 
            SqlCommand cmd = dbBegin(query, out q);
            if(cmd == null) {
                log("dbExec:fail to dbBegin - " + query);
                return false;
            }
            q.prepare(query, cmd, new Dictionary<string, object>() { { "tablename", tablename } });
            var result = cmd.ExecuteScalar();
            return result == DBNull.Value ? false : (bs.to<int>(result) == 1 ? true : false);
        }
        public static int dbExec(out Dictionary<string, ValiResult> err, out int insertId, string query, params string[] kv) {
            return dbExec(out err, out insertId, query, opt<object>(kv));
        }
        public static int dbExec(out Dictionary<string, ValiResult> err, out int insertId, string query, Dictionary<string, object> opt = null) {
            Query q;
            SqlCommand cmd = dbBegin(query, out q);
            insertId = 0;
            if(cmd == null) {
                err = null;
                log("dbExec:fail to dbBegin - " + query);
                return 0;
            }
            err = q.prepare(query, cmd, opt);
            if(err != null) return 0;
            int row = cmd.ExecuteNonQuery();

            cmd = cmd.Connection.CreateCommand();
            cmd.CommandText = "SELECT @@IDENTITY";
            Object id = cmd.ExecuteScalar();
            insertId = id == DBNull.Value ? 0 : bs.to<int>(id); 
            dbEnd(cmd);
            return row;
        }
        public static T dbSelect<T>(out Dictionary<string, ValiResult> err, string query, params string[] kv) {
            return dbSelect<T>(out err, query, opt<object>(kv));
        }
        public static T dbSelect<T>(out Dictionary<string, ValiResult> err, string query, Dictionary<string, object> opt = null) {
            Query q;
            SqlCommand cmd = dbBegin(query, out q);
            if(cmd == null) {
                err = null;
                log("dbSelect:fail to dbBegin - " + query);
                return default(T);
            }
            err = q.prepare(query, cmd, opt);
            if(err != null) return default(T);
            SqlDataReader rs = cmd.ExecuteReader();
            int j = rs.FieldCount;
            T result = default(T);
            switch(TYPES[typeof(T)]) {
            case "int":
                if(j == 1 && rs.Read()) result = (dynamic)rs.GetInt32(0);
                break;
            case "bool":
                if(j == 1 && rs.Read()) result = (dynamic)rs.GetBoolean(0);
                break;
            case "string":
                if(j == 1 && rs.Read()) result = (dynamic)rs.GetString(0);
                break;
            case "float":
                if(j == 1 && rs.Read()) result = (dynamic)rs.GetFloat(0);
                break;
            case "double":
                if(j == 1 && rs.Read()) result = (dynamic)rs.GetDouble(0);
                break;
            case "list<string>":
                result = (dynamic)new List<String>();
                while(rs.Read()) ((dynamic)result).Add(rs.GetString(0));
                break;
            case "list<int>":
                result = (dynamic)new List<int>();
                while(rs.Read()) ((dynamic)result).Add(rs.GetInt32(0));
                break;
            case "list<object[]>":
                result = (dynamic)new List<Object[]>();
                while(rs.Read()) {
                    Object[] record = new Object[j];
                    rs.GetValues(record);
                    ((dynamic)result).Add(record);
                }
                break;
            case "list<dictionary<string,string>>":
                result = (dynamic)new List<Dictionary<String, String>>();
                while(rs.Read()) {
                    Dictionary<String, String> record1 = new Dictionary<String, String>();
                    Object[] record2 = new Object[j];
                    rs.GetValues(record2);
                    for(var i = 0; i < record2.Length; i++) {
                        record1.Add(rs.GetName(i), record2.GetValue(i) + "");
                    }
                    ((dynamic)result).Add(record1);
                }
                break;
            case "dictionary<string,string>":
                if(rs.Read()) {
                    result = (dynamic)new Dictionary<String, String>();
                    Object[] record = new Object[j];
                    rs.GetValues(record);
                    for(var i = 0; i < record.Length; i++) {
                        ((dynamic)result).Add(rs.GetName(i), record.GetValue(i) + "");
                    }
                }
                break;
            }
            dbEnd(cmd);
            return result;
        }

        private static Dictionary<string, string> conns = new Dictionary<string, string>();
        private static ConcurrentDictionary<string, Query> queries = new ConcurrentDictionary<string, Query>();
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
                dbQuery(dbKey, "upfile/get_cat", "select t0.upfilecat_rowid,t0.title,t0.intype,t0.inkey,t0.infilter,t0.basepath,t0.maxsize,STUFF((select ',' + title from upfilecatext where upfilecat_rowid = t0.upfilecat_rowid FOR XML PATH('')), 1, 1, '')exts from upfilecat t0 where t0.title=@title:string@");
                dbQuery(dbKey, "upfile/add", "insert into upfile(upfilecatext_rowid,originname,upfile)values((select upfilecatext_rowid from upfilecatext where upfilecat_rowid=@upfilecat_rowid:integer@ and title=@ext:string@),@originname:upfile.originname@,@upfile:upfile.upfile@)");
                dbQuery(dbKey, "upfile/get", "select t0.upfile_rowid,t0.originname,t0.upfile,t2.title cat,t1.title ext,t0.regdate from upfile t0 left join upfilecatext t1 on t0.upfilecatext_rowid=t1.upfilecatext_rowid left join upfilecat t2 on t1.upfilecat_rowid=t2.upfilecat_rowid where upfile_rowid=@upfile_rowid:integer@");
            }
        }
        private static void dbConn(string key, string conn) {
            if(!conns.ContainsKey(key)) conns[key] = conn;
        }
        private static SqlConnection dbConn(string key) {
            if(!conns.ContainsKey(key)) {
                log("dbConn:get:no exist key - " + key);
                return null;
            }
            return new SqlConnection(conns[key]);
        }
        private static SqlCommand dbBegin(string query, out Query q) {
            string[] strs = query.Split(':');
            string target = strs[0], sqlKey = strs[1];
            if(!queries.TryGetValue(sqlKey, out q)) {
                log("dbBegin:fail to get query - " + sqlKey);
                return null;
            }
            SqlConnection conn = dbConn(target);
            if(conn == null) return null;
            try {
                conn.Open();
                SqlCommand cmd = conn.CreateCommand();
                return cmd;
            } catch(SqlException e) {
                log("dbBegin:fail to connection - " + e.Message);
            }
            return null;
        }
        private static void dbEnd(SqlCommand cmd) {
            cmd.Connection.Close();
        }
    }
}