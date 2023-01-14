using System;
using System.IO;
using System.Security.Cryptography;

namespace Encryption
{
    public static class Encryption
    {
        public static void Encrypt(PasswordManager pm, string password, Stream output)
        {
            AesManaged rm = new AesManaged();

            rm.GenerateIV();

            Rfc2898DeriveBytes rdb = new Rfc2898DeriveBytes(password, rm.IV);
            rm.Key = rdb.GetBytes(16);

            output.Write(rm.IV);

            ICryptoTransform encryptor = rm.CreateEncryptor();

            using CryptoStream csEncrypt = new CryptoStream(output, encryptor, CryptoStreamMode.Write);
            
            // Write json file
            pm.GetJson(csEncrypt);
        }
        
        public static PasswordManager Decrypt(Stream input, string password)
        {
            AesManaged rm = new AesManaged();

            byte[] iv = new byte[16];
            input.Read(iv);

            rm.IV = iv;
            Rfc2898DeriveBytes rdb = new Rfc2898DeriveBytes(password, rm.IV);
            rm.Key = rdb.GetBytes(16);

            ICryptoTransform decryptor = rm.CreateDecryptor();

            using CryptoStream csDecrypt = new CryptoStream(input, decryptor, CryptoStreamMode.Read);
            
            return new PasswordManager(csDecrypt);
        }
    }
}
