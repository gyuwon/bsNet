using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;

namespace com.bsidesoft.cs {
    public partial class bs {
        class Query {
            private static Regex qsep = new Regex("@([^@]+)@");
            private static Dictionary<string, SqlDbType> fieldType = new Dictionary<string, SqlDbType>() {
                {"bit", SqlDbType.Bit}, {"tinyint", SqlDbType.TinyInt}, {"smallint", SqlDbType.SmallInt},
                {"int", SqlDbType.Int}, {"bigint", SqlDbType.BigInt},{"decimal", SqlDbType.Decimal},
                {"float", SqlDbType.Float}, {"real", SqlDbType.Real},
                {"smallmoney", SqlDbType.SmallMoney}, {"money", SqlDbType.Money}, 
                {"date", SqlDbType.Date}, {"time", SqlDbType.Time}, {"datetimeoffset", SqlDbType.DateTimeOffset}, {"timestamp", SqlDbType.Timestamp},
                {"smalldatetime", SqlDbType.SmallDateTime}, {"datetime", SqlDbType.DateTime}, {"datetime2", SqlDbType.DateTime2}, 
                {"char", SqlDbType.Char}, {"varchar", SqlDbType.VarChar}, {"text", SqlDbType.Text},
                {"nchar", SqlDbType.NChar}, {"nvarchar", SqlDbType.NVarChar}, {"ntext", SqlDbType.NText},
                {"binary", SqlDbType.Binary}, {"varbinary", SqlDbType.VarBinary}, {"image", SqlDbType.Image},
                {"uniqueidentifier", SqlDbType.UniqueIdentifier},  {"xml", SqlDbType.Xml}
            };
            private static ConcurrentDictionary<string, Dictionary<string, Object[]>> tableInfo = new ConcurrentDictionary<string, Dictionary<string, Object[]>>();
            private static string tableInfoSQL = @"select f1.name,
                (select top 1 s0.name from sys.types s0 where s0.system_type_id=f1.system_type_id),f1.is_nullable,f1.is_identity,
                (select top 1 value from sys.extended_properties s0 where f0.object_id=s0.major_id and f1.column_id=s0.minor_id and s0.name='MS_Description')
                from sys.tables f0,sys.columns f1 
                where f0.object_id=f1.object_id and f0.name='";
            class Item {
                internal string key;
                internal SqlDbType type;
            }
            private string db;
            private string sql;
            private List<string> replacer = new List<string>();
            private List<Item> param = new List<Item>();
            private Vali vali = new Vali();
            private bs bs;

            public Query(bs b, string target, string query) {
                bs = b;
                db = target;
                sql = qsep.Replace(query, parse);
            }
            private Dictionary<string, Object[]> tInfo(string table) {
                if(!tableInfo.ContainsKey(table)) {
                    SqlConnection conn = bs.dbConn(db);
                    if(conn == null) return null;
                    try {
                        conn.Open();
                        SqlCommand cmd = conn.CreateCommand();
                        cmd.CommandText = tableInfoSQL + table + "'";
                        SqlDataReader rs = cmd.ExecuteReader();
                        int k = rs.FieldCount;
                        var r = new Dictionary<string, Object[]>();
                        while(rs.Read()) {
                            Object[] record = new Object[k];
                            rs.GetValues(record);
                            r.Add(record[0] + "", record);
                        }
                        conn.Close();
                        if(!tableInfo.TryAdd(table, r)) {
                            log("tInfo:fail to add - " + table);
                            return null;
                        }
                    } catch(SqlException e) {
                        log("tInfo:fail to connection - " + e.Message);
                        return null;
                    }
                }
                var result = new Dictionary<string, Object[]>();
                if(!tableInfo.TryGetValue(table, out result)) {
                    log("tInfo:fail to get - " + table);
                    return null;
                }
                return result;
            }
            private string parse(Match m) {
                var v = m.Groups[1].Value;
                if(v.Contains(":")) {
                    var i = v.IndexOf(':');
                    var key = v.Substring(0, i);
                    var type = SqlDbType.NChar;
                    var rule = v.Substring(i + 1);
                    if(rule.Contains(".")) {
                        var j = rule.IndexOf('.');
                        var table = tInfo(rule.Substring(0, j));
                        if(table == null) return "@" + v;
                        var fieldName = rule.Substring(j + 1);
                        var field = table[fieldName];
                        vali.add(key, field[4] is DBNull ? "" : (string)field[4]);
                        type = fieldType[(string)field[1]];
                    } else {
                        if(rule.Contains("int")) type = fieldType["int"];
                        else if(rule.Contains("float")) type = fieldType["float"];
                        else type = fieldType["nvarchar"];
                        vali.add(key, rule);
                    }
                    param.Add(new Item() { key = key, type = type });
                    return "@" + key;
                } else {
                    replacer.Add(v);
                    return "@" + v + "@";
                }
            }
            internal Dictionary<string, ValiResult> prepare(string q, SqlCommand cmd, Dictionary<string, string> opt) {
               var result = bs.valiResult();
                vali.setMsg(q);
                if(!vali.check(out result, opt)) return result;
                var query = sql;
                foreach(var k in replacer) query = new Regex("@" + k + "@").Replace(query, opt[k]);
                cmd.CommandText = query;
                foreach(var k in param) {
                    var p = new SqlParameter("@" + k.key, k.type);
                    p.Value = result[k.key].value;
                    cmd.Parameters.Add(p);
                }
                return null;
            }
        }

        public static Dictionary<string, ValiResult> dbError() {
            return null;
        }
        
        private ConcurrentDictionary<string, SqlConnection> conns = new ConcurrentDictionary<string, SqlConnection>();
        private ConcurrentDictionary<string, Query> queries = new ConcurrentDictionary<string, Query>();
        private void dbInit(IConfigurationRoot configuration) {
            foreach(var k in configuration.GetSection("ConnectionStrings").GetChildren()) dbConn(k.Key, k.Value);
            var query = configuration.GetSection("query");
            foreach(var db in query.GetChildren()) {
                foreach(var q in query.GetSection(db.Key).GetChildren()) dbQuery(db.Key, q.Key, q.Value);
            }
        }
        public void dbConn(string key, string conn) {
            if(conns.ContainsKey(key)) {
                log("dbConn:set:no exist key - " + key);
                return;
            }
            if(!conns.TryAdd(key, new SqlConnection(conn))) {
                log("dbConn:set:fail to add key - " + key + ", " + conn);
                return;
            }
        }
        internal SqlConnection dbConn(string key) {
            if(!conns.ContainsKey(key)) {
                log("dbConn:get:no exist key - " + key);
                return null;
            }
            SqlConnection conn;
            if(!conns.TryGetValue(key, out conn)) {
                log("dbConn:get:fail to get key - " + key);
                return null;
            }
            return conn;
        }
        public void dbQuery(string path) {
            foreach(var k in JObject.Parse(File.ReadAllText(path))) {
                if(!dbQuery(k.Key, k.Value.Value<string>(), path)) {
                    log("dbQuery:fail to add in " + path);
                    break;
                }
            }
        }
        public bool dbQuery(string db, string key, string sql) {
            if(queries.ContainsKey(key)) {
                log("dbQuery:exist key - " + key);
                return false;
            }
            var query = new Query(this, db, sql);
            return queries.TryAdd(key, query);
        }
        private SqlCommand dbBegin(string query, out Query q) {
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
        private void dbEnd(SqlCommand cmd) {
            cmd.Connection.Close();
        }
        public int dbExec(string query, params object[] args) { // "conn:querykey"
            Query q;
            SqlCommand cmd = dbBegin(query, out q);
            if(cmd == null) return 0;
            int row = cmd.ExecuteNonQuery();
            dbEnd(cmd);
            return row;
        }
        
        public List<Object[]> dbSelect(out Dictionary<string, ValiResult> err, string query, params string[] kv) {
            return dbSelect(out err, query, opt(kv));
        }
        public List<Object[]> dbSelect(out Dictionary<string, ValiResult> err, string query, Dictionary<string, string> opt = null) {
            Query q;
            SqlCommand cmd = dbBegin(query, out q);
            if(cmd == null) {
                err = null;
                log("dbSelect:fail to dbBegin - " + query);
                return null;
            }
            err = q.prepare(query, cmd, opt);
            if(err != null) return null;
            SqlDataReader rs = cmd.ExecuteReader();
            int j = rs.FieldCount;
            List<Object[]> result = new List<Object[]>();
            while(rs.Read()) {
                Object[] record = new Object[j];
                rs.GetValues(record);
                result.Add(record);
            }
            dbEnd(cmd);
            return result;
        }
    }
}
