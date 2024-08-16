using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Encryption
{
    public static class Encryption
    {
        public enum Algorithm
        {
            AES16HMAC,
            AES32HMAC,
            AES32SHA
        }
        
        private const string AES16HMACKey = "AES16_HMACSHA1";
        private const string AES32HMACKey = "AES32_HMACSHA1";
        private const string AES32SHAKey = "AES32_SHA";
        
        private static void WriteString(this Stream stream, string str) => stream.Write(Encoding.Latin1.GetBytes(str));
        private static string ReadString(this Stream stream, int count)
        {
            Span<byte> buffer = stackalloc byte[count];
            int nc = stream.Read(buffer);
            return Encoding.Latin1.GetString(buffer.Slice(0, nc));
        }
        
        public static void Encrypt(PasswordManager pm, string password, Stream output)
        {
            output.Position = 0;
            string alg = Program.Algorithm switch
            {
                Algorithm.AES16HMAC => AES16HMACKey,
                Algorithm.AES32HMAC => AES32HMACKey,
                Algorithm.AES32SHA => AES32SHAKey,
                _ => throw new Exception()
            };
            output.WriteString(alg);
            
            CryptoStream csEncrypt;
            
            if (Program.Algorithm == Algorithm.AES16HMAC)
            {
                csEncrypt = EnAES16HMAC(output, password);
            }
            else if (Program.Algorithm == Algorithm.AES32HMAC)
            {
                csEncrypt = EnAES32HMAC(output, password);
            }
            else if (Program.Algorithm == Algorithm.AES32SHA)
            {
                csEncrypt = EnAES32SHA(output, password);
            }
            else { return; }
            
            // Write json file
            pm.WriteToStream(csEncrypt);
            
            csEncrypt.Dispose();
        }
        
        private static CryptoStream EnAES16HMAC(Stream output, string password)
        {
            AesManaged rm = new AesManaged();
            
            rm.GenerateIV();
            output.Write(rm.IV);
            
            Rfc2898DeriveBytes rdb = new Rfc2898DeriveBytes(password, rm.IV);
            rm.Key = rdb.GetBytes(16);
            
            return new CryptoStream(output, rm.CreateEncryptor(), CryptoStreamMode.Write, true);
        }
        private static CryptoStream EnAES32HMAC(Stream output, string password)
        {
            AesManaged rm = new AesManaged();
            
            rm.GenerateIV();
            output.Write(rm.IV);
            
            Rfc2898DeriveBytes rdb = new Rfc2898DeriveBytes(password, rm.IV);
            rm.Key = rdb.GetBytes(32);
            
            return new CryptoStream(output, rm.CreateEncryptor(), CryptoStreamMode.Write, true);
        }
        private static CryptoStream EnAES32SHA(Stream output, string password)
        {
            AesManaged rm = new AesManaged();
            
            rm.GenerateIV();
            output.Write(rm.IV);
            
            rm.Key = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            
            return new CryptoStream(output, rm.CreateEncryptor(), CryptoStreamMode.Write, true);
        }
        
        public static PasswordManager Decrypt(Stream input, string password)
        {
            input.Position = 0;
            string str = input.ReadString(14);
            CryptoStream csDecrypt;
            
            if (str.StartsWith(AES16HMACKey))
            {
                input.Position = AES16HMACKey.Length;
                csDecrypt = AES16HMAC(input, password);
            }
            else if (str.StartsWith(AES32HMACKey))
            {
                input.Position = AES32HMACKey.Length;
                csDecrypt = AES32HMAC(input, password);
            }
            else if (str.StartsWith(AES32SHAKey))
            {
                input.Position = AES32SHAKey.Length;
                csDecrypt = AES32SHA(input, password);
            }
            else
            {
                // old format
                input.Position = 0;
                csDecrypt = AES16HMAC(input, password);
            }
            
            try
            {
                return new PasswordManager(csDecrypt);
            }
            catch (Exception)
            {
                csDecrypt.Dispose();
                return null;
            }
        }
        private static CryptoStream AES16HMAC(Stream input, string password)
        {
            AesManaged rm = new AesManaged();
            
            byte[] iv = new byte[16];
            input.Read(iv);
            
            rm.IV = iv;
            Rfc2898DeriveBytes rdb = new Rfc2898DeriveBytes(password, rm.IV);
            rm.Key = rdb.GetBytes(16);
            
            return new CryptoStream(input,  rm.CreateDecryptor(), CryptoStreamMode.Read, true);
        }
        private static CryptoStream AES32HMAC(Stream input, string password)
        {
            AesManaged rm = new AesManaged();
            
            Aes aes = Aes.Create();
            
            byte[] iv = new byte[16];
            input.Read(iv);
            
            rm.IV = iv;
            Rfc2898DeriveBytes rdb = new Rfc2898DeriveBytes(password, rm.IV);
            rm.Key = rdb.GetBytes(32);
            
            return new CryptoStream(input,  rm.CreateDecryptor(), CryptoStreamMode.Read, true);
        }
        private static CryptoStream AES32SHA(Stream input, string password)
        {
            AesManaged rm = new AesManaged();
            
            byte[] iv = new byte[16];
            input.Read(iv);
            
            rm.IV = iv;
            rm.Key = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            
            return new CryptoStream(input, rm.CreateDecryptor(), CryptoStreamMode.Read, true);
        }
    }
}
