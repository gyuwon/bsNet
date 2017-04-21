using System;
using System.ComponentModel.DataAnnotations;

namespace com.bsidesoft.cs {
    public partial class bs {
        public static int toI(object v) {
            if(v is ushort) return Convert.ToInt32((ushort)v);
            if(v is string) return Convert.ToInt32((string)v);
            if(v is float) return Convert.ToInt32((float)v);
            if(v is sbyte) return Convert.ToInt32((sbyte)v);
            if(v is object) return Convert.ToInt32((object)v);
            if(v is double) return Convert.ToInt32((double)v);
            if(v is long) return Convert.ToInt32((long)v);
            if(v is short) return Convert.ToInt32((short)v);
            if(v is uint) return Convert.ToInt32((uint)v);
            if(v is decimal) return Convert.ToInt32((decimal)v);
            if(v is char) return Convert.ToInt32((char)v);
            if(v is byte) return Convert.ToInt32((byte)v);
            if(v is bool) return Convert.ToInt32((bool)v);
            if(v is ulong) return Convert.ToInt32((ulong)v);
            return (int)v;
        }
        public static float toF(object v) {
            if(v is ulong) return Convert.ToSingle((ulong)v);
            if(v is uint) return Convert.ToSingle((uint)v);
            if(v is ushort) return Convert.ToSingle((ushort)v);
            if(v is string) return Convert.ToSingle((string)v);
            if(v is sbyte) return Convert.ToSingle((sbyte)v);
            if(v is object) return Convert.ToSingle((object)v);
            if(v is decimal) return Convert.ToSingle((decimal)v);
            if(v is long) return Convert.ToSingle((long)v);
            if(v is int) return Convert.ToSingle((int)v);
            if(v is short) return Convert.ToSingle((short)v);
            if(v is double) return Convert.ToSingle((double)v);
            if(v is byte) return Convert.ToSingle((byte)v);
            if(v is bool) return Convert.ToSingle((bool)v);
            return (float)v;
        }
        public static double toD(object v) {
            if(v is object) return Convert.ToDouble((object)v);
            if(v is ulong) return Convert.ToDouble((ulong)v);
            if(v is uint) return Convert.ToDouble((uint)v);
            if(v is ushort) return Convert.ToDouble((ushort)v);
            if(v is string) return Convert.ToDouble((string)v);
            if(v is float) return Convert.ToDouble((float)v);
            if(v is sbyte) return Convert.ToDouble((sbyte)v);
            if(v is int) return Convert.ToDouble((int)v);
            if(v is long) return Convert.ToDouble((long)v);
            if(v is short) return Convert.ToDouble((short)v);
            if(v is decimal) return Convert.ToDouble((decimal)v);
            if(v is byte) return Convert.ToDouble((byte)v);
            if(v is bool) return Convert.ToDouble((bool)v);
            return (double)v;
        }
        public static string toS(object v) {
            if(v is string) return (string)v;
            return v + "";
        }
        public static bool toB(object v) {
            if(v is sbyte) return Convert.ToBoolean((sbyte)v);
            if(v is ulong) return Convert.ToBoolean((ulong)v);
            if(v is uint) return Convert.ToBoolean((uint)v);
            if(v is ushort) return Convert.ToBoolean((ushort)v);
            if(v is string) return Convert.ToBoolean((string)v);
            if(v is float) return Convert.ToBoolean((float)v);
            if(v is object) return Convert.ToBoolean((object)v);
            if(v is long) return Convert.ToBoolean((long)v);
            if(v is int) return Convert.ToBoolean((int)v);
            if(v is short) return Convert.ToBoolean((short)v);
            if(v is double) return Convert.ToBoolean((double)v);
            if(v is decimal) return Convert.ToBoolean((decimal)v);
            if(v is byte) return Convert.ToBoolean((byte)v);
            return (bool)v;
        }
    }
}
