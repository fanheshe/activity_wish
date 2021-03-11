using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace JKCore.Infrastructure
{
    public class EncryptHelper
    {
        #region AES - CEB
        public static string key = "www.yytjs.cn";
        /// <summary>
        /// AES 加密
        /// </summary>
        /// <param name="str"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string AESEncrypt(string str)
        {
            if (string.IsNullOrEmpty(str)) return null;
            Byte[] toEncryptArray = Encoding.UTF8.GetBytes(str);

            System.Security.Cryptography.RijndaelManaged rm = new System.Security.Cryptography.RijndaelManaged
            {
                Key = Encoding.UTF8.GetBytes(key.PadRight(32)),
                Mode = System.Security.Cryptography.CipherMode.ECB,
                Padding = System.Security.Cryptography.PaddingMode.PKCS7
            };

            System.Security.Cryptography.ICryptoTransform cTransform = rm.CreateEncryptor();
            Byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            //return HttpUtility.UrlEncode(Convert.ToBase64String(resultArray, 0, resultArray.Length));
            return Uri.EscapeDataString(Convert.ToBase64String(resultArray, 0, resultArray.Length));
        }

        /// <summary>
        /// AES 解密
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string AESDecrypt(string data)
        {
            //data = HttpUtility.UrlDecode(data);
            data = Uri.UnescapeDataString(data);

            byte[] encryptedBytes = Convert.FromBase64String(data);
            byte[] bKey = new byte[32];
            Array.Copy(Encoding.UTF8.GetBytes(key.PadRight(bKey.Length)), bKey, bKey.Length);

            MemoryStream mStream = new MemoryStream(encryptedBytes);

            SymmetricAlgorithm aes = Aes.Create();

            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;
            aes.KeySize = 128;
            aes.Key = bKey;

            CryptoStream cryptoStream = new CryptoStream(mStream, aes.CreateDecryptor(), CryptoStreamMode.Read);

            byte[] tmp = new byte[encryptedBytes.Length + 32];
            int len = cryptoStream.Read(tmp, 0, encryptedBytes.Length + 32);
            byte[] ret = new byte[len];
            Array.Copy(tmp, 0, ret, 0, len);
            var cebResult = Encoding.UTF8.GetString(ret);

            //特殊处理，因为加salt
            int lastIndex = cebResult.LastIndexOf("}") + 1;

            return cebResult.Substring(0, lastIndex);

        }
        #endregion

        #region SHA1
        /// <summary>
        /// SHA1 加密，16进制小写
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string SHA1Encrypt(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }

            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(str));
                string shaStr = BitConverter.ToString(hash);

                shaStr = shaStr.Replace("-","");
                shaStr = shaStr.ToLower();

                return shaStr;
            }
        }
        #endregion
    }
}
