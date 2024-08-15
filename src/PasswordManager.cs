using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Encryption
{
    public class EntryContainer
    {
        public string Name { get; set; }
        public List<KeyValuePair<string, string>> Entries { get; } = new List<KeyValuePair<string, string>>();
    }
    
    public class PasswordManager : IEnumerable<EntryContainer>
    {
        public PasswordManager(string json)
            : this(new MemoryStream(Encoding.UTF8.GetBytes(json)))
        {
            
        }
        public PasswordManager(Stream stream)
        {
            JsonElement root = JsonDocument.Parse(stream).RootElement;
            
            // try
            // {
            //     root = JsonDocument.Parse(stream).RootElement;
            // }
            // catch (JsonException)
            // {
            //     Console.WriteLine("Failed to load json.");
            //     return;
            // }
            
            //int length = root.GetArrayLength();
            
            foreach (JsonProperty jp in root.EnumerateObject())
            {
                EntryContainer ec = new EntryContainer();
                
                ec.Name = jp.Name;
                JsonElement je = jp.Value;
                
                foreach (JsonProperty jpEntry in je.EnumerateObject())
                {
                    if (jpEntry.Value.ValueKind != JsonValueKind.String)
                    {
                        throw new Exception("Invalid JSON.");
                    }
                    
                    ec.Entries.Add(new KeyValuePair<string, string>(jpEntry.Name, jpEntry.Value.GetString()));
                }
                
                _groups.Add(ec);
            }
        }
        
        public PasswordManager() { }
        
        public int GroupCount => _groups.Count;
        public EntryContainer this[int index] => _groups[index];
        
        private List<EntryContainer> _groups = new List<EntryContainer>();
        public EntryContainer AddGroup(string name)
        {
            EntryContainer ec = new EntryContainer()
            {
                Name = name
            };
            _groups.Add(ec);
            return ec;
        }
        public void RemoveGroup(string name) => _groups.RemoveAll(ec => ec.Name == name);
        
        public IEnumerator<EntryContainer> GetEnumerator() => _groups.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _groups.GetEnumerator();
        
        public void WriteToStream(Stream stream)
        {
            Utf8JsonWriter jw = new Utf8JsonWriter(stream, new JsonWriterOptions() { Indented = true });
            
            jw.WriteStartObject();
            
            foreach (EntryContainer ec in _groups)
            {
                jw.WriteStartObject(ec.Name);
                foreach (KeyValuePair<string, string> keyPair in ec.Entries)
                {
                    jw.WriteString(keyPair.Key, keyPair.Value);
                }
                jw.WriteEndObject();
            }
            jw.WriteEndObject();
            jw.Flush();
            jw.Dispose();
        }
    }
}
