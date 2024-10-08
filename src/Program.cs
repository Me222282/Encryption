﻿using System;
using System.IO;
using Zene.GUI;
using Zene.Structs;
using Zene.Windowing;

namespace Encryption
{
    class Program : GUIWindow
    {
        public static bool ReadOnly { get; private set; } = false;
        public static Encryption.Algorithm Algorithm { get; private set; } = Encryption.Algorithm.AES32SHA;
        
        static void Main(string[] args)
        {
            Core.Init();
            
            int i = 0;
            bool algArg = false;
            for (; i < args.Length; i++)
            {
                if (algArg)
                {
                    Algorithm = args[i].ToLower() switch
                    {
                        "aes16_hmac" => Encryption.Algorithm.AES16HMAC,
                        "aes32_hmac" => Encryption.Algorithm.AES32HMAC,
                        "aes32_sha" => Encryption.Algorithm.AES32SHA,
                        _ => (Encryption.Algorithm)(-1)
                    };
                    if ((int)Algorithm == -1)
                    {
                        Console.WriteLine("Invalid algorithm.");
                        return;
                    }
                    algArg = false;
                    continue;
                }
                
                if (args[i] == "-r" || args[i] == "--readonly")
                {
                    ReadOnly = true;
                    continue;
                }
                if (args[i] == "-a" || args[i] == "--algorithm")
                {
                    algArg = true;
                    continue;
                }
                
                break;
            }
            if (algArg)
            {
                Console.WriteLine("Missing argument.");
                return;
            }
            
            Window w;
            
            if (args.Length > i)
            {
                string path = args[i];
                if (!File.Exists(path))
                {
                    Console.WriteLine($"Could not find file: {path}");
                    return;
                }
                
                FileAccess fa = ReadOnly ? FileAccess.Read : FileAccess.ReadWrite;
                w = new Program(800, 500, "AES Manager", new FileStream(path, FileMode.Open, fa));
            }
            else
            {
                w = new Program(800, 500, "AES Manager");
            }
            
            w.RunMultithread();
            w.Dispose();
            
            Core.Terminate();
        }
        
        public Program(int width, int height, string title, Stream file)
            : base(width, height, title)
        {
            _file = file;
            _fileOpen = true;
            
            _lm = new LayoutManager(RootElement, new Xml());
            LoadLayout(LayoutSelect.Input);
        }
        
        public Program(int width, int height, string title)
            : base(width, height, title)
        {
            //_file = new FileStream("passwords.aes", FileMode.Create);
            _fileOpen = false;
            
            _lm = new LayoutManager(RootElement, new Xml());
            //LoadLayout(LayoutSelect.Input);
        }
        
        private bool _fileOpen;
        private Stream _file;
        private PasswordManager _pm;
        private string _password;
        
        private LayoutManager _lm;
        
        private void LoadLayout(LayoutSelect layout) => _lm.SelectLayout(layout);

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            
            if (e[Keys.S] && e[Mods.Control])
            {
                if (_pm == null || ReadOnly) { return; }
                Encryption.Encrypt(_pm, _password, _file);
                return;
            }
            if (e[Keys.A] && e[Mods.Control])
            {
                if (ReadOnly) { return; }
                AddGroupEvent(null, null);
                return;
            }
            if (e[Keys.C] && e[Mods.Control] && e[Mods.Shift])
            {
                if (ReadOnly) { return; }
                //if (RootElement.Elements.Length > 0) { return; }
                if (_pm != null && !ReadOnly)
                {
                    Encryption.Encrypt(_pm, _password, _file);
                    _pm = null;
                }
                LoadLayout(LayoutSelect.Input);
                if (_file != null) { _file.Close(); }
                _file = new FileStream("passwords.aes", FileMode.Create);
                _fileOpen = false;
                return;
            }
        }
        protected override void OnStop(EventArgs e)
        {
            base.OnStop(e);
            
            if (_pm != null && !ReadOnly)
            {
                Encryption.Encrypt(_pm, _password, _file);
            }
            
            if (_file == null) { return; }
            _file.Close();
        }
        protected override void OnStart(EventArgs e)
        {
            base.OnStart(e);
            
            _ttb = new TempTextBox(new TextLayout(5d, 5d, 0d, 0d, 1.9d, 0d, true))
            {
                TextSize = 30d,
            };
            _ttb.Entered += PushGroup;
            _ttb.Canceled += CancelGroup;
        }
        protected override void OnFileDrop(FileDropEventArgs e)
        {
            base.OnFileDrop(e);
            
            //if (RootElement.Elements.Length > 0) { return; }
            if (_pm != null && !Program.ReadOnly)
            {
                Encryption.Encrypt(_pm, _password, _file);
                _pm = null;
            }
            LoadLayout(LayoutSelect.Input);
            if (_file != null) { _file.Close(); }
            _file = new FileStream(e.Paths[0], FileMode.Open);
            _fileOpen = true;
        }

        private void OnPasswordEntered(object sender, EventArgs e)
        {
            PasswordEnter pe = sender as PasswordEnter;
            _password = pe.GetPassword();
            pe.Clear();
            
            if (_fileOpen)
            {
                _pm = Encryption.Decrypt(_file, _password);
                if (_pm == null) { return; }
            }
            else
            {
                _pm = new PasswordManager();
            }
            
            ListActions la = _lm.ViewContainer.Children.StartGroupAction();
            LoadPMElements(la);
            la.Apply();
            LoadLayout(LayoutSelect.View);
        }
        
        private Button _addGroup;
        private TempTextBox _ttb;
        private ScaleLayout _scaleLayout = new ScaleLayout(5d);
        private Layout _countainerL = new Layout(0d, 0d, 1.9d, 0d);
        private void LoadPMElements(ListActions container)
        {
            container.Clear();
            
            foreach (EntryContainer ec in _pm)
            {
                //AddContainer(container, ec);
                container.Add(new ECElement(_countainerL, _scaleLayout, ec));
            }
            
            if (Program.ReadOnly) { return; }
            
            _addGroup = new Button(new TextLayout(5d, 5d, 0d, 0d, 1.9d, 0d, true))
            {
                TextSize = 30d,
                Text = "Add Group",
                BorderWidth = 0
            };
            _addGroup.Click += AddGroupEvent;
            container.Add(_addGroup);
        }
        
        private void CancelGroup(object sender, EventArgs e)
        {
            ListActions la = _lm.ViewContainer.Children.StartGroupAction();
            la.Remove(_ttb);
            la.Add(_addGroup);
            la.EndingFocus = _addGroup;
            la.Apply();
        }
        private void AddGroupEvent(object sender, EventArgs e)
        {
            ListActions la = _lm.ViewContainer.Children.StartGroupAction();
            _ttb.Text = "";
            la.Remove(_addGroup);
            la.Add(_ttb);
            la.EndingFocus = _ttb;
            la.Apply();
        }
        private void PushGroup(object sender, EventArgs e)
        {
            ListActions la = _lm.ViewContainer.Children.StartGroupAction();
            
            string name = _ttb.Text;
            la.Remove(_ttb);
            if (name == null) { return; }
            name = name.Trim();
            if (name.Length == 0) { return; }
            
            ECElement ece = new ECElement(_countainerL, _scaleLayout, _pm.AddGroup(name));
            la.Add(ece);
            la.Add(_addGroup);
            la.EndingFocus = ece;
            
            la.Apply();
        }
    }
}
