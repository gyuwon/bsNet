using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace com.bsidesoft.cs {
    public partial class bs {
        public static int toI(object v) {
            if(v is string) return Convert.ToInt32((string)v);
            return (int)v;
        }
        public static float toF(object v) {
            if(v is string) return Convert.ToSingle((string)v);
            return (float)v;
        }
        public static double toD(object v) {
            if(v is string) return Convert.ToDouble((string)v);
            return (double)v;
        }
        public static string toS(object v) {
            if(v is string) return (string)v;
            return v + "";
        }
    }
}
