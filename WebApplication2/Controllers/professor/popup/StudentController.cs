using com.bsidesoft.cs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace WebApplication2.Controllers {
    public class StudentController:Controller {
        private bs bs;
        public StudentController(bs b) {
            bs = b;
        }
        /*
        public Dictionary<string, object> _excel(ActionExecutingContext c) {

        }*/
        public async Task<IActionResult> excel(IFormFile upfile) {
            var package = new ExcelPackage(upfile.OpenReadStream());
            ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
            int rowCount = worksheet.Dimension.Rows;
            int colCount = worksheet.Dimension.Columns;
            if(colCount < 2 || rowCount < 3) {
                return Json(new { error = "잘못된 엑셀파일입니다." });
            }
            var members = new List<Dictionary<string, string>>();
            for(int row = 2; row <= rowCount; row++) {
                members.Add(new Dictionary<string, string>() {
                    {"num",  worksheet.Cells[row, 1].Value.ToString()},
                    {"name",  worksheet.Cells[row, 2].Value.ToString() },
                    {"pw",  worksheet.Cells[row, 3].Value.ToString() }
                });
            }

            //TODO members를 하나씩 돌면서 insert 할 것. insert의 적용수가 0이면 num이 중복임. 중복은 무시하면 됨. 
            //하지만 다른 vali 에러가 나면 vali 에러난 것끼리 전부 묶어서 에러 출력을 해야 함 

            return Json(new {members=members });
        }

    }
}