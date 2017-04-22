using Microsoft.AspNetCore.Mvc;

namespace com.bsidesoft.cs {
    public partial class bs {
        public static object before(Controller c) {
            return c.ViewBag.bsBefore;
        }
    }
}