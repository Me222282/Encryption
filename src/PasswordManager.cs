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
        public int Order { get; set; }
        public string Name { get; set; }
        public List<KeyValuePair<string, string>> Entries { get; } = new List<KeyValuePair<string, string>>();
    }
    
    public class PasswordManager : IEnumerable<EntryContainer>
    {
        private const string OrderLabel = "ORDER";
        
        public PasswordManager(string json, bool old)
            : this(new MemoryStream(Encoding.UTF8.GetBytes(json)), old)
        {
            
        }
        public PasswordManager(Stream stream, bool old)
        {
            JsonElement root = JsonDocument.Parse(stream).RootElement;
            
            int max = 0;
            int count = 0;
            foreach (JsonProperty jp in root.EnumerateObject())
            {
                EntryContainer ec = new EntryContainer();
                
                ec.Order = -1;
                ec.Name = jp.Name;
                JsonElement je = jp.Value;
                
                foreach (JsonProperty jpEntry in je.EnumerateObject())
                {
                    if (!old && jpEntry.Value.ValueKind == JsonValueKind.Number && jpEntry.Name == OrderLabel)
                    {
                        ec.Order = jpEntry.Value.GetInt32();
                        continue;
                    }
                    
                    if (jpEntry.Value.ValueKind != JsonValueKind.String)
                    {
                        throw new Exception("Invalid JSON.");
                    }
                    
                    ec.Entries.Add(new KeyValuePair<string, string>(jpEntry.Name, jpEntry.Value.GetString()));
                }
                
                // support old files
                if (old) { ec.Order = count; }
                if (ec.Order == -1)
                {
                    throw new Exception("Invalid JSON - missing group order.");
                }
                if (max < ec.Order) { max = ec.Order; }
                
                _groups.Add(ec);
                count++;
            }
            _maxOrder = max;
        }
        
        public PasswordManager() { }
        
        private int _maxOrder;
        public int GroupCount => _groups.Count;
        public EntryContainer this[int index] => _groups[index];
        
        private List<EntryContainer> _groups = new List<EntryContainer>();
        public EntryContainer AddGroup(string name)
        {
            _maxOrder++;
            EntryContainer ec = new EntryContainer()
            {
                Order = _maxOrder,
                Name = name
            };
            _groups.Add(ec);
            return ec;
        }
        public bool CanAddGroup(string name) => !_groups.Exists(g => g.Name == name);
        public void RemoveGroup(EntryContainer group) => _groups.Remove(group);
        
        public IEnumerator<EntryContainer> GetEnumerator() => _groups.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _groups.GetEnumerator();
        
        public void WriteToStream(Stream stream)
        {
            Utf8JsonWriter jw = new Utf8JsonWriter(stream, new JsonWriterOptions() { Indented = true });
            
            jw.WriteStartObject();
            
            foreach (EntryContainer ec in _groups)
            {
                jw.WriteStartObject(ec.Name);
                jw.WriteNumber(OrderLabel, ec.Order);
                foreach (KeyValuePair<string, string> keyPair in ec.Entries)
                {
                    // in middle of entering new value
                    if (keyPair.Key == null) { continue; }
                    
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
