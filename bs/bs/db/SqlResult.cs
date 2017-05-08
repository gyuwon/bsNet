using System.Collections.Generic;

namespace com.bsidesoft.cs {
    public partial class bs {
        public static SqlResult<T> dbResult<T>(){ return null;}
        public class SqlResult<T> {
            public Dictionary<string, ValiResult> vali;
            public bool valiError;
            public bool noRecord;
            public bool castFail;
            public int insertId;
            public T result = default(T);
            internal SqlResult(Dictionary<string, ValiResult> v) {
                vali = v;
                if(vali != null) valiError = noRecord = true;
            }
        }
    }
}