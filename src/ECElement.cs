using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Zene.GUI;
using Zene.Structs;

namespace Encryption
{
    public class ECElement : Container
    {
        public ECElement(ILayout layout, ILayoutManager lm, EntryContainer ec)
            : base(layout)
        {
            LayoutManager = lm;
            EC = ec;
            Graphics.Colour = ColourF.Grey;
            
            AddChild(new Label(_tl2) { Text = ec.Name, TextSize = 20f, BorderWidth = 0f });
            
            Span<KeyValuePair<string, string>> span = CollectionsMarshal.AsSpan(ec.Entries);
            for (int i = 0; i < span.Length; i++)
            {
                AddChild(CreateEntryGraphic(span[i].Key));
            }
            
            if (Program.ReadOnly) { return; }
            
            _addEG = new Container(_cl);
            _addEG.LayoutManager = _scaleLayout;
            Button ae = new Button(new TextLayout(5f, 5f, 0f, 0f, 0.5f, 0f)) { Text = "Add Entry", TextSize = 20f };
            ae.Click += AddEntryEvent;
            _addEG.AddChild(ae);
            Button rm = new Button(new TextLayout(5f, 5f, 0f, 0f, 0.5f, 0f)) { Text = "Delete Group", TextSize = 20f };
            rm.Click += DeleteGroup;
            _addEG.AddChild(rm);
            
            AddChild(_addEG);
            
            // create add group
            _addGroup = new Container(_cl);
            _addGroup.LayoutManager = _scaleLayout;
            TextLayout tl = new TextLayout(5f, 5f, 0f, 0f, 250f, 0f, false);
            _addLabel = new TextInput(tl) { TextSize = 15f };
            _addValue = new TextInput(tl) { TextSize = 15f };
            _addGroup.AddChild(_addLabel);
            _addGroup.AddChild(_addValue);
            
            Button cc = new Button(_tl2)
            {
                Text = "Confirm",
                TextSize = 15f,
                BorderWidth = 0f
            };
            cc.Click += ConfirmEvent;
            _addGroup.AddChild(cc);
            
            Button cd = new Button(_tl2)
            {
                Text = "Cancel",
                TextSize = 15f,
                BorderWidth = 0f
            };
            cd.Click += CancelEvent;
            _addGroup.AddChild(cd);
        }
        
        private Container _addEG;
        public EntryContainer EC;
        private Layout _cl = new Layout(0f, 0f, 2f, 0f);
        private TextLayout _llb = new TextLayout(5f, 5f, 0f, 0f, 0.7f, 0f);
        private TextLayout _tl2 = new TextLayout(5f, 5f);
        private ScaleLayout2 _scaleLayout = new ScaleLayout2(5f);
        
        private Container _addGroup;
        private TextInput _addLabel;
        private TextInput _addValue;
        private int _addIndex;
        private IElement _oldGroup;
        
        private Container CreateEntryGraphic(string name)
        {
            Container c = new Container(_cl);
            c.LayoutManager = _scaleLayout;
            c.AddChild(new Label(_llb) { Text = name, TextSize = 15f });
            
            Button cc = new Button(_tl2)
            {
                Text = "Copy",
                TextSize = 15f,
                BorderWidth = 0f,
                Id = name
            };
            cc.Click += CopyEvent;
            c.AddChild(cc);
            
            if (!Program.ReadOnly)
            {
                Button ce = new Button(_tl2)
                {
                    Text = "Edit",
                    TextSize = 15f,
                    BorderWidth = 0f,
                    Id = name
                };
                ce.Click += EditEvent;
                c.AddChild(ce);
                
                Button cd = new Button(_tl2)
                {
                    Text = "Del",
                    TextSize = 15f,
                    BorderWidth = 0f,
                    Id = name
                };
                cd.Click += DeleteEvent;
                c.AddChild(cd);
            }
            
            return c;
        }
        
        private void DeleteEvent(object sender, EventArgs e)
        {
            Button ib = sender as Button;
            if (ib == null) { return; }
            
            EC.Entries.RemoveAll(t => ib.Id == t.Key);
            RemoveChild(ib.Parent);
        }
        private void EditEvent(object sender, EventArgs e)
        {
            // cancel old action first 
            // simpler if multiple cannot be changed at the same time
            if (_addGroup.Parent == this)
            {
                CancelEvent(null, null);
            }
            
            Button ib = sender as Button;
            if (ib == null) { return; }
            
            int index = EC.Entries.FindIndex(t => ib.Id == t.Key);
            _addIndex = index;
            _addLabel.Text = ib.Id;
            _addValue.Text = EC.Entries[index].Value;
            
            ListActions la = Children.StartGroupAction();
            
            _oldGroup = ib.Parent;
            la.Replace(ib.Parent, _addGroup);
            la.EndingFocus = _addLabel;
            la.Apply();
        }
        private void CopyEvent(object sender, EventArgs e)
        {
            Button ib = sender as Button;
            if (ib == null) { return; }
            
            Window.ClipBoard = EC.Entries.Find(t => ib.Id == t.Key).Value;
        }
        private void AddEntryEvent(object sender, EventArgs e)
        {
            // cancel old action first 
            // simpler if multiple cannot be changed at the same time
            if (_addGroup.Parent == this)
            {
                CancelEvent(null, null);
            }
            
            ListActions la = Children.StartGroupAction();
            _addLabel.Text = "";
            _addValue.Text = "";
            
            _oldGroup = null;
            _addIndex = EC.Entries.Count;
            EC.Entries.Add(new KeyValuePair<string, string>());
            
            // la.Remove(_addEG);
            int end = Children.IndexOf(_addEG);
            la.Insert(end, _addGroup);
            la.EndingFocus = _addLabel;
            la.Apply();
        }
        private void ConfirmEvent(object sender, EventArgs e)
        {
            string name = _addLabel.Text;
            if (EC.Entries.Exists(k => k.Key == name)) { return; }
            
            ListActions la = Children.StartGroupAction();
            IElement rep = ManageConfirm();
            la.Replace(_addGroup, rep);
            _oldGroup = null;
            // la.Add(_addEG);
            la.EndingFocus = _addEG;
            la.Apply();
        }
        private Container ManageConfirm()
        {
            string key = _addLabel.Text;
            string value = _addValue.Text;
            if (key == null || value == null) { return null; }
            key = key.Trim();
            if (key.Length == 0 || value.Length == 0) { return null; }
            
            EC.Entries[_addIndex] = new KeyValuePair<string, string>(key, value);
            return CreateEntryGraphic(key);
        }
        private void CancelEvent(object sender, EventArgs e)
        {
            ListActions la = Children.StartGroupAction();
            la.Replace(_addGroup, _oldGroup);
            _oldGroup = null;
            // la.Add(_addEG);
            la.EndingFocus = _addEG;
            la.Apply();
        }
        private void DeleteGroup(object sender, EventArgs e)
        {
            Parent.Children.Remove(this);
        }
    }
}