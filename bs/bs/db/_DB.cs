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
        public static int dbExec(string query, params object[] args) { // "conn:querykey"
            Query q;
            SqlCommand cmd = dbBegin(query, out q);
            if(cmd == null) return 0;
            int row = cmd.ExecuteNonQuery();
            dbEnd(cmd);
            return row;
        }
        public static List<Object[]> dbSelect(out Dictionary<string, ValiResult> err, string query, params string[] kv) {
            return dbSelect(out err, query, opt(kv));
        }
        public static List<Object[]> dbSelect(out Dictionary<string, ValiResult> err, string query, Dictionary<string, string> opt = null) {
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
        private static ConcurrentDictionary<string, SqlConnection> conns = new ConcurrentDictionary<string, SqlConnection>();
        private static ConcurrentDictionary<string, Query> queries = new ConcurrentDictionary<string, Query>();
        private static void dbInit(IConfigurationRoot configuration) {
            foreach(var k in configuration.GetSection("ConnectionStrings").GetChildren()) dbConn(k.Key, k.Value);
            var query = configuration.GetSection("query");
            foreach(var db in query.GetChildren()) {
                foreach(var q in query.GetSection(db.Key).GetChildren()) dbQuery(db.Key, q.Key, q.Value);
            }
        }
        private static void dbConn(string key, string conn) {
            if(conns.ContainsKey(key)) {
                log("dbConn:set:no exist key - " + key);
                return;
            }
            if(!conns.TryAdd(key, new SqlConnection(conn))) {
                log("dbConn:set:fail to add key - " + key + ", " + conn);
                return;
            }
        }
        private static SqlConnection dbConn(string key) {
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