using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace com.bsidesoft.cs {
    public partial class bs {
        private static Dictionary<Type, string> TYPES = new Dictionary<Type, string>() {
            {typeof(bool), "bool"}, {typeof(char), "char"}, {typeof(sbyte), "sbyte"}, {typeof(byte), "byte"},
            {typeof(ushort), "ushort"}, {typeof(uint), "uint"}, {typeof(ulong), "ulong"},
            {typeof(short), "short"}, {typeof(int), "int"}, {typeof(long), "long"},
            {typeof(float), "float"}, {typeof(double), "double"}, {typeof(decimal), "decimal"},
            {typeof(string), "string"},{typeof(string[]), "string[]"},

            {typeof(StreamReader), "streamreader"}, {typeof(FileStream), "filestream"},
            {typeof(JObject), "jobject"},
            {typeof(List<Object[]>), "list<object[]>"},
            {typeof(List<Dictionary<String, String>>), "list<dictionary<string,string>>"},
            {typeof(List<String>), "list<string>"},
            {typeof(List<int>), "list<int>"},
            {typeof(Dictionary<String, String>), "dictionary<string,string>"}
        };
        public static T to<T>(object v) {
            if (v is String && (String)v == "") v = default(T); 
            return (T)Convert.ChangeType(v, typeof(T));
        }
    }
}