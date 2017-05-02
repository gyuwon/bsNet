using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;

namespace com.bsidesoft.cs {
    public partial class bs {
        public static bool rsOrderChange(ref List<object[]> rs, int index, int r, bool isUp) {
            var result = false;
            int i, j;
            if(isUp) {
                for(i = 0; i < rs.Count; i++) {
                    if(to<int>(rs[i][index]) == r) {
                        if(i > 0) {
                            var temp = rs[i - 1];
                            rs[i - 1] = rs[i];
                            rs[i] = temp;
                            result = true;
                        }
                        break;
                    }
                }
            } else {
                for(i = 0, j = rs.Count; i < j; i++) {
                    if(to<int>(rs[i][index]) == r) {
                        if(i > j - 1) {
                            var temp = rs[i + 1];
                            rs[i + 1] = rs[i];
                            rs[i] = temp;
                            result = true;
                        }
                        break;
                    }
                }
            }
            return result;
        }
        public static bool rsOrderChange(ref List<Dictionary<String, String>> rs, string key, int r, bool isUp) {
            var result = false;
            int i, j;
            if(isUp) {
                for(i = 0; i < rs.Count; i++) {
                    if(to<int>(rs[i][key]) == r) {
                        if(i > 0) {
                            var temp = rs[i - 1];
                            rs[i - 1] = rs[i];
                            rs[i] = temp;
                            result = true;
                        }
                        break;
                    }
                }
            } else {
                for(i = 0, j = rs.Count; i < j; i++) {
                    if(to<int>(rs[i][key]) == r) {
                        if(i < j - 1) {
                            var temp = rs[i + 1];
                            rs[i + 1] = rs[i];
                            rs[i] = temp;
                            result = true;
                        }
                        break;
                    }
                }
            }
            return result;
        }
    }
}