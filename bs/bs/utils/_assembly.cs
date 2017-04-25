using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace com.bsidesoft.cs {
    public partial class bs {
        private static ConcurrentDictionary<string, Assembly> dlls = new ConcurrentDictionary<string, Assembly>();
        private static void assemblyInit() {
            /*
            foreach(var f in Directory.GetFiles(bs.path(), "*.dll")) {
                var a = AssemblyLoadContext.Default.LoadFromAssemblyPath(bs.path() + "/" + f);
                dlls.TryAdd(a.GetName().Name, a);
            }
            */
        }
        public static Type getType(string cls) {
            /*
            foreach(var k in dlls) {
                var t = k.Value.GetType(k.Key + "." + cls);
                if(t != null) return t;
            }
            */
            return null;
        }
    }
}
