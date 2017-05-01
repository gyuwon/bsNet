using System.Security.Cryptography;
using System.Text;

namespace com.bsidesoft.cs {
    public partial class bs {
        private static UnicodeEncoding byteConverter = new UnicodeEncoding();
        private static RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(1024);
        private static RSAEncryptionPadding padding = RSAEncryptionPadding.Pkcs1;

        static public string rsaEncrypt(string v, RSAParameters key, bool padding) {
            try {
                rsa.ImportParameters(key);
                return byteConverter.GetString(rsa.Encrypt(byteConverter.GetBytes(v), padding));
            } catch(CryptographicException e) {
                return e + "";
            }
        }
        static public string rsaDecrypt(string v, RSAParameters key, bool padding) {
            try {
                rsa.ImportParameters(key);
                return byteConverter.GetString(rsa.Decrypt(byteConverter.GetBytes(v), padding));
            } catch(CryptographicException e) {
                return null;
            }
        }
    }
}