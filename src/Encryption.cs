using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Zene.Graphics;

namespace Encryption
{
    public static class Encryption
    {
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
        
        public static void Encrypt(PasswordManager pm, byte[] key, Stream output)
        {
            output.Position = 0;
            output.WriteString(AES32SHAKey);
            
            CryptoStream csEncrypt = EnAES(output, key);
            
            // Write json file
            pm.WriteToStream(csEncrypt);
            
            csEncrypt.Dispose();
        }
        public static byte[] GetEncryptKey(string password)
        {
            byte[] pb = Encoding.UTF8.GetBytes(password);
            return SHA256.HashData(pb);
        }
        
        private static CryptoStream EnAES(Stream output, byte[] key)
        {
            Aes rm = Aes.Create();
            
            // rm.GenerateIV();
            output.Write(rm.IV);
            rm.Key = key;
            
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
        private static byte[] ReadAll(Stream input)
        {
            List<byte> l = new List<byte>();
            int bt = input.ReadByte();
            while (bt >= 0)
            {
                l.Add((byte)bt);
                bt = input.ReadByte();
            }
            return l.ToArray();
        }
        private static CryptoStream AES16HMAC(Stream input, string password)
        {
            Aes rm = Aes.Create();
            
            byte[] iv = new byte[16];
            input.Read(iv);
            
            rm.IV = iv;
            // original default iterations and algorithm - obsolete due to security risks
            Rfc2898DeriveBytes rdb = new Rfc2898DeriveBytes(password, rm.IV, 1000, HashAlgorithmName.SHA1);
            rm.Key = rdb.GetBytes(16);
            
            return new CryptoStream(input,  rm.CreateDecryptor(), CryptoStreamMode.Read, true);
        }
        private static CryptoStream AES32HMAC(Stream input, string password)
        {
            Aes rm = Aes.Create();
            
            byte[] iv = new byte[16];
            input.Read(iv);
            
            rm.IV = iv;
            // original default iterations and algorithm - obsolete due to security risks
            Rfc2898DeriveBytes rdb = new Rfc2898DeriveBytes(password, rm.IV, 1000, HashAlgorithmName.SHA1);
            rm.Key = rdb.GetBytes(32);
            
            return new CryptoStream(input,  rm.CreateDecryptor(), CryptoStreamMode.Read, true);
        }
        private static CryptoStream AES32SHA(Stream input, string password)
        {
            Aes rm = Aes.Create();
            
            byte[] iv = new byte[16];
            input.Read(iv);
            
            rm.IV = iv;
            rm.Key = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            
            return new CryptoStream(input, rm.CreateDecryptor(), CryptoStreamMode.Read, true);
        }
    }
}
