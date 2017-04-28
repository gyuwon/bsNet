using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace com.bsidesoft.cs {
    public partial class bs {
        public static object before(Controller c) {
            return c.ViewBag.bsBefore;
        }
    }
}