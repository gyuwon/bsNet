"use strict";
const Net = (function(){
	var isRun = 0, curr, 
	nextReq = function(){
		var i, j, t0, t1, t2;
		if(curr.req.length == curr.runIdx){
			t0 = curr, curr = 0, isRun = 0;
			if(t0.end) t0.end(t0.res);
		}else{
			for(t0 = curr.req[curr.runIdx++], t1 = [t0[0], typeof t0[1] == 'function' ? t0[1]() : t0[1]], i = 2, j = t0.length; i < j; ){
				t1[t1.length] = t0[i++], t2 = t0[i++];
				if(typeof t2 == 'function') t2 = t2(); 
				t1[t1.length] = t2;
			}
			if(i = bs.local('atoken')) t1[t1.length] = 'atoken', t1[t1.length] = i;
			if(i = bs.local('stoken')) t1[t1.length] = 'stoken', t1[t1.length] = i;
			if(i = bs('memobj.amo')) t1[t1.length] = '@amo', t1[t1.length] = i;
			if(i = bs('memobj.pmo')) t1[t1.length] = '@pmo', t1[t1.length] = i;
			if(i = bs('memobj.tmo')) t1[t1.length] = '@tmo', t1[t1.length] = i;
			curr.req[curr.runIdx - 1] = t1;
			bs[curr.method[curr.runIdx - 1]].apply(null, t1);
		}
	},
	add = function(method, path){
		var arg, i, j;
		if(isRun == 1) return console.error('net.run is running');
		arg = [function(v, status){
			var t0;
			bs('memobj.amo', this.getResponseHeader('amo'));
			bs('memobj.pmo', this.getResponseHeader('pmo'));
			bs('memobj.tmo', this.getResponseHeader('tmo'));
			
			if(status == 200){
				curr.res[curr.res.length] = v;
			}else{
				curr.res[curr.res.length] = '';
			}
			if(curr.process){
				curr.process(curr.res[curr.res.length-1], curr.res.length-1);
			}
			nextReq();
		},  path];
		for(i = 2, j = arguments.length; i < j;){
			arg[arg.length] = arguments[i++], arg[arg.length] = arguments[i++];
		}
		if(!curr) curr = {runIdx:0, req:[], res:[], end:0, process:0, method:[]};
		curr.req[curr.req.length] = arg;
		curr.method[curr.method.length] = method;
		return this;
	};
	return {
		run:function(end, process){ 
			var arg, i;
			if(isRun == 1) return console.error('net.run is running');
			for(i = 2; i < arguments.length; i++){
				arg = arguments[i];
				add.apply(null, arg);
			}
			isRun = 1, curr.end = end, curr.process = process, nextReq();
		}
	};
})();
const testStart = (title, net)=>{
	const init = flow=>{
		const netProcess = (v, idx)=>{
			let t0;
			if(!(t0 = flow.S('data'))) t0 = [];
			t0[idx] = v;
			flow.S('data', t0);
			if(t0 = net[idx].end) t0(v, idx);
		}, 
		netEnd = v=>{
			flow.run()
		};
		flow.S(
			'check', (flow, idx, v)=>{
				let data = flow.S('data')[idx], t = net[idx];
				flow.ok(t.msg + ' 성공'), flow.fail(t.msg + ' 실패');
				return t.check(data) ? 1 : 0;
				//return t.check(data);
			},
			'run', (flow, msg, key)=>{
				if(msg) flow.ok(msg);
				if(key) setTimeout(_=>flow.run(key));
				return 1;
			}
		);
		let t, i, j, arg;
		for(arg = [netEnd, netProcess], i = 0, j = net.length ; i < j ; i++){
			t = net[i];
			if(!t.net || !t.msg || !t.check) return console.error('test에 net, msg, check키중 하나가 없음!');
			arg[arg.length] = t.net;
		}
		Net.run.apply(null, arg);
		flow.hold();
	};
	let checkList = [];
	net.forEach((v, i)=>{
		checkList.push(flow=>flow.S('check')(flow, i, v), 1);
	});
	let arg = [title, init];
	bsTest.run.apply(bsTest, [title, init].concat(checkList));
};
bs(
	'path', (i, path, p1, p2)=>{
		let t = path;
		if(p1) t += '/' + (typeof p1 == 'function' ? p1() : p1);
		if(p2) t += '/' + (typeof p2 == 'function' ? p2() : p2);
		return t + (path.indexOf('?') == -1 ? '?' : '&') + i;
	}
);