(function(W){
	var fn;
	var Report = function(method){
		this.method = method;
		this.called = [];
		this.param = null;
		this.count = 1;
		this.when = null;
		this.then = null;
		this.isOK = true;
		this.msg = [];
	};
	fn = Report.prototype;
	fn.isSameParam = function(arg){
		var i;
		if(!this.param) return true;
		i = this.param.length;
		if(i != arg.length) return false;
		while(i--) if(this.param[i] !== arg[i]) return false;
		return true;
	};
	fn.report = function(){
		var method = this.method, called = this.called, call, param, i, j, k;
		if(this.count != called.length){
			this.isOK = false;
			this.msg.push('called ' + called.length +' <strong>!==</strong> expected ' + this.count);
		}else{
			if(this.param) param = JSON.stringify(this.param);
			for(i = 0, j = called.length; i < j; i++){
				call = called[i];
				if(!this.isSameParam(call)){
					this.isOK = false;
					this.msg.push('param(' + JSON.stringify(Array.prototype.slice.call(call, 0)) + ') <strong>!==</strong> expected(' + param + ')');
				}
				if(this.when) for(k in this.when) if(this.when.hasOwnProperty(k) && this.when[k] !== call.state[k]){
					this.isOK = false;
					this.msg.push('state(' + k + ':' + this.when[k] + ') <strong>!==</strong> expected(' + k + ':' + call.state[k] + ')');
				}
			}
		}
		return this;
	};
	var Mock = function(title){
		this.title = title;
		this.state = {};
		this.reports = [];
		this.isOK = true;
	};
	fn = Mock.prototype;
	fn.enable = function(){
		var a = arguments, i = a.length;
		while(i--) this[a[i]] = function(){};
		return this;
	};
	fn.test = function(method, option){
		var v = new Report(method), key, k;
		if(option){
			key = 'param,count,when,then'.split(',');
			k = key.length;
			while(k--) if(key[k] in option) v[key[k]] = option[key[k]];
		}
		this.reports.push(v);
		this[method] = function(){
			var k, state;
			if(v.then && v.isSameParam(arguments)){
				state = {};
				for(k in v.then) if(v.then.hasOwnProperty(k)) state[k] = v.then[k];
				this.state = state;
			}
			arguments.state = this.state;
			v.called.push(arguments);
		};
		return this;
	};
	fn.report = function(){
		var item, called, call, param, i, j, k, l, m;
		for(i = 0, j = this.reports.length; i < j; i++){
			if(!this.reports[i].report().isOK) this.isOK = false;
		}
		return this;
	};
	var Flow = function(){
		this._data = {};
		this._mock = [];
	};
	fn = Flow.prototype;
	fn.test = function(test){this._test = test;};
	fn.init = function(){
		this._isHold = false;
		this._fail = '';
		this._ok = '';
		this._mock.length = 0;
	};
	fn.hold = function(){this._isHold = true;};
	fn.isHold = function(){return this._isHold;};
	fn.ok = function(v){this._ok = v;};	
	fn.fail = function(v){this._fail = v;};
	fn.S = function(){
		var a = arguments, i = 0 , j = a.length, k, v;
		while(i < j){
			k = a[i++];
			if(i == j) return this._data[k];
			v = a[i++];
			if(v === null) delete this._data[k];
			else this._data[k] = v;
		}
		return v;
	};
	fn.run = function(key){
		if(key) tests[key].run(this);
		else this._test.next(this);
	};
	fn.isMock = function(){return this._mock.length > 0;};
	fn.mock = function(title){
		var mock = new Mock(title);
		this._mock.push(mock);
		return mock;
	};
	var f2s = (function(){
		var r0 = /</g, r1 = /\t/g;
		return function(func){
			var str, tab, i, j;
			str = func.toString();
			if(str.indexOf("\n") == -1) return str;
			str = str.split("\n"),
			tab = str[str.length - 1],
			tab = tab.substr(0, tab.length - 1);
			for( i = 0, j = str.length ; i < j ; i++ )
				if(str[i].substr(0, tab.length) == tab) str[i] = str[i].substr(tab.length);
			return str.join("\n").replace(r0, "&lt;").replace(r1, "  ");
		};
	})();
	var tests = {}, allTests = [];
	var Test = function(description, tear, list){
		allTests.push(this);
		this.description = description;
		this.tear = tear;
		this.list = list;
		this.cursor = -1;
		this.result = [];
		this.isOK = true;
	};
	fn = Test.prototype;
	fn.end = function(flow){
		var isOK, i, j;
		for(isOK = true, i = 0, j = allTests.length; i < j; i++){
			if(!allTests[i].isOK){
				isOK = false;
				break;
			}
		}
		test._runner(isOK, this, flow._data);
	};
	fn.mock = function(flow, result){
		var mock = result.mock = flow._mock.slice(0), msg = [], v;
		mock.isOK = true;
		for(i = 0, j = mock.length; i < j; i++) if(!mock[i].report().isOK) this.isOK = mock.isOK = false;
	};
	fn.next = function(flow){
		var i, f, v, r, result, cnt = 100;
		while(cnt--){
			i = ++this.cursor;
			f = this.list[i*2];
			v = this.list[i*2 + 1];
			flow.init();
			r = f(flow);
			this.result.push(result = {f:f2s(f), ok:flow._ok, fail:flow._fail});
			if(flow.isMock()) this.mock(flow, result);
			else{
				result.r = r;
				result.v = typeof v == 'function' ? v(flow, r) : v;
				if(result.r !== result.v) this.isOK = false;
			}
			if(this.cursor < this.list.length/2 - 1){
				if(flow.isHold()) return;
			}else return this.end(flow);
		}
	};
	fn.run = function(flow){
		if(!flow) flow = new Flow();
		flow.test(this);
		this.cursor = -1;
		this.result.length = 0;
		if(this.tear){
			flow.init();
			this.tear.call(null, flow);
			if(flow.isHold()) return;
		}
		this.next(flow);
	};
	var rf2t = /^(.+\/\*\n)([\s\S]+)(\n\*\/.+)$/g;
	var test = {
		runner:function(runner){
			test._runner = runner;
		},
		add:function(key, description, tear){
			var test;
			test = new Test(description, tear, Array.prototype.slice.call(arguments, 3));
			if(key) tests[key] = test;
			return test;
		},
		run:function(){
			var a = Array.prototype.slice.call(arguments, 0);
			a.unshift(null);
			test.add.apply(null, a).run();
		},
		doc:function(t, d){
			var doc = document.getElementById('bsTestDoc'), title, collapse, body, isOpen;
			if(!doc){
				doc = document.body.appendChild(document.createElement('div'));
				doc.style.cssText = 'margin:10px 0';
				doc.id = 'bsTestDoc';
				doc.innerHTML = '<style>#bsTestDoc '+ [
					'h2 h3{margin:0;padding:0}',
					'h2{border-bottom:1px solid #fcc;font-size:16px;padding:0 0 5px 3px;font-weight:normal}',
					'h3{border-bottom:1px solid #666;border-left:4px solid #a66;padding:0 0 2px 5px;font-size:12px;font-weight:bold}',
					'pre{background:#ededed;padding:5px}'
				].join('#bsTestDoc ') + '</style>'
			}
			doc = doc.appendChild(document.createElement('div'));
			doc.style.cssText = 'transition:all 1s;background:#e3e3fa;margin:5px 10px;border-radius:10px;border:1px solid #888;padding:10px';
			title = doc.appendChild(document.createElement('h2'));
			if(t.charAt(0) == '@') isOpen = true, t = t.substr(1);
			title.innerHTML = t;
			collapse = title.appendChild(document.createElement('span'));
			collapse.innerHTML = isOpen ? '▲' : '▼';
			collapse.style.cssText = 'color:#a66;font-size:11px';
			title.onclick = function(e){
				if(collapse.innerHTML == '▲'){
					collapse.innerHTML = '▼';
					body.style.display = 'none';
					doc.style.backgroundColor = '#fafae3';
				}else{
					collapse.innerHTML = '▲';
					body.style.display = 'block';
					doc.style.backgroundColor = '#fff';
				}
			};
			body = doc.appendChild(document.createElement('div'));
			if(typeof d == 'function'){
				d = d.toString().replace(rf2t, '$2');
			}
			body.style.display = isOpen ? 'block' : 'none';
			body.innerHTML = marked(d);
		}
	};
	if(W['module']) module.exports = test;
	else W.bsTest = test;
	test.runner(function(isOK, test, data){
		var root = document.getElementById('bsTestRunner'), div, item, mock, cnt, report, msgs, msg, i, j, k, l, m, n, html;
		if(!root){
			root = document.createElement('div');
			root.id = 'bsTestRunner';
			root.style.cssText = 'margin:5px;padding:5px;background:#ddd';
			root.innerHTML = '<div id="bsTestRunnerResult" style="padding:3px 20px;background:#fff;font-size:30px;font-weight:bold;line-height:35px;border-bottom:1px solid #aaa;border-right:1px solid #aaa"></div>';
			document.body.appendChild(root).onclick = function(e){
				var target = e.target;
				do{
					if(target.tagName == 'LI'){
						target = target.getElementsByTagName('pre')[0].style;
						target.display = target.display == 'none' ? 'block' : 'none';
						return;
					}
				}while(target = target.parentNode);
			};
		}
		document.getElementById('bsTestRunnerResult').innerHTML = '<span style="color:' + (isOK ? 'green' : 'red') + '">' + (isOK ? 'OK' : 'FAIL') + '</span>';
		for(html = '', i = 0, j = test.result.length; i < j; i++){
			item = test.result[i];
			isOK = item.mock ? item.mock.isOK : (item.r === item.v);
			html += '<li style="margin:5px 0;padding:10px;border-bottom:1px dashed #aaa;list-style:none;clear:both">' +
				'<pre style="display:none;margin:0 0 10px 0;background:#eee">' + item.f + '</pre>' +
				'<div style="margin-right:10px;float:left;padding:3px;border:1px solid #ccc;line-height:17px;font-size:15px;font-weight:bold;color:' + (isOK ? 'green' : 'red') + '">' + (isOK ? 'OK' : 'FAIL') + '</div>' +
				'<div style="font-weight:bold;padding:3px;font-size:15px;line-height:17px">';
			if(item.mock){
				if(isOK) html += 'mock OK';
				else{
					for(msgs = [], k = cnt = 0, l = item.mock.length; k < l; k++){
						mock = item.mock[k];
						if(!mock.isOK){
							for(cnt++, msg = '', m = 0, n = mock.reports.length; m < n; m++){
								report = mock.reports[m];
								if(!report.isOK){
									msg += '<div style="font-weight:normal"><strong>' + report.method + '</strong> : <br>' + report.msg.join('<br>') + '</div>';
								}
							}
							msgs.push(msg);
						}
					}
					html += 'mock Fail : ' + cnt + '<br clear="both"><div style="margin-top:3px;background:#ededed">' + msgs.join('<hr>') + '</div>';
				}
			}else{
				html += '<span style="font-size:11px;font-weight:thin;color:#999">result:</span> ' + item.r + 
					(isOK ? ' === ' : ' !== ') +
					'<span style="font-size:11px;font-weight:thin;color:#999">expect:</span> ' + item.v;
			}
			if(isOK && item.ok) html += '<div style="margin-right:10px;float:left;background:#cece92">' + item.ok + '</div>';
			else if(!isOK && item.fail) html += '<div style="margin-right:10px;float:left;background:#ffaa92">' + item.fail + '</div>';
			html += '</div></li>';
		}
		div = root.appendChild(document.createElement('div'));
		div.style.cssText = "margin-top:10px;border-radius:10px;border-bottom:1px solid #aaa;border-right:1px solid #aaa;background:#fff;padding:5px";
		div.innerHTML = '<div style="margin:10px;min-height:30px;font-size:16px">' + 
			test.description + 
			'<div style="float:left;padding:5px;margin-right:10px;border:1px solid #666;line-height:30px;font-size:20px;font-weight:bold;color:' + (test.isOK ? 'green' : 'red') + '">' + (test.isOK ? 'OK' : 'FAIL') + '</div>' +
			'<ul style="cursor:pointer;clear:both;margin:15px;padding:0">' + html + '</ul>' +
		'</div>';
	});
})(this);
(function(){function e(e){this.tokens=[],this.tokens.links={},this.options=e||a.defaults,this.rules=p.normal,this.options.gfm&&(this.rules=this.options.tables?p.tables:p.gfm)}function t(e,t){if(this.options=t||a.defaults,this.links=e,this.rules=u.normal,this.renderer=this.options.renderer||new n,this.renderer.options=this.options,!this.links)throw new Error("Tokens array requires a `links` property.");this.options.gfm?this.rules=this.options.breaks?u.breaks:u.gfm:this.options.pedantic&&(this.rules=u.pedantic)}function n(e){this.options=e||{}}function r(e){this.tokens=[],this.token=null,this.options=e||a.defaults,this.options.renderer=this.options.renderer||new n,this.renderer=this.options.renderer,this.renderer.options=this.options}function s(e,t){return e.replace(t?/&/g:/&(?!#?\w+;)/g,"&amp;").replace(/</g,"&lt;").replace(/>/g,"&gt;").replace(/"/g,"&quot;").replace(/'/g,"&#39;")}function i(e){return e.replace(/&([#\w]+);/g,function(e,t){return t=t.toLowerCase(),"colon"===t?":":"#"===t.charAt(0)?String.fromCharCode("x"===t.charAt(1)?parseInt(t.substring(2),16):+t.substring(1)):""})}function l(e,t){return e=e.source,t=t||"",function n(r,s){return r?(s=s.source||s,s=s.replace(/(^|[^\[])\^/g,"$1"),e=e.replace(r,s),n):new RegExp(e,t)}}function o(){}function h(e){for(var t,n,r=1;r<arguments.length;r++){t=arguments[r];for(n in t)Object.prototype.hasOwnProperty.call(t,n)&&(e[n]=t[n])}return e}function a(t,n,i){if(i||"function"==typeof n){i||(i=n,n=null),n=h({},a.defaults,n||{});var l,o,p=n.highlight,u=0;try{l=e.lex(t,n)}catch(c){return i(c)}o=l.length;var g=function(e){if(e)return n.highlight=p,i(e);var t;try{t=r.parse(l,n)}catch(s){e=s}return n.highlight=p,e?i(e):i(null,t)};if(!p||p.length<3)return g();if(delete n.highlight,!o)return g();for(;u<l.length;u++)!function(e){return"code"!==e.type?--o||g():p(e.text,e.lang,function(t,n){return t?g(t):null==n||n===e.text?--o||g():(e.text=n,e.escaped=!0,void(--o||g()))})}(l[u])}else try{return n&&(n=h({},a.defaults,n)),r.parse(e.lex(t,n),n)}catch(c){if(c.message+="\nPlease report this to https://github.com/chjj/marked.",(n||a.defaults).silent)return"<p>An error occured:</p><pre>"+s(c.message+"",!0)+"</pre>";throw c}}var p={newline:/^\n+/,code:/^( {4}[^\n]+\n*)+/,fences:o,hr:/^( *[-*_]){3,} *(?:\n+|$)/,heading:/^ *(#{1,6}) *([^\n]+?) *#* *(?:\n+|$)/,nptable:o,lheading:/^([^\n]+)\n *(=|-){2,} *(?:\n+|$)/,blockquote:/^( *>[^\n]+(\n(?!def)[^\n]+)*\n*)+/,list:/^( *)(bull) [\s\S]+?(?:hr|def|\n{2,}(?! )(?!\1bull )\n*|\s*$)/,html:/^ *(?:comment *(?:\n|\s*$)|closed *(?:\n{2,}|\s*$)|closing *(?:\n{2,}|\s*$))/,def:/^ *\[([^\]]+)\]: *<?([^\s>]+)>?(?: +["(]([^\n]+)[")])? *(?:\n+|$)/,table:o,paragraph:/^((?:[^\n]+\n?(?!hr|heading|lheading|blockquote|tag|def))+)\n*/,text:/^[^\n]+/};p.bullet=/(?:[*+-]|\d+\.)/,p.item=/^( *)(bull) [^\n]*(?:\n(?!\1bull )[^\n]*)*/,p.item=l(p.item,"gm")(/bull/g,p.bullet)(),p.list=l(p.list)(/bull/g,p.bullet)("hr","\\n+(?=\\1?(?:[-*_] *){3,}(?:\\n+|$))")("def","\\n+(?="+p.def.source+")")(),p.blockquote=l(p.blockquote)("def",p.def)(),p._tag="(?!(?:a|em|strong|small|s|cite|q|dfn|abbr|data|time|code|var|samp|kbd|sub|sup|i|b|u|mark|ruby|rt|rp|bdi|bdo|span|br|wbr|ins|del|img)\\b)\\w+(?!:/|[^\\w\\s@]*@)\\b",p.html=l(p.html)("comment",/<!--[\s\S]*?-->/)("closed",/<(tag)[\s\S]+?<\/\1>/)("closing",/<tag(?:"[^"]*"|'[^']*'|[^'">])*?>/)(/tag/g,p._tag)(),p.paragraph=l(p.paragraph)("hr",p.hr)("heading",p.heading)("lheading",p.lheading)("blockquote",p.blockquote)("tag","<"+p._tag)("def",p.def)(),p.normal=h({},p),p.gfm=h({},p.normal,{fences:/^ *(`{3,}|~{3,}) *(\S+)? *\n([\s\S]+?)\s*\1 *(?:\n+|$)/,paragraph:/^/}),p.gfm.paragraph=l(p.paragraph)("(?!","(?!"+p.gfm.fences.source.replace("\\1","\\2")+"|"+p.list.source.replace("\\1","\\3")+"|")(),p.tables=h({},p.gfm,{nptable:/^ *(\S.*\|.*)\n *([-:]+ *\|[-| :]*)\n((?:.*\|.*(?:\n|$))*)\n*/,table:/^ *\|(.+)\n *\|( *[-:]+[-| :]*)\n((?: *\|.*(?:\n|$))*)\n*/}),e.rules=p,e.lex=function(t,n){var r=new e(n);return r.lex(t)},e.prototype.lex=function(e){return e=e.replace(/\r\n|\r/g,"\n").replace(/\t/g,"    ").replace(/\u00a0/g," ").replace(/\u2424/g,"\n"),this.token(e,!0)},e.prototype.token=function(e,t,n){for(var r,s,i,l,o,h,a,u,c,e=e.replace(/^ +$/gm,"");e;)if((i=this.rules.newline.exec(e))&&(e=e.substring(i[0].length),i[0].length>1&&this.tokens.push({type:"space"})),i=this.rules.code.exec(e))e=e.substring(i[0].length),i=i[0].replace(/^ {4}/gm,""),this.tokens.push({type:"code",text:this.options.pedantic?i:i.replace(/\n+$/,"")});else if(i=this.rules.fences.exec(e))e=e.substring(i[0].length),this.tokens.push({type:"code",lang:i[2],text:i[3]});else if(i=this.rules.heading.exec(e))e=e.substring(i[0].length),this.tokens.push({type:"heading",depth:i[1].length,text:i[2]});else if(t&&(i=this.rules.nptable.exec(e))){for(e=e.substring(i[0].length),h={type:"table",header:i[1].replace(/^ *| *\| *$/g,"").split(/ *\| */),align:i[2].replace(/^ *|\| *$/g,"").split(/ *\| */),cells:i[3].replace(/\n$/,"").split("\n")},u=0;u<h.align.length;u++)h.align[u]=/^ *-+: *$/.test(h.align[u])?"right":/^ *:-+: *$/.test(h.align[u])?"center":/^ *:-+ *$/.test(h.align[u])?"left":null;for(u=0;u<h.cells.length;u++)h.cells[u]=h.cells[u].split(/ *\| */);this.tokens.push(h)}else if(i=this.rules.lheading.exec(e))e=e.substring(i[0].length),this.tokens.push({type:"heading",depth:"="===i[2]?1:2,text:i[1]});else if(i=this.rules.hr.exec(e))e=e.substring(i[0].length),this.tokens.push({type:"hr"});else if(i=this.rules.blockquote.exec(e))e=e.substring(i[0].length),this.tokens.push({type:"blockquote_start"}),i=i[0].replace(/^ *> ?/gm,""),this.token(i,t,!0),this.tokens.push({type:"blockquote_end"});else if(i=this.rules.list.exec(e)){for(e=e.substring(i[0].length),l=i[2],this.tokens.push({type:"list_start",ordered:l.length>1}),i=i[0].match(this.rules.item),r=!1,c=i.length,u=0;c>u;u++)h=i[u],a=h.length,h=h.replace(/^ *([*+-]|\d+\.) +/,""),~h.indexOf("\n ")&&(a-=h.length,h=this.options.pedantic?h.replace(/^ {1,4}/gm,""):h.replace(new RegExp("^ {1,"+a+"}","gm"),"")),this.options.smartLists&&u!==c-1&&(o=p.bullet.exec(i[u+1])[0],l===o||l.length>1&&o.length>1||(e=i.slice(u+1).join("\n")+e,u=c-1)),s=r||/\n\n(?!\s*$)/.test(h),u!==c-1&&(r="\n"===h.charAt(h.length-1),s||(s=r)),this.tokens.push({type:s?"loose_item_start":"list_item_start"}),this.token(h,!1,n),this.tokens.push({type:"list_item_end"});this.tokens.push({type:"list_end"})}else if(i=this.rules.html.exec(e))e=e.substring(i[0].length),this.tokens.push({type:this.options.sanitize?"paragraph":"html",pre:"pre"===i[1]||"script"===i[1]||"style"===i[1],text:i[0]});else if(!n&&t&&(i=this.rules.def.exec(e)))e=e.substring(i[0].length),this.tokens.links[i[1].toLowerCase()]={href:i[2],title:i[3]};else if(t&&(i=this.rules.table.exec(e))){for(e=e.substring(i[0].length),h={type:"table",header:i[1].replace(/^ *| *\| *$/g,"").split(/ *\| */),align:i[2].replace(/^ *|\| *$/g,"").split(/ *\| */),cells:i[3].replace(/(?: *\| *)?\n$/,"").split("\n")},u=0;u<h.align.length;u++)h.align[u]=/^ *-+: *$/.test(h.align[u])?"right":/^ *:-+: *$/.test(h.align[u])?"center":/^ *:-+ *$/.test(h.align[u])?"left":null;for(u=0;u<h.cells.length;u++)h.cells[u]=h.cells[u].replace(/^ *\| *| *\| *$/g,"").split(/ *\| */);this.tokens.push(h)}else if(t&&(i=this.rules.paragraph.exec(e)))e=e.substring(i[0].length),this.tokens.push({type:"paragraph",text:"\n"===i[1].charAt(i[1].length-1)?i[1].slice(0,-1):i[1]});else if(i=this.rules.text.exec(e))e=e.substring(i[0].length),this.tokens.push({type:"text",text:i[0]});else if(e)throw new Error("Infinite loop on byte: "+e.charCodeAt(0));return this.tokens};var u={escape:/^\\([\\`*{}\[\]()#+\-.!_>])/,autolink:/^<([^ >]+(@|:\/)[^ >]+)>/,url:o,tag:/^<!--[\s\S]*?-->|^<\/?\w+(?:"[^"]*"|'[^']*'|[^'">])*?>/,link:/^!?\[(inside)\]\(href\)/,reflink:/^!?\[(inside)\]\s*\[([^\]]*)\]/,nolink:/^!?\[((?:\[[^\]]*\]|[^\[\]])*)\]/,strong:/^__([\s\S]+?)__(?!_)|^\*\*([\s\S]+?)\*\*(?!\*)/,em:/^\b_((?:__|[\s\S])+?)_\b|^\*((?:\*\*|[\s\S])+?)\*(?!\*)/,code:/^(`+)\s*([\s\S]*?[^`])\s*\1(?!`)/,br:/^ {2,}\n(?!\s*$)/,del:o,text:/^[\s\S]+?(?=[\\<!\[_*`]| {2,}\n|$)/};u._inside=/(?:\[[^\]]*\]|[^\[\]]|\](?=[^\[]*\]))*/,u._href=/\s*<?([\s\S]*?)>?(?:\s+['"]([\s\S]*?)['"])?\s*/,u.link=l(u.link)("inside",u._inside)("href",u._href)(),u.reflink=l(u.reflink)("inside",u._inside)(),u.normal=h({},u),u.pedantic=h({},u.normal,{strong:/^__(?=\S)([\s\S]*?\S)__(?!_)|^\*\*(?=\S)([\s\S]*?\S)\*\*(?!\*)/,em:/^_(?=\S)([\s\S]*?\S)_(?!_)|^\*(?=\S)([\s\S]*?\S)\*(?!\*)/}),u.gfm=h({},u.normal,{escape:l(u.escape)("])","~|])")(),url:/^(https?:\/\/[^\s<]+[^<.,:;"')\]\s])/,del:/^~~(?=\S)([\s\S]*?\S)~~/,text:l(u.text)("]|","~]|")("|","|https?://|")()}),u.breaks=h({},u.gfm,{br:l(u.br)("{2,}","*")(),text:l(u.gfm.text)("{2,}","*")()}),t.rules=u,t.output=function(e,n,r){var s=new t(n,r);return s.output(e)},t.prototype.output=function(e){for(var t,n,r,i,l="";e;)if(i=this.rules.escape.exec(e))e=e.substring(i[0].length),l+=i[1];else if(i=this.rules.autolink.exec(e))e=e.substring(i[0].length),"@"===i[2]?(n=this.mangle(":"===i[1].charAt(6)?i[1].substring(7):i[1]),r=this.mangle("mailto:")+n):(n=s(i[1]),r=n),l+=this.renderer.link(r,null,n);else if(this.inLink||!(i=this.rules.url.exec(e))){if(i=this.rules.tag.exec(e))!this.inLink&&/^<a /i.test(i[0])?this.inLink=!0:this.inLink&&/^<\/a>/i.test(i[0])&&(this.inLink=!1),e=e.substring(i[0].length),l+=this.options.sanitize?s(i[0]):i[0];else if(i=this.rules.link.exec(e))e=e.substring(i[0].length),this.inLink=!0,l+=this.outputLink(i,{href:i[2],title:i[3]}),this.inLink=!1;else if((i=this.rules.reflink.exec(e))||(i=this.rules.nolink.exec(e))){if(e=e.substring(i[0].length),t=(i[2]||i[1]).replace(/\s+/g," "),t=this.links[t.toLowerCase()],!t||!t.href){l+=i[0].charAt(0),e=i[0].substring(1)+e;continue}this.inLink=!0,l+=this.outputLink(i,t),this.inLink=!1}else if(i=this.rules.strong.exec(e))e=e.substring(i[0].length),l+=this.renderer.strong(this.output(i[2]||i[1]));else if(i=this.rules.em.exec(e))e=e.substring(i[0].length),l+=this.renderer.em(this.output(i[2]||i[1]));else if(i=this.rules.code.exec(e))e=e.substring(i[0].length),l+=this.renderer.codespan(s(i[2],!0));else if(i=this.rules.br.exec(e))e=e.substring(i[0].length),l+=this.renderer.br();else if(i=this.rules.del.exec(e))e=e.substring(i[0].length),l+=this.renderer.del(this.output(i[1]));else if(i=this.rules.text.exec(e))e=e.substring(i[0].length),l+=s(this.smartypants(i[0]));else if(e)throw new Error("Infinite loop on byte: "+e.charCodeAt(0))}else e=e.substring(i[0].length),n=s(i[1]),r=n,l+=this.renderer.link(r,null,n);return l},t.prototype.outputLink=function(e,t){var n=s(t.href),r=t.title?s(t.title):null;return"!"!==e[0].charAt(0)?this.renderer.link(n,r,this.output(e[1])):this.renderer.image(n,r,s(e[1]))},t.prototype.smartypants=function(e){return this.options.smartypants?e.replace(/--/g,"—").replace(/(^|[-\u2014/(\[{"\s])'/g,"$1‘").replace(/'/g,"’").replace(/(^|[-\u2014/(\[{\u2018\s])"/g,"$1“").replace(/"/g,"”").replace(/\.{3}/g,"…"):e},t.prototype.mangle=function(e){for(var t,n="",r=e.length,s=0;r>s;s++)t=e.charCodeAt(s),Math.random()>.5&&(t="x"+t.toString(16)),n+="&#"+t+";";return n},n.prototype.code=function(e,t,n){if(this.options.highlight){var r=this.options.highlight(e,t);null!=r&&r!==e&&(n=!0,e=r)}return t?'<pre><code class="'+this.options.langPrefix+s(t,!0)+'">'+(n?e:s(e,!0))+"\n</code></pre>\n":"<pre><code>"+(n?e:s(e,!0))+"\n</code></pre>"},n.prototype.blockquote=function(e){return"<blockquote>\n"+e+"</blockquote>\n"},n.prototype.html=function(e){return e},n.prototype.heading=function(e,t,n){return"<h"+t+' id="'+this.options.headerPrefix+n.toLowerCase().replace(/[^\w]+/g,"-")+'">'+e+"</h"+t+">\n"},n.prototype.hr=function(){return this.options.xhtml?"<hr/>\n":"<hr>\n"},n.prototype.list=function(e,t){var n=t?"ol":"ul";return"<"+n+">\n"+e+"</"+n+">\n"},n.prototype.listitem=function(e){return"<li>"+e+"</li>\n"},n.prototype.paragraph=function(e){return"<p>"+e+"</p>\n"},n.prototype.table=function(e,t){return"<table>\n<thead>\n"+e+"</thead>\n<tbody>\n"+t+"</tbody>\n</table>\n"},n.prototype.tablerow=function(e){return"<tr>\n"+e+"</tr>\n"},n.prototype.tablecell=function(e,t){var n=t.header?"th":"td",r=t.align?"<"+n+' style="text-align:'+t.align+'">':"<"+n+">";return r+e+"</"+n+">\n"},n.prototype.strong=function(e){return"<strong>"+e+"</strong>"},n.prototype.em=function(e){return"<em>"+e+"</em>"},n.prototype.codespan=function(e){return"<code>"+e+"</code>"},n.prototype.br=function(){return this.options.xhtml?"<br/>":"<br>"},n.prototype.del=function(e){return"<del>"+e+"</del>"},n.prototype.link=function(e,t,n){if(this.options.sanitize){try{var r=decodeURIComponent(i(e)).replace(/[^\w:]/g,"").toLowerCase()}catch(s){return""}if(0===r.indexOf("javascript:")||0===r.indexOf("vbscript:"))return""}var l='<a href="'+e+'"';return t&&(l+=' title="'+t+'"'),l+=">"+n+"</a>"},n.prototype.image=function(e,t,n){var r='<img src="'+e+'" alt="'+n+'"';return t&&(r+=' title="'+t+'"'),r+=this.options.xhtml?"/>":">"},r.parse=function(e,t,n){var s=new r(t,n);return s.parse(e)},r.prototype.parse=function(e){this.inline=new t(e.links,this.options,this.renderer),this.tokens=e.reverse();for(var n="";this.next();)n+=this.tok();return n},r.prototype.next=function(){return this.token=this.tokens.pop()},r.prototype.peek=function(){return this.tokens[this.tokens.length-1]||0},r.prototype.parseText=function(){for(var e=this.token.text;"text"===this.peek().type;)e+="\n"+this.next().text;return this.inline.output(e)},r.prototype.tok=function(){switch(this.token.type){case"space":return"";case"hr":return this.renderer.hr();case"heading":return this.renderer.heading(this.inline.output(this.token.text),this.token.depth,this.token.text);case"code":return this.renderer.code(this.token.text,this.token.lang,this.token.escaped);case"table":var e,t,n,r,s,i="",l="";for(n="",e=0;e<this.token.header.length;e++)r={header:!0,align:this.token.align[e]},n+=this.renderer.tablecell(this.inline.output(this.token.header[e]),{header:!0,align:this.token.align[e]});for(i+=this.renderer.tablerow(n),e=0;e<this.token.cells.length;e++){for(t=this.token.cells[e],n="",s=0;s<t.length;s++)n+=this.renderer.tablecell(this.inline.output(t[s]),{header:!1,align:this.token.align[s]});l+=this.renderer.tablerow(n)}return this.renderer.table(i,l);case"blockquote_start":for(var l="";"blockquote_end"!==this.next().type;)l+=this.tok();return this.renderer.blockquote(l);case"list_start":for(var l="",o=this.token.ordered;"list_end"!==this.next().type;)l+=this.tok();return this.renderer.list(l,o);case"list_item_start":for(var l="";"list_item_end"!==this.next().type;)l+="text"===this.token.type?this.parseText():this.tok();return this.renderer.listitem(l);case"loose_item_start":for(var l="";"list_item_end"!==this.next().type;)l+=this.tok();return this.renderer.listitem(l);case"html":var h=this.token.pre||this.options.pedantic?this.token.text:this.inline.output(this.token.text);return this.renderer.html(h);case"paragraph":return this.renderer.paragraph(this.inline.output(this.token.text));case"text":return this.renderer.paragraph(this.parseText())}},o.exec=o,a.options=a.setOptions=function(e){return h(a.defaults,e),a},a.defaults={gfm:!0,tables:!0,breaks:!1,pedantic:!1,sanitize:!1,smartLists:!1,silent:!1,highlight:null,langPrefix:"lang-",smartypants:!1,headerPrefix:"",renderer:new n,xhtml:!1},a.Parser=r,a.parser=r.parse,a.Renderer=n,a.Lexer=e,a.lexer=e.lex,a.InlineLexer=t,a.inlineLexer=t.output,a.parse=a,"undefined"!=typeof module&&"object"==typeof exports?module.exports=a:"function"==typeof define&&define.amd?define(function(){return a}):this.marked=a}).call(function(){return this||("undefined"!=typeof window?window:global)}());