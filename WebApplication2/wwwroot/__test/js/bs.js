(function (W) {
    "use strict"
    var NONE = -1, STR = 'string', NUM = 'number', OBJ = 'object', FUN = 'function',
        ATTR = 1, UTIL = 2, STYLE = 3, EVENT = 4, THIS = 5,
        FAIL = { toString: function () { return 'bs.FAIL'; } },
        doc = W.document, docel, body, head,
        docNew = function (v) {
            var el = doc.createElement(v);
            el.S = domS;
            return el;
        },
        docId = (function () {
            var c = {};
            return function (id) {
                var el;
                if (id.charAt(0) == '@') c[id = id.substr(1)] = null;
                if (el = c[id], !el) if (c[id] = el = doc.getElementById(id)) el.S = domS;
                return el;
            };
        })(),
        docTag = (function () {
            var c = {};
            return function (tag) {
                return c[tag] || (c[tag] = doc.getElementsByTagName(tag));
            };
        })(),
        encode = encodeURIComponent, decode = decodeURIComponent,
        debug, err = (function () {
            var isDebug;
            debug = function (v) { isDebug = v; };
            return function () {
                console.log(Array.prototype.join.call(arguments, ','));
                if (isDebug) throw new Error(arguments.join(','));
                return FAIL;
            };
        })(),
        DATA = {
            app: {}, sys: {
                dom: {
                    handler: {},
                    attr: {}
                },
                sysEv: {}
            }
        },
        app = DATA.app, sys = DATA.sys, attr = sys.dom.attr, handler = sys.dom.handler, eOn, eOff,
        wkey, bs, bsdoc, ns = 'data-', templateData, render, renderUpdate,
        json, log, detect,
        Style, Css, domS, domGroup;

    if (doc) body = docNew('body'), head = docTag('head')[0], docel = doc.documentElement;
    wkey = function (k, v) { Object.defineProperty(W, k, { value: v }); };
    bs = function (v) {
        var target, a, i, j, k, v, m, n, isCursor;
        if (v && typeof v == OBJ) for (i in v) app[i] = v[i];
        else {
            a = arguments, i = 0, j = a.length;
            while (i < j) {
                k = a[i++].trim();
                if (k.charAt(0) == '.') {
                    if (k = k.substr(1), !k) return templateData;
                    if (k.indexOf('.') == NONE) return templateData[k];
                    target = templateData, isCursor = 1;
                } else target = app;
                if (k.indexOf('.') != NONE) {
                    for (k = k.split('.'), m = 0, n = k.length - 1; m < n; m++) {
                        if (k[m] in target) {
                            v = target[k[m]];
                            if (!v || typeof v != OBJ) return err('bs:0');
                        } else target[k[m]] = v = {};
                        target = v;
                    }
                    k = k[n];
                }
                if (isCursor || i == j) return target[k];
                v = a[i++];
                if (v === null) delete target[k];
                else target[k] = v;
            }
            return v;
        }
    };
    bsdoc = (function () {
        var doc = {},
            generate = function (k) {
                var d = doc[k];
                return '<section><h2><a name="#doc-' + k + '">' + k + '</a></h2><p>' + d.des + '</p><h3>arguments</h3><ul>' +
                    d.arg.reduce(function (p, v) { return p + '<li>' + v + '</li>'; }, '') +
                    '</ul><h3>return</h3><p>' + d.ret + '</p><pre><code>' + d.sample + '</code></pre></section>';
            };
        return function (k, v) {
            var arr, i, j, list;
            switch (arguments.length) {
                case 2: return doc[k] = v;
                case 1: return generate(k);
                default:
                    arr = [];
                    for (i in doc) if (doc.hasOwnProperty(i)) arr.push(i);
                    arr.sort();
                    list = '', arr = arr.map(function (v) {
                        list += generate(v);
                        return '<li><a href="#doc-' + v + '">' + v + '</a></li>';
                    });

                    return [keys, list];
            }
        };
    })();
    (function () {
        var through = function (v) { return v; }, o, k, f;
        o = {
            value: (function () {
                var r = {};
                return function () {
                    var a = arguments, i = 0, j = a.length, k, key;
                    while (i < j) {
                        k = a[i++];
                        if (i && typeof k == 'object') bsdoc(key, k);
                        Object.defineProperty(this, key = k, (r.value = a[i++], r));
                    }
                    return this;
                };
            })()
        };
        try {
            Object.defineProperty(Object.prototype, 'bsImmutable', o);
        } catch (e) {
            Object.prototype.bsImmutable = function () {
                var a = arguments, i = 0, j = a.length, k, key;
                while (i < j) {
                    k = a[i++];
                    if (i && typeof k == 'object') bsdoc(key, k);
                    this[key = k] = a[i++];
                }
                return this;
            };
        }
        //Object
        for (k in o = {
            freeze: through,
            seal: through,
            defineProperty: function (o, k, d) {
                if ('value' in d) o[k] = d.value;
                else if ('get' in d) {
                    if ('set' in d) o[k] = function () {
                        if (arguments.length) d['set'].call(o, arguments[0]);
                        else return d['get'].call(o);
                    };
                    else o[k] = d['get'];
                }
                return o;
            },
            defineProperties: function (o, d) {
                var k;
                for (k in d) Object.defineProperty(o, k, d[k])
                return o;
            },
            assign: function (t) {
                var a = arguments, i, j, k;
                for (i = 1, j = a.length; i < j; i++) {
                    for (k in a[i]) if (a[i].hasOwnProperty(k)) t[k] = a[i][k];
                }
                return t;
            },
            create: (function () {
                var cls = function () { };
                return function (fn, prop) {
                    var r;
                    cls.prototype = fn;
                    r = new cls();
                    if (prop) Object.defineProperties(r, prop);
                    cls.prototype = null;
                    return r;
                };
            })()
        }) if (o.hasOwnProperty(k)) if (!Object[k]) Object.bsImmutable(k, o[k]);
        //Array
        for (k in o = {
            from: function (v) {
                var r = [], i;
                if (typeof v[Symb.iterator] == 'function') {
                    v = v[Symb.iterator]();
                }
                if (typeof v.next == 'function') {
                    do {
                        i = v.next();
                        if (i.done) break;
                        else r[r.length] = i.value;
                    } while (true);
                } else if (i = v.length) {
                    while (i--) r[i] = v[i];
                }
                return r;
            },
            isArray: function (v) {
                return v instanceof Array || v === Array.prototype ? true : false;
            }
        }) if (o.hasOwnProperty(k)) if (!Array[k]) Array.bsImmutable(k, o[k]);
        //Array.prototype
        for (k in o = {
            trim: function (isNew) {
                var arr = isNew ? [] : this, i = arr.length;
                while (i--) if (typeof arr[i] == 'string') arr[i] = arr[i].trim();
                return arr;
            },
            indexOf: function (v, I) {
                var i, j, k, l;
                if (j = this.length) for (I = I || 0, i = I, k = parseInt((j - i) * .5) + i + 1, j--; i < k; i++) if (this[l = i] === v || this[l = j - i + I] === v) return l;
                return -1;
            },
            forEach: function (f) {
                for (var i = 0, j = this.length; i < j; i++) f(this[i], i, this);
            },
            forInterval: function (f, t) {
                var i = 0, id, self = this;
                id = setInterval(function () {
                    if (f(self[i], i, self) || ++i == self.length) clearInterval(id);
                }, t || 1);
            },
            map: function (f) {
                for (var r = [], i = 0, j = this.length; i < j; i++) r[i] = f(this[i], i, this);
                return r;
            },
            filter: function (f) {
                for (var r = [], i = 0, j = this.length; i < j; i++) if (f(this[i], i, this)) r[r.length] = this[i];
                return r;
            },
            reduce: function () {
                for (var f = arguments[0], i = 0, j = this.length, r = arguments.length == 2 ? arguments[1] : this[i++]; i < j; i++) r = f(r, this[i], i, this);
                return r;
            },
            watch: function () {
                for (var f = arguments[0], i = 0, j = this.length, r = arguments.length == 2 ? arguments[1] : this[i++]; i < j; i++) {
                    r = f(r, this[i], i, this);
                    if (r == bs.FAIL) break;
                }
                return r;
            },
            reduceInterval: function () {
                var a = arguments, f = a[0], i = 0, r = a.length > 1 ? a[1] : this[i++], id, self = this;
                var s, stop = function () { s = true; };
                id = setInterval(function () {
                    s = false;
                    r = f(r, self[i], i, self);
                    if (s || ++i == self.length) {
                        clearInterval(id);
                        if (typeof a[2] == 'function') a[2]();
                    }
                }, t || 1);
            },
            reverse: function () {
                var i, j, k, l;
                for (i = 0, j = parseInt(this.length / 2); i < j; i++) k = this[i], this[i] = this[l = this.length - 1 - i], this[l] = k;
                return this;
            }
        }) if (o.hasOwnProperty(k)) if (!Array.prototype[k]) Array.prototype.bsImmutable(k, o[k]);
        //String.prototype
        for (k in o = {
            isNumber: function () { return parseFloat(this) + '' == this; },
            ex: (function () {
                var arg = [bs], param = ['bs'], p,
                    r0 = /\.\{([^}]+)\}/g, f0 = function (_, v) {
                        var i, j, k;
                        if (v.indexOf('.') == NONE) v = templateData[v];
                        else {
                            for (v = v.split('.'), i = 0, j = v.length, k = templateData; i < j; i++) {
                                if (!k) return '';
                                k = k[v[i]];
                            }
                            v = k;
                        }
                        return typeof v == 'function' ? v() : v;
                    },
                    r1 = /\@\{([^}]+)\}/g, f1 = function (_, v) {
                        v = bs(v);
                        return typeof v == 'function' ? v() : v;
                    },
                    r2 = /\$\{([^}]+)\}/g, f2 = function (_, v) {
                        return (new Function(p, 'return (' + v + ');')).apply(null, arg);
                    };
                return function (v) {
                    var s = this, a, i, j, k, c = 10;
                    do {
                        k = 0;
                        if (s.indexOf('.{') != NONE) s = s.replace(r0, f0), k = 1;
                        if (s.indexOf('@{') != NONE) s = s.replace(r1, f1), k = 1;
                        if (s.indexOf('${') != NONE) {
                            param.length = arg.length = 1, a = arguments, i = 0, j = a.length;
                            while (i < j) param[param.length] = a[i++], arg[arg.length] = a[i++];
                            p = param.join(','), s = s.replace(r2, f2), k = 1;
                        }
                    } while (k && c--);
                    return s;
                };
            })(),
            trim: (function () {
                var trim = /^[\s\uFEFF\xA0]+|[\s\uFEFF\xA0]+$/g;
                return function () { return this.replace(trim, ''); };
            })(),
            cut: function (l, r) {
                var v = this;
                if (l) v = v.substr(l);
                if (r) v = v.substr(0, v.length - r);
                return v;
            },
            right: function (v) { return this.substr(this.length - v); },
            startsWith: function (v) { return this.substr(0, v.length) == v; },
            endsWith: function (v) { return this.substr(this.length - v.length) == v; },
            repeat: function (v) {
                var r = '', key = this;
                while (v--) r += key;
                return r;
            }
        }) if (o.hasOwnProperty(k)) if (!String.prototype[k]) String.prototype.bsImmutable(k, o[k]);
        //Date.prototype
        if (!Date.now) Date.bsImmutable('now', function () { return +new Date; });
        for (k in o = {
            toISOString: f = function () {
                var v;
                return this.getUTCFullYear() +
                    '-' + (v = '0' + (this.getUTCMonth() + 1)).substr(v.length - 2) +
                    '-' + (v = '0' + this.getUTCDate()).substr(v.length - 2) +
                    'T' + (v = '0' + this.getUTCHours()).substr(v.length - 2) +
                    ':' + (v = '0' + this.getUTCMinutes()).substr(v.length - 2) +
                    ':' + (v = '0' + this.getUTCSeconds()).substr(v.length - 2) +
                    '.' + (this.getUTCMilliseconds() / 1000).toFixed(3).slice(2, 5) +
                    'Z';
            },
            toJSON: f
        }) if (o.hasOwnProperty(k)) if (!Date.prototype[k]) Date.prototype.bsImmutable(k, o[k]);
        if (!W['requestAnimationFrame']) (function () {
            var offset = Date.now();
            v = 'equestAnimationFrame';
            W['r' + v] = W['webkitR' + v] || W['mozR' + v] || W['msR' + v] || (Date.now ? function (v) {
                return setTimeout(v.__raf__ || (v.__raf__ = function () { v(Date.now() - offset); }), 16.7);
            } : function (v) {
                return setTimeout(v.__raf__ || (v.__raf__ = function () { v(+new Date - offset); }), 16.7);
            });
            v = 'ancelAnimationFrame';
            wkey('c' + v, W['webkitC' + v] || W['mozC' + v] || W['msC' + v] || clearTimeout);
        })();
        if (!W['performance']) wkey('performance', {});
        if (!W.performance.now) performance.bsImmutable('now', function () { return Date.now() - offset; });
    })();


    (function () {
        if (W.bsES6) return;
        Object.defineProperty(W, 'bsES6', { value: true });
        var rF2T = /^.+\/\*\n([\s\S]+)\n\*\/.+$/g;
        (function () { //Symbol
            var uuid = 0, keys = {}, syms = {},
                S = function (key) { return new Sym(key); },
                Sym = function (key) {
                    this.id = (key || '') + '@@Symb:' + (+new Date) + ':' + (uuid++) + ':' + bs.randf(10000, 99999);
                    return Object.freeze(this);
                };
            Sym.prototype.bsImmutable('toString', function () { return this.id; });
            Object.freeze(Sym.prototype);
            S.bsImmutable(
                'iterator', '@@iterator',
                'for', function (k) { return keys[k] || (syms[keys[k] = S(k)] = k, keys[k]); },
                'keyFor', function (s) { return syms[s]; }
            );
            wkey('Symb', Object.freeze(S));
        })();
        (function () { //template
            var rEX = /\$\{([^}]+)\}/g, rSpt = /\$\{[^}]+\}/g,
                defaultTag = function (strings) {
                    var result = '', values = arguments, i, j, k;
                    for (i = 0, j = strings.length, k = values.length; i < j; i++) {
                        result += strings[i];
                        if (i + 1 < k) result += values[i + 1];
                    }
                    return result;
                };
            wkey('Tmpl', function (tag, str, vo) {
                var values, k, key, val;
                if (!tag) tag = defaultTag;
                if (!vo) vo = {};
                if (typeof str == 'function') str = str.toString().replace(rF2T, '$1');
                values = [str.split(rSpt)];
                if (str.indexOf('${') > -1) {
                    key = [], val = [];
                    for (k in vo) key.push(k), val.push(vo[k]);
                    key = key.join(',');
                    str.replace(rEX, function (v, ex) {
                        values.push((new Function(key, 'return ' + ex)).apply(null, val) + '');
                    });
                }
                return tag.apply(null, values);
            });
        })();
        (function () { //Iterator
            var pool = [], done = Object.freeze({ value: undefined, done: true }),
                next = {
                    string: function () {
                        if (this.c < this.v.length) return this.value = this.v.charAt(this.c++), this;
                        return pool.push(this), done;
                    },
                    object: function () {
                        if (this.c < this.v.length) return this.value = this.v[this.c++], this;
                        return pool.push(this), done;
                    }
                };
            Array.prototype[Symb.iterator] = String.prototype[Symb.iterator] = function () {
                var iter = pool.length ? pool.pop() : {}, t = typeof this;
                iter.c = 0, iter.v = this, iter.next = next[t],
                    iter.done = !this.length,
                    iter.value = t == 'string' ? this.charAt(0) : this[0];
                return iter;
            };
        })();
        (function () {//destructuring
            var pool = {}, Dest = (function () {
                var DEST = '[^dest^]', DEFAULT = '[^default^]',
                    getVal, getData, parse, destructuring, Var, Dest,
                    arr = [], obj = [], a, o, ad, od, at, ot,
                    rObj = /\{[^\{\[\]\}]*\}/g, rArr = /\[[^\{\[\]\}]*\]/g,
                    oR = function (v) { return ot[o] = v, '@o_' + od + '_' + (o++) + '@'; },
                    aR = function (v) { return at[a] = v, '@a_' + ad + '_' + (a++) + '@'; },
                    rO = /@o_[^@]+@/g, rA = /@a_[^@]+@/g, rR = /^@o_[^@]+@|@a_[^@]+@$/,
                    rNum = /^[-]?[.0-9]+$/, rStr = /^('[^']*'|"[^"]*")$/,
                    primi = { 'true': true, 'false': false, 'null': null };
                getData = function (d) {
                    var target = d.search(rO) > -1 ? obj : d.search(rA) > -1 ? arr : 0;
                    if (target) return d = d.substring(1, d.length - 1).split('_'), target[d[1]][d[2]];
                    return false;
                };
                getVal = function (d) {
                    var target = d.search(rO) > -1 ? obj : d.search(rA) > -1 ? arr : 0;
                    if (target) return d = d.substring(1, d.length - 1).split('_'), JSON.parse(target[d[1]][d[2]]);
                    else if (d.search(rStr) > -1) return d.substring(1, d.length - 1);
                    else if (d.search(rNum) > -1) return parseFloat(d);
                    else if (d = primi[d]) return d;
                };
                Dest = function (dest) {
                    var loop, r = this[DEST] = {};
                    arr.length = obj.length = a = o = ad = od = 0, dest = dest.trim();
                    do {
                        loop = 0;
                        if (dest.search(rObj) > -1) obj[od] = ot = [], dest = dest.replace(rObj, oR), od++ , loop = 1;
                        if (dest.search(rArr) > -1) arr[ad] = at = [], dest = dest.replace(rArr, aR), ad++ , loop = 1;
                    } while (loop);
                    if (dest.indexOf('=') > -1) dest = dest.split('='), r[DEFAULT] = getVal(dest[1].trim()), dest = dest[0].trim();
                    if (dest.search(rR) == -1) throw 'invalid destructuring';
                    parse(dest, r);
                };
                Dest.prototype.bsImmutable('value', function (v) {
                    var result = {};
                    destructuring(this[DEST], v === undefined ? this[DEST][DEFAULT] : v, result);
                    return result;
                });
                Var = function (k) {
                    var i = k.indexOf('=');
                    if (i > -1) this[DEFAULT] = getVal(k.substr(i + 1).trim()), k = k.substring(0, i).trim();
                    this.k = k;
                };
                Var.prototype.toString = function () { return this.k; };
                parse = function (dest, r) {
                    var v, isObj;
                    dest = dest.trim();
                    if (v = getData(dest)) {
                        isObj = v.charAt(0) == '{' ? 1 : 0;
                        v.substring(1, v.length - 1).split(',').forEach(function (v, idx) {
                            var p;
                            v = v.trim();
                            if (isObj) {
                                p = v.indexOf(':');
                                v = p > -1 ? [v.substring(0, p), v.substr(p + 1)] : [v, v];
                                if (p = parse(v[1], {})) r[v[0].trim()] = p;
                            } else if (p = parse(v, {})) r[idx] = p;
                        });
                        return r;
                    } else return dest ? new Var(dest) : undefined;
                };
                destructuring = function (target, v, result) {
                    var k, key, iter, iterR, i, j;
                    if (iter = v) {
                        while (typeof iter[Symb.iterator] == 'function') iter = iter[Symb.iterator]();
                        if (typeof iter.next == 'function') iterR = [];
                    }
                    for (k in target) {
                        if (target.hasOwnProperty(k)) {
                            key = target[k];
                            if (key instanceof Var) {
                                if (key.k.substr(0, 3) == '...') {
                                    i = key.k.substr(3);
                                    if (iterR) {
                                        For.Of(iter, function (v) { iterR.push(v); });
                                        result[i] = iterR.slice(k);
                                    } else if (v instanceof Array) result[i] = v.slice(k);
                                    else throw 'invalid Array';
                                } else {
                                    if (parseInt(k, 10) + '' == k) {
                                        if (!(v instanceof Array) && iterR) {
                                            k = parseInt(k, 10);
                                            while (iterR.length - 1 < k) {
                                                j = iter.next();
                                                if (j.done) break;
                                                iterR.push(j.value);
                                            }
                                            result[key] = iterR[k] === undefined ? key[DEFAULT] : iterR[k];
                                            continue;
                                        }
                                    }
                                    result[key] = v[k] === undefined ? key[DEFAULT] : v[k];
                                }
                            } else if (key && typeof key == 'object') destructuring(key, v[k], result);
                        }
                    }
                };
                return Dest;
            })();
            wkey('Dest', function (dest, v) {
                if (typeof dest == 'function') dest = dest.toString().replace(rF2T, '$1');
                return (pool[dest] || (pool[dest] = new Dest(dest))).value(v);
            });
        })();
        (function () {//for of
            var f = function (iter, f) {
                var cnt = 100000, v;
                while (iter[Symb.iterator]) iter = iter[Symb.iterator]();
                while (cnt--) {
                    v = iter.next();
                    if (v.done) break;
                    f(dest ? Dest(dest, v.value) : v.value);
                }
                dest = prev;
            }, dest, prev, obj = Object.freeze({ Of: f }),
                For = function (d) { return prev = dest, dest = d, obj; };
            For.bsImmutable('Of', f);
            wkey('For', For);
        })();
        wkey('Gene', (function () {
            var SELF, pool = [], Generator = function () { }, fn = Generator.prototype;
            fn.init = (function () {
                var ONCE = '[^once^]', ID = '[^ID^]',
                    once = function () {
                        var id, a, i, j;
                        if (this[ONCE][id = this[ID]++]) return;
                        this[ONCE][id] = 1;
                        a = arguments, i = 0, j = a.length;
                        while (i < j) this[a[i++]] = a[i++];
                    };
                return function (f, ec, context, su, suCall) {
                    this.f = f;
                    if (!ec) ec = {};
                    ec[ONCE] = {}, ec[ID] = 1, ec.once = once, this.ec = ec;
                    this.ids = {};
                    if (typeof context == 'function') delete this._context, this.context = context;
                    else this._context = context, delete this.context;
                    if (suCall) delete this._super, this.Super = su;
                    else this._super = su, delete this.Super;
                    this.value = undefined, this.done = false;
                };
            })();
            fn.Super = function () { return this._super; };
            fn.context = function () { return this._context; };
            fn.next = (function () {
                var done = Object.freeze({ done: true });
                return function () {
                    var r, prevS, prevSelf;
                    if (this.done) return this;
                    if (this.stack) {
                        r = this.stack.next();
                        if (r.done) this.stack = null;
                        else return r;
                    }
                    this.isYieldActive = false, this.seed = 1, this.y$ = this.y = FAIL;
                    prevSelf = SELF, SELF = this, prevS = W.Super, W.Super = this.Super();
                    this.f.call(this.context(), this.ec);
                    SELF = prevSelf, W.Super = prevS;
                    if (this.y !== FAIL) return this.value = this.y, this;
                    if (this.y$ !== FAIL) {
                        r = this.y$.next();
                        if (!r.done) return this.stack = this.y$, r;
                    }
                    return pool[pool.length] = this, done;
                };
            })();
            wkey('Unused', function () {
                var id = 'U' + (SELF.seed++);
                if (SELF.ids[id]) return false;
                return SELF.ids[id] = true;
            });
            (function () {
                var order = function (cnt) {
                    var id;
                    if (SELF.isYieldActive) return true;
                    id = 'Y' + (SELF.seed++);
                    if (SELF.ids[id] === undefined) SELF.ids[id] = cnt ? cnt < 0 ? 100000 : cnt : 1;
                    if (!SELF.ids[id]) return true;
                    SELF.ids[id]--;
                    SELF.isYieldActive = true;
                };
                wkey('Yield', function (v, cnt) {
                    if (order(cnt)) return true;
                    return SELF.y = v, false;
                });
                wkey('Yield$', function (v, cnt) {
                    if (order(cnt)) return true;
                    while (typeof v[Symb.iterator] == 'function') v = v[Symb.iterator]();
                    return SELF.y$ = v, false;
                });
            })();
            return function (f, context, su, suCall) {
                return function (ec) {
                    var g = pool.length ? pool.pop() : new Generator();
                    g.init(f, ec, context || null, su || null, suCall);
                    return g;
                };
            };
        })());
        wkey('Class', (function () {
            var instances = {}, ID = '[^id^]',
                getId = function () { return this[ID]; },
                removeId = function () { return delete instances[this[ID]]; };
            bs.bsImmutable(
                'fromId', function (id) { return instances[id]; },
                'removeId', function (id) { delete instances[id]; }
            );

            var keyword = { constructor: 1, register: 1 }, protoInit = {};
            var SC = (function () {
                var ext, self, sc = function () { ext.apply(self, arguments); }, root = function () { };
                return function (s, e) { W.Super = e ? (self = s, ext = e, sc) : root; };
            })();
            var mkSM = function (sm, f) { return function () { return f.apply(sm['^self^'], arguments); }; };
            var mkS = function (c, ext, f) {
                var m = function () {
                    var r, prev;
                    if (this instanceof m) throw 'only method';
                    prev = W.Super;
                    W.Super = ext, r = f.apply(c, arguments), W.Super = prev;
                    return r;
                };
                return m;
            };
            var mkSG = function (c, ext, f) {
                f = Gene(f, null, ext);
                return function (ec) { return f(ec); };
            };
            var mkM = function (f, sm) {
                var m = function () {
                    var r, prev;
                    if (this instanceof m) throw 'only method';
                    prev = W.Super;
                    return sm['^self^'] = this, W.Super = sm, r = f.apply(this, arguments), W.Super = prev, r;
                };
                return m;
            };
            var mkMG = function (f, sm) {
                var self, m;
                f = Gene(f, function () { return self; }, function () { return sm['^self^'] = self, sm; }, true);
                return function (ec) { return self = this, f(ec); };
            };
            return function () {
                var a = arguments, ext = typeof a[0] == 'function' ? a[0] : null, cls = a[ext ? 1 : 0];
                var register = cls && cls.register, cstrt = cls && cls['constructor'], cname = cstrt ? cstrt.name || '' : '';
                var c, sm, fn, k, proto;

                c = function (v) {
                    var prev;
                    if (v === protoInit) return;
                    if (!(this instanceof c)) throw 'only new';
                    if (register) instances[this[ID] = cname + ':' + Symb()] = this;
                    if (cstrt) prev = W.Super, SC(this, ext), cstrt.apply(this, arguments), W.Super = prev;
                };
                sm = { '^self^': null }; //super method
                if (ext) {
                    proto = ext.prototype;
                    for (k in proto) if (proto.hasOwnProperty(k)) sm[k] = mkSM(sm, proto[k]);
                    Object.seal(sm);
                    c.prototype = new ext(protoInit);
                }
                fn = c.prototype;
                if (register) fn.getId = getId, fn.removeId = removeId;
                if (cls) for (k in cls) if (!keyword[k] && cls.hasOwnProperty(k)) {
                    if (k.substr(0, 7) == 'static ') {
                        if (k.charAt(7) != '*') c[k.substr(7)] = mkS(c, ext, cls[k]);
                        else c[k.substr(8)] = mkSG(c, ext, cls[k]);
                    } else {
                        if (k.charAt(0) != '*') fn[k] = mkM(cls[k], sm);
                        else fn[k.substr(1)] = mkMG(cls[k], sm);
                    }
                }
                return c;
            };
        })());
    })();
    //DOM
    if (doc) (function () {
        var t = 'abbr,article,aside,audio,bdi,canvas,data,datalist,details,figcaption,figure,footer,header,hgroup,mark,meter,nav,output,progress,section,summary,time,video'.split(','),
            f = doc.createDocumentFragment(), i = t.length, j = 'createElement' in f, el = doc.createElement('DIV');
        while (i--) {
            docNew(t[i]);
            if (j) f.createElement(t[i]);
        }
        if (!('firstElementChild' in el) || !('nextElementSibling' in el)) (function () {
            var k, o,
                mk = function (k0, k1) {
                    return function () {
                        var el = this[k0];
                        if (!el) return null;
                        do if (el.nodeType == 1) return el; while (el = el[k1])
                        return null;
                    };
                };
            for (k in o = {
                firstElementChild: mk('firstChild', 'nextSibling'),
                lastElementChild: mk('lastChild', 'previousSibling'),
                previousElementSibling: mk('previousSibling', 'previousSibling'),
                nextElementSibling: mk('nextSibling', 'nextSibling'),
                children: function () {
                    var v = { length: 0 }, el = this.firtsNode;
                    do if (el.nodeType == 1) v[v.length++] = el; while (el = el.nextSibling)
                    return v;
                }
            }) if (o.hasOwnProperty(k)) if (!docel[k]) Object.defineProperty(Element.prototype, k, { get: o[k] });
        })();
    })();
    bs.bsImmutable(
        'file', W['File'] && W['Blob'] ? function (filename, content, type) {
            var blob;
            if (!filename) return err(5008);
            try {
                return new File([new Blob([content], { type: type })], filename);
            } catch (e) {
                err(5008);
            }
        } : function () { err(5008); }
    );
    bs.bsImmutable('DETECT', detect = (function () {
        var detect,
            navi = W['navigator'], agent = navi.userAgent.toLowerCase(), platform = navi.platform.toLowerCase(), app = navi.appVersion.toLowerCase(),
            device = 'pc', browser, bv, os, osv, cssPrefix, stylePrefix, docMode, ie7mode,
            i, t0,
            edge = function () {
                var a = agent.toLowerCase();
                if (a.indexOf('edge/') == NONE) return;
                return browser = 'edge', bv = parseInt(/edge[\/]([0-9.]+)/.exec(a)[1]);
            },
            ie = function () {
                if (agent.indexOf('msie') < 0 && agent.indexOf('trident') < 0) return;
                if (agent.indexOf('iemobile') > -1) os = 'winMobile';
                return browser = 'ie', bv = agent.indexOf('msie 7') > -1 && agent.indexOf('trident') > -1 ? (ie7mode = 1, -1) : agent.indexOf('msie') < 0 ? 11 : parseFloat(/msie ([\d]+)/.exec(agent)[1]);
            },
            chrome = function () {
                if (agent.indexOf(i = 'chrome') < 0 && agent.indexOf(i = 'crios') < 0) return;
                return browser = 'chrome', bv = parseFloat((i == 'chrome' ? /chrome\/([\d]+)/ : /crios\/([\d]+)/).exec(agent)[1]);
            },
            firefox = function () { return agent.indexOf('firefox') < 0 ? 0 : (browser = 'firefox', bv = parseFloat(/firefox\/([\d]+)/.exec(agent)[1])); },
            safari = function () { return agent.indexOf('safari') < 0 ? 0 : (browser = 'safari', bv = parseFloat(/safari\/([\d]+)/.exec(agent)[1])); },
            naver = function () { return agent.indexOf('naver') < 0 ? 0 : browser = 'naver'; },
            zombie = function () { return agent.indexOf('zombie') < 0 ? 0 : browser = 'zombie'; };
        if (agent.indexOf('android') > -1) {
            browser = os = 'android', device = agent.indexOf('mobile') == -1 ? (browser += 'Tablet', 'tablet') : 'mobile',
                osv = (i = /android ([\d.]+)/.exec(agent)) ? (i = i[1].split('.'), parseFloat(i[0] + '.' + i[1])) : 0,
                naver() || chrome() || firefox() || (bv = i = /safari\/([\d.]+)/.exec(agent) ? parseFloat(i[1]) : 0);
        } else if (agent.indexOf(i = 'ipad') > -1 || agent.indexOf(i = 'iphone') > -1) {
            device = i == 'ipad' ? 'tablet' : 'mobile', browser = os = i, osv = (i = /os ([\d_]+)/.exec(agent)) ? (i = i[1].split('_'), parseFloat(i[0] + '.' + i[1])) : 0,
                naver() || chrome() || firefox() || (bv = (i = /mobile\/([\S]+)/.exec(agent)) ? parseFloat(i[1]) : 0);
        } else if (platform.indexOf('win') > -1) {
            for (i in t0 = { '5.1': 'xp', '6.0': 'vista', '6.1': '7', '6.2': '8', '6.3': '8.1', '10.': '10' }) {
                if (agent.indexOf('windows nt ' + i) > -1) {
                    osv = t0[i];
                    break;
                }
            }
            os = 'win', zombie() || edge() || ie() || chrome() || firefox();
        } else if (platform.indexOf('mac') > -1) os = 'mac', i = /os x ([\d._]+)/.exec(agent)[1].replace('_', '.').split('.'), osv = parseFloat(i[0] + '.' + i[1]), chrome() || firefox() || safari();
        else os = app.indexOf('x11') > -1 ? 'unix' : app.indexOf('linux') > -1 ? 'linux' : 0, chrome() || firefox();
        docMode = doc['documentMode'] || 0;
        switch (browser) {
            case 'ie':
                cssPrefix = '-ms-', stylePrefix = 'ms';
                if (bv == 6) doc.execCommand('BackgroundImageCache', false, true), p[p.length] = function () { doc.body.style.position = 'relative'; };
                else if (bv == -1) bv = !('getContext' in (t0 = doc.createElement('canvas'))) ? 8 :
                    !('msTransition' in body.style) && !('transition' in body.style) ? 9 :
                        t0.getContext('webgl') ? 11 : 10;
                break;
            case 'firefox': cssPrefix = '-moz-', stylePrefix = 'Moz'; break;
            case 'edge': cssPrefix = '-ms-', stylePrefix = 'ms'; break;
            default: cssPrefix = '-webkit-', stylePrefix = 'webkit';
        }
        return {
            device: device, browser: browser, browserVer: bv, ie7mode: ie7mode, os: os, osVer: osv, sony: agent.indexOf('sony') > -1 ? 1 : 0,
            docMode: docMode, cssPrefix: cssPrefix, stylePrefix: stylePrefix, transform3d: browser != 'ie' || bv > 9,
            language: navigator.language || navigator.userLanguage
        };
    })());
    (function () {
        var zone = (new Date()).getTimezoneOffset() * 60000,
            _get = function (date, isUTC) {
                var i, t0, h, m, s;
                if (typeof date == 'string') {
                    if (date.indexOf('Z') != NONE && date.indexOf('T') != NONE) {
                        date = date.replace('Z', '').replace('T', ' ');
                        isUTC = true;
                    }
                    i = date.split('-');
                    if (i[2] && i[2].indexOf(' ') > -1) {
                        t0 = i[2].split(' '), i[2] = t0[0], t0 = t0[1].split(':'),
                            t0[2] = t0[2].split('.');
                        h = parseInt(t0[0], 10), m = parseInt(t0[1], 10), s = parseInt(t0[2][0], 10);
                    } else h = m = s = 0;
                    i = new Date(parseInt(i[0], 10), parseInt(i[1], 10) - 1, parseInt(i[2], 10), h, m, s);
                    if (t0 && t0[2][1]) i.setTime(i.getTime() + parseInt(t0[2][1], 10));
                } else i = date instanceof Date ? date : new Date;
                if (isUTC) i.setTime(i.getTime() + zone);
                return i;
            },
            _leapYear = function (v) { return (v % 4 == 0 && v % 100 != 0) || v % 400 == 0; },
            _date = function (part, date) {
                var i;
                switch (part) {
                    case 'Y': return date.getFullYear() + '';
                    case 'y': return i = _date('Y', date), i.substr(i.length - 2);
                    case 'm': return i = '00' + _date('n', date), i.substr(i.length - 2);
                    case 'n': return (date.getMonth() + 1) + '';
                    case 'd': return i = '00' + _date('j', date), i.substr(i.length - 2);
                    case 'j': return date.getDate() + '';
                    case 'H': return i = '00' + _date('G', date), i.substr(i.length - 2);
                    case 'h': return i = '00' + _date('g', date), i.substr(i.length - 2);
                    case 'G': return date.getHours() + '';
                    case 'g': return parseInt(date.getHours()) % 12 + '' || '0';
                    case 'i': return i = '00' + date.getMinutes(), i.substr(i.length - 2);
                    case 's': return i = '00' + date.getSeconds(), i.substr(i.length - 2);
                    case 'u': return i = '000' + date.getMilliseconds(), i.substr(i.length - 3);
                    case 'w': return prettyData[_ln].day[date.getDay()];
                    default: return part;
                }
            },
            addKey = { y: 'FullYear', m: 'Month', d: 'Date', h: 'Hours', i: 'Minutes', s: 'Seconds', ms: 'Milliseconds' },
            _diff,
            prettyKey = 'second,minute,hour,day,month,year'.split(','), prettyData, _ln = detect.language;
        sys.date = prettyData = {};
        bs.bsImmutable(
            'date', _get,
            'dateAdd', function (k, v, d, isUTC) {
                return (k = addKey[k]) ? (d = _get(d, isUTC), d['set' + k](d['get' + k]() + v), d) : err('DATE.add');
            },
            'dateDiff', _diff = function (interval, dateOld, isUTCOld, dateNew, isUTCNew) {
                var date1, date2, d1_year, d1_month, d1_date, d2_year, d2_month, d2_date, i, j, k, d, year, month, order, temp;
                date1 = _get(dateOld, isUTCOld);
                date2 = _get(dateNew, isUTCNew);
                switch (interval.toLowerCase()) {
                    case 'y': return date2.getFullYear() - date1.getFullYear();
                    case 'm': return (date2.getFullYear() - date1.getFullYear()) * 12 + date2.getMonth() - date1.getMonth();
                    case 'd':
                        if (date2 > date1) order = 1;
                        else order = -1, i = date1, date1 = date2, date2 = i;
                        d1_year = date1.getFullYear(), d1_month = date1.getMonth(), d1_date = date1.getDate(),
                            d2_year = date2.getFullYear(), d2_month = date2.getMonth(), d2_date = date2.getDate(),
                            j = d2_year - d1_year, d = 0;
                        if (j > 0) {
                            d += _diff('d', new Date(d1_year, d1_month, d1_date), new Date(d1_year, 11, 31));
                            d += _diff('d', new Date(d2_year, 0, 1), new Date(d2_year, d2_month, d2_date));
                            for (year = d1_year + 1, i = 1; i < j; i++ , year++) d += _leapYear(year) ? 366 : 365;
                        } else {
                            temp = [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31];
                            if (_leapYear(d1_year)) temp[1]++;
                            j = d2_month - d1_month;
                            if (j > 0) {
                                d += _diff('d', new Date(d1_year, d1_month, d1_date), new Date(d1_year, d1_month, temp[d1_month])) + 1;
                                d += _diff('d', new Date(d2_year, d2_month, 1), new Date(d2_year, d2_month, d2_date));
                                month = d1_month + 1;
                                for (i = 1; i < j; i++) d += temp[month++];
                            } else d += d2_date - d1_date;
                        }
                        return d * order;
                    case 'h': return parseInt((date2.getTime() - date1.getTime()) / 3600000);
                    case 'i': return parseInt((date2.getTime() - date1.getTime()) / 60000);
                    case 's': return parseInt((date2.getTime() - date1.getTime()) / 1000);
                    case 'ms': return date2.getTime() - date1.getTime();
                    default: return null;
                }
            },
            'datePart', function (part, date, isUTC) {
                var part, i, j, result;
                date = _get(date, isUTC), part = part || 'Y-m-d H:i:s', result = '';
                for (i = 0, j = part.length; i < j; i++) result += _date(part.charAt(i), date);
                return result;
            },
            'datePretty', function (targetDate, isUTC0, baseDate, isUTC1) {
                var m = prettyData[_ln].pretty, t0, t1, v;
                targetDate = _get(targetDate, isUTC0),
                    baseDate = _get(baseDate, isUTC1),
                    v = Math.round(+baseDate / 1000 - targetDate / 1000);
                if (v == 0) return m.now;
                if (v > 0) t0 = m.past;
                else if (v < 0) {
                    t0 = targetDate, targetDate = baseDate, baseDate = t0;
                    t0 = m.future, v *= -1;
                }
                if (v < 60) t1 = m.second;
                else if ((v = Math.round(v / 60)) < 60) t1 = m.minute;
                else if ((v = Math.round(v / 60)) < 60) t1 = m.hout;
                else if ((v = Math.round(v / 24)) < 30) t1 = m.day, v = _diff('d', targetDate, baseDate);
                else if (v < 365) t1 = m.month, v = _diff('m', targetDate, baseDate);
                else t1 = m.year, v = _diff('y', targetDate, baseDate);
                t1 = t1[v] || t1[t1.length - 1];
                return t0.ex('time', t1.ex('time', v));
            },
            'dateLang', function (v) { _ln = v; },
            'dateTemplate', function (ln, v) {
                var t0, i, k;
                if (v && typeof v == 'object' &&
                    v.day instanceof Array && v.day.length == 7 &&
                    v.pretty && typeof v.pretty == 'object' &&
                    typeof v.pretty.now == 'string' &&
                    typeof v.pretty.past == 'string' && v.pretty.past.indexOf('${time}') != NONE &&
                    typeof v.pretty.future == 'string' && v.pretty.future.indexOf('${time}') != NONE
                ) {
                    i = prettyKey.length;
                    while (i--) {
                        t0 = v.pretty[prettyKey[i]];
                        if (!(t0 instanceof Array) || t0.length < 2 || t0[t0.length - 1].indexOf('${time}') == NONE) {
                            k = 1;
                            break;
                        }
                    }
                    if (!k) return prettyData[ln] = v;
                }
                return err('DATE.i18n');
            }
        );
        bs.dateTemplate('ko-KR', {
            day: ['일', '월', '화', '수', '목', '금', '토'],
            pretty: {
                now: '지금', past: '${time} 후', future: '${time} 전',
                second: [0, '${time}초'], minute: [0, '${time}분'], hour: [0, '${time}시간'],
                day: [0, '${time}일'], month: [0, '${time}개월'], year: [0, '${time}년']
            }
        }),
            bs.dateTemplate('en-US', {
                day: ['sun', 'mon', 'tue', 'wed', 'thu', 'fri', 'sat'],
                pretty: {
                    now: 'Now', past: '${time} ago', future: 'in ${time}',
                    second: [0, 'a few second', '${time} seconds'],
                    minute: [0, 'a minute', '${time} minutes'],
                    hour: [0, 'an hour', '${time} hours'],
                    day: [0, 'a day', '${time} days'],
                    month: [0, 'a month', '${time} months'],
                    year: [0, 'a year', '${time} years']
                }
            }),
            bs.dateLang('ko-KR');
    })();
    bs.bsImmutable('FAIL', FAIL, 'doc', bsdoc);
    (function () {
        var lock = {};
        bs.bsImmutable(
            'lock', function () {
                var a = arguments, i = a.length;
                while (i--) lock[a[i]] = 1;
            },
            'unlock', function () {
                var a = arguments, i = a.length;
                while (i--) lock[a[i]] = 0;
            },
            'isLock', function () {
                var a = arguments, i = a.length;
                while (i--) if (lock[a[i]]) return 1;
            },
            'locker', function (k, f, time) {
                var t;
                if (time) t = function () { if (bs.isLock(k)) bs.unlock(k); };
                return function () {
                    if (lock[k]) return;
                    if (time) setTimeout(t, time);
                    return lock[k] = 1, f.apply(this, arguments);
                };
            },
            'unlocker', function (k, f) {
                return function () {
                    if (!lock[k]) return;
                    return lock[k] = 0, f.apply(this, arguments);
                };
            });
    })();
    bs.bsImmutable('white', (function () {
        var white, e = {},
            check = function (obj, vali, i) {
                var v = obj[i];
                if (vali = vali && vali[i]) if (vali(v)) return v;
                if (typeof v == 'string') v = obj[i] = v.trim();
                return v ? 1 : (err('bsW:' + i), e);
            };
        return function (obj, vali) {
            var i, j;
            if (!arguments.length) return white;
            if (obj instanceof Array) {
                if (vali && !(vali instanceof Array)) return err('bsW:vali!=array', vali);
                for (i = 0, j = obj.length; i < j; i++) if (e === check(obj, vali, i)) return FAIL;
            } else for (i in obj) if (obj.hasOwnProperty(i)) if (e === check(obj, vali, i)) return FAIL;
            return white = obj;
        };
    })());
    bs.bsImmutable('log', log = (function () {
        var log, div, r = /[<]/g, d = new Date, init = function () {
            if (docId('bsConsole')) return;
            div = docNew('div');
            div.innerHTML = '<style>' +
                '#bsConsole{font-family:arial;position:fixed;z-index:999999;background:#fff;bottom:0;left:0;right:0;height:200px;overflow:hideen}' +
                '#bsConsoleTab{background:#999;height:20px;color:#fff}' +
                '#bsConsoleTabConsole,#bsConsoleTabElement{font-size:11px;margin:0 10px;line-height:20px;float:left}' +
                '#bsConsoleView{font-size:10px;overflow-y:scroll;height:180px}' +
                '#bsConsoleViewElement{word-break:break-all;word-wrap:break-word}' +
                '.bsConsoleItem{font-size:11px;border:1px solid #000;padding:5px;margin:0 5px;float:left}' +
                '.bsConsoleTime{font-size:10px;padding:5px}' +
                '</style>' +
                '<div id="bsConsole">' +
                '<div id="bsConsoleTab">' +
                '<div id="bsConsoleTabConsole">Console</div><div id="bsConsoleTabElement">Element</div>' +
                '</div>' +
                '<div id="bsConsoleView">' +
                '<div id="bsConsoleViewConsole"></div><div id="bsConsoleViewElement" style="display:none"></div>' +
                '</div>' +
                '</div>';
            doc.body.appendChild(div.firstChild),
                doc.body.appendChild(div.firstChild),
                docId('bsConsole').onclick = function (e) {
                    e = e.target;
                    switch (e.id) {
                        case 'bsConsoleTab': docId('bsConsole').style.height = docId('bsConsole').style.height == '200px' ? '20px' : '200px'; break;
                        case 'bsConsoleTabElement':
                            docId('bsConsoleViewConsole').style.display = 'none';
                            docId('bsConsoleViewElement').style.display = 'block';
                            docId('bsConsoleViewElement').innerHTML = '<pre>' +
                                ('<html>\n' + docTag('html')[0].innerHTML + '\n</html>').replace(r, '&lt;') +
                                '</pre>';
                            break;
                        case 'bsConsoleTabConsole':
                            doc.getElementById('bsConsoleViewConsole').style.display = 'block';
                            doc.getElementById('bsConsoleViewElement').style.display = 'none';
                    }
                };
        };
        log = function () {
            var a = arguments, i = 0, j = a.length, v, item = '<div style="clear:both"><div class="bsConsoleTime">' + (d.setTime(Date.now()), d.toJSON()) + '</div>';
            init();
            while (i < j) {
                if ((v = a[i++]) && typeof v == 'object') v = JSON.stringify(v);
                item += '<div class="bsConsoleItem">' + v + '</div>';
            }
            div.innerHTML = item + '</div>';
            docId('bsConsoleViewConsole').appendChild(div.childNodes[0]);
        };
        if (!W['console']) W.console = { log: log };
        return log;
    })());
    bs.bsImmutable('namespace', function (v) { ns = 'data-' + v + '-'; }),
        (function () {
            var mk = function (m) {
                var t = {};
                return m = Math[m], function (r) { return t[r] || t[r] === 0 ? 0 : (t[r] = m(r)); };
            },
                rc = 0, rand = {};
            bs.bsImmutable(
                /*
                'rand', function(a, b){return parseInt((rand[rc = (++rc) % 7000] || (rand[rc] = Math.random())) * (b - a + 1)) + a;},
                'randf', function(a, b){return (rand[rc = (++rc) % 7000] || (rand[rc] = Math.random())) * (b - a) + a;},
                */
                'rand', function (a, b) { return parseInt(Math.random() * (b - a + 1)) + a; },
                'randf', function (a, b) { return Math.random() * (b - a) + a; },
                'sin', mk('sin'), 'cos', mk('cos'), 'tan', mk('tan'), 'atan', mk('atan'),
                'toradian', Math.PI / 180, 'toangle', 180 / Math.PI
            );
        })(),
        (function () {
            var encode = encodeURIComponent, decode = decodeURIComponent, mk = function (target) {
                var cache = {};
                return function (v) {
                    var query = v || location[target].substr(1), t0, t1, i, j;
                    if (!query) return;
                    if (!cache[query]) {
                        t0 = query.split('&'), i = t0.length, t1 = {};
                        while (i--) t0[i] = t0[i].split('='), t1[decode(t0[i][0])] = decode(t0[i][1]);
                        cache[query] = t1;
                        cache[query].__full = query;
                    }
                    return cache[query];
                };
            };
            bs.bsImmutable(
                'encode', encode, 'decode', decode,
                'queryString', mk('search'),
                'hash', mk('hash'),
                'ck', function (key/*, val, expire, path*/) {
                    var t0, t1, t2, i, j, v;
                    t0 = doc.cookie.split(';'), i = t0.length;
                    if (arguments.length == 1) {
                        while (i--) if (t0[i].substring(0, j = t0[i].indexOf('=')).trim() == key) return decode(t0[i].substr(j + 1).trim());
                        return null;
                    } else {
                        v = arguments[1], t1 = key + '=' + encode(v) + ';domain=' + doc.domain + ';path=' + (arguments[3] || '/');
                        if (v === null) t0 = new Date, t0.setTime(t0.getTime() - 86400000), t1 += ';expires=' + t0.toUTCString();
                        else if (arguments[2]) t0 = new Date, t0.setTime(t0.getTime() + arguments[2] * 86400000), t1 += ';expires=' + t0.toUTCString();
                        return doc.cookie = t1, v;
                    }
                });
        })(),
        bs.bsImmutable(
            'xml', (function () {
                var filter = function (v) {
                    if (typeof v == 'string') {
                        if (v.substr(0, 20).indexOf('<![CDATA[') > -1) v = v.substring(0, 20).replace('<![CDATA[', '') + v.substr(20);
                        if (v.substr(v.length - 5).indexOf(']]>') > -1) v = v.substring(0, v.length - 5) + v.substr(v.length - 5).replace(']]>', '');
                        return v.trim();
                    } else return '';
                }, text, stack = [], type,
                    parse = W['DOMParser'] ? (function () {
                        var worker = new DOMParser;
                        text = 'textContent';
                        type = 1;
                        return function (v) { return worker.parseFromString(v, "text/xml"); };
                    })() : (function () {
                        var t0 = 'MSXML2.DOMDocument', i, j;
                        text = 'text';
                        t0 = ['Microsoft.XMLDOM', 'MSXML.DOMDocument', t0, t0 + '.3.0', t0 + '.4.0', t0 + '.5.0', t0 + '.6.0'], i = t0.length;
                        while (i--) {
                            try { new ActiveXObject(j = t0[i]); } catch (e) { continue; }
                            break;
                        }
                        return function (v) {
                            var worker = new ActiveXObject(j);
                            return worker.loadXML(v), worker;
                        };
                    })();
                return function (v, result) {
                    var node, attr, name, sub, prev, parent, i, j, k, l, n, m;
                    if (!result || typeof result != 'object' || typeof result != 'function') result = {};
                    stack.length = 0,
                        v = { nodes: parse(filter(v)).childNodes, parent: result };
                    do {
                        for (i = 0, j = v.nodes.length; i < j; i++) {
                            sub = {}, node = type ? v.nodes[i] : v.nodes.nextNode(), name = node.nodeName,
                                parent = v.parent;
                            switch (node.nodeType) {
                                case 3: parent.value = node[text].trim(); break;
                                case 4: parent.value = filter(node[text].trim()); break;
                                case 1:
                                    if (attr = node.attributes) {
                                        k = attr.length;
                                        while (k--) sub['$' + attr[k].name] = attr[k].value;
                                    }
                                    if (node.childNodes && (n = node.childNodes.length)) stack[stack.length] = { parent: sub, nodes: node.childNodes };
                                    if (prev = parent[name]) {
                                        if (prev.length === undefined) parent[name] = { length: 2, 0: prev, 1: sub };
                                        else parent[name][prev.length++] = sub;
                                    } else parent[name] = sub;
                            }
                        }
                    } while (v = stack.pop())
                    return result;
                };
            })()
        ),
        (function () {
            var pool = { length: 0 }, lists = [], keys = {}, max = 100, uuid = 0, loop, ani, rate,
                stop = 1, pause = 0, inc = 0,
                PI = Math.PI, HPI = PI * .5, bio, a;
            rate = 0,
                ani = {
                    linear: function (c, b) { return a = rate, b * a + c },
                    backIn: function (c, b) { return a = rate, b * a * a * (2.70158 * a - 1.70158) + c },
                    backOut: function (c, b) { return a = rate - 1, b * (a * a * (2.70158 * a + 1.70158) + 1) + c },
                    backInOut: bio = function (c, b) {
                        a = rate * 2;
                        if (1 > a) return .5 * b * a * a * (3.5949095 * a - 2.5949095) + c;
                        else return a -= 2, .5 * b * (a * a * (3.70158 * a + 2.70158) + 2) + c;
                    },
                    sineIn: function (c, b) { return a = rate, -b * Math.cos(a * HPI) + b + c; },
                    sineOut: function (c, b) { return a = rate, b * Math.sin(a * HPI) + c; },
                    sineInOut: function (c, b) { return a = rate, .5 * -b * (Math.cos(PI * a) - 1) + c; },
                    circleIn: function (c, b) { return a = rate, -b * (Math.sqrt(1 - a * a) - 1) + c; },
                    circleOut: function (c, b) { return a = rate - 1, b * Math.sqrt(1 - a * a) + c; },
                    circleInOut: function (c, b) {
                        a = rate * 2;
                        if (1 > a) return .5 * -b * (Math.sqrt(1 - a * a) - 1) + c;
                        return a -= 2, .5 * b * (Math.sqrt(1 - a * a) + 1) + c;
                    },
                    quadraticIn: function (c, b) { return a = rate, b * a * a + c; },
                    quadraticOut: function (c, b) { return a = rate, -b * a * (a - 2) + c; },
                    bounceOut: function (c, b) {
                        a = rate;
                        if (0.363636 > a) return 7.5625 * b * a * a + c;
                        if (0.727272 > a) return a -= 0.545454, b * (7.5625 * a * a + 0.75) + c;
                        if (0.90909 > a) return a -= 0.818181, b * (7.5625 * a * a + 0.9375) + c;
                        return a -= 0.95454, b * (7.5625 * a * a + 0.984375) + c;
                    }
                    //bounceIn:function(a,c,b,d,e){return b-bio((e-d)/e,0,b)+c},
                    //bounceInOut:function(a,c,b,d,e){if(d<0.5*e)return d*=2,0.5*ease[13](d/e,0,b,d,e)+c;d=2*d-e;return 0.5*ease[14](d/e,0,b,d,e)+0.5*b+c},
                },
                loop = function (t) {
                    var f, r, i, j, k, isEnd, cnt, list, isTurn;
                    if (stop) return;
                    if (!pause && (i = lists.length)) {
                        t += inc;
                        while (i--) {
                            list = lists[i], cnt = 0, k = j = list.length;
                            while (j--) {
                                if (f = list[j]) {
                                    if (f.__start__ < t) {
                                        isTurn = isEnd = 0;
                                        if (f.__term__) {
                                            r = (t - f.__start__) / f.__term__;
                                            r = r > 1 ? 1 : r < 0 ? 0 : r;
                                            if (f.__end__ < t) {
                                                f.__loop__--;
                                                if (f.__loop__) {
                                                    f.__start__ = performance.now();
                                                    f.__end__ = f.__term__ ? (f.__start__ + f.__term__) : 0;
                                                    r = 0;
                                                    isTurn = 1;
                                                } else isEnd = 1;
                                            }
                                        } else r = 0;
                                        ani.rate = rate = r;
                                        ani.loop = f.__loop__;
                                        if (isTurn && f.turn) f.turn(t, ani);
                                        if (f.update) f.update(t, ani);
                                        if (!f.__isPause__ && f.ANI(t, ani) || isEnd) {
                                            f.__key__ = keys[f.__key__] = list[j] = 0, cnt++;
                                            if (f.end) f.end(t, ani);
                                        }
                                    }
                                } else cnt++;
                            }
                            if (cnt == k) pool[pool.length++] = list, lists.splice(i, 1);
                        }
                    }
                    requestAnimationFrame(loop);
                },
                bs.bsImmutable(
                    'ani', function (key, f) {
                        var list, i, j;
                        if (f) {
                            if (typeof f.ANI != FUN) return err('ani0');
                            if (f.__key__) return err('ani1');
                            if (key && keys[key]) return err('ani2');

                            f.__key__ = key || uuid++ ,
                                f.__start__ = (f.delay * 1000 || 0) + performance.now() + inc,
                                f.__term__ = f.time ? f.time * 1000 : 0,
                                f.__end__ = f.__term__ ? (f.__start__ + f.__term__) : 0;
                            f.__isPause__ = 0;
                            f.__loop__ = typeof f.loop == 'number' ? f.loop : 1;
                            if ((i = lists.length) && (i = lists[i - 1]).length < max) list = i;
                            else lists[lists.length] = list = pool.length ? pool[--pool.length] : { length: 0 };
                            list[list.length++] = keys[key] = f;
                            if (stop) stop = 0, requestAnimationFrame(loop);
                        } else if (key && f === null) {
                            if (keys[key]) {
                                i = lists.length;
                                out:
                                while (i--) {
                                    list = lists[i], j = list.length;
                                    while (j--) if (list[j].key == key) {
                                        list[j] = 0;
                                        break out;
                                    }
                                }
                            }
                        }
                    },
                    'stop', function () {
                        var list, i, j, f;
                        if (stop) return;
                        stop = 1, inc = pause = 0, i = lists.length;
                        while (i--) {
                            list = lists[i], j = list.length;
                            while (j--) f = list[j], f.__key__ = keys[f.__key__] = list[j] = 0;
                            list.length = 0, pool[pool.length++] = list;
                        }
                        lists.length = 0;
                    },
                    'pause', function () {
                        var f, a = arguments, i = a.length;
                        if (i) {
                            while (i--) {
                                f = keys[a[i]];
                                if (!f.__isPause__) f.__isPause__ = performance.now();
                            }
                        } else {
                            if (pause) return;
                            pause = performance.now();
                        }
                    },
                    'resume', function () {
                        var f, a = arguments, i = a.length;
                        if (i) {
                            while (i--) {
                                f = keys[a[i]];
                                if (f.__isPause__) {
                                    f.__start__ += performance.now() - f.__isPause__;
                                    f.__end__ = f.__term__ ? (f.__start__ + f.__term__) : 0;
                                    f.__isPause__ = 0;
                                }
                            }
                        } else {
                            if (!pause) return;
                            inc += performance.now() - pause;
                            pause = 0;
                        }
                    });
        })(),
        (function () {
            var root = 'scrollHeight' in body ? 0 : docel, scroll, prev;
            scroll = function (t, ani) {
                var ease;
                ease = ani[this.ease],
                    W.scrollTo(ease(this.sx, this.tx), ease(this.sy, this.ty));
            };
            bs.bsImmutable(
                'scrollTo', function (x, y, isPercent, time, ease, end) {
                    var a = arguments, i, j;
                    if (!root && !(root = doc.body)) return;
                    if (isPercent) {
                        x *= ((i = root.scrollWidth) > (j = root.clientWidth) ? i : j) * .01,
                            y *= ((i = root.scrollHeight) > (j = root.clientHeight) ? i : j) * .01;
                    }
                    if (time) {
                        bs.ani('@scroll', null);
                        a.ANI = scroll,
                            a.end = end,
                            a.x = x, a.y = y,
                            a.sx = docel.scrollLeft || W.pageXOffset || 0,
                            a.sy = docel.scrollTop || W.pageYOffset || 0,
                            a.tx = x - a.sx, a.ty = y - a.sy,
                            a.time = time,
                            a.ease = ease || 'linear',
                            bs.ani('@scroll', prev = a);
                    } else W.scrollTo(x, y);
                });
        })();
    (function () {
        var c = doc.createElement('canvas'), ct;
        if (typeof c.getContext == 'function') {
            ct = c.getContext('2d');
            bs.bsImmutable(
                'img2src', function (img) {
                    var w = c.width = img.width, h = c.height = img.height;
                    ct.drawImage(img, 0, 0, w, h, 0, 0, w, h);
                    return c.toDataURL();
                });
        }
    })();
    (function () {
        var timeout = 5000, async, xAttr, baseHeader, http, port, mk, RSA, rsaList, rsaListUrl, rsaKey, rsa;
        RSA = (function () {
            var v, b64map = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/"; var b64pad = "="; function hex2b64(d) { var b; var e; var a = ""; for (b = 0; b + 3 <= d.length; b += 3) { e = parseInt(d.substring(b, b + 3), 16); a += b64map.charAt(e >> 6) + b64map.charAt(e & 63) } if (b + 1 == d.length) { e = parseInt(d.substring(b, b + 1), 16); a += b64map.charAt(e << 2) } else { if (b + 2 == d.length) { e = parseInt(d.substring(b, b + 2), 16); a += b64map.charAt(e >> 2) + b64map.charAt((e & 3) << 4) } } while ((a.length & 3) > 0) { a += b64pad } return a } function b64tohex(e) { var c = ""; var d; var a = 0; var b; for (d = 0; d < e.length; ++d) { if (e.charAt(d) == b64pad) { break } v = b64map.indexOf(e.charAt(d)); if (v < 0) { continue } if (a == 0) { c += int2char(v >> 2); b = v & 3; a = 1 } else { if (a == 1) { c += int2char((b << 2) | (v >> 4)); b = v & 15; a = 2 } else { if (a == 2) { c += int2char(b); c += int2char(v >> 2); b = v & 3; a = 3 } else { c += int2char((b << 2) | (v >> 4)); c += int2char(v & 15); a = 0 } } } } if (a == 1) { c += int2char(b << 2) } return c } function b64toBA(e) { var d = b64tohex(e); var c; var b = new Array(); for (c = 0; 2 * c < d.length; ++c) { b[c] = parseInt(d.substring(2 * c, 2 * c + 2), 16) } return b } var dbits; var canary = 244837814094590; var j_lm = ((canary & 16777215) == 15715070); function BigInteger(e, d, f) { if (e != null) { if ("number" == typeof e) { this.fromNumber(e, d, f) } else { if (d == null && "string" != typeof e) { this.fromString(e, 256) } else { this.fromString(e, d) } } } } function nbi() { return new BigInteger(null) } function am1(f, a, b, e, h, g) { while (--g >= 0) { var d = a * this[f++] + b[e] + h; h = Math.floor(d / 67108864); b[e++] = d & 67108863 } return h } function am2(f, q, r, e, o, a) { var k = q & 32767, p = q >> 15; while (--a >= 0) { var d = this[f] & 32767; var g = this[f++] >> 15; var b = p * d + g * k; d = k * d + ((b & 32767) << 15) + r[e] + (o & 1073741823); o = (d >>> 30) + (b >>> 15) + p * g + (o >>> 30); r[e++] = d & 1073741823 } return o } function am3(f, q, r, e, o, a) { var k = q & 16383, p = q >> 14; while (--a >= 0) { var d = this[f] & 16383; var g = this[f++] >> 14; var b = p * d + g * k; d = k * d + ((b & 16383) << 14) + r[e] + o; o = (d >> 28) + (b >> 14) + p * g; r[e++] = d & 268435455 } return o } if (j_lm && (navigator.appName == "Microsoft Internet Explorer")) { BigInteger.prototype.am = am2; dbits = 30 } else { if (j_lm && (navigator.appName != "Netscape")) { BigInteger.prototype.am = am1; dbits = 26 } else { BigInteger.prototype.am = am3; dbits = 28 } } BigInteger.prototype.DB = dbits; BigInteger.prototype.DM = ((1 << dbits) - 1); BigInteger.prototype.DV = (1 << dbits); var BI_FP = 52; BigInteger.prototype.FV = Math.pow(2, BI_FP); BigInteger.prototype.F1 = BI_FP - dbits; BigInteger.prototype.F2 = 2 * dbits - BI_FP; var BI_RM = "0123456789abcdefghijklmnopqrstuvwxyz"; var BI_RC = new Array(); var rr, vv; rr = "0".charCodeAt(0); for (vv = 0; vv <= 9; ++vv) { BI_RC[rr++] = vv } rr = "a".charCodeAt(0); for (vv = 10; vv < 36; ++vv) { BI_RC[rr++] = vv } rr = "A".charCodeAt(0); for (vv = 10; vv < 36; ++vv) { BI_RC[rr++] = vv } function int2char(a) { return BI_RM.charAt(a) } function intAt(b, a) { var d = BI_RC[b.charCodeAt(a)]; return (d == null) ? -1 : d } function bnpCopyTo(b) { for (var a = this.t - 1; a >= 0; --a) { b[a] = this[a] } b.t = this.t; b.s = this.s } function bnpFromInt(a) { this.t = 1; this.s = (a < 0) ? -1 : 0; if (a > 0) { this[0] = a } else { if (a < -1) { this[0] = a + DV } else { this.t = 0 } } } function nbv(a) { var b = nbi(); b.fromInt(a); return b } function bnpFromString(h, c) { var e; if (c == 16) { e = 4 } else { if (c == 8) { e = 3 } else { if (c == 256) { e = 8 } else { if (c == 2) { e = 1 } else { if (c == 32) { e = 5 } else { if (c == 4) { e = 2 } else { this.fromRadix(h, c); return } } } } } } this.t = 0; this.s = 0; var g = h.length, d = false, f = 0; while (--g >= 0) { var a = (e == 8) ? h[g] & 255 : intAt(h, g); if (a < 0) { if (h.charAt(g) == "-") { d = true } continue } d = false; if (f == 0) { this[this.t++] = a } else { if (f + e > this.DB) { this[this.t - 1] |= (a & ((1 << (this.DB - f)) - 1)) << f; this[this.t++] = (a >> (this.DB - f)) } else { this[this.t - 1] |= a << f } } f += e; if (f >= this.DB) { f -= this.DB } } if (e == 8 && (h[0] & 128) != 0) { this.s = -1; if (f > 0) { this[this.t - 1] |= ((1 << (this.DB - f)) - 1) << f } } this.clamp(); if (d) { BigInteger.ZERO.subTo(this, this) } } function bnpClamp() { var a = this.s & this.DM; while (this.t > 0 && this[this.t - 1] == a) { --this.t } } function bnToString(c) { if (this.s < 0) { return "-" + this.negate().toString(c) } var e; if (c == 16) { e = 4 } else { if (c == 8) { e = 3 } else { if (c == 2) { e = 1 } else { if (c == 32) { e = 5 } else { if (c == 4) { e = 2 } else { return this.toRadix(c) } } } } } var g = (1 << e) - 1, l, a = false, h = "", f = this.t; var j = this.DB - (f * this.DB) % e; if (f-- > 0) { if (j < this.DB && (l = this[f] >> j) > 0) { a = true; h = int2char(l) } while (f >= 0) { if (j < e) { l = (this[f] & ((1 << j) - 1)) << (e - j); l |= this[--f] >> (j += this.DB - e) } else { l = (this[f] >> (j -= e)) & g; if (j <= 0) { j += this.DB; --f } } if (l > 0) { a = true } if (a) { h += int2char(l) } } } return a ? h : "0" } function bnNegate() { var a = nbi(); BigInteger.ZERO.subTo(this, a); return a } function bnAbs() { return (this.s < 0) ? this.negate() : this } function bnCompareTo(b) { var d = this.s - b.s; if (d != 0) { return d } var c = this.t; d = c - b.t; if (d != 0) { return d } while (--c >= 0) { if ((d = this[c] - b[c]) != 0) { return d } } return 0 } function nbits(a) { var c = 1, b; if ((b = a >>> 16) != 0) { a = b; c += 16 } if ((b = a >> 8) != 0) { a = b; c += 8 } if ((b = a >> 4) != 0) { a = b; c += 4 } if ((b = a >> 2) != 0) { a = b; c += 2 } if ((b = a >> 1) != 0) { a = b; c += 1 } return c } function bnBitLength() { if (this.t <= 0) { return 0 } return this.DB * (this.t - 1) + nbits(this[this.t - 1] ^ (this.s & this.DM)) } function bnpDLShiftTo(c, b) { var a; for (a = this.t - 1; a >= 0; --a) { b[a + c] = this[a] } for (a = c - 1; a >= 0; --a) { b[a] = 0 } b.t = this.t + c; b.s = this.s } function bnpDRShiftTo(c, b) { for (var a = c; a < this.t; ++a) { b[a - c] = this[a] } b.t = Math.max(this.t - c, 0); b.s = this.s } function bnpLShiftTo(j, e) { var b = j % this.DB; var a = this.DB - b; var g = (1 << a) - 1; var f = Math.floor(j / this.DB), h = (this.s << b) & this.DM, d; for (d = this.t - 1; d >= 0; --d) { e[d + f + 1] = (this[d] >> a) | h; h = (this[d] & g) << b } for (d = f - 1; d >= 0; --d) { e[d] = 0 } e[f] = h; e.t = this.t + f + 1; e.s = this.s; e.clamp() } function bnpRShiftTo(g, d) { d.s = this.s; var e = Math.floor(g / this.DB); if (e >= this.t) { d.t = 0; return } var b = g % this.DB; var a = this.DB - b; var f = (1 << b) - 1; d[0] = this[e] >> b; for (var c = e + 1; c < this.t; ++c) { d[c - e - 1] |= (this[c] & f) << a; d[c - e] = this[c] >> b } if (b > 0) { d[this.t - e - 1] |= (this.s & f) << a } d.t = this.t - e; d.clamp() } function bnpSubTo(d, f) { var e = 0, g = 0, b = Math.min(d.t, this.t); while (e < b) { g += this[e] - d[e]; f[e++] = g & this.DM; g >>= this.DB } if (d.t < this.t) { g -= d.s; while (e < this.t) { g += this[e]; f[e++] = g & this.DM; g >>= this.DB } g += this.s } else { g += this.s; while (e < d.t) { g -= d[e]; f[e++] = g & this.DM; g >>= this.DB } g -= d.s } f.s = (g < 0) ? -1 : 0; if (g < -1) { f[e++] = this.DV + g } else { if (g > 0) { f[e++] = g } } f.t = e; f.clamp() } function bnpMultiplyTo(c, e) { var b = this.abs(), f = c.abs(); var d = b.t; e.t = d + f.t; while (--d >= 0) { e[d] = 0 } for (d = 0; d < f.t; ++d) { e[d + b.t] = b.am(0, f[d], e, d, 0, b.t) } e.s = 0; e.clamp(); if (this.s != c.s) { BigInteger.ZERO.subTo(e, e) } } function bnpSquareTo(d) { var a = this.abs(); var b = d.t = 2 * a.t; while (--b >= 0) { d[b] = 0 } for (b = 0; b < a.t - 1; ++b) { var e = a.am(b, a[b], d, 2 * b, 0, 1); if ((d[b + a.t] += a.am(b + 1, 2 * a[b], d, 2 * b + 1, e, a.t - b - 1)) >= a.DV) { d[b + a.t] -= a.DV; d[b + a.t + 1] = 1 } } if (d.t > 0) { d[d.t - 1] += a.am(b, a[b], d, 2 * b, 0, 1) } d.s = 0; d.clamp() } function bnpDivRemTo(n, h, g) { var x = n.abs(); if (x.t <= 0) { return } var k = this.abs(); if (k.t < x.t) { if (h != null) { h.fromInt(0) } if (g != null) { this.copyTo(g) } return } if (g == null) { g = nbi() } var d = nbi(), a = this.s, l = n.s; var w = this.DB - nbits(x[x.t - 1]); if (w > 0) { x.lShiftTo(w, d); k.lShiftTo(w, g) } else { x.copyTo(d); k.copyTo(g) } var p = d.t; var b = d[p - 1]; if (b == 0) { return } var o = b * (1 << this.F1) + ((p > 1) ? d[p - 2] >> this.F2 : 0); var C = this.FV / o, B = (1 << this.F1) / o, A = 1 << this.F2; var u = g.t, s = u - p, f = (h == null) ? nbi() : h; d.dlShiftTo(s, f); if (g.compareTo(f) >= 0) { g[g.t++] = 1; g.subTo(f, g) } BigInteger.ONE.dlShiftTo(p, f); f.subTo(d, d); while (d.t < p) { d[d.t++] = 0 } while (--s >= 0) { var c = (g[--u] == b) ? this.DM : Math.floor(g[u] * C + (g[u - 1] + A) * B); if ((g[u] += d.am(0, c, g, s, 0, p)) < c) { d.dlShiftTo(s, f); g.subTo(f, g); while (g[u] < --c) { g.subTo(f, g) } } } if (h != null) { g.drShiftTo(p, h); if (a != l) { BigInteger.ZERO.subTo(h, h) } } g.t = p; g.clamp(); if (w > 0) { g.rShiftTo(w, g) } if (a < 0) { BigInteger.ZERO.subTo(g, g) } } function bnMod(b) { var c = nbi(); this.abs().divRemTo(b, null, c); if (this.s < 0 && c.compareTo(BigInteger.ZERO) > 0) { b.subTo(c, c) } return c } function Classic(a) { this.m = a } function cConvert(a) { if (a.s < 0 || a.compareTo(this.m) >= 0) { return a.mod(this.m) } else { return a } } function cRevert(a) { return a } function cReduce(a) { a.divRemTo(this.m, null, a) } function cMulTo(a, c, b) { a.multiplyTo(c, b); this.reduce(b) } function cSqrTo(a, b) { a.squareTo(b); this.reduce(b) } Classic.prototype.convert = cConvert; Classic.prototype.revert = cRevert; Classic.prototype.reduce = cReduce; Classic.prototype.mulTo = cMulTo; Classic.prototype.sqrTo = cSqrTo; function bnpInvDigit() { if (this.t < 1) { return 0 } var a = this[0]; if ((a & 1) == 0) { return 0 } var b = a & 3; b = (b * (2 - (a & 15) * b)) & 15; b = (b * (2 - (a & 255) * b)) & 255; b = (b * (2 - (((a & 65535) * b) & 65535))) & 65535; b = (b * (2 - a * b % this.DV)) % this.DV; return (b > 0) ? this.DV - b : -b } function Montgomery(a) { this.m = a; this.mp = a.invDigit(); this.mpl = this.mp & 32767; this.mph = this.mp >> 15; this.um = (1 << (a.DB - 15)) - 1; this.mt2 = 2 * a.t } function montConvert(a) { var b = nbi(); a.abs().dlShiftTo(this.m.t, b); b.divRemTo(this.m, null, b); if (a.s < 0 && b.compareTo(BigInteger.ZERO) > 0) { this.m.subTo(b, b) } return b } function montRevert(a) { var b = nbi(); a.copyTo(b); this.reduce(b); return b } function montReduce(a) { while (a.t <= this.mt2) { a[a.t++] = 0 } for (var c = 0; c < this.m.t; ++c) { var b = a[c] & 32767; var d = (b * this.mpl + (((b * this.mph + (a[c] >> 15) * this.mpl) & this.um) << 15)) & a.DM; b = c + this.m.t; a[b] += this.m.am(0, d, a, c, 0, this.m.t); while (a[b] >= a.DV) { a[b] -= a.DV; a[++b]++ } } a.clamp(); a.drShiftTo(this.m.t, a); if (a.compareTo(this.m) >= 0) { a.subTo(this.m, a) } } function montSqrTo(a, b) { a.squareTo(b); this.reduce(b) } function montMulTo(a, c, b) { a.multiplyTo(c, b); this.reduce(b) } Montgomery.prototype.convert = montConvert; Montgomery.prototype.revert = montRevert; Montgomery.prototype.reduce = montReduce; Montgomery.prototype.mulTo = montMulTo; Montgomery.prototype.sqrTo = montSqrTo; function bnpIsEven() { return ((this.t > 0) ? (this[0] & 1) : this.s) == 0 } function bnpExp(h, j) { if (h > 4294967295 || h < 1) { return BigInteger.ONE } var f = nbi(), a = nbi(), d = j.convert(this), c = nbits(h) - 1; d.copyTo(f); while (--c >= 0) { j.sqrTo(f, a); if ((h & (1 << c)) > 0) { j.mulTo(a, d, f) } else { var b = f; f = a; a = b } } return j.revert(f) } function bnModPowInt(b, a) { var c; if (b < 256 || a.isEven()) { c = new Classic(a) } else { c = new Montgomery(a) } return this.exp(b, c) } BigInteger.prototype.copyTo = bnpCopyTo; BigInteger.prototype.fromInt = bnpFromInt; BigInteger.prototype.fromString = bnpFromString; BigInteger.prototype.clamp = bnpClamp; BigInteger.prototype.dlShiftTo = bnpDLShiftTo; BigInteger.prototype.drShiftTo = bnpDRShiftTo; BigInteger.prototype.lShiftTo = bnpLShiftTo; BigInteger.prototype.rShiftTo = bnpRShiftTo; BigInteger.prototype.subTo = bnpSubTo; BigInteger.prototype.multiplyTo = bnpMultiplyTo; BigInteger.prototype.squareTo = bnpSquareTo; BigInteger.prototype.divRemTo = bnpDivRemTo; BigInteger.prototype.invDigit = bnpInvDigit; BigInteger.prototype.isEven = bnpIsEven; BigInteger.prototype.exp = bnpExp; BigInteger.prototype.toString = bnToString; BigInteger.prototype.negate = bnNegate; BigInteger.prototype.abs = bnAbs; BigInteger.prototype.compareTo = bnCompareTo; BigInteger.prototype.bitLength = bnBitLength; BigInteger.prototype.mod = bnMod; BigInteger.prototype.modPowInt = bnModPowInt; BigInteger.ZERO = nbv(0); BigInteger.ONE = nbv(1); function Arcfour() { this.i = 0; this.j = 0; this.S = new Array() } function ARC4init(d) { var c, a, b; for (c = 0; c < 256; ++c) { this.S[c] = c } a = 0; for (c = 0; c < 256; ++c) { a = (a + this.S[c] + d[c % d.length]) & 255; b = this.S[c]; this.S[c] = this.S[a]; this.S[a] = b } this.i = 0; this.j = 0 } function ARC4next() { var a; this.i = (this.i + 1) & 255; this.j = (this.j + this.S[this.i]) & 255; a = this.S[this.i]; this.S[this.i] = this.S[this.j]; this.S[this.j] = a; return this.S[(a + this.S[this.i]) & 255] } Arcfour.prototype.init = ARC4init; Arcfour.prototype.next = ARC4next; function prng_newstate() { return new Arcfour() } var rng_psize = 256; var rng_state; var rng_pool; var rng_pptr; function rng_seed_int(a) { rng_pool[rng_pptr++] ^= a & 255; rng_pool[rng_pptr++] ^= (a >> 8) & 255; rng_pool[rng_pptr++] ^= (a >> 16) & 255; rng_pool[rng_pptr++] ^= (a >> 24) & 255; if (rng_pptr >= rng_psize) { rng_pptr -= rng_psize } } function rng_seed_time() { rng_seed_int(new Date().getTime()) } if (rng_pool == null) { rng_pool = new Array(); rng_pptr = 0; var t; if (navigator.appName == "Netscape" && navigator.appVersion < "5" && window.crypto) { var z = window.crypto.random(32); for (t = 0; t < z.length; ++t) { rng_pool[rng_pptr++] = z.charCodeAt(t) & 255 } } while (rng_pptr < rng_psize) { t = Math.floor(65536 * Math.random()); rng_pool[rng_pptr++] = t >>> 8; rng_pool[rng_pptr++] = t & 255 } rng_pptr = 0; rng_seed_time() } function rng_get_byte() { if (rng_state == null) { rng_seed_time(); rng_state = prng_newstate(); rng_state.init(rng_pool); for (rng_pptr = 0; rng_pptr < rng_pool.length; ++rng_pptr) { rng_pool[rng_pptr] = 0 } rng_pptr = 0 } return rng_state.next() } function rng_get_bytes(b) { var a; for (a = 0; a < b.length; ++a) { b[a] = rng_get_byte() } } function SecureRandom() { } SecureRandom.prototype.nextBytes = rng_get_bytes; function parseBigInt(b, a) { return new BigInteger(b, a) } function linebrk(c, d) { var a = ""; var b = 0; while (b + d < c.length) { a += c.substring(b, b + d) + "\n"; b += d } return a + c.substring(b, c.length) } function byte2Hex(a) { if (a < 16) { return "0" + a.toString(16) } else { return a.toString(16) } } function pkcs1pad2(e, h) { if (h < e.length + 11) { alert("Message too long for RSA"); return null } var g = new Array(); var d = e.length - 1; while (d >= 0 && h > 0) { var f = e.charCodeAt(d--); if (f < 128) { g[--h] = f } else { if ((f > 127) && (f < 2048)) { g[--h] = (f & 63) | 128; g[--h] = (f >> 6) | 192 } else { g[--h] = (f & 63) | 128; g[--h] = ((f >> 6) & 63) | 128; g[--h] = (f >> 12) | 224 } } } g[--h] = 0; var b = new SecureRandom(); var a = new Array(); while (h > 2) { a[0] = 0; while (a[0] == 0) { b.nextBytes(a) } g[--h] = a[0] } g[--h] = 2; g[--h] = 0; return new BigInteger(g) } function RSAKey() { this.n = null; this.e = 0; this.d = null; this.p = null; this.q = null; this.dmp1 = null; this.dmq1 = null; this.coeff = null } function RSASetPublic(b, a) { if (b != null && a != null && b.length > 0 && a.length > 0) { this.n = parseBigInt(b, 16); this.e = parseInt(a, 16) } else { alert("Invalid RSA public key") } } function RSADoPublic(a) { return a.modPowInt(this.e, this.n) } function RSAEncrypt(d) { var a = pkcs1pad2(d, (this.n.bitLength() + 7) >> 3); if (a == null) { return null } var e = this.doPublic(a); if (e == null) { return null } var b = e.toString(16); if ((b.length & 1) == 0) { return b } else { return "0" + b } } RSAKey.prototype.doPublic = RSADoPublic; RSAKey.prototype.setPublic = RSASetPublic; RSAKey.prototype.encrypt = RSAEncrypt;
            return function (b, f, a, e) { var d = new RSAKey(); d.setPublic(b, f); var c = d.encrypt(a); return e ? linebrk(hex2b64(c), 64) : linebrk(c, 64) };
        })(),
            rsaList = {}, rsaListUrl = {},
            rsaKey = {}, rsa = function (v) { this.v = v; },
            mk = function (method) { return function (end, url) { return http(method, end, url, arguments); }; };
        bs.bsImmutable(
            'timeout', function () { return arguments.length ? (timeout = parseInt(arguments[0] * 1000)) : timeout * .001; },
            'baseheader', function (k, v) {
                if (v) baseHeader[k] = v;
                return baseHeader[k];
            },
            'rsa', function () {
                var i = 0, j = arguments.length, k, v, r, p;
                while (i < j) {
                    k = arguments[i++];
                    if (i == j) return rsaList[k];
                    v = arguments[i++];
                    if (v === null) delete rsaList[k];
                    rsaList[k] = v;
                    for (r in v) rsaListUrl[r] = v[r], rsaListUrl[r].rsa = k;
                }
                return v;
            },
            'post', mk('POST'), 'put', mk('PUT'), 'del', mk('DELETE'), 'get', mk('GET')
        );
        baseHeader = {
            'Cache-Control': 'no-cache',
            'Content-Type': function (method) {
                return (method == 'GET' ? 'text/plain' : "application/x-www-form-urlencoded") + "; charset=UTF-8";
            }
        },
            xAttr = [],
            port = function (u) {
                var i = u.indexOf('://');
                if (i < 0) return '';
                u = u.substr(i + 3), i = u.indexOf('/');
                if (i > -1) u = u.substring(0, i);
                return (i = u.indexOf(':')) < 0 ? '' : u.substr(i + 1);
            },
            http = (function () {
                var a = [], b = [], send = [], urlID = 0, head = [];
                return function http(method, end, U, arg) {
                    var cnt, r, urlCache, body, isBlob, isJsonBody, responseType, rsaurl, rsaTarget, param, i, j, k, v, u;
                    if (!end) return err(5007); //sync disable
                    body = '', xAttr.length = head.length = send.length = 0,
                        j = arg.length, U = U.trim();
                    if (j > 3) {
                        i = 2, urlCache = 0;
                        for (k in rsaListUrl) if (U.indexOf(k) > -1) {
                            rsaTarget = rsaListUrl[k];
                            if (!rsaKey[rsaurl = rsaListUrl[k].rsa]) {
                                param = arguments;
                                return bs.get(function (v) {
                                    if (v === null) return err(5020);
                                    if ((v = JSON.parse(v)).modulus && v.exponent) rsaKey[rsaurl] = v;
                                    else return err(5021);
                                    http.apply(null, param);
                                }, rsaurl);
                            }
                            break;
                        }
                        while (i < j) {
                            if (!(k = arg[i++].trim()).length) return err(5005);
                            if (v = arg[i], v !== null && v !== undefined) {
                                if (rsaTarget && rsaTarget.indexOf(k) > -1) v = new rsa(v);
                                arg[i] = v instanceof rsa && rsaKey[rsaurl] ? RSA(rsaKey[rsaurl].modulus, rsaKey[rsaurl].exponent, v.v, 1) :
                                    typeof v == 'string' ? v.trim() :
                                        W['File'] && v instanceof File ? (isBlob = 1, v) :
                                            typeof v == 'object' ? JSON.stringify(v) :
                                                typeof v == 'function' ? v(method) :
                                                    v + '';
                            }
                            i++;
                        }
                        if (isBlob) {
                            if (W['FormData']) body = new FormData();
                            else return err(5006);
                        }
                        i = 2;
                        while (i < j) {
                            k = arg[i++], v = arg[i++];
                            if (v !== null && v !== undefined) switch (k) {
                                case '@withCredentials': xAttr[xAttr.length] = 'WithCredentials', xAttr[xAttr.length] = v; break;
                                case '@urlCache': urlCache = v, head.push('Cache-Control', 'max-age=86400'); break;
                                case '@responseType': responseType = v;
                                case 'json':
                                    if (v) body = v, isJsonBody = 1;
                                    break;
                                default:
                                    if (k.charAt(0) === '@') head.push(k.substr(1), v);
                                    else if (isBlob) {
                                        if (v instanceof File) body.append(k, v, v.fileName);
                                        else body.append(k, v);
                                    } else if (!isJsonBody) send.push(k, v);
                            }
                        }
                        if (!isJsonBody && !isBlob) {
                            body = '', i = 0, j = send.length;
                            while (i < j) body += encodeURIComponent(send[i++]) + '=' + encodeURIComponent(send[i++]) + '&';
                            body = body.substr(0, body.length - 1);
                        }
                    }
                    //URL처리
                    u = U.trim().split('#');
                    U = u[0];
                    U += method == 'GET' && body ? (U.indexOf('?') > -1 ? '&' : '?') + body : '';
                    U += urlCache ? '' : (U.indexOf('?') > -1 ? '&' : '?') + 'bsNC=' + Date.now() + (urlID++);
                    U += u[1] ? '#' + u[1] : '';
                    if (method == 'GET' && U.length > 512) err(5004, U);
                    //Header 처리
                    a.length = b.length = i = 0, j = head.length;
                    while (i < j) {
                        if (baseHeader[k = head[i++]]) b.push(k);
                        if (v = head[i++], v) a.push(k, v);
                    }
                    for (i in baseHeader) if (baseHeader.hasOwnProperty(i) && b.indexOf(i) == -1) {
                        v = baseHeader[i],
                            a.push(i, typeof v == 'function' ? v(method) : v);
                    }
                    k = async(end);
                    if (responseType) k.responseType = responseType;
                    k.open(method, U, true);
                    i = 0, j = a.length;
                    while (i < j) {
                        if (isBlob && a[i] == 'Content-Type') {
                            i += 2;
                            continue;
                        }
                        k.setRequestHeader(a[i++], a[i++]);
                    }
                    k.send(body);
                    //if(rsaurl && !rsaKey[rsaurl].cache) rsaKey[rsaurl] = 0;		//rsa캐쉬정리
                };
            })();
        (function () {
            var xhr, xhrType;
            if ("ActiveXObject" in W && !"CanvasRenderingContext2D" in W) {
                xhrType = 1,
                    xhr = (function () {
                        var t0, i, ver;
                        xhrType = 1;
                        t0 = "MSXML2.XMLHTTP", t0 = ["Microsoft.XMLHTTP", t0, t0 + '.3.0', t0 + '.4.0', t0 + '.5.0'],
                            i = t0.length;
                        while (i--) {
                            try {
                                new ActiveXObject(ver = t0[i]);
                            } catch (e) { continue; }
                            break;
                        }
                        return function () { return new ActiveXObject(ver); };
                    })();
            } else {
                xhrType = 0,
                    xhr = function () {
                        var t0 = new XMLHttpRequest, i = 0, j = xAttr.length;
                        while (i < j) t0[xAttr[i++]] = xAttr[i++];
                        return t0;
                    };
            }
            async = function (end) {
                var timeId, ontimeout = function () {
                    if (timeId == -1) return;
                    if (x.readyState !== 4) x.abort();
                    timeId = -1, x.onreadystatechange = null, end(null, 'timeout');
                }, x = xhr();
                if ("ontimeout" in x) xhr.timeout = timeout, xhr.ontimeout = ontimeout;
                else timeId = setTimeout(ontimeout, timeout);
                x.onreadystatechange = function () {
                    var response, status;
                    if (x.readyState !== 4 || timeId == -1) return;
                    clearTimeout(timeId), timeId = -1;
                    response = x.responseType == '' ? x.responseText : x.response,
                        status = x.status;
                    if (status == 0 || status == 200) status = 200;
                    else response = null;
                    xhrType ? delete x.onreadystatechange : x.onreadystatechange = null;
                    end.call(x, response, status);
                };
                return x;
            };
        })();

    })();
    (function () {
        var conf = { key: {}, keyF: {}, val: {}, nopx: {} }, key,
            nopx = conf.nopx, keys = conf.key, keyF = conf.keyF, vals = conf.val,
            bodyStyle = body.style, fn;

        sys.dom.style = conf,
            Style = function (s) { this.st = s, this.u = {}; },
            Style.bsImmutable(
                'fn', fn = function (type, k, v) {
                    if (type == 'key') {
                        attr[k] = STYLE;
                        if (typeof v == FUN) keyF[k] = v;
                        else keys[k] = v;
                    } else conf[type][k] = v;
                    return v;
                },
                'key', key = (function () {
                    var prefix = detect.stylePrefix,
                        r = /-[a-z]/g, re = function (_0) { return _0.charAt(1).toUpperCase(); };
                    return function (k) {
                        var t0;
                        if (k.charAt(0) == '-') k = k.substr(k.indexOf('-', 1) + 1);
                        t0 = k.replace(r, re);
                        if (t0 in bodyStyle || (t0 = prefix + t0.charAt(0).toUpperCase() + t0.substr(1)) in bodyStyle) return fn('key', k, t0);
                        else attr[k] = NONE;
                    };
                })()
            ),
            Style.prototype.bsImmutable(
                'S', function (arg, i) {
                    var j = arg.length, k, v;
                    while (i < j) {
                        k = arg[i++];
                        if (i == j) return this.g(k);
                        v = this.s(k, arg[i++]);
                    }
                    return v;
                },
                'g', (function () {
                    var self, K, rn = /^([-]?[.0-9]+)([^-.0-9]*)$/g,
                        rne = function () { self[K] = parseFloat(arguments[1]), self.u[K] = arguments[2]; };
                    return function (k) {
                        var t0;
                        if (t0 = keys[k]) k = t0;
                        else if (t0 = keyF[k]) return t0(this);
                        else if (!(k = key(k))) return null;
                        if (!(k in this)) {
                            if (rn.test(t0 = this.st[k])) self = this, K = k, t0.replace(rn, rne);
                            else this[k] = t0.isNumber() ? parseFloat(t0) : t0, this.u[k] = '';
                        }
                        return this[k];
                    };
                })(),
                's', (function () {
                    var tester = docNew('div').style;
                    return function (k, v) {
                        var isStr, t0, s, u;
                        if (t0 = keys[k]) k = t0;
                        else if (t0 = keyF[k]) return t0(this, v);
                        else if (!(k = key(k))) return null;
                        s = this.st, u = this.u;
                        if (v === null && k in this) return delete this[k], delete u[k], s[k] = '', null;
                        if (isStr = typeof v == STR) {
                            if ((v = v.ex(this)).isNumber()) v = parseFloat(v), isStr = 0;
                            else if (t0 = vals[v.substr(0, 4)]) {
                                if ((v = t0(v)).isNumber()) isStr = 0;
                            } else if ((t0 = v.indexOf(':')) != NONE && v.charAt(t0 + 1) != '/') {
                                u[k] = v.substr(t0 + 1), v = parseFloat(v.substr(0, t0)), isStr = 0;
                            }
                        }
                        if (!(k in u)) {
                            if (isStr || nopx[k]) u[k] = '';
                            else {
                                tester[k] = '11px';
                                if (tester[k] == '11px') u[k] = 'px';
                                else u[k] = '', nopx[k] = 1;
                            }
                        }
                        if (!isStr || v.charAt(0) != '-') return s[k] = (this[k] = v) + u[k], v;
                        return null;
                    };
                })()
            ),
            fn('key', 'style', (function () {
                var arg = [];
                return function (self, v) {
                    var t0, t1, i, j;
                    v = v.split(';'), i = v.length;
                    while (i--) t0 = v[i], self.s(t0.substring(0, t1 = t0.indexOf(':')).trim(), t0.substr(t1 + 1).trim());
                };
            })()),
            fn('key', 'float', 'styleFloat' in body.style ? 'styleFloat' : 'cssFloat' in body.style ? 'cssFloat' : 'float'),
            (function () {
                var mode, transform, mx, i, t0, tkey = key('transform'), prefix = detect.stylePrefix;
                if (detect.device == 'pc') {
                    if (detect.browser == 'ie') mode = detect.browserVer > 9 ? 1 : detect.browserVer > 8 ? 1 : 0;
                    else mode = 2;
                } else {
                    mode = docNew('div');
                    mode.style['transform' in mode.style ? 'transform' : prefix + 'Transform'] = 'rotateX(0)';
                    mode = mode.style.cssText.indexOf('rotateX(') > -1 ? 1 : 1;
                }
                transform = function () { }, i = transform.prototype;
                i.length = i.tx = i.ty = i.tz = i.rx = i.ry = i.rz = 0, i.sx = i.sy = i.sz = 1,
                    i.txu = i.tyu = i.tzu = 'px', i.rxu = i.ryu = i.rzu = 'deg',
                    i.toString = mode == 2 ? function () {
                        return 'translateX(' + this.tx + this.txu + ') translateY(' + this.ty + this.tyu + ') translateZ(' + this.tz + this.tzu + ') ' +
                            'scaleX(' + this.sx + ') scaleY(' + this.sy + ') scaleZ(' + this.sz + ') ' +
                            'rotateX(' + this.rx + this.rxu + ') rotateY(' + this.ry + this.ryu + ') rotateZ(' + this.rz + this.rzu + ')';
                    } : mode ? function () {
                        return 'translate(' + this.tx + this.txu + ',' + this.ty + this.tyu + ') ' +
                            'scale(' + this.sx + ',' + this.sy + ') ' +
                            'rotate(' + this.rz + this.rzu + ')';
                    } : function () { return ''; },
                    mx = function (k) {
                        return function (self, v) {
                            var t0 = self.bsTrans || (self.bsTrans = new transform);
                            if (v === undefined) return t0[k];
                            else if (v === null) return delete t0[k], self.s[tkey] = t0, v;
                            else return t0[k] = v, self.s[tkey] = t0;
                        };
                    };
                for (i in (t0 = 'tx,ty,tz,sx,sy,sz,rx,ry,rz,txu,tyu,tzu,rxu,ryu,rzu'.split(','))) fn('key', t0[i], mx(t0[i]));
            })();
        if (!('opacity' in body.style)) {
            fn('key', 'opacity', function (self, v) {
                if (v === undefined) return self.opacity;
                else if (v === null) return delete self.opacity, style.filter = '', v;
                else return self.s.filter = 'alpha(opacity=' + parseInt(v * 100) + ')', self.opacity = v;
            }),
                fn('val', 'rgba', function (v) {
                    var t0 = v.substring(v.indexOf('(') + 1, v.indexOf(')')).split(',');
                    t0[3] = parseFloat(t0[3]);
                    return 'rgb(' + parseInt((255 + t0[0] * t0[3]) * .5) + ',' +
                        parseInt((255 + t0[1] * t0[3]) * .5) + ',' +
                        parseInt((255 + t0[2] * t0[3]) * .5) + ')';
                });
        }
    })();
    (function () {
        var CR = W['CSSRule'], id = 0, sheet, ruleSet, idx, add, del, Css,
            Font, KeyFrame;
        Css = function (key) {
            var t0, v;
            if (key.charAt(0) == '@') {
                if (key == '@font-face') this.type = 5, this.s = new Font;
                else if (!key.indexOf('@keyframes')) this.type = 7, this.s = new KeyFrame(key);
                else return;
            } else {
                this.type = 1,
                    this.r = add(t0, v),
                    this.s = new Style(this.r.style);
            }
        },
            Css.prototype.bsImmutable('S', function (v) {
                var type = this.type, t0, t1;
                if (v === null) {
                    if (type == 1) del(this.r);
                    else this.s.S(null);
                } else return this.s.S(v, 0);
            }),
            bs.bsImmutable(
                'css', (function () {
                    var VAR = {}, parser, p;
                    parser = (function () {
                        var rc = /\/\*(?:.|[\r\n])*?\*\//g,
                            r0 = /[-][-][a-zA-Z][a-zA-Z0-9]+/g, rf = function (v) { return VAR[v]; };
                        return function (v) {
                            var t0, prefix, body, i, j, k, l;
                            v = v.replace(rc, '').trim();
                            if (v.indexOf('}}') > -1) {
                                for (v = v.split('}}'), i = 0, j = v.length; i < j; i++) {
                                    t0 = v[i],
                                        prefix = t0.substring(0, k = t0.indexOf('{{'));
                                    if ((l = prefix.lastIndexOf('}')) > -1) p(prefix.substring(0, l + 1), ''), prefix = prefix.substr(l + 1);
                                    prefix = prefix.trim(),
                                        body = t0.substr(k + 2).trim();
                                    if (prefix.substr(0, 2) == '--') VAR[prefix] = body;
                                    else p(body.replace(r0, rf), prefix + (prefix ? ' ' : ''));
                                }
                            } else p(v, '');
                        };
                    })(),
                        p = (function () {
                            var arg = [], c = docNew('div').style,
                                css = function () {
                                    c.cssText = '', (new Style(c)).S(arg, 0);
                                    return c.cssText;
                                };
                            return function (v, prefix) {
                                var w = '', s, b, t0, t1, i, j, k, l, m;
                                for (v = v.split('}'), i = 0, j = v.length; i < j; i++) if (t0 = v[i].trim()) {
                                    s = t0.substring(0, k = t0.indexOf('{')).trim(), b = t0.substr(k + 1);
                                    for (t0 = b.split(';'), arg.length = k = 0, l = t0.length; k < l; k++) {
                                        t1 = t0[k], arg[arg.length] = t1.substring(0, m = t1.indexOf(':')).trim(),
                                            arg[arg.length] = (t1 = t1.substr(m + 1).trim()).isNumber() ? parseFloat(t1) : t1;
                                    }
                                    if (s.indexOf('@') == NONE) {
                                        w += prefix + s.split(',').trim().join(',' + prefix) + '{' + css() + '}\n';
                                    } else new Css(s).S(v);
                                }
                                if (w) {
                                    head.appendChild(t0 = docNew('style')),
                                        t0['styleSheet'] ? (t0['styleSheet'].cssText = w) : (t0.innerHTML = w);
                                }
                            };
                        })();
                    return function (v) {
                        v.substr(v.length - 4) == '.css' ? bs.get(parser, v) :
                            parser(v);
                    };
                })()),

            head.appendChild(sheet = docNew('style')),
            sheet.id = 'utCss' + (id++),
            sheet = sheet.styleSheet || sheet.sheet,
            ruleSet = sheet.cssRules || sheet.rules,
            idx = function (rs, rule) {
                var i, j, k, l;
                for (i = 0, j = rs.length, k = parseInt(j * .5) + 1, j--; i < k; i++) {
                    if (rs[l = i] === rule || rs[l = j - i] === rule) return l;
                }
                return -1;
            },
            add = sheet['insertRule'] ? function (k) { return sheet.insertRule(k + '{}', ruleSet.length); } :
                function (k, v) { return sheet.addRule(k, ' '), ruleSet[ruleSet.length - 1]; },
            del = sheet['deleteRule'] ? function (v) { sheet.deleteRule(idx(ruleSet, v)); } :
                function (v) { sheet.removeRule(idx(ruleSet, v)); },
            KeyFrame = function (k) {
                if (CR) {
                    if (!CR.KEYFRAME_RULE) {
                        k = k.substr(k.indexOf(' ') + 1);
                        if (CR.WEBKIT_KEYFRAME_RULE) k = '@-webkit-keyframes ' + k;
                        else if (CR.MOZ_KEYFRAME_RULE) k = '@-moz-keyframes ' + k;
                        else return err('Css0', k);
                    }
                    this.__r__ = add(k);
                }
                return err('Css1', k);
            },
            KeyFrame.prototype.S = function (a) {
                var t0, k, i, j;
                if (a === null) return del(this.r);
                k = a[0], i = 1, j = a.length;
                if (!this[k] && a[1] !== null) {
                    this.__r__.insertRule(k + '{}'),
                        this[k] = { r: t0 = this.__r__.cssRules[this.__r__.cssRules.length - 1], s: Style(t0.style) };
                }
                if (a[1] === null) {
                    if (this[k]) {
                        this.__r__.deleteRule(idx(this.__r__.cssRules, this[k].r)),
                            delete this[k];
                    }
                    return;
                }
                return this[k].s.S(arguments, 1);
            },
            Font = function () { },
            Font.prototype.S = function (a) {
                var src, local, t0, i, j;
                if (a === null) {
                    if (this.updated) head.removeChild(docId('utCss' + this.updated));
                    return;
                }
                i = 0, j = a.length;
                while (i < j) {
                    k = a[i++];
                    if (i == j) return this[k];
                    v = a[i++];
                    if (v === null) delete this[k];
                    else this[k] = v;
                }
                if (this['font-family'] && this.src) {
                    if (this.updated) head.removeChild(docId('utCss' + this.updated));
                    src = this.src.split(','), i = src.length, local = [];
                    while (i--) {
                        src[i] = src[i].trim();
                        if (!src[i].indexOf('local(')) local[local.length] = src[i];
                        else switch (src[i].substr(src[i].length - 4)) {
                            case '.eot': src.eot = src[i]; break;
                            case 'woff': src.woff = src[i]; break;
                            case '.ttf': src.ttf = src[i]; break;
                            case '.svg': src.svg = src[i]; break;
                            default:
                                src.eot = src[i] + '.eot',
                                    src.woff = src[i] + '.woff',
                                    src.ttf = src[i] + '.ttf',
                                    src.svg = src[i] + '.svg';
                        }
                    }
                    j = (local.length ? local.join(',') + ',' : '') +
                        (src.eot ? detect.browser == 'ie' ? "url('" + src.eot + "?#iefix') format('embedded-opentype')," : "url('" + src.eot + "')," : '') +
                        (src.woff ? "url('" + src.woff + "')," : '') +
                        (src.ttf ? "url('" + src.ttf + "')  format('truetype')," : '') +
                        (src.svg ? "url('" + src.svg + "')  format('svg')," : '');
                    if (j.charAt(j.length - 1) == ',') j = j.substring(0, j.length - 1);
                    t0 = docNew('style'),
                        t0.id = 'utCss' + (this.updated = id++),
                        head.appendChild(t0),
                        (t0.styleSheet || t0.sheet).cssText = '@font-face{font-family:' + this['font-family'] + ';src:' + j + '}';
                }
                return v;
            };
    })();
    if (doc) (function () {
        domGroup = function (el) {
            var el0, group;
            if (group = el.bsG) return group;
            el.S = domS, el0 = el;
            do {
                if (group = el0.getAttribute(ns + 'group')) return el.bsG = group;
            } while ((el0 = el0.parentNode) && el0.nodeType == 1);
            return el.bsG = '__root__';
        },
            bs.bsImmutable(
                'getId', docId,
                'size', (function () {
                    var v = {}, ow, oh, w, h;
                    if (W['outerWidth']) {
                        return function () {
                            v.outerWidth = W.outerWidth,
                                v.outerHeight = W.outerHeight,
                                v.innerHeight = W.innerHeight;
                            if (W.outerWidth - W.innerWidth < 40) v.innerWidth = W.innerWidth;
                            else v.innerWidth = v.outerWidth - (detect.device == 'pc' && (docel.scrollHeight || (doc.body ? doc.body.scrollHeight : 0)) - (docel.offsetHeight || (doc.body ? doc.body.offsetHeight : 0)) ? 17 : 0) - 16;
                            return v;
                        };
                    } else {
                        ow = docel.offsetWidth, oh = docel.offsetHeight;
                        W.resizeTo(500, 500),
                            w = 500 - docel.offsetWidth, h = 500 - docel.offsetHeight,
                            W.resizeTo(w + ow, h + oh);
                        return function () {
                            v.outerWidth = docel.offsetWidth + w,
                                v.outerHeight = docel.offsetHeight + h,
                                v.innerWidth = docel.clientWidth || doc.body.clientWidth,
                                v.innerHeight = docel.clientHeight || doc.body.clientHeight;
                            return v;
                        };
                    }
                })(),
                'scroll', (function () {
                    var v = {};
                    return function () {
                        v.left = docel.scrollLeft || W.pageXOffset || 0,
                            v.top = docel.scrollTop || W.pageYOffset || 0,
                            v.height = doc.body.scrollHeight,
                            v.width = doc.body.scrollWidth;
                        return v;
                    };
                })(),
                'marker', function (v) {
                    var r = doc.getElementsByTagName('b'), el, els, i, j;
                    for (els = [], i = 0, j = r.length; i < j; i++) {
                        if (!v || r[i].getAttribute('data-marker') == v) {
                            els[els.length] = {
                                key: v,
                                el: r[i].nextElementSibling,
                                data: r[i].getAttribute('data-data')
                            };
                        }
                    }
                    return els;
                },
                'domS', domS = (function () {
                    var html, del, insertBefore, util, x, y;
                    del = function (el) {
                        if (el.parentNode) {
                            el.parentNode.removeChild(el);
                            if ('outerHTML' in el && el.parentNode) el.outerHTML = '';
                        }
                    },
                        html = (function () {
                            var div = doc.createElement('div'), tbody = doc.createElement('tbody'),
                                tags = {
                                    tr: [1, '<table><tbody>', '</tbody></table>'], th: [2, '<table><tbody><tr>', '</tr></tbody></table>'],
                                    col: [1, '<table><tbody></tbody><colgroup>', '</colgroup></table>'], option: [0, '<select>', '</select>']
                                }, t0, i;
                            tags.td = tags.th, tags.optgroup = tags.option,
                                t0 = 'thead,tfoot,tbody,caption,colgroup'.split(','), i = t0.length;
                            while (i--) tags[t0[i]] = [0, '<table>', '</table>'];
                            return function (str, target, mode) {
                                var t0, t1, t2, t3, i, j, n0, n1, n2, parent, tbodyStr;
                                str += '',
                                    tbodyStr = str.toLowerCase().indexOf('tbody') > -1 ? true : false,
                                    t0 = str.trim(), n0 = t0.indexOf(' '), n1 = t0.indexOf('>'), n2 = t0.indexOf('/'),
                                    t1 = (n0 != -1 && n0 < n1) ? t0.substring(1, n0) : (n2 != -1 && n2 < n1) ? t0.substring(1, n2) : t0.substring(1, n1),
                                    t1 = t1.toLowerCase();
                                if (mode == 'html' && target.nodeName.toLowerCase() == 'table' && t1 == 'tr') tbodyStr = true, t1 = 'tbody';
                                if (mode == '>' || 'html+' && t1 == 'tr' && target) target = target.getElementsByTagName('tbody')[0] || (target.appendChild(tbody), target.getElementsByTagName('tbody')[0]);
                                if (tags[t1]) {
                                    if (div.innerHTML) del(div.childNodes);
                                    div.innerHTML = tags[t1][1] + str + tags[t1][2], t2 = div.childNodes[0];
                                    if (tags[t1][0]) for (i = 0; i < tags[t1][0]; i++) t2 = t2.childNodes[0];
                                    parent = t2;
                                } else div.innerHTML = str, parent = div;
                                i = parent.childNodes.length;
                                if (!target) return parent.childNodes;
                                else if (mode == 'html') {
                                    if (target.innerHTML) del(target.childNodes);
                                    while (i--) target.appendChild(parent.childNodes[0]);
                                } else if (mode == 'html+') while (i--) target.appendChild(parent.childNodes[0]);
                                else if (mode == '+html') {
                                    i = target.childNodes.length, t0 = { length: i };
                                    while (i--) t0[i] = target.childNodes[i];
                                    for (i = 0, j = parent.childNodes.length; i < j; i++) target.appendChild(parent.childNodes[0]);
                                    for (i = 0, j = t0.length; i < j; i++) target.appendChild(t0[i]);
                                }
                                else while (i--) target.appendChild(parent.childNodes[0]);
                                j = target.childNodes.length;
                                while (j--) if (target.childNodes[j].nodeType == 1 && target.childNodes[j].nodeName == 'TBODY' && !target.childNodes[j].childNodes.length && !tbodyStr) target.removeChild(target.childNodes[j]);
                                return target.innerHTML || target;
                            };
                        })(),
                        insertBefore = function (parent, child, n) {
                            var t0, cnt, i, j;
                            for (t0 = parent.childNodes, cnt = i = 0, j = t0.length; i < j; i++) {
                                if (t0[i].nodeType == 1) {
                                    if (cnt == n) {
                                        parent.insertBefore(t0[i], child);
                                        break;
                                    }
                                    cnt++;
                                }
                            }
                        },
                        util = {
                            '@': (function () {
                                var key = {};
                                return function (el, k, v) {
                                    k = k.substr(1);
                                    if (v === undefined) return el[k] || (k != 'value' && el.getAttribute(k)) || null;
                                    if (v === null) {
                                        el.removeAttribute(k);
                                        try { delete el[k]; } catch (e) { };
                                    } else if (el) el[k] = v, el.setAttribute(k, v);
                                    return v;
                                };
                            })(),
                            '*': function (el, k, v) {
                                k = 'data-' + k.substr(1);
                                if (v === undefined) return el.getAttribute(k);
                                if (v === null) el.removeAttribute(k);
                                else el.setAttribute(k, v);
                                return v;
                            },
                            '_': (function () {
                                var view = doc.defaultView;
                                return view && view.getComputedStyle ? function (el, k) {
                                    return view.getComputedStyle(el, '').getPropertyValue(k.substr(1));
                                } : function (el, k) {
                                    return el.currentStyle[Style.key(k.substr(1))];
                                };
                            })(),
                            '<': function (el, k, v) {
                                var t0, cnt, i, j;
                                if (v === undefined) return el.parentNode;
                                if (v === null) del(el);
                                else {
                                    if (v.nodeType != 1) return;
                                    if ((k = k.substr(1)) && k.isNumber()) insertBefore(v, el, parseInt(k, 10));
                                    else v.appendChild(el);
                                }
                            },
                            '>': (function () {
                                var children, nodes = [], r = /^[0-9]+$/;
                                children = doc.createElement('div').children ? function (el) {
                                    return el.children;
                                } : function (el) {
                                    var result = { length: 0 }, i, j;
                                    for (el = el.childNodes, i = 0, j = el.length; i < j; i++) {
                                        if (el[i].nodeType == 1) result[result.length++] = el[i];
                                    }
                                    return result;
                                }
                                return function (el, k, v) {
                                    var t0, t1, i, j;
                                    k = k.substr(1);
                                    if (v === undefined) {
                                        t0 = children(el);
                                        if (k == '$') return t0[t0.length - 1];
                                        else if (k.isNumber()) return t0[k];
                                        else if (k) {
                                            return el.querySelectorAll(k);
                                        }
                                        return t0;
                                    }
                                    if (v === null) {
                                        if (k.isNumber()) del(children(el)[k]);
                                        else {
                                            t0 = el.childNodes, i = t0.length;
                                            while (i--) del(t0[i]);
                                        }
                                    } else if (v = typeof v == STR ? html(sel) : v.nodeType == 1 || v.nodeType == 11 ? v : 0) {
                                        if (k.isNumber()) insertBefore(el, v, parseInt(k, 10));
                                        else el.appendChild(v);
                                    } else if (ev.nodeName == 'TABLE' && typeof v == STR) return html(v, el, '>');
                                };
                            })(),
                            pageX: x = function (el) { var i = 0; do i += el.offsetLeft; while (el = el.offsetParent); return i; },
                            pageY: y = function (el) { var i = 0; do i += el.offsetTop; while (el = el.offsetParent); return i; },
                            clientX: function (el) { return x(el) - x(el.parentNode); },
                            clientY: function (el) { return y(el) - y(el.parentNode); },
                            submit: function (el) { el.submit(); },
                            focus: function (el) { el.focus(); },
                            blur: function (el) { el.blur(); },
                            group: function (el) {
                                do {
                                    if (el[ns + 'group']) return el;
                                } while (el = el.parentNode)
                                return doc.body;
                            },
                            html: function (el, k, v) { return v === undefined ? el.innerHTML : el.innerHTML = v; },//html(v, el, 'html');
                            'html+': function (el, k, v) { return html(v, el, 'html+'); },
                            '+html': function (el, k, v) { return html(v, el, '+html'); },
                            'class': function (el, k, v) { return v === undefined ? el.className : (el.className = v); },
                            'class+': function (el, k, v) {
                                var i;
                                return !(i = el.className.trim()) ? (el.className = v) : i.split(' ').indexOf(v) == NONE ? (el.className = v + ' ' + i) : i;
                            },
                            'class-': function (el, k, v) {
                                var t0 = el.className.trim(), i;
                                if (!t0) return;
                                t0 = t0.split(' ');
                                if ((i = t0.indexOf(v)) != NONE) t0.splice(i, 1);
                                el.className = (t0 = t0.join(' ').trim()) ? t0 : '';
                            },
                            checked: function (el, k, v) {
                                if (v === undefined) return el.checked;
                                el.checked = v ? true : null;
                            },
                            selected: function (el, k, v) {
                                if (v === undefined) return el.selected;
                                el.selected = v ? true : null;
                            }
                        };
                    return function () {
                        var prefix, a = arguments, i = 0, j = a.length, k, v, f, info, self;
                        if (a[0] === null) return del(this);
                        if (a[0].nodeType == 1) self = a[0], i = 1;
                        else self = this;
                        if (a[i] instanceof Array || Object.prototype.toString.call(a[i]) == '[object Arguments]') a = a[i], i = 0, j = a.length;
                        while (i < j) {
                            if (!attr[k = a[i++]]) {
                                if (k == 'this') attr[k] = THIS;
                                else if (v = (util[k] || util[k.charAt(0)])) attr[k] = v;
                                else Style.key(k);
                            }
                            f = attr[k];
                            if (i == j) switch (f) {
                                case STYLE: return (self.bsStyle || (self.bsStyle = new Style(self.style))).g(k);
                                case THIS: return self;
                                default: return f != NONE ? f(self, k) : null;
                            }
                            v = a[i++];
                            if (typeof v == STR) {
                                v = v.ex();
                                if (v.isNumber()) v = parseFloat(v);
                                if (v == 'null') v = null;
                            }
                            if (f == STYLE) (self.bsStyle || (self.bsStyle = new Style(self.style))).s(k, v);
                            else if (f != NONE) f(self, k, v);
                        }
                        return v;
                    };
                })()
            );
    })();
    bs.bsImmutable('systemEvent', function () {
        var a = arguments, i = 0, j = a.length, k, v, s;
        while (i < j) k = a[i++], v = a[i++], s = sys.sysEv[k] || (sys.sysEv[k] = []), s[s.length] = v, bs.on(k);
    });
    (function () {
        var ev, attach = {}, //IE 중복 리스너
            on = eOn = W['addEventListener'] ? function (el, type, listener) { el.addEventListener(type, listener); } :
                W['attachEvent'] ? function (el, type, listener) {
                    var i, j, t0;
                    if (!attach[type]) attach[type] = { el: [], li: [] };
                    for (t0 = attach[type], i = 0, j = t0.el.length; i < j; i++) if (t0.el[i] == el && t0.li[i] == listener) return;
                    t0.el.push(el), t0.li.push(listener), el.attachEvent('on' + type, listener);
                } : function (el, type, listener) { el['on' + type] = listener; },
            off = eOff = W['removeEventListener'] ? function (el, type, listener) { el.removeEventListener(type, listener); } :
                W['detachEvent'] ? function (el, type, listener) {
                    var i, j, t0;
                    if (t0 = attach[type]) {
                        i = t0.el.length;
                        while (i--) if (t0.el[i] == el && t0.li[i] == listener) t0.el.splice(i, 1), t0.li.splice(i, 1);
                    }
                    el.detachEvent('on' + type, listener);
                } : function (el, type, listener) { el['on' + type] = null; },
            e2v = {}, v2e = {}, scrollState = 0,
            delay = { touchmove: 1, mousemove: 1, resize: 1 },//, scroll:1},
            throttleEvent = (function () {
                var wrap = {}, E, etype = {}, mk = function (type) {
                    var f = function (t) { handleEvent(f.e); };
                    return f;
                };
                return function (e) {
                    var v, type = e.type;
                    if (v = etype[type]) cancelAnimationFrame(v);
                    v = wrap[type] || (wrap[type] = mk(type));
                    v.e = e, etype[type] = requestAnimationFrame(v);
                };
            })(),
            bindTarget = (function () {
                var bind = { click: doc };
                return function (type) { return bind[type] || ('on' + type in W ? W : 'on' + type in doc ? doc : 0); };
            })(),
            handleEvent = (function () {
                var cache = {}, ls = {},
                    KEY = (function () {
                        var t0 = { downed: {}, code2name: {}, name2code: {} },
                            t1 = 'a,65,b,66,c,67,d,68,e,69,f,70,g,71,h,72,i,73,j,74,k,75,l,76,m,77,n,78,o,79,p,80,q,81,r,82,s,83,t,84,u,85,v,86,w,87,x,88,y,89,z,90,back,8,tab,9,enter,13,shift,16,control,17,alt,18,pause,19,caps,20,esc,27,space,32,pageup,33,pagedown,34,end,35,home,36,left,37,up,38,right,39,down,40,insert,45,delete,46,numlock,144,scrolllock,145,0,48,1,49,2,50,3,51,4,52,5,53,6,54,7,55,8,56,9,57'.split(','),
                            i = 0, j = t1.length, k, v;
                        while (i < j) t0.name2code[k = t1[i++]] = v = parseInt(t1[i++]), t0.code2name[v] = k;
                        return t0;
                    })(), keyName = KEY.code2name, keyCode = KEY.name2code,
                    posCat = {
                        down: detect.device == 'pc' ? 4 : 1, move: detect.device == 'pc' ? 5 : 2, up: detect.device == 'pc' ? 6 : 3,
                        touchstart: 1, touchmove: 2, touchend: 3,
                        mousedown: 4, mousemove: 5, mouseup: 6, click: 6, mouseover: 6, mouseout: 6
                    };
                bs.bsImmutable('KEY', KEY);
                ev = { __pos__: 0 };
                ev.bsImmutable(
                    'wheelDelta', function () {
                        var e = this.event, d = e.detail, w = e['wheelDelta'] ? e.wheelDelta : -e.deltaY * 20, n = 225, n1 = n - 1;
                        d = d ? w && (f = w / d) ? d / f : -d / 1.35 : w / 120;
                        d = d < 1 ? d < -1 ? (-d * d - n1) / n : d : (d * d + n1) / n / 2;
                        if (d < -1) d = -1;
                        else if (d > 1) d = 1;
                        return d;
                    },
                    'data', function (k, v) {
                        if (v === null) return this.target.removeAttribute('data-' + k);
                        if (v !== undefined) this.target.setAttribute('data-' + k, v);
                        return this.target.getAttribute('data-' + k);
                    },
                    'key', function (k) {
                        var e = this.event;
                        switch (k) {
                            case 'ctrl': return e.metaKey || e.ctrlKey;
                            case 'shift': return e.shiftKey;
                            case 'button': return e.button;
                            default: return this.keyCode == keyCode[k];
                        }
                    },
                    'isLeave', function () {
                        var pos = this.pos(), el;
                        if (arguments.length) pos = pos.touches[arguments[0]];
                        if (this.target.scrollState != scrollState + '') return true;
                        if (pos.distanceX < -5 || pos.distanceX > 5 || pos.distanceY < -5 || pos.distanceY > 5) return true;
                        if (el = doc.elementFromPoint(pos.pageX - (docel.scrollLeft || W.pageXOffset || 0), pos.pageY - (docel.scrollTop || W.pageYOffset || 0))) {
                            do {
                                if (this.target == el) return false;
                            } while (el = el.parentNode)
                            return true;
                        }
                        return true;
                    },
                    'group', (function () {
                        var stack = { length: 0 }, groups = { '__root__': { el: doc.body, find: {}, keys: {} } },
                            init = function () {
                                var t = ev.groupTarget, i, j, k;
                                if (t.__group__) return t.__group__;
                                i = t;
                                do {
                                    if (i.attributes[ns + 'group']) return t.__group__ = { el: (i.S = domS, i), find: {}, keys: {} };
                                } while ((i = i.parentNode) && i.nodeType == 1);
                                return t.__group__ = groups.__root__;
                            };
                        return {}.bsImmutable(
                            'el', function () { return init().el; },
                            'S', function () { return domS(init().el, arguments); },
                            'info', function () { return init().el.bsGroup || {}; },
                            'record', function () { return init().el.bsGroup.record || {}; },
                            'data', function (k, v) {
                                var el = init().el;
                                if (v === null) return el.removeAttribute('data-' + k);
                                if (v !== undefined) el.setAttribute('data-' + k, v);
                                return el.getAttribute('data-' + k);
                            },
                            'key', function (key) {
                                var group = init(), k = group.name + ':' + key, r, keys = group.keys;
                                if (r = keys[k], !r) keys[k] = r = this.find('key', key);
                                return r;
                            },
                            'find', function (k) {
                                var group, el, i, v, v0, t0;
                                group = init(), i = k.charAt(0) == '@' ? (k = k.substr(1), 1) : 0, v = arguments[1];
                                if (!i && (el = group.find[k + ':' + v])) return el;
                                el = group.el.firstElementChild, i = stack.length = 0;
                                do {
                                    if (!el.attributes[ns + 'group']) {
                                        if ((v0 = el.getAttribute(ns + k)) && !v || v0 == v) return el.S = domS, group.find[k + ':' + v] = el;
                                        if (t0 = el.firstElementChild) stack[stack.length++] = t0;
                                    }
                                    if (t0 = el.nextElementSibling) stack[stack.length++] = t0;
                                } while (stack.length && (el = stack[--stack.length]))
                                return null;
                            },
                            'parent', function () {
                                ev.groupTarget = init().el.parentNode;
                                return this;
                            },
                            'prev', function () {
                                ev.groupTarget = init().el.previousSibling;
                                return this;
                            },
                            'next', function () {
                                ev.groupTarget = init().el.nextSibling;
                                return this;
                            }
                        );
                    })(),
                    'pos', (function () {
                        var client, pos, pageX, pageY;
                        client = function (k, prop, off, scr) {
                            return function (v) {
                                var rect;
                                if (this.length) {//터치일때
                                    if (!v) v = 0;
                                    rect = this.touches[v].target.getBoundingClientRect();
                                    this.touches[v][k] - rect[prop] - (W[off] || docel[scr] || 0);
                                } else {
                                    rect = ev.target.getBoundingClientRect();
                                    return ev.event[k] - rect[prop] - (W[off] || docel[scr] || 0);
                                }
                            };
                        },
                            pos = {},
                            pos.bsImmutable(
                                'touches', [],
                                'clientX', client('clientX', 'left', 'pageXOffset', 'scrollLeft'),
                                'clientY', client('clientY', 'top', 'pageYOffset', 'scrollTop')
                            );
                        if (detect.browser != 'ie' || detect.browserVer > 8) pageX = 'pageX', pageY = 'pageY';
                        else pageX = 'clientX', pageY = 'clientY';
                        return function () {
                            var type, e, X, Y, id, t0, t1, t2, i, j, k, m;
                            if (this.__pos__) return pos;
                            this.__pos__ = 1;
                            if (type = posCat[this.type] || posCat[v2e[this.type]]) {
                                e = this.event, t0 = pos.touches, t0.length = 0;
                                if (type < 4) {
                                    t1 = '', i = 2;
                                    while (i--) {
                                        t2 = i ? e.changedTouches : e.touches, j = t2.length;
                                        while (j--) {
                                            id = t2[j].identifier, t1 += id + ' ', m = t0.length, k = 1;
                                            while (m--) if (t0[m].identifier == id) {
                                                k = 0;
                                                break;
                                            }
                                            if (k) t0[t0.length] = t2[j];
                                        }
                                    }
                                    i = t0.length;
                                    while (i--) {
                                        if (t1.indexOf(t0[i].identifier) == NONE) t0.splice(i, 1);
                                        else {
                                            t1 = t0[i], X = t1.pageX, Y = t1.pageY;
                                            if (type == 1) t1.startX = X, t1.startY = Y;
                                            else {
                                                t1.distanceX = X - t1.startX, t1.distanceY = Y - t1.startY,
                                                    t1.moveX = X - t1.oldX, t1.moveY = Y - t1.oldY;
                                            }
                                            t1.oldX = X, t1.oldX = Y;
                                        }
                                    }
                                    if (t1 = t0[0]) {
                                        pos.pageX = t1.pageX, pos.pageY = t1.pageY,
                                            pos.distanceX = t1.distanceX, pos.distanceY = t1.distanceY,
                                            pos.moveX = t1.moveX, pos.moveY = t1.moveY;
                                    }
                                } else {
                                    pos.pageX = X = e[pageX], pos.pageY = Y = e[pageY];
                                    if (type == 4) pos.startX = X, pos.startY = Y;
                                    else {
                                        pos.distanceX = X - pos.startX, pos.distanceY = Y - pos.startY,
                                            pos.moveX = X - pos.oldX, pos.moveY = Y - pos.oldY;
                                    }
                                    pos.oldX = X, pos.oldY = Y;
                                }
                                if (type == 1 || type == 4) this.target.scrollState = scrollState;
                            }
                            return pos;
                        };
                    })()
                );
                return function (e) {
                    var type0, type, el, a, i, j, k;
                    if (!e) e = event;
                    el = e.target || e.srcElement, type = e.type;
                    if (!el) return console.log('no e.target,e.srcElement');
                    if (el == W || el === doc) {
                        if (k = sys.sysEv[type]) {
                            ev.event = e, ev.target = el, ev.type = type;
                            for (i = 0, j = k.length; i < j; i++) if (k[i].call(W, ev)) break;
                        }
                    } else do {
                        a = el.attributes, type0 = 0;//이벤트에 해당되는 키확인(가상키포함) : data-click, data-down(가상)
                        if (k = a[ns + type] || (type0 = e2v[type]) && a[ns + type0]) {
                            ev.event = e, ev.type = type, ev.keyName = keyName[ev.keyCode = e.keyCode];
                            ev.groupTarget = ev.target = el, ev.__group__ = domGroup(el), ev.__pos__ = 0;
                            if (posCat[type] == 1 || posCat[type] == 4) ev.pos();
                            k = k.value;
                            if (k = ls[k] || (ls[k] = bs(k))) k.call(el, ev);
                            return;
                        }
                        if (a[ns + 'group']) return;
                    } while ((el = el.parentNode) && el.nodeType == 1);
                };
            })();
        bs.bsImmutable(
            'dom2e', function (el, type) {
                if (typeof el == 'string') el = bs.getId(el);
                ev.groupTarget = ev.target = el,
                    ev.type = type,
                    ev.__group__ = domGroup(el),
                    ev.__pos__ = 0;
                return ev;
            },
            'on', function () {
                var a = arguments, i = a.length, k, type;
                while (i--) {
                    type = a[i], type = v2e[type] || type;
                    if (typeof type == FUN) type(1);
                    else if (k = bindTarget(type)) on(k, type, delay[a[i]] ? throttleEvent : handleEvent);
                }
            },
            'off', function () {
                var a = arguments, i = a.length, k, type;
                while (i--) {
                    type = a[i], type = v2e[type] || type;
                    if (typeof type == FUN) type();
                    if (k = bindTarget(type)) off(k, type, delay[a[i]] ? throttleEvent : handleEvent);
                }
            }),
            (function () {
                var downed = bs.KEY.downed, code2name = bs.KEY.code2name;
                bs.systemEvent(
                    'keydown', function (e) { downed[code2name[e.keyCode]] = 1; },
                    'keyup', function (e) { downed[code2name[e.keyCode]] = 0; }
                );
                bs.on('keydown', 'keyup');
            })(),
            (function () {
                var f = function () {
                    var a = arguments, i = 0, j = a.length, k, v;
                    while (i < j) k = a[i++], v = a[i++], v2e[k] = v, e2v[v] = k;
                };
                if (detect.device == 'pc') f('down', 'mousedown', 'up', 'mouseup', 'move', 'mousemove', 'over', 'mouseover', 'out', 'mouseout');
                else f('down', 'touchstart', 'up', 'touchend', 'move', 'touchmove', 'over', 'mouseover', 'out', 'mouseout');
                f('wheel', 'DOMMouseScroll' in body ? 'DOMMouseScroll' : 'onwheel' in body ? 'wheel' : 'mousewheel');
            })();
        if (!W['onorientationchange']) (function () {
            var listener = function (e) { handleEvent(E); }, E = { target: W, type: 'orientationchange' };
            v2e['orientationchange'] = function (isOn) { (isOn ? on : off)(W, 'resize', listener); };
        })();
        if (!W['onhashchange']) v2e['hashchange'] = (function () {
            var on, old, E = { target: W, type: 'hashchange' },
                f = function () {
                    if (!on) return;
                    if (old != location.hash) old = location.hash, handleEvent(E);
                    requestAnimationFrame(f);
                };
            return function (isOn) { if (on = isOn) old = location.hash, requestAnimationFrame(f); };
        })();
        if (!('onscroll' in W)) v2e['scroll'] = (function () {
            var on, oldX, oldY, E = { target: W, type: 'hashchange' },
                f = function () {
                    var x, y;
                    if (!on) return;
                    x = W.pageXOffset || docel.scrollLeft || 0, y = W.pageYOffset || docel.scrollTop || 0;
                    if (oldX != x || oldY != y) oldX = x, oldY = y, handleEvent(E);
                    requestAnimationFrame(f);
                };
            return function (isOn) { if (on = isOn) oldX = W.pageXOffset || docel.scrollLeft || 0, oldY = W.pageYOffset || docel.scrollTop || 0, requestAnimationFrame(f); };
        })();
        bs.systemEvent('scroll', function (e) { scrollState++; });
    })();
    (function () {
        var RULESET = {}, RULE = {}, FILTER = {}, MSG = {}, VALI = {}, RuleSet, Rule, Vali;
        Rule = (function () {
            var Rule = function Rule(check) { this.bsImmutable('_check', check); };
            Rule.prototype.bsImmutable(
                'check', function (v, safe, msg) { return this._check(v, this._param, safe) || msg(typeof this._msg == 'function' ? this._msg(v, this._param, safe) : this._msg); }
            );
            return Rule;
        })(),
            RuleSet = (function () {
                var msg = function (v) { return msg.msg = v, 0; },
                    sep = /[&|]/g, rmsg = /@value@/gi,
                    RuleSet = function () { };
                RuleSet.prototype.bsImmutable(
                    'rule', function (a) {
                        var rules = [], arg, r, i, j, k, v;
                        while ((i = a.search(sep)) != NONE || (i = a.length)) {
                            r = a.substring(0, i);
                            if ((k = r.indexOf('(')) > -1) {
                                arg = r.substring(k + 1, r.length - 1).split(','),
                                    j = arg.length;
                                while (j--) {
                                    arg[j] = v = arg[j].trim();
                                    if (v.isNumber()) arg[j] = parseFloat(v);
                                }
                                r = r.substr(0, k);
                            }
                            k = r.indexOf(':'),
                                rules[rules.length] = v = RULE[r.substr(0, k)](),
                                v._msg = MSG[r.substr(k + 1)] || r.substr(k + 1), v._param = arg || null;
                            k = a.charAt(i);
                            if (k == '&' || k == '|') rules[rules.length] = k;
                            a = a.substr(i + 1);
                        }
                        this._rules = rules;
                        return this;
                    },
                    'check', function (result, safe) {
                        var t0 = this._rules, r, k, v, i = 0, j = t0.length;
                        v = result.value, msg.msg = '';
                        while (i < j) {
                            if (r = t0[i++].check(v, safe, msg)) {
                                result.msg = 'ok', v = r;
                            } else {
                                result.msg = msg.msg || 'invalid value:' + result.key + ' = ' + k,
                                    result.ok = 0;
                            }
                            if (i < j && t0[i++] == '|' && !r) result.ok = 1;
                            if (!result.ok) return result;
                        }
                        result.value = safe[result.key] = v;
                    }
                );
                return RuleSet;
            })(),
            Vali = (function () {
                var Vali = function () { this.r = {}, this._rules = {}; };
                Vali.prototype.bsImmutable(
                    'ruleSet', function (k, v) { this._rules[k] = v; },
                    'validate', function (data, isPartial) {
                        var rules, result, values, ok, ruleSet, r, i, j, k, v
                        rules = this._rules, result = {}, values = {}, ok = 1;
                        if (isPartial) {
                            for (k in data) if (data.hasOwnProperty(k)) {
                                rules[k].check(result[k] = r = { key: k, value: data[k], ok: 1 }, values);
                                if (!r.ok) ok = 0;
                            }
                        } else {
                            for (k in rules) if (rules.hasOwnProperty(k)) {
                                result[k] = r = { key: k, ok: 1 };
                                if (k in data) r.value = data[k], rules[k].check(r, values);
                                else r.ok = 0, r.msg = 'no data:' + k;
                                if (!r.ok) ok = 0;
                            }
                        }
                        return this.r.ok = ok, this.r.result = result, this.r.values = values, this.r;
                    }
                );
                return Vali;
            })(),
            bs.bsImmutable(
                'OR', '||',
                'rule', (function () {
                    var mk = function (k) { return function () { return new Rule(k); }; };
                    return function () {
                        var a = arguments, i, j;
                        i = 0, j = a.length;
                        while (i < j) RULE[a[i++]] = mk(a[i++]);
                    };
                })(),
                'msg', function () {
                    var a = arguments, i = 0, j = a.length;
                    while (i < j) MSG[a[i++]] = a[i++];
                },
                'valiApi', function (body, data) {
                    var bstack = [], dstack = [], result = {}, cbody, cdata, k, i;
                    do {
                        for (k in body) if (body.hasOwnProperty(k)) {
                            cbody = body[k];
                            cdata = data[k];
                            if (cbody === null) {
                                if (!data.hasOwnProperty(k)) {
                                    err('undefined key - ' + k);
                                    throw 1;
                                }
                            } else if (typeof cbody == 'object') {
                                if (cbody instanceof Array) {
                                    cdata.forEach(function (v, i) {
                                        bstack.push(cbody[0]);
                                        dstack.push(cdata[i]);
                                    });
                                } else {
                                    for (i in cbody) if (!cbody.hasOwnProperty(i)) {
                                        if (!cdata.hasOwnProperty(i)) {
                                            err('undefined data-key:' + i);
                                            console.log('required', cbody, 'data', cdata);
                                            throw 1;
                                        }
                                        bstack.push(cbody[i]);
                                        dstack.push(cdata[i]);
                                    }
                                }
                            } else if ('number,string,boolean'.indexOf(cbody)) {
                                if (typeof cdata != cbody) {
                                    err('invalid type key:' + k + ', val:' + cdata + ', required:' + cbody + ', supplied:' + (typeof cdata));
                                    throw 1;
                                }
                            } else {
                                result.value = cdata;
                                (new RuleSet()).rule(cbody).check(result, {});
                                if (!result.ok) {
                                    err('invalidation key:' + k + ', val:' + cdata);
                                    console.log(cbody, result);
                                    throw 1;
                                }
                            }
                        }
                    } while ((body = bstack.pop()) && (data = dstack.pop()));
                },
                'validate', (function () {
                    var cache = {}, tags = 'INPUT,TEXTAREA,SELECT'.split(','), stack = { length: 0 },
                        idx = function (el) {
                            var i = 0;
                            while (el = el.previousSibling) i++;
                            return i;
                        };
                    return function (el, group, partial) {
                        var cacheKey, vali, data, i, k, v;
                        if (!(cacheKey = el.getAttribute('data-cache')) || !cache[cacheKey] || !(v = cache[cacheKey][group])) {
                            vali = new Vali, data = {}, stack.length = 0;
                            if ((v = el.getAttribute('data-vali')) && (v = v.trim()) && domGroup(el) == group) {
                                i = v.indexOf(','),
                                    vali.ruleSet(k = v.substring(0, i), new RuleSet().rule(v.substr(i + 1))),
                                    data[k] = el;
                            }
                            el = el.firstElementChild;
                            do {
                                if (v = el.firstElementChild) stack[stack.length++] = v;
                                if (v = el.nextElementSibling) stack[stack.length++] = v;
                                if ((v = el.getAttribute('data-vali')) && (v = v.trim()) && domGroup(el) == group) {
                                    i = v.indexOf(','),
                                        vali.ruleSet(k = v.substring(0, i), new RuleSet().rule(v.substr(i + 1))),
                                        data[k] = el;
                                }
                            } while (stack.length && (el = stack[--stack.length]))
                            v = { vali: vali, data: data };
                            if (cacheKey) (cache[cacheKey] || (cache[cacheKey] = {}))[group] = v;
                        }
                        data = {};
                        for (k in v.data) if (v.data.hasOwnProperty(k) && (!partial || partial.indexOf(k) != NONE)) {
                            el = v.data[k];
                            data[k] = el.S(tags.indexOf(el.tagName) == NONE ? 'html' : '@value');
                        }
                        return v.vali.validate(data, partial);
                    };
                })()
            );
        (function () {
            var reg = {
                'ip': /^((([0-9])|(1[0-9]{1,2})|(2[0-4][0-9])|(25[0-5]))[.]){3}(([0-9])|([1-9][0-9]{1,2})|(2[0-4][0-9])|(25[0-5]))$/,
                'url': /^^https?:[/][/][a-zA-Z0-9.-]+[.]+[A-Za-z]{2,4}([:][0-9]{2,4})?/,
                'email': /^[0-9a-zA-Z-_.]+@[0-9a-zA-Z-]+[.]+[A-Za-z]{2,4}$/,
                'korean': /^[ㄱ-힣]+$/,
                'japanese': /^[ぁ-んァ-ヶー一-龠！-ﾟ・～「」“”‘’｛｝〜−]+$/,
                'alpha': /^[a-z]+$/,
                'ALPHA': /^[A-Z]+$/,
                'num': /^-?[0-9.]+$/,
                'alphanum': /^[a-z0-9]+$/,
                '1alpha': /^[a-z]/,
                '1ALPHA': /^[A-Z]/
            }, f = {
                'equalto': function (v, a, safe) { return v == safe[a[0]] ? v : 0; },
                'max': function (v, a) { return v.length < a[0] ? v : 0; },
                'min': function (v, a) { return v.length > a[0] ? v : 0; },
                'length': function (v, a) { return v.length == a[0] ? v : 0; },
                'range': function (v, a) { return a[0] <= v.length && v.length <= a[1] ? v : 0; },
                'float': function (v, a) { return '' + parseFloat(v) === v ? v : 0; },
                'int': function (v, a) { return '' + parseInt(v, 10) === v ? v : 0; },
                'in': function (v, a) { return a.indexOf(v) > -1 ? v : 0; },
                'notin': function (v, a) { return a.indexOf(v) == -1 ? v : 0; }
            }, k, mk;
            mk = function (k) { return function (v, a, safe) { return reg[k].test(v) ? v : 0; }; };
            for (k in reg) bs.rule(k, mk(k));
            for (k in f) bs.rule(k, f[k]);
        })();
    })();
    bs.css('.tmpl,.bsMarker[' + ns + 'tmpl]{display:none}'),
        bs.bsImmutable(
            'render', (function () {
                var TMPL = {}, frag, elLoop, param, update, currData, Tmpl, T = 'top,body,bottom'.split(',');
                frag = (function () {
                    var pool = [];
                    return function () {
                        if (arguments.length) pool.push(arguments[0]);
                        else return pool.length ? pool.pop() : doc.createDocumentFragment();
                    };
                })();
                elLoop = function (el, f, arg) {
                    var stack = { length: 0 }, k;
                    do {
                        if (k = el.firstElementChild) stack[stack.length++] = k;
                        if (k = el.nextElementSibling) stack[stack.length++] = k;
                        f(el, arg);
                    } while (stack.length && (el = stack[--stack.length]));
                };
                currData = (function () {
                    var stack = [], c = 'INFO,LIST,SUP,INDEX,@hash,clone'.split(','), clone = function () {
                        var a = arguments, i, obj;
                        if (a.length) {
                            i = a.length, obj = {};
                            while (i--) obj[a[i]] = this[a[i]];
                        } else {
                            i = c.length, obj = Object.assign({}, this);
                            while (i--) delete obj[c[i]];
                        }
                        return obj;
                    }, toJSON = function () {
                        var r = Object.assign({}, this), k;
                        k = c.length;
                        while (k--) delete r[c[k]];
                        return r;
                    };
                    return function (prevData, curr, i) {
                        var s = curr[i], k, l;
                        if (!s) return;
                        templateData = s;/*
			stack.length = 0, stack[0] = templateData = s;
			do{
				if(s && typeof s == 'object'){
					k = c.length;
					while(k--) delete s[c[k]];
					if(s.splice){
						for(k = 0, l = s.length; k < l; k++) if(s[k] && typeof s[k] == 'object') stack[stack.length] = s[k];
					}else{
						for(k in s) if(s[k] && typeof s[k] == 'object' && s.hasOwnProperty(k)) stack[stack.length] = s[k];
					}
				}
			}while(s = stack.pop());*/
                        templateData['@hash'] = k = JSON.stringify(templateData);
                        templateData.INDEX = i;
                        templateData.INFO = curr[0].INFO, templateData.LIST = curr, templateData.SUP = prevData;
                        templateData.clone = clone;
                        templateData.toJSON = toJSON;
                        return k;
                    };
                })();
                param = (function () {
                    var defaultvm = function (m) { return m; },
                        stack = [], uuid,
                        r0 = /\[@.$]{[^}]+\}/g, r1 = /~([0-9]+)~/g,
                        rf0 = function (_) { return stack[uuid] = _, '~' + (uuid++) + '~'; },
                        rf1 = function (_, v) { return stack[v]; },
                        mkBS = function (k) { return function () { return bs(k); }; },
                        mk$ = function (k) { return function () { return (new Function('bs', 'return (' + k + ');'))(bs); }; },
                        param = [],
                        primitive = { 'true': true, 'false': false, 'null': null };
                    return function (el) {
                        var p, v, i, j, k;
                        if (p = el.bsParam, p === undefined) {
                            p = el.getAttribute(ns + 's');
                            if (p) el.removeAttribute(ns + 's'), p = p.split(',');
                            if (v = el.getAttribute(ns + 'vm')) {
                                el.removeAttribute(ns + 'vm');
                                if (!p) p = {};
                                if (p.view = el.getAttribute(ns + 'view')) el.removeAttribute(ns + 'view');
                                else return err('param', 'view');
                                i = v.indexOf('(');
                                if (i == NONE) p.vm = defaultvm, k = v.indexOf('{') == NONE ? '@{' + v + '}' : v;
                                else {
                                    p.vm = bs(v.substring(0, i));
                                    k = v.substring(i + 1, v.length - 1).trim();
                                }
                                if (k) {
                                    stack.length = uuid = 0;
                                    k = k.replace(r0, rf0).split(','), i = k.length;
                                    while (i--) {
                                        j = k[i];
                                        if (j.indexOf('~') != NONE) j = j.replace(r1, rf1);
                                        switch (j.substr(0, 2)) {
                                            case '.{': k[i] = mkBS('.' + j.substring(2, j.length - 1)); break;
                                            case '@{': k[i] = mkBS(j.substring(2, j.length - 1)); break;
                                            case '${': k[i] = mk$(j.substring(2, j.length - 1)); break;
                                            default:
                                                if (j in primitive) k[i] = primitive[j];
                                                else if (j.isNumber()) k[i] = parseFloat(j);
                                                else if (
                                                    (j.charAt(0) == '[' && j.charAt(j.length - 1) == ']') ||
                                                    (j.charAt(0) == '{' && j.charAt(j.length - 1) == '}')
                                                ) k[i] = JSON.parse(j);
                                        }
                                    }
                                    p.m = function () {
                                        var i = k.length;
                                        param.length = 0;
                                        while (i--) param[i] = typeof k[i] == 'function' ? k[i]() : k[i];
                                        return param;
                                    };
                                }
                            }
                            el.bsParam = p || false;
                        }
                        return p;
                    };
                })();
                update = renderUpdate = function (el, param, isNew) {
                    var tmpl, prevData, curr, prev, hash, p, el0, el1, i, j;
                    if (param.length) domS(el, param);
                    if (param.vm) {
                        if (param.view != '*') tmpl = TMPL[param.view];
                        if (prev = el.bsPrevData || 0, !prev) el.innerHTML = '';
                        curr = param.vm.apply(null, param.m ? param.m() : null);
                        if (!(curr instanceof Array)) curr = [curr];
                        prevData = templateData;
                        templateData = curr;
                        if (tmpl) tmpl.update(el, curr, prevData, prev, isNew);
                        else {
                            for (i = curr[0] && curr[0].INFO ? 1 : 0, j = curr.length; i < j; i++) {
                                hash = currData(prevData, curr, i);
                                el0 = TMPL[templateData['@tmpl']].body.pop();
                                if (p = prev[i], !p) el.appendChild(el0);
                                else if (p != hash) {
                                    el1 = el.children[i];
                                    el1.bsTMPL.drain(el1);
                                    el.insertBefore(el0, el1);
                                    el.removeChild(el1);
                                }
                            }
                            if (i < prev.length) {
                                el0 = el.children[i];
                                do {
                                    el1 = el0.nextSibling;
                                    el0.bsTMPL.drain(el0);
                                    el.removeChild(el0);
                                } while (el0 = el1);
                            }
                        }
                        if (j = el.bsPrevData, !j) el.bsPrevData = j = [];
                        j.length = i = curr.length;
                        while (i--) j[i] = curr[i]['@hash'];
                        templateData = prevData;
                    }
                };
                Tmpl = (function () {
                    var Idom, Role;
                    Idom = (function () {
                        var Idom = function (el, param) {
                            var idx, prev;
                            this.param = param;
                            this.length = 0, this.id = Symb();
                            while (el.parentNode && el.parentNode.nodeType == 1) {
                                idx = 0, prev = el;
                                while (prev = prev.previousSibling) idx++;
                                this[this.length++] = idx;
                                el = el.parentNode;
                            }
                        }, fn = Idom.prototype;
                        fn.render = function (el) {
                            var id = this.id, r = el.bsIdoms, el0, i;
                            if (!r) el.bsIdoms = r = {};
                            if (el0 = r[id], !el0) {
                                el0 = el, i = this.length;
                                while (i-- && el0) el0 = el0.childNodes[this[i]];
                                r[id] = el0;
                            }
                            el0.bsParam = this.param;
                            update(el0, this.param);
                        };
                        return Idom;
                    })();
                    Role = (function () {
                        var loop = function (el, els) {
                            var p = param(el);
                            if (p) els.push(new Idom(el, p));
                        },
                            Role = function (tmpl, role, root, drop) {
                                this.tmpl = tmpl, this.role = role, this.root = root;
                                this.els = [], this.pool = [];
                                this.drop = drop;
                            }, fn = Role.prototype;
                        fn.init = function () { elLoop(this.root, loop, this.els); };
                        fn.drain = function (el) { this.pool.push(el); };
                        fn.pop = function () {
                            var el;
                            if (this.pool.length) el = this.pool.pop();
                            else {
                                el = this.root.cloneNode(true);
                                el.bsTMPL = TMPL[this.tmpl];
                            }
                            this.update(el);
                            return el;
                        };
                        fn.update = function (el) {
                            var els = this.els, i = els.length, g;
                            while (i--) els[i].render(el);
                            if (g = el.bsGroup, !g) el.bsGroup = g = {};
                            i = 'header,footer'.indexOf(this.role) == NONE && 'INDEX' in templateData;
                            g.record = templateData;
                            el.setAttribute(ns + 'group',
                                (g.tmpl = this.tmpl) + '|' +
                                (g.role = this.role) +
                                (i ? '|' + (g.index = i ? templateData.INDEX : -1) : '')
                            );
                        };
                        return Role;
                    })();
                    return (function () {
                        var ROLE = 'footer,bottom,body,top,header'.split(','),
                            LOOP = 'bottom,body,top'.split(','),
                            DROP = 'first,last,odd,even'.split(','),
                            Tmpl = function (tmpl) { this.tmpl = tmpl; },
                            fn = Tmpl.prototype;
                        fn.role = function (el) {
                            var role = el.getAttribute(ns + 'role') || 'body', drop, d, i, r;
                            drop = el.getAttribute(ns + 'drop') || '', d = {}, i = DROP.length;
                            while (i--) d[DROP[i]] = drop.indexOf(DROP[i]) != NONE;
                            r = this[role] = new Role(this.tmpl, role, el, d);
                            el.parentNode.removeChild(el);
                            el.removeAttribute(ns + 'drop');
                            el.removeAttribute(ns + 'role');
                            el.removeAttribute(ns + 'tmpl');
                            domS(el, 'class-', 'tmpl');
                        };
                        fn.init = function () {
                            var i = ROLE.length;
                            while (i--) if (this[ROLE[i]]) this[ROLE[i]].init();
                        };
                        fn.drain = function (el) { this[el.bsGroup.role].drain(el); };
                        fn.update = function (el, curr, prevData, prev, isNew) {
                            var target, footer, hash, role, p, isSame, isFrag, f, t, i, j, k, first, isOdd, drop;
                            f = frag();
                            if (prev) {
                                target = el.firstElementChild;
                                if (this.header) this.header.update(target), target = target.nextSibling;
                                if (this.footer && prev) el.removeChild(footer = el.lastElementChild);
                            } else if (this.header) f.appendChild(this.header.pop()), isFrag = true;

                            for (isOdd = true, first = i = curr[0] && curr[0].INFO ? 1 : 0, j = curr.length; i < j; i++ , isOdd = !isOdd) {
                                p = prev[i], hash = currData(prevData, curr, i), k = 3;
                                if (p) isSame = p == hash;
                                while (k--) if (role = this[LOOP[k]]) {
                                    drop = role.drop;
                                    if ((i == first && drop.first) || (i == j - 1 && drop.last) || (isOdd && drop.odd) || (!isOdd && drop.even)) continue;
                                    if (p) {
                                        if (isNew || !isSame) role.update(target);
                                        target = target.nextSibling;
                                    } else f.appendChild(role.pop()), isFrag = true;
                                }
                            }
                            if (prev) {
                                for (j = prev.length; i < j; i++) {
                                    k = 3;
                                    while (k--) if (role = this[LOOP[k]]) {
                                        role.drain(t = target);
                                        target = target.nextSibling;
                                        el.removeChild(t);
                                    }
                                }
                            }
                            if (this.footer) {
                                if (footer) this.footer.update(footer);
                                else footer = this.footer.pop();
                                f.appendChild(footer);
                                isFrag = true;
                            }
                            if (isFrag) el.appendChild(f);
                            frag(f);
                        };
                        return Tmpl;
                    })();
                })();
                bs('VM', {
                    range: (function () {
                        var r = [];
                        return function (s, e) {
                            var a = arguments, i, j = a.length, v;
                            r.length = 0;
                            while (s <= e) {
                                v = { INDEX: s++ }, i = 2;
                                while (i < j) v[a[i++]] = a[i++];
                                r[r.length] = v;
                            }
                            return r;
                        };
                    })(),
                    item: (function () {
                        var r = [];
                        return function () {
                            var a = arguments, i = 0, j = a.length, v;
                            r.length = 0;
                            while (i < j) r[r.length] = { INDEX: i, item: a[i] }, i++;
                            return r;
                        };
                    })()
                });
                return (function () {
                    var views = {}, isScaned = false, c = 0,
                        loop = function (el, view) {
                            var p = el;
                            do {
                                if (p.bsGroup) return;
                                if (p.nodeType === 1 && p.getAttribute(ns + 'tmpl')) return;
                            } while (p = p.parentNode);
                            if (c) console.log(el);
                            if (p = el.bsParam || param(el)) view.push(el, p);
                        },
                        scan = function (el) {
                            var key, i;
                            if (typeof el == STR) el = docId(el);
                            if (el.bsScaned) return;
                            key = [];
                            elLoop(el, function (el) {
                                var k = el.getAttribute(ns + 'tmpl');
                                if (k) {
                                    key[key.length] = k;
                                    if (!TMPL[k]) TMPL[k] = new Tmpl(k);
                                    TMPL[k].role(el);
                                }
                            });
                            i = key.length;
                            while (i--) TMPL[key[i]].init();
                            el.bsScaned = true;
                        };
                    bs.bsImmutable('scan', scan);
                    return function (el, isNew) {
                        var view, i, j;
                        if (!isScaned) {
                            isScaned = true;
                            scan(doc.body);
                        };
                        el = typeof el == STR ? el = docId(el) : el || doc.body;
                        if (view = views[el.bsId], !view || scan) {
                            loop(el, views[el.bsId = Symb() + ''] = view = []);
                            if (el.firstElementChild) elLoop(el.firstElementChild, loop, view);
                        }
                        for (i = 0, j = view.length; i < j;) update(view[i++], view[i++], isNew);
                    };
                })();
            })()
        );
    if ('localStorage' in W) (function () {
        var base, L = W['localStorage'];
        bs.bsImmutable(
            'localSize', function (unit) {
                var key, sum = 0;
                for (key in L) sum += (key.length + (L[key].length || 0)) * 2;
                switch (unit) {
                    case 'kb': return sum / 1024;
                    case 'mb': return sum / 1024 / 1024;
                }
                return sum;
            },
            'local', base = function () {
                var t0, t1, i = 0, j = arguments.length, k, v, m, n;
                while (i < j) {
                    k = arguments[i++];
                    if (i == j) {
                        if (t0 = localStorage.getItem(k)) {
                            if (t0.charAt(0) == '⒩') {
                                for (t1 = '', m = 0, n = parseInt(t0.substr(1)); m < n; m++) t1 += localStorage.getItem(k + '::' + m);
                                t0 = t1;
                            }
                            if (t0.charAt(0) == '@') t0 = JSON.parse(t0.substr(1));
                        }
                        return t0;
                    }
                    v = arguments[i++];
                    if (v === null) {
                        t0 = localStorage.getItem(k);
                        if (t0 && t0.charAt(0) == '⒩') for (m = 0, n = parseInt(t0.substr(1)); m < n; m++) localStorage.removeItem(k + '::' + m);
                        localStorage.removeItem(k);
                    } else {
                        t0 = (v && typeof v == OBJ ? '@' + JSON.stringify(v) : v) + '', n = 0;
                        while (1) {
                            try {
                                if (!n) localStorage.setItem(k, t0);
                                else for (localStorage.setItem(k, '⒩' + n), m = 0, t1 = Math.ceil(t0.length / n); m < n; m++) {
                                    localStorage.setItem(k + '::' + m, t0.substr(t1 * m, t1));
                                }
                                break;
                            } catch (e) {
                                n++;
                            }
                        }
                    }
                }
                return v;
            }
        );
    })();
    (function () {
        var api = {}, error = {}, amo, tmo, pmo, read, write;
        if (W['localStorage']) {
            read = function (key) { return bs.local(key); };
            write = function (key, val) { bs.local(key, val); };
        } else {
            read = function (key) { return bs.ck(key); };
            write = function (key, val) { bs.ck(key, val, 365); };
        }
        tmo = read('tmo') || null;
        pmo = read('pmo') || null;
        bs.bsImmutable(
            'api_', function () {
                var k, v, a = arguments, i = 2, j = a.length, base = a[0] == '@base' ? a[1] : { url: '', json: '' };
                while (i < j) k = a[i++], v = a[i++], v['@base'] = base, api[k] = v;
            },
            'api', function (f, key, arg) {
                var v, url, urlKey, k, t, isj = 1;
                if (!arg) arg = {};
                key = key.split(':');
                urlKey = key[0].charAt(0) == '@' ? 'json' : 'url';
                if (urlKey == 'json') key[0] = key[0].substr(1);
                v = api[key[0]] && api[key[0]][key[1]];
                if (!v) {
                    err('api:undefined api' + key);
                    throw 1;
                }

                if (v[urlKey]) url = v[urlKey];
                if (!url && api[key[0].url]) url = api[key[0].url](key[1], urlKey == 'json');
                if (!url && api['@gateway']) url = api['@gateway'](key[0], key[1], urlKey == 'json');
                if (!url) {
                    err('api:url error-' + key);
                    throw 1;
                }
                t = [
                    function (data) {
                        var err, i, j;
                        try {
                            data = JSON.parse(data);
                            if (urlKey == 'url') {
                                amo = this.getResponseHeader('amo');
                                write('tmo', tmo = this.getResponseHeader('tmo') || null);
                                write('pmo', pmo = this.getResponseHeader('pmo') || null);
                            }
                        } catch (e) {
                            err('api:invalid json-' + key);
                            f.call(this, null);
                        }
                        if (err = data.error) {
                            for (i = 0, j = err.length; i < j; i++) error[err[i].method](err[i]);
                        } else if (data.data) {
                            f.call(this, data.data);
                        } else {
                            err('api:invalid data-' + key);
                            f.call(this, null);
                        }
                    },
                    (arg['@rootpath'] ? arg['@rootpath'] : '') + (api[key[0]]['@base'][urlKey] || '') + url,
                    '@amo', amo, '@tmo', tmo, '@pmo', pmo,
                    '@rootpath', '', '@timeoffset', new Date().getTimezoneOffset()
                ];
                for (k in arg) {
                    if (k.charAt(0) == '@') {
                        t[t.length] = k, t[t.length] = arg[k];
                        delete arg[k];
                    } else if (W['File'] && arg[k] instanceof File) isj = 0;
                }
                bs.valiApi(v.body, arg);
                if (isj) {
                    t[t.length] = '@isj', t[t.length] = '1';
                    t[t.length] = 'json', t[t.length] = arg;
                } else {
                    for (k in arg) t[t.length] = k, t[t.length] = arg[k];
                }
                bs.post.apply(bs, t);
            },
            'apierr', function () {
                var a = arguments, i = 0, j = a.length;
                while (i < j) error[a[i++]] = a[i++];
            });
    })();
    bs.boot = function () {
        var s, style, i;
        s = doc.getElementsByTagName('style'), i = s.length;
        while (i--) {
            style = s[i];
            if (!style.getAttribute('data-ignore') && !style.getAttribute('data-boot')) {
                style.setAttribute('data-boot', 'processed');
                (style.styleSheet || style.sheet).disabled = true;
                bs.css(style['styleSheet'] ? style.styleSheet.cssText : style.innerHTML);
            }
        }
        s = null;
    };
    if (W['module'] && module['exports']) module.exports = bs;
    else W['bs'] = bs;
})(this);
