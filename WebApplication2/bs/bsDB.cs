using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.ComponentModel.DataAnnotations;

namespace com.bsidesoft.cs {
    class bsRule:ValidationAttribute {

    }
    public partial class bs {
        private ConcurrentDictionary<string, SqlConnection> conns = new ConcurrentDictionary<string, SqlConnection>();
        private ConcurrentDictionary<string, Query> queries = new ConcurrentDictionary<string, Query>();

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
                if(!dbQuery(k.Key, k.Value.Value<string>(), path)) break;
            }
        }
        
        
        public bool dbQuery(string db, string key, string sql, string path = "-") {
            if(queries.ContainsKey(key)) {
                log("dbQuery:exist key - " + key + " in " + path);
                return false;
            }
            /*
            bs.dbQuery("local", "a", "select * from test where title=@title:test.title@");
            bs.dbQuery("local", "a", "select * from test where title=@title:range[1,2]|or|integer,<%,%>,<%>@");
            bs.dbQuery("local", "a", "select * from test where title=@title:$rule1.a.b.c@");
            */
            var query = new Query(this, db, sql);
            
            log(query.sql);

            return queries.TryAdd(key, query);
        }
        private SqlCommand dbBegin(string query) {
            string[] strs = query.Split(':');
            string target = strs[0], sqlKey = strs[1];
            Query q;
            if(!queries.ContainsKey(sqlKey)) {
                log("dbExec:no exist query - " + sqlKey);
                return null;
            }
            if(!queries.TryGetValue(sqlKey, out q)) {
                log("dbExec:fail to get query - " + sqlKey);
                return null;
            }
            SqlConnection conn = dbConn(target);
            if(conn == null) return null;
            try {
                conn.Open();
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = q.sql;
                return cmd;
            } catch(SqlException e) {
                log("dbExec:fail to connection - " + e.Message);
            }
            return null;
        }
        private void dbEnd(SqlCommand cmd) {
            cmd.Connection.Close();
        }
        public int dbExec(string query, params object[] args) { // "conn:querykey"
            SqlCommand cmd = dbBegin(query);
            if(cmd == null) return 0;
            int row = cmd.ExecuteNonQuery();
            dbEnd(cmd);
            return row;
        }
        public List<Object[]> dbSelect(string query, params object[] args) {
            SqlCommand cmd = dbBegin(query);
            if(cmd == null) return null;
            /*
            foreach(var k in args) {
                
            }
            */
            SqlParameter p = new SqlParameter("@title", SqlDbType.NChar);
            p.Value = args[0];
            cmd.Parameters.Add(p);
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
