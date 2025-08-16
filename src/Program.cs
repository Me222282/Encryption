using System;
using System.IO;
using Zene.GUI;
using Zene.Structs;
using Zene.Windowing;

namespace Encryption
{
    public class Program : GUIWindow
    {
        public static bool ReadOnly { get; private set; } = false;
        
        static void Main(string[] args)
        {
            Core.Init();
            
            int i = 0;
            for (; i < args.Length; i++)
            {   
                if (args[i] == "-r" || args[i] == "--readonly")
                {
                    ReadOnly = true;
                    continue;
                }
                
                break;
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
                
                w = new Program(800, 500, "AES Manager", path);
            }
            else
            {
                w = new Program(800, 500, "AES Manager");
            }
            
            w.RunMultithread();
            w.Dispose();
            
            Core.Terminate();
        }
        
        public Program(int width, int height, string title, string path)
            : base(width, height, title)
        {
            _file = new FileStream(path, FileMode.Open, ReadOnly ? FileAccess.Read : FileAccess.ReadWrite);
            _fileOpen = true;
            
            _lm = new LayoutManager(RootElement, this);
            Actions.Push(() => _lm.PathLabel.Text = path);
            LoadLayout(LayoutSelect.Input);
        }
        
        public Program(int width, int height, string title)
            : base(width, height, title)
        {
            // _file = new FileStream("passwords.aes", FileMode.Create);
            _fileOpen = false;
            
            _lm = new LayoutManager(RootElement, this);
            LoadLayout(LayoutSelect.Empty);
        }
        
        private bool _fileOpen;
        private Stream _file;
        private PasswordManager _pm;
        private byte[] _key;
        
        private LayoutManager _lm;
        
        private void LoadLayout(LayoutSelect layout) => _lm.SelectLayout(layout);

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            
            if (e[Keys.S] && e[Mods.Control])
            {
                if (_pm == null || ReadOnly) { return; }
                Encryption.Encrypt(_pm, _key, _file);
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
                if (_pm != null)
                {
                    Encryption.Encrypt(_pm, _key, _file);
                    _pm = null;
                }
                _lm.PathLabel.Text = "passwords.aes";
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
                Encryption.Encrypt(_pm, _key, _file);
            }
            
            if (_file == null) { return; }
            _file.Close();
        }
        protected override void OnStart(EventArgs e)
        {
            base.OnStart(e);
            
            _ttb = new TempTextBox(new TextLayout(5f, 5f, 0f, 0f, 1.9f, 0f, true))
            {
                TextSize = 30f,
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
                Encryption.Encrypt(_pm, _key, _file);
                _pm = null;
            }
            _lm.PathLabel.Text = e.Paths[0];
            LoadLayout(LayoutSelect.Input);
            if (_file != null) { _file.Close(); }
            _file = new FileStream(e.Paths[0], FileMode.Open);
            _fileOpen = true;
        }

        internal void OnPasswordEntered(object sender, EventArgs e)
        {
            PasswordEnter pe = sender as PasswordEnter;
            string password = pe.GetPassword();
            pe.Clear();
            _key = Encryption.GetEncryptKey(password);
            
            if (_fileOpen)
            {
                _pm = Encryption.Decrypt(_file, password);
                if (_pm == null) { return; }
            }
            else
            {
                _pm = new PasswordManager();
            }
            
            GC.Collect();
            
            ListActions la = _lm.ViewContainer.Children.StartGroupAction();
            LoadPMElements(la);
            la.Apply();
            // reset to origin
            _lm.ViewContainer.Properties.SetYScroll(0f);
            _lm.ViewContainer.Properties.SetXScroll(0f);
            LoadLayout(LayoutSelect.View);
        }
        
        private Button _addGroup;
        private TempTextBox _ttb;
        private ScaleLayout _scaleLayout = new ScaleLayout(5f);
        private Layout _countainerL = new Layout(0f, 0f, 2f, 0f);
        private void LoadPMElements(ListActions container)
        {
            container.Clear();
            
            foreach (EntryContainer ec in _pm)
            {
                //AddContainer(container, ec);
                container.Add(new ECElement(_countainerL, _scaleLayout, ec));
            }
            
            if (Program.ReadOnly) { return; }
            
            _addGroup = new Button(new TextLayout(5f, 5f, 0f, 0f, 2f, 0f, true))
            {
                TextSize = 30f,
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
        
        internal void SortAlphabetically(object sender, EventArgs e)
        {
            _lm.ViewContainer.Children.Sort((a, b) =>
            {
                if (a is Button) { return 1; }
                if (b is Button) { return -1; }
                
                ECElement ece1 = a as ECElement;
                ECElement ece2 = b as ECElement;
                if (ece1 == null || ece2 == null) { return 0; }
                
                return ece1.EC.Name.CompareTo(ece2.EC.Name);
            });
        }
        internal void SortTimeOrder(object sender, EventArgs e)
        {
            _lm.ViewContainer.Children.Sort((a, b) =>
            {
                if (a is Button) { return 1; }
                if (b is Button) { return -1; }
                
                ECElement ece1 = a as ECElement;
                ECElement ece2 = b as ECElement;
                if (ece1 == null || ece2 == null) { return 0; }
                
                return ece1.EC.Order.CompareTo(ece2.EC.Order);
            });
        }
    }
}
