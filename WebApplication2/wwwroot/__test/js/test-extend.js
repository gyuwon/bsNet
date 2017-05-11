"use strict";
const Net = (function () {
    var isRun = 0, curr,
        jsonReq = function (req) {
            let result = {};
            for (let k in req) {
                if (typeof req[k] == 'function') result[k] = req[k]();
                //if(typeof result[k] == 'object') result[k] = jsonReq(result[k]);
                else result[k] = req[k];
            }
            return result;
        },
        nextReq = function () {
            var i, j, t0, t1, t2;
            if (curr.req.length == curr.runIdx) {
                t0 = curr, curr = 0, isRun = 0;
                if (t0.end) t0.end(t0.res); //netEnd호출 
            } else {
                for (t0 = curr.req[curr.runIdx++], t1 = [t0[0], typeof t0[1] == 'function' ? t0[1]() : t0[1]], i = 2, j = t0.length; i < j;) {
                    t1[t1.length] = t0[i++], t2 = t0[i++];
                    if (typeof t2 == 'function') t2 = t2();
                    if (t1[t1.length - 1] == 'json') t2 = jsonReq(t2);
                    t1[t1.length] = t2;
                }
                //if(i = bs.local('atoken')) t1[t1.length] = 'atoken', t1[t1.length] = i;
                //if(i = bs.local('stoken')) t1[t1.length] = 'stoken', t1[t1.length] = i;
                if (i = bs('memobj.amo')) t1[t1.length] = '@amo', t1[t1.length] = i;
                if (i = bs.local('memobj.pmo')) t1[t1.length] = '@pmo', t1[t1.length] = i;
                if (i = bs.local('memobj.tmo')) t1[t1.length] = '@tmo', t1[t1.length] = i;

                curr.req[curr.runIdx - 1] = t1;
                bs[curr.method[curr.runIdx - 1]].apply(null, t1);
            }
        },
        add = function (method, path) {
            var arg, i, j;
            if (isRun == 1) return console.error('net.run is running');
            arg = [function (v, status) {
                var t0;
                bs('memobj.amo', this.getResponseHeader('amo'));
                bs.local('memobj.pmo', this.getResponseHeader('pmo'));
                bs.local('memobj.tmo', this.getResponseHeader('tmo'));
                if (status == 200) {
                    curr.res[curr.res.length] = v;
                } else {
                    curr.res[curr.res.length] = '';
                }
                if (curr.process) {
                    curr.process(curr.res[curr.res.length - 1], curr.res.length - 1);
                }
                nextReq();
            }, path];
            for (i = 2, j = arguments.length; i < j;) {
                arg[arg.length] = arguments[i++], arg[arg.length] = arguments[i++];
            }
            arg[arg.length] = '@timeoffset', arg[arg.length] = new Date().getTimezoneOffset();
            arg[arg.length] = '@ver', arg[arg.length] = "driver android 2.0.2";
            if (!curr) curr = { runIdx: 0, req: [], res: [], end: 0, process: 0, method: [] };
            curr.req[curr.req.length] = arg;
            curr.method[curr.method.length] = method;
            return this;
        };
    return {
        run: function (end, process) {
            var arg, i;
            if (isRun == 1) return console.error('net.run is running');
            for (i = 2; i < arguments.length; i++) {
                arg = arguments[i];
                add.apply(null, arg);
            }
            isRun = 1, curr.end = end, curr.process = process, nextReq();
        }
    };
})();
const TEST = (_ => {
    let nets = [], keys = [], net;
    const init = flow => {
        const netProcess = (v, idx) => {
            let t0, t1;
            if (!(t0 = flow.S('data'))) t0 = [];
            t0[idx] = v;
            flow.S('data', t0);
            t1 = net[idx];

            //end 함수 호출 
            if (t0 = t1.end) t0(v, idx);

            //success, fail 함수를 등록했으면 json파싱하고 적절히 data를 파싱한뒤 원본 데이타 대신 해석된 데이타로 대체한다. 
            if (t1.isAPI) {
                try {
                    v = JSON.parse(v);
                    if (v.data) {
                        v = v.data;
                        if (t1.success) v = t1.success(v, idx);
                    } else {
                        v = { error: v.error };
                        if (t1.fail) v = t1.fail(v, idx);
                    }
                } catch (e) {
                    v = { error: [{ method: 'alert', msg: 'Unknown error' }] };
                    if (t1.fail) v = t1.fail(v, idx);
                }
                flow.S('data')[idx] = v;
            }
        },
        netEnd = v => {
            flow.run();
            next();
        };
        flow.S(
            'check', (flow, idx, v) => {
                let data = flow.S('data')[idx], t = net[idx];
                flow.ok(t.msg + ' 성공'), flow.fail(t.msg + ' 실패');
                return t.check(data) ? 1 : 0;
                //return t.check(data);
            },
            'run', (flow, msg, key) => {
                if (msg) flow.ok(msg);
                if (key) setTimeout(_ => flow.run(key));
                return 1;
            }
        );
        let t, i, j, arg;
        for (arg = [netEnd, netProcess], i = 0, j = net.length; i < j; i++) {
            t = net[i];
            if (!t.net || !t.msg || !t.check) return console.error('test에 net, msg, check키중 하나가 없음!');
            if (t.isAPI == undefined) t.isAPI = 1;
            arg[arg.length] = t.net;
        }
        Net.run.apply(null, arg);
        flow.hold();
    };
    const next = _ => {
        if (keys.length == 0) return;
        let k = keys.shift();
        let checkList = [];
        net = nets[k];
        net.forEach((v, i) => {
            checkList.push(flow => flow.S('check')(flow, i, v), 1);
        });
        bsTest.run.apply(bsTest, [k, init].concat(checkList));
    };
    return {
        add: (key, ...v) => {
            if (!nets[key]) {
                nets[key] = [];
            }
            nets[key] = nets[key].concat(v);
        },
        start: (...v) => {
            keys = v;
            next();
        }
        /*
        add: (...v) => {
            net = net.concat(v);
        },
        start: title => {
            let checkList = [];
            net.forEach((v, i) => {
                checkList.push(flow => flow.S('check')(flow, i, v), 1);
            });
            bsTest.run.apply(bsTest, [title, init].concat(checkList));
        }*/
    }
})();
/*
const testStart = (title, net) => {
    TEST.add.apply(TEST, net);
    TEST.start(title);
};
*/
bs(
    'path', (i, path, p1, p2) => {
        let t = path;
        if (p1) t += '/' + (typeof p1 == 'function' ? p1() : p1);
        if (p2) t += '/' + (typeof p2 == 'function' ? p2() : p2);
        return t + (path.indexOf('?') == -1 ? '?' : '&') + i;
    },
    'zeroPad', (nr, base) => {
        let len = (String(base).length - String(nr).length) + 1;
        return len > 0 ? new Array(len).join('0') + nr : nr;
    },
    'compare', (a, b) => {
        let stack = [[a, b]], i;
        for (let [A, B] of stack) {
            if ((!A || typeof A != "object") && A !== B) return false;
            if (A instanceof Array) {
                if (!(A instanceof Array) || A.length != B.length) return false;
                A.forEach((v, i) => stack.push([v, B[i]]));
            } else if ((i = Object.keys(A)).length != Object.keys(B).length) return false;
            i.forEach(i => stack.push([A[i], B[i]]));
        }
        return true;
    },
    //base64를 Blob데이타 만들기 
    'base64toBlob', (base64Data, contentType) => {
        let sliceSize, byteCharacters, bytesLength, slicesCount, byteArrays;
        contentType = contentType || '',
            sliceSize = 1024, byteCharacters = atob(base64Data),
            bytesLength = byteCharacters.length,
            slicesCount = Math.ceil(bytesLength / sliceSize),
            byteArrays = new Array(slicesCount);
        for (let sliceIndex = 0; sliceIndex < slicesCount; ++sliceIndex) {
            let begin, end, bytes;
            begin = sliceIndex * sliceSize, end = Math.min(begin + sliceSize, bytesLength),
                bytes = new Array(end - begin);
            for (let offset = begin, i = 0; offset < end; ++i, ++offset) {
                bytes[i] = byteCharacters[offset].charCodeAt(0);
            }
            byteArrays[sliceIndex] = new Uint8Array(bytes);
        }
        return new Blob(byteArrays, { type: contentType });
    },
    'rand', _ => Math.floor(Math.random() * 1000000),
    'alert', (v, idx, code, msg) => {
        let e;
        if (!v || !v.error) return console.log('실패를 체크해야 하는데 성공함'), false;
        if (!(e = v.error[idx])) return console.log('에러 idx에 해당 에러 데이터 없음'), false;
        if (e.method != 'alert') return console.log('method가 alert가 아님'), false;
        return e.code == code && ((e.msg && e.msg == msg) || (e.title && e.contents && e.contents == msg));
    },
    'rerender', (v, idx, ...arg) => {
        let e;
        if (!v || !v.error) return console.log('실패를 체크해야 하는데 성공함'), false;
        if (!(e = v.error[idx])) return console.log('에러 idx에 해당 에러 데이터 없음'), false;
        if (e.method != 'rerender') return console.log('method가 rerender가 아님'), false;
        if (!e.vali) return console.log('vali키가 없음'), false;
        let i, j, k, l, m, n;
        for (j = arg.length, i = 0; i < j;) {
            k = arg[i++], v = arg[i++];
            k = k.split('.'), n = e.vali[k[0]];
            if (!n) return console.log('키에 대한 vali값을 발견하지 못함 k = ' + k.join('.')), false;
            for (m = k.length, l = 1; l < m; l++) {
                if (typeof n != 'object') return console.log('키에 대한 서버 vali값이 없음. k = ' + k.join('.')), false;
                n = n[k[l]];
            }
            if (n.msg || n.msg == '') {
                if (n.msg != v) return console.log('메시지가 틀림 k = ' + k.join('.') + ', 클라msg =' + v, ' 서버msg = ' + n.msg), false;
            } else if (typeof n == 'object' && typeof v == 'object') {
                if (JSON.stringify(n) != JSON.stringify(v)) return console.log('서버,클라 vali 비교 데이타 틀림. k=' + k.join('.')), false;
            } else return console.log('잘못된 값으로 비교한 것으로 보임. 테스트 코드 수정할 것! k=' + k.join('.')), false;
        }
        return true;
    },
    'ok', (v, ...arg) => {
        if (!v || v.error) return console.log('성공을 check해야 하는데 에러가 발생함', v, arg), false;
        let i, j, k, l, m, n, o;
        for (j = arg.length, i = 0; i < j;) {
            k = arg[i++], o = arg[i++];
            k = k.split('.'), n = v[k[0]];
            for (m = k.length, l = 1; l < m; l++) {
                if (typeof n != 'object') return console.log('키에 대한 서버값이 없음. k = ' + k.join('.')), false;
                n = n[k[l]];
            }
            if (typeof o == 'function') {
                o = o();
                console.log('===========', o);
            }
            if (typeof n == 'object' && typeof o == 'object') {
                console.log('비교값 : ' + JSON.stringify(o));
                console.log('서버 리스폰스 값: ' + JSON.stringify(n));

                if (JSON.stringify(n) != JSON.stringify(o)) return console.log('서버,클라 객체 데이타가 값이 틀림. k=' + k.join('.')), false;
            } else if (n != o) return console.log('서버,클라 데이타가 값이 틀림. k=' + k.join('.')), false;
        }
        return true;
    }
);