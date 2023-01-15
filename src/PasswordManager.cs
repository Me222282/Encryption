using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Encryption
{
    public class PasswordManager
    {
        public PasswordManager(string json)
            : this(new MemoryStream(Encoding.UTF8.GetBytes(json)))
        {
            
        }
        public PasswordManager(Stream stream)
        {
            JsonElement root;
            
            try
            {
                root = JsonDocument.Parse(stream).RootElement;
            }
            catch (JsonException) { return; }
            
            int length = root.GetArrayLength();
            
            foreach (JsonProperty jp in root.EnumerateObject())
            {
                string name = jp.Name;
                JsonElement je = jp.Value;
                
                if (je.ValueKind != JsonValueKind.String)
                {
                    throw new Exception("Invalid JSON.");
                }
                
                _passwords.Add(name, je.GetString());
            }
        }
        
        public PasswordManager()
        {
        }
        
        private readonly Dictionary<string, string> _passwords = new Dictionary<string, string>();
        
        public string GetPassword(string value) => _passwords[value];
        public void AddPassword(string name, string password) => _passwords.Add(name, password);
        public bool RemovePassword(string name) => _passwords.Remove(name);
        
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => _passwords.GetEnumerator();
        
        public void GetJson(Stream stream)
        {
            Utf8JsonWriter jw = new Utf8JsonWriter(stream);
            
            foreach (KeyValuePair<string, string> keyPair in _passwords)
            {
                jw.WriteString(keyPair.Key, keyPair.Value);
            }
        }
    }
}
