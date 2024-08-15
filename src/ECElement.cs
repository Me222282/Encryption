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
            
            AddChild(new Label(_tl2) { Text = ec.Name, TextSize = 20d, BorderWidth = 0 });
            
            Span<KeyValuePair<string, string>> span = CollectionsMarshal.AsSpan(ec.Entries);
            for (int i = 0; i < span.Length; i++)
            {
                AddEntry(span[i].Key);
            }
            
            if (Program.ReadOnly) { return; }
            
            _addEG = new Container(_cl);
            _addEG.LayoutManager = _scaleLayout;
            Button ae = new Button(new TextLayout(5d, 5d, 0d, 0d, 0.5, 0d)) { Text = "Add Entry", TextSize = 20d };
            ae.Click += AddEntryEvent;
            _addEG.AddChild(ae);
            Button rm = new Button(new TextLayout(5d, 5d, 0d, 0d, 0.5, 0d)) { Text = "Delete Group", TextSize = 20d };
            rm.Click += DeleteGroup;
            _addEG.AddChild(rm);
            
            AddChild(_addEG);
            
            // create add group
            _addGroup = new Container(_cl);
            _addGroup.LayoutManager = _scaleLayout;
            TextLayout tl = new TextLayout(5d, 5d, 0d, 0d, 250d, 0d, false);
            _addLabel = new TextInput(tl) { TextSize = 15d };
            _addValue = new TextInput(tl) { TextSize = 15d };
            _addGroup.AddChild(_addLabel);
            _addGroup.AddChild(_addValue);
            
            Button cc = new Button(_tl2)
            {
                Text = "Confirm",
                TextSize = 15,
                BorderWidth = 0
            };
            cc.Click += ConfirmEvent;
            _addGroup.AddChild(cc);
            
            Button cd = new Button(_tl2)
            {
                Text = "Cancel",
                TextSize = 15,
                BorderWidth = 0
            };
            cd.Click += CancelEvent;
            _addGroup.AddChild(cd);
        }
        
        private Container _addEG;
        private EntryContainer _ec;
        private Layout _cl = new Layout(0d, 0d, 1.9, 0d);
        private TextLayout _llb = new TextLayout(5d, 5d, 0d, 0d, 0.7, 0d);
        private TextLayout _tl2 = new TextLayout(5d, 5d);
        private TextLayout _tl3 = new TextLayout(5d, 5d);
        private ScaleLayout2 _scaleLayout = new ScaleLayout2(5d);
        
        private Container _addGroup;
        private TextInput _addLabel;
        private TextInput _addValue;
        
        private void AddEntry(string name)
        {
            Container c = new Container(_cl);
            c.LayoutManager = _scaleLayout;
            c.AddChild(new Label(_llb) { Text = name, TextSize = 15d });
            
            Button cc = new Button(_tl2)
            {
                Text = "Copy",
                TextSize = 15d,
                BorderWidth = 0,
                Id = name
            };
            cc.Click += CopyEvent;
            c.AddChild(cc);
            
            Button cd = new Button(_tl2)
            {
                Text = "Del",
                TextSize = 15d,
                BorderWidth = 0,
                Id = name
            };
            cd.Click += DeleteEvent;
            c.AddChild(cd);
            
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
            _addLabel.Text = "";
            _addValue.Text = "";
            RemoveChild(_addEG);
            AddChild(_addGroup);
            Handle.Focus = _addLabel;
        }
        private void ConfirmEvent(object sender, EventArgs e)
        {
            RemoveChild(_addGroup);
            ManageConfirm();
            AddChild(_addEG);
            Handle.Focus = _addEG;
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
            RemoveChild(_addGroup);
            AddChild(_addEG);
            Handle.Focus = _addEG;
        }
        private void DeleteGroup(object sender, EventArgs e)
        {
            Parent.Children.Remove(this);
        }
    }
}