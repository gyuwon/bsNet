using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;

namespace com.bsidesoft.cs {
    public partial class bs {
        class Query {
            static Regex qsep = new Regex("@([^@]+)@");
            static ConcurrentDictionary<string, Dictionary<string, Object[]>> tableInfo = new ConcurrentDictionary<string, Dictionary<string, Object[]>>();

            public string db { get; private set; }
            public string sql { get; private set; }
            public List<string> replacer = new List<string>();
            public List<string> param = new List<string>();
            private bs bs;

            public Query(bs b, string target, string query) {
                bs = b;
                db = target;
                sql = qsep.Replace(query, parse);
            }
            private string parse(Match m) {
                string v = m.Groups[1].Value;
                if(v.Contains(":")) {
                    int i = v.IndexOf(':');
                    param.Add(v.Substring(0, i));
                    string rule = v.Substring(i + 1);
                    if(rule.Contains(".")) {
                        int j = rule.IndexOf('.');
                        string table = rule.Substring(0, j), field = rule.Substring(j + 1);
                        if(!tableInfo.ContainsKey(table)) {
                            SqlConnection conn = bs.dbConn(db);
                            if(conn == null) return null;
                            try {
                                conn.Open();
                                SqlCommand cmd = conn.CreateCommand();
                                cmd.CommandText = "select f1.name," +
                                    "(select top 1 s0.name from sys.types s0 where s0.system_type_id=f1.system_type_id),f1.is_nullable,f1.is_identity," +
                                    "(select top 1 value from sys.extended_properties s0 where f0.object_id=s0.major_id and f1.column_id=s0.minor_id and s0.name='MS_Description')comment " +
                                    "from sys.tables f0,sys.columns f1 " +
                                    "where f0.object_id=f1.object_id and f0.name='" + table + "'";
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
                                    bs.log("parse:fail to add - " + table);
                                    return "@" + v;
                                }
                            } catch(SqlException e) {
                                bs.log("parse:fail to connection - " + e.Message);
                                return "@" + v;
                            }
                        }
                        var result = new Dictionary<string, Object[]>();
                        if(!tableInfo.TryGetValue(field, out result)) {
                            bs.log("parse:invalid field name - " + field + " in " + table);
                            return "@" + v;
                        }


                    }
                    return "@" + v;
                } else {
                    replacer.Add(v);
                    return "@" + v + "@";
                }
            }
        }
    }
}
