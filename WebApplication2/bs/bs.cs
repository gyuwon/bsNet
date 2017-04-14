using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.IO;
using Microsoft.Extensions.Logging;
using System;

namespace com.bsidesoft.cs {
    public partial class bs{
        public static readonly object OK = new { }, FAIL = new { };
        public static bool isOK(object v) {
            return v == OK;
        }
        public static bool isFAIL(object v) {
            return v == FAIL;
        }

        internal static ILogger logger;
        public static void log(params string[] args) {
            logger.LogInformation(String.Join(", ", args));
        }

        public bs() {
        }
    }
}
