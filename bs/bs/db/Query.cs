using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace com.bsidesoft.cs {
    public partial class bs {
        internal class Query {
            internal static Query get(string query) {
                Query q;
                if(!queries.TryGetValue(query, out q)) {
                    log("get:fail to get query - " + query);
                    return null;
                }
                return q;
            }
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
            private Dictionary<string, int> keys = new Dictionary<string, int>();
            private Vali vali = new Vali();
            
            public Query(string target, string query) {
                db = target;
                sql = qsep.Replace(query, parse);
            }
            internal bool isInsert() {
                return sql.Contains("insert into");
            }
            private Dictionary<string, Object[]> tInfo(string table) {
                if(!tableInfo.ContainsKey(table)) {
                    SqlConnection conn = dbConnGet(db);
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
                    var id = 0;
                    if(!vali.hasKey(key)) {
                        if(rule.Contains(".")) {
                            var j = rule.IndexOf('.');
                            var table = tInfo(rule.Substring(0, j));
                            if(table == null) return "@" + v;
                            var fieldName = rule.Substring(j + 1);
                            var field = table[fieldName];
                            type = fieldType[(string)field[1]];
                            rule = field[4] is DBNull ? "" : (string)field[4];
                        } else {
                            if(rule.Contains("int")) type = fieldType["int"];
                            else if(rule.Contains("float")) type = fieldType["float"];
                            else type = fieldType["nvarchar"];

                        }
                        vali.add(key, rule);
                        keys.Add(key, 0);
                    } else {
                        id = ++keys[key];
                    }
                    param.Add(new Item() { key = key, type = type });
                    return "@" + key + "_" + id;
                } else {
                    replacer.Add(v);
                    return "@" + v + "@";
                }
            }
            internal Dictionary<string, ValiResult> prepare(string q, SqlCommand cmd, params object[] arg) {
                return prepare(q, cmd, opt<object>(arg));
            }
            internal Dictionary<string, ValiResult> prepare(string q, SqlCommand cmd, Dictionary<string, object> opt = null) {
                var result = valiResult();
                vali.setMsg(q);
                if(!vali.check(out result, opt)) return result;
                var query = sql;
                foreach(var k in replacer) query = new Regex("@" + k + "@").Replace(query, opt[k] + "");
                cmd.CommandText = query;
                cmd.Parameters.Clear();
                foreach(var k in param) {
                    for(var i = keys[k.key]; i > -1; i--) {
                        var p = new SqlParameter("@" + k.key + "_" + i, k.type);
                        p.Value = result[k.key].value;
                        cmd.Parameters.Add(p);
                    }
                }
                return null;
            }
        }
    }
}