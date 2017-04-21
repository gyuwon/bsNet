using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Text;

namespace com.bsidesoft.cs {
    public partial class bs {
        public class bsFilterAction:IActionFilter {
            public void OnActionExecuting(ActionExecutingContext c) {
                //c의 주소에 따라 bs.S 레벨에 저장된 람다를 호출
            }
            public void OnActionExecuted(ActionExecutedContext c) {
                
            }
        }
    }
}
