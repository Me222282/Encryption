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
            
            output.Position = 0;
            output.Write(rm.IV);

            ICryptoTransform encryptor = rm.CreateEncryptor();

            using CryptoStream csEncrypt = new CryptoStream(output, encryptor, CryptoStreamMode.Write, true);
            
            // Write json file
            pm.WriteToStream(csEncrypt);
        }
        
        public static PasswordManager Decrypt(Stream input, string password)
        {
            AesManaged rm = new AesManaged();

            byte[] iv = new byte[16];
            input.Position = 0;
            input.Read(iv);

            rm.IV = iv;
            Rfc2898DeriveBytes rdb = new Rfc2898DeriveBytes(password, rm.IV);
            rm.Key = rdb.GetBytes(16);

            ICryptoTransform decryptor = rm.CreateDecryptor();
            
            using CryptoStream csDecrypt = new CryptoStream(input, decryptor, CryptoStreamMode.Read, true);
            
            try
            {
                return new PasswordManager(csDecrypt);
            }
            catch (Exception)
            {
                csDecrypt.Dispose();
                decryptor.Dispose();
                rdb.Dispose();
                rm.Dispose();
                return null;
            }
        }
    }
}
