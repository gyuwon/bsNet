using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace com.bsidesoft.cs {
    public partial class bs {
        public class RSAKeyPair {
            private String privateKey = null;
            private String publicKey = null;
            private Dictionary<string, string> publicInfo = null;
            private RSACryptoServiceProvider csp;
            internal RSAKeyPair() {
                csp = new RSACryptoServiceProvider(1024);
            }
            public Dictionary<string,string> getPublicInfo() {
                if(publicInfo == null) {
                    RSAParameters param = csp.ExportParameters(false);
                    publicInfo = new Dictionary<string, string>() {
                        { "exponent", byteArrayToHexString(param.Exponent) },
                        { "modulus", byteArrayToHexString(param.Modulus) }
                    };
                }
                return publicInfo;
            }
            public string encrypt(string plaintext) {
                var plainTextBytes = Encoding.UTF8.GetBytes(plaintext);
                var cipherBytes = csp.Encrypt(plainTextBytes, RSAEncryptionPadding.Pkcs1);
                return Convert.ToBase64String(cipherBytes);
            }
            public string decrypt(string cipherText) {
                var cipherBytes = Convert.FromBase64String(cipherText);
                var plainTextBytes = csp.Decrypt(cipherBytes, RSAEncryptionPadding.Pkcs1);
                return Encoding.UTF8.GetString(plainTextBytes);
            }
            public string getPrivateKey() {
                if(privateKey == null) privateKey = ExportPrivateKey(csp);
                return privateKey;
            }
            public string getPublicKey() {
                if(publicKey == null) publicKey = ExportPublicKey(csp);
                return publicKey;
            }
            private static string ExportPrivateKey(RSACryptoServiceProvider csp) {
                if(csp.PublicOnly) throw new ArgumentException("CSP does not contain a private key", "csp");
                TextWriter outputStream = new StringWriter();
                var parameters = csp.ExportParameters(true);
                using(var stream = new MemoryStream()) {
                    var writer = new BinaryWriter(stream);
                    writer.Write((byte)0x30); // SEQUENCE
                    using(var innerStream = new MemoryStream()) {
                        var innerWriter = new BinaryWriter(innerStream);
                        EncodeIntegerBigEndian(innerWriter, new byte[] { 0x00 }); // Version
                        EncodeIntegerBigEndian(innerWriter, parameters.Modulus);
                        EncodeIntegerBigEndian(innerWriter, parameters.Exponent);
                        EncodeIntegerBigEndian(innerWriter, parameters.D);
                        EncodeIntegerBigEndian(innerWriter, parameters.P);
                        EncodeIntegerBigEndian(innerWriter, parameters.Q);
                        EncodeIntegerBigEndian(innerWriter, parameters.DP);
                        EncodeIntegerBigEndian(innerWriter, parameters.DQ);
                        EncodeIntegerBigEndian(innerWriter, parameters.InverseQ);
                        var length = (int)innerStream.Length;
                        EncodeLength(writer, length);
                        writer.Write(innerStream.GetBuffer(), 0, length);
                    }

                    var base64 = Convert.ToBase64String(stream.GetBuffer(), 0, (int)stream.Length).ToCharArray();
                    outputStream.WriteLine("-----BEGIN RSA PRIVATE KEY-----");
                    // Output as Base64 with lines chopped at 64 characters
                    for(var i = 0; i < base64.Length; i += 64) {
                        outputStream.WriteLine(base64, i, Math.Min(64, base64.Length - i));
                    }
                    outputStream.WriteLine("-----END RSA PRIVATE KEY-----");
                    return outputStream.ToString();
                }
            }
            private static string ExportPublicKey(RSACryptoServiceProvider csp) {
                TextWriter outputStream = new StringWriter();

                var parameters = csp.ExportParameters(false);
                using(var stream = new MemoryStream()) {
                    var writer = new BinaryWriter(stream);
                    writer.Write((byte)0x30); // SEQUENCE
                    using(var innerStream = new MemoryStream()) {
                        var innerWriter = new BinaryWriter(innerStream);
                        EncodeIntegerBigEndian(innerWriter, new byte[] { 0x00 }); // Version
                        EncodeIntegerBigEndian(innerWriter, parameters.Modulus);
                        EncodeIntegerBigEndian(innerWriter, parameters.Exponent);

                        //All Parameter Must Have Value so Set Other Parameter Value Whit Invalid Data  (for keeping Key Structure  use "parameters.Exponent" value for invalid data)
                        EncodeIntegerBigEndian(innerWriter, parameters.Exponent); // instead of parameters.D
                        EncodeIntegerBigEndian(innerWriter, parameters.Exponent); // instead of parameters.P
                        EncodeIntegerBigEndian(innerWriter, parameters.Exponent); // instead of parameters.Q
                        EncodeIntegerBigEndian(innerWriter, parameters.Exponent); // instead of parameters.DP
                        EncodeIntegerBigEndian(innerWriter, parameters.Exponent); // instead of parameters.DQ
                        EncodeIntegerBigEndian(innerWriter, parameters.Exponent); // instead of parameters.InverseQ

                        var length = (int)innerStream.Length;
                        EncodeLength(writer, length);
                        writer.Write(innerStream.GetBuffer(), 0, length);
                    }

                    var base64 = Convert.ToBase64String(stream.GetBuffer(), 0, (int)stream.Length).ToCharArray();
                    outputStream.WriteLine("-----BEGIN PUBLIC KEY-----");
                    // Output as Base64 with lines chopped at 64 characters
                    for(var i = 0; i < base64.Length; i += 64) {
                        outputStream.WriteLine(base64, i, Math.Min(64, base64.Length - i));
                    }
                    outputStream.WriteLine("-----END PUBLIC KEY-----");

                    return outputStream.ToString();
                }
            }
            private static void EncodeLength(BinaryWriter stream, int length) {
                if(length < 0) throw new ArgumentOutOfRangeException("length", "Length must be non-negative");
                if(length < 0x80) {
                    // Short form
                    stream.Write((byte)length);
                } else {
                    // Long form
                    var temp = length;
                    var bytesRequired = 0;
                    while(temp > 0) {
                        temp >>= 8;
                        bytesRequired++;
                    }
                    stream.Write((byte)(bytesRequired | 0x80));
                    for(var i = bytesRequired - 1; i >= 0; i--) {
                        stream.Write((byte)(length >> (8 * i) & 0xff));
                    }
                }
            }
            private static void EncodeIntegerBigEndian(BinaryWriter stream, byte[] value, bool forceUnsigned = true) {
                stream.Write((byte)0x02); // INTEGER
                var prefixZeros = 0;
                for(var i = 0; i < value.Length; i++) {
                    if(value[i] != 0) break;
                    prefixZeros++;
                }
                if(value.Length - prefixZeros == 0) {
                    EncodeLength(stream, 1);
                    stream.Write((byte)0);
                } else {
                    if(forceUnsigned && value[prefixZeros] > 0x7f) {
                        // Add a prefix zero to force unsigned if the MSB is 1
                        EncodeLength(stream, value.Length - prefixZeros + 1);
                        stream.Write((byte)0);
                    } else {
                        EncodeLength(stream, value.Length - prefixZeros);
                    }
                    for(var i = prefixZeros; i < value.Length; i++) {
                        stream.Write(value[i]);
                    }
                }
            }
            private static string HexAlphabet = "0123456789abcdef";
            private static string byteArrayToHexString(byte[] Bytes) {
                StringBuilder Result = new StringBuilder(Bytes.Length * 2);
                foreach(byte B in Bytes) {
                    Result.Append(HexAlphabet[(int)(B >> 4)]);
                    Result.Append(HexAlphabet[(int)(B & 0xF)]);
                }
                return Result.ToString();
            }
        }
        public static RSAKeyPair rsaGenerate() {
            return new RSAKeyPair();
        }
        
    }
}