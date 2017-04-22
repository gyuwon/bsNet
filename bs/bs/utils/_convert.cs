﻿using Newtonsoft.Json.Linq;
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
            {typeof(JObject), "jobject"}
        };
        public static T to<T>(object v) {
            var a = 12.3F;
            var b = 12;
            var c = (int)a + b;
            return (T)Convert.ChangeType(v, typeof(T));
        }
        /*
        public static T Plus<T>(object a, object b) {
            return (T)((dynamic)to<T>(a) + (dynamic)to<T>(b));
        }
        */
    }
}