using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace com.bsidesoft.cs {
    public partial class bs {
        public static void log(params string[] args) {
            if (logger == null) return;
            logger.LogInformation(String.Join(", ", args));
        }
    }
}
