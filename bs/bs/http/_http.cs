using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace com.bsidesoft.cs {
    public partial class bs {
        public static string reqPath(HttpRequest r) {
            return r.Path;
        }
        public static JObject reqJson(HttpRequest r) {
            return JObject.Parse(new StreamReader(r.Body, Encoding.UTF8, true, 20000, true).ReadToEnd());
        }
        public static async Task<T> GET<T>(string url) {
            var client = new HttpClient();
            
            client.DefaultRequestHeaders.Add("test", "abd");
            var response = await client.GetAsync(url);
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
