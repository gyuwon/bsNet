using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace com.bsidesoft.cs {
    public partial class bs {
        public class SqlHandler {
            private static HashSet<string> scalarType = new HashSet<string>() { "int", "bool", "string", "float", "double" };
            private static Dictionary<string, Func<SqlDataReader, int, object>> rsResult = new Dictionary<string, Func<SqlDataReader, int, object>>{
                {"list<string>", (rs, fc)=>{
                    var v = new List<String>();
                    while(rs.Read()) v.Add(rs.GetString(0));
                    return v;
                }},
                {"list<int>", (rs, fc)=>{
                    var v = new List<int>();
                    while(rs.Read()) v.Add(rs.GetInt32(0));
                    return v;
                }},
                {"list<object[]>", (rs, fc)=>{
                    var v = new List<Object[]>();
                    while(rs.Read()) {
                        Object[] record = new Object[fc];
                        rs.GetValues(record);
                        v.Add(record);
                    }
                    return v;
                }},
                {"list<dictionary<string,string>>", (rs, fc)=>{
                    var v = new List<Dictionary<String, String>>();
                    while(rs.Read()) {
                        var record = new Dictionary<String, String>();
                        for(var i = 0; i<fc; i++) record.Add(rs.GetName(i), rs.GetValue(i) + "");
                        v.Add(record);
                    }
                    return v;
                }},
                {"dictionary<string,string>", (rs, fc)=>{
                    var v = new Dictionary<String, String>();
                    if(!rs.Read()) return v;
                    for(var i = 0; i < fc; i++) v.Add(rs.GetName(i), rs.GetValue(i) + "");
                    return v;
                }}
            };

            SqlCommand cmd;

            internal SqlHandler(SqlCommand c) {
                cmd = c;
            }
            public SqlResult<int> exec(string query, params object[] kv) {
                return exec(query, opt<object>(kv));
            }
            public async Task<SqlResult<int>> execAsync(string query, params object[] kv) {
                return await execAsync(query, opt<object>(kv));
            }
            public SqlResult<int> exec(string query, Dictionary<string, object> opt = null) {
                var q = Query.get(query);
                var result = new SqlResult<int>(q.prepare(query, cmd, opt));
                if(result.valiError) return result;
                result.result = cmd.ExecuteNonQuery();
                if(q.isInsert()) {
                    cmd.CommandText = "SELECT @@IDENTITY";
                    cmd.Parameters.Clear();
                    var id = cmd.ExecuteScalar();
                    result.insertId = id == DBNull.Value ? 0 : bs.to<int>(id);
                }
                return result;
            }
            public async Task<SqlResult<int>> execAsync(string query, Dictionary<string, object> opt = null) {
                var q = Query.get(query);
                var result = new SqlResult<int>(q.prepare(query, cmd, opt));
                if(result.valiError) return result;
                result.result = await cmd.ExecuteNonQueryAsync();
                if(q.isInsert()) {
                    cmd.CommandText = "SELECT @@IDENTITY";
                    cmd.Parameters.Clear();
                    var id = await cmd.ExecuteScalarAsync();
                    result.insertId = id == DBNull.Value ? 0 : bs.to<int>(id);
                }
                return result;
            }
            public async Task<SqlResult<T>> selectAsync<T>(string query, params string[] kv) {
                return await selectAsync<T>(query, opt<object>(kv));
            }
            public SqlResult<T> select<T>(string query, params string[] kv) {
                return select<T>(query, opt<object>(kv));
            }
            public SqlResult<T> select<T>(string query, Dictionary<string, object> opt = null) {
                var result = new SqlResult<T>(Query.get(query).prepare(query, cmd, opt));
                if(result.valiError) return result;
                var type = TYPES[typeof(T)];
                if(scalarType.Contains(type)) {
                    var v = cmd.ExecuteScalar();
                    if(v == null) result.noRecord = true;
                    else try {
                            result.result = (T)v;
                        } catch(Exception e) {
                            result.noRecord = result.castFail = true;
                        }
                } else {
                    var rs = cmd.ExecuteReader();
                    if(!rs.HasRows) result.noRecord = true;
                    else if(rsResult.ContainsKey(type)) result.result = (T)rsResult[type](rs, rs.FieldCount);
                    else result.noRecord = result.castFail = true;
                }
                return result;
            }
            public async Task<SqlResult<T>> selectAsync<T>(string query, Dictionary<string, object> opt = null) {
                var result = new SqlResult<T>(Query.get(query).prepare(query, cmd, opt));
                if(result.valiError) return result;
                var type = TYPES[typeof(T)];
                if(scalarType.Contains(type)) {
                    var v = await cmd.ExecuteScalarAsync();
                    if(v == null) result.noRecord = true;
                    else try {
                            result.result = (T)v;
                        } catch(Exception e) {
                            result.noRecord = result.castFail = true;
                        }
                } else {
                    var rs = await cmd.ExecuteReaderAsync();
                    if(!rs.HasRows) result.noRecord = true;
                    else if(rsResult.ContainsKey(type)) result.result = (T)rsResult[type](rs, rs.FieldCount);
                    else result.noRecord = result.castFail = true;
                }
                return result;
            }
        }
    }
}