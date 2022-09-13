using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 using System.Security.Cryptography;
 using System.IO;

namespace SNSelfOrder
{
    public class SecurityController
    {
        string[] codebook = new string[] {"321820",
                                          "713784",
                                          "593471",
                                          "236190",
                                          "634303",
                                          "757759",
                                          "959175",
                                          "852314",
                                          "432315",
                                          "164270" };
        public bool DcodeCode(string accCode, out int C, out DateTime rdate, out DateTime edate)
        {
            bool ret = true;
            rdate = DateTime.Today;
            edate = DateTime.Today;
            C = 0;
            try
            {

                int yy = 19;
                string CStr = accCode.Substring(0, 1);
                int.TryParse(CStr, out C);
                string data = accCode.Substring(1);
                string[] strV = data.Split(new string[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                if (strV.Length == 3)
                {
                    int.TryParse(strV[0], out yy);
                    yy += 2000;
                    DateTime aDate = new DateTime(yy, 1, 1).AddDays(-1);
                    int r = 0;
                    int.TryParse(strV[1], out r);
                    int t = 0;
                    int.TryParse(strV[2], out t);
                    rdate = aDate.AddDays(r);
                    edate = rdate.AddDays(t).AddDays(1);
                }
                else ret = false;
            }
            catch (Exception err)
            {
                ret = false;
            }
            return ret;
        }
        public string Encrypt(string data)
        {
            int keyindex = 0;
            Random rand = new Random();
            Random rand1 = new Random(rand.Next());

            keyindex = rand1.Next(0, 9);
            string encData = null;
            byte[][] keys = GetHashKeys(codebook[keyindex]);

            try
            {
                string eData = EncryptStringToBytes_Aes(data, keys[0], keys[1]);
                if (eData.EndsWith("==="))
                {
                    eData = eData.Substring(0, eData.Length - 3);
                    eData += "i";
                }
                else if (eData.EndsWith("=="))
                {
                    eData = eData.Substring(0, eData.Length - 2);
                    eData += "k";
                }
                else if (eData.EndsWith("="))
                {
                    eData = eData.Substring(0, eData.Length - 1);
                    eData += "n";
                }
                encData = keyindex.ToString() + eData;
            }
            catch (CryptographicException) { }
            catch (ArgumentNullException) { }

            return encData;
        }
        public string Encrypt(int keyindex, string data)
        {
            string encData = null;
            byte[][] keys = GetHashKeys(codebook[keyindex]);

            try
            {
                encData = EncryptStringToBytes_Aes(data, keys[0], keys[1]);
            }
            catch (CryptographicException) { }
            catch (ArgumentNullException) { }

            return encData;
        }
        public string Encrypt(string key, string data)
        {
            string encData = null;
            byte[][] keys = GetHashKeys(key);

            try
            {
                encData = EncryptStringToBytes_Aes(data, keys[0], keys[1]);
            }
            catch (CryptographicException) { }
            catch (ArgumentNullException) { }

            return encData;
        }
        public string Decrypt(string data)
        {
            string decData = null;
            int keyindex = 0;
            string S = data.Substring(0, 1);
            int.TryParse(S, out keyindex);
            string eData = data.Substring(1);
            if (eData.EndsWith("i"))
            {
                eData = eData.Substring(0, eData.Length - 1);
                eData += "===";
            }
            else if (eData.EndsWith("k"))
            {
                eData = eData.Substring(0, eData.Length - 1);
                eData += "==";
            }
            else if (eData.EndsWith("n"))
            {
                eData = eData.Substring(0, eData.Length - 1);
                eData += "=";
            }

            byte[][] keys = GetHashKeys(codebook[keyindex]);

            try
            {
                decData = DecryptStringFromBytes_Aes(eData, keys[0], keys[1]);
            }
            catch (CryptographicException) { }
            catch (ArgumentNullException) { }

            return decData;
        }
        public string Decrypt(int keyindex, string data)
        {
            string decData = null;
            byte[][] keys = GetHashKeys(codebook[keyindex]);

            try
            {
                decData = DecryptStringFromBytes_Aes(data, keys[0], keys[1]);
            }
            catch (CryptographicException) { }
            catch (ArgumentNullException) { }

            return decData;
        }
        public string Decrypt(string key, string data)
        {
            string decData = null;
            byte[][] keys = GetHashKeys(key);

            try
            {
                decData = DecryptStringFromBytes_Aes(data, keys[0], keys[1]);
            }
            catch (CryptographicException) { }
            catch (ArgumentNullException) { }

            return decData;
        }

        private byte[][] GetHashKeys(string key)
        {
            byte[][] result = new byte[2][];
            Encoding enc = Encoding.UTF8;

            SHA256 sha2 = new SHA256CryptoServiceProvider();

            byte[] rawKey = enc.GetBytes(key);
            byte[] rawIV = enc.GetBytes(key);

            byte[] hashKey = sha2.ComputeHash(rawKey);
            byte[] hashIV = sha2.ComputeHash(rawIV);

            Array.Resize(ref hashIV, 16);

            result[0] = hashKey;
            result[1] = hashIV;

            return result;
        }

        private static string EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            byte[] encrypted;

            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt =
                            new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            return Convert.ToBase64String(encrypted);
        }

        private static string DecryptStringFromBytes_Aes(string cipherTextString, byte[] Key, byte[] IV)
        {
            string plaintext = null;
            try
            {
                byte[] cipherText = Convert.FromBase64String(cipherTextString);

                if (cipherText == null || cipherText.Length <= 0)
                    throw new ArgumentNullException("cipherText");
                if (Key == null || Key.Length <= 0)
                    throw new ArgumentNullException("Key");
                if (IV == null || IV.Length <= 0)
                    throw new ArgumentNullException("IV");



                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = Key;
                    aesAlg.IV = IV;

                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                    {
                        using (CryptoStream csDecrypt =
                                new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                plaintext = srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch { }
            return plaintext;
        }
    }
}

