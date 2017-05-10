using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace com.bsidesoft.cs {
    public partial class bs {
        public static string reqPath(HttpRequest r) {
            return r.Path;
        }
        public static JObject reqJson(HttpRequest r) {
            try {
                return JObject.Parse(new StreamReader(r.Body, Encoding.UTF8, true, 20000, true).ReadToEnd());
            } catch(Exception e) {
                log("JSON 해석 오류. " + e.Message);
                return new JObject();
            }
        }
        public static async Task<T> GET<T>(string url, params object[] arg) {
            return await http<T>(HttpMethod.Get, url, opt<object>(arg));
        }
        public static async Task<T> GET<T>(string url, Dictionary<string, object> opt = null) {
            return await http<T>(HttpMethod.Get, url, opt);
        }
        public static async Task<T> POST<T>(string url, params object[] arg) {
            return await http<T>(HttpMethod.Post, url, opt<object>(arg));
        }
        public static async Task<T> POST<T>(string url, Dictionary<string, object> opt = null) {
            return await http<T>(HttpMethod.Post, url, opt);
        }
        public static async Task<T> PUT<T>(string url, params object[] arg) {
            return await http<T>(HttpMethod.Put, url, opt<object>(arg));
        }
        public static async Task<T> PUT<T>(string url, Dictionary<string, object> opt = null) {
            return await http<T>(HttpMethod.Put, url, opt);
        }
        public static async Task<T> DELETE<T>(string url, params object[] arg) {
            return await http<T>(HttpMethod.Delete, url, opt<object>(arg));
        }
        public static async Task<T> DELETE<T>(string url, Dictionary<string, object> opt = null) {
            return await http<T>(HttpMethod.Delete, url, opt);
        }
        private static async Task<T> http<T>(HttpMethod method, string url, Dictionary<string, object> opt) {
            var client = new HttpClient();
            var data = new Dictionary<string, string>();
            var files = new Dictionary<string, StreamContent>();
            var encoder = UrlEncoder.Create();
            StringContent jsonBody = null;
            if(opt != null) {
                foreach(var key in opt) {
                    var k = key.Key;
                    var v = key.Value;
                    if(k[0] == '@') client.DefaultRequestHeaders.Add(k.Substring(1), encoder.Encode(v + ""));
                    else if(k == "json") jsonBody = new StringContent((v is JObject ? (JObject)v : v) + "");
                    else if(v is Stream) files.Add(k, new StreamContent((Stream)v));
                    else data[k] = v + "";
                }
            }
            var request = new HttpRequestMessage(method, url);
            if(files.Count > 0) {
                var mc = new MultipartFormDataContent();
                foreach(var key in files) mc.Add(key.Value, key.Key);
                foreach(var key in data) mc.Add(new StringContent(key.Value), key.Key);
                request.Content = mc;
            } else if(data.Count > 0) {
                if(method == HttpMethod.Get) request.RequestUri = new Uri(QueryHelpers.AddQueryString(url, data));
                else request.Content = new FormUrlEncodedContent(data);
            }
            var response = await client.SendAsync(request);
            switch(TYPES[typeof(T)]) {
            case "byte[]": return to<T>(await response.Content.ReadAsByteArrayAsync());
            case "stream": return to<T>(await response.Content.ReadAsStreamAsync());
            case "string": return to<T>(await response.Content.ReadAsStringAsync());
            default:
                log("GET:invalid type - " + typeof(T));
                return default(T);
            }
        }
    }
}
