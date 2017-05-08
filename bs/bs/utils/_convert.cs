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

            {typeof(byte[]), "byte[]"}, {typeof(Stream), "stream"},

            {typeof(StreamReader), "streamreader"}, {typeof(FileStream), "filestream"},
            {typeof(JObject), "jobject"},
            {typeof(List<Object[]>), "list<object[]>"},
            {typeof(List<Dictionary<string, string>>), "list<dictionary<string,string>>"},
            {typeof(List<string>), "list<string>"},
            {typeof(List<int>), "list<int>"},
            {typeof(Dictionary<string, string>), "dictionary<string,string>"}
        };
        public static T to<T>(object v) {
            if (v is string && (string)v == "") v = default(T); 
            return (T)Convert.ChangeType(v, typeof(T));
        }
    }
}