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
            _ec = ec;
            Graphics.Colour = ColourF.Grey;
            
            AddChild(new Label(_tl2) { Text = ec.Name, TextSize = 20f, BorderWidth = 0f });
            
            Span<KeyValuePair<string, string>> span = CollectionsMarshal.AsSpan(ec.Entries);
            for (int i = 0; i < span.Length; i++)
            {
                AddEntry(span[i].Key);
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
        private EntryContainer _ec;
        private Layout _cl = new Layout(0f, 0f, 1.9f, 0f);
        private TextLayout _llb = new TextLayout(5f, 5f, 0f, 0f, 0.7f, 0f);
        private TextLayout _tl2 = new TextLayout(5f, 5f);
        private TextLayout _tl3 = new TextLayout(5f, 5f);
        private ScaleLayout2 _scaleLayout = new ScaleLayout2(5f);
        
        private Container _addGroup;
        private TextInput _addLabel;
        private TextInput _addValue;
        
        private void AddEntry(string name)
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
            
            AddChild(c);
        }
        
        private void DeleteEvent(object sender, EventArgs e)
        {
            Button ib = sender as Button;
            if (ib == null) { return; }
            
            _ec.Entries.RemoveAll(t => ib.Id == t.Key);
            RemoveChild(ib.Parent);
        }
        private void CopyEvent(object sender, EventArgs e)
        {
            Button ib = sender as Button;
            if (ib == null) { return; }
            
            Window.ClipBoard = _ec.Entries.Find(t => ib.Id == t.Key).Value;
        }
        private void AddEntryEvent(object sender, EventArgs e)
        {
            ListActions la = Children.StartGroupAction();
            _addLabel.Text = "";
            _addValue.Text = "";
            la.Remove(_addEG);
            la.Add(_addGroup);
            la.EndingFocus = _addLabel;
            la.Apply();
        }
        private void ConfirmEvent(object sender, EventArgs e)
        {
            ListActions la = Children.StartGroupAction();
            la.Remove(_addGroup);
            ManageConfirm();
            la.Add(_addEG);
            la.EndingFocus = _addEG;
            la.Apply();
        }
        private void ManageConfirm()
        {
            string key = _addLabel.Text;
            string value = _addValue.Text;
            if (key == null || value == null) { return; }
            key = key.Trim();
            if (key.Length == 0 || value.Length == 0) { return; }
            
            _ec.Entries.Add(new KeyValuePair<string, string>(key, value));
            AddEntry(key);
        }
        private void CancelEvent(object sender, EventArgs e)
        {
            ListActions la = Children.StartGroupAction();
            la.Remove(_addGroup);
            la.Add(_addEG);
            la.EndingFocus = _addEG;
            la.Apply();
        }
        private void DeleteGroup(object sender, EventArgs e)
        {
            Parent.Children.Remove(this);
        }
    }
}