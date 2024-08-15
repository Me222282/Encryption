using System;
using System.IO;
using Zene.GUI;
using Zene.Structs;
using Zene.Windowing;

namespace Encryption
{
    class Program : GUIWindow
    {
        public static bool ReadOnly { get; private set; } = false;
        
        static void Main(string[] args)
        {
            Core.Init();
            
            int i = 0;
            if (args.Length > 0 && args[i] == "-r")
            {
                i++;
                ReadOnly = true;
            }
            
            Window w;
            
            if (args.Length > i)
            {
                FileAccess fa = ReadOnly ? FileAccess.Read : FileAccess.ReadWrite;
                w = new Program(800, 500, "Aes Manager", new FileStream(args[i], FileMode.Open, fa));
            }
            else
            {
                w = new Program(800, 500, "Aes Manager");
            }
            
            w.RunMultithread();
            
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
            
            Actions.Push(() =>
            {
                LoadPMElements(_lm.ViewContainer);
                LoadLayout(LayoutSelect.View);
            });
        }
        
        private Button _addGroup;
        private TempTextBox _ttb;
        private ScaleLayout _scaleLayout = new ScaleLayout(5d);
        private Layout _countainerL = new Layout(0d, 0d, 1.9d, 0d);
        private void LoadPMElements(IElement container)
        {
            container.Children.Clear();
            
            foreach (EntryContainer ec in _pm)
            {
                //AddContainer(container, ec);
                container.Children.Add(new ECElement(_countainerL, _scaleLayout, ec));
            }
            
            if (Program.ReadOnly) { return; }
            
            _addGroup = new Button(new TextLayout(5d, 5d, 0d, 0d, 1.9d, 0d, true))
            {
                TextSize = 30d,
                Text = "Add Group",
                BorderWidth = 0
            };
            _addGroup.Click += AddGroupEvent;
            container.Children.Add(_addGroup);
        }
        private void AddContainer(IElement parent, EntryContainer ec)
        {
            Container c = new Container(_countainerL);
            c.Graphics.Colour = ColourF.DarkGrey;
            c.LayoutManager = _scaleLayout;
            c.AddChild(new Label(new TextLayout(5d, 5d)) { Text = ec.Name, TextSize = 20d, BorderWidth = 0 });
            c.AddChild(new Button(new TextLayout(5d, 5d, 0d, 0d, 0.5, 0d)) { Text = "Add Entry", TextSize = 20d });
            
            parent.Children.Add(c);
        }
        
        private void CancelGroup(object sender, EventArgs e)
        {
            Actions.Push(() =>
            {
                _lm.ViewContainer.Children.Remove(_ttb);
                _lm.ViewContainer.Children.Add(_addGroup);
                RootElement.Focus = _addGroup;
            });
        }
        private void AddGroupEvent(object sender, EventArgs e)
        {
            Actions.Push(() =>
            {
                _ttb.Text = "";
                _lm.ViewContainer.Children.Remove(_addGroup);
                _lm.ViewContainer.Children.Add(_ttb);
                RootElement.Focus = _ttb;
            });
        }
        private void PushGroup(object sender, EventArgs e)
        {
            Actions.Push(() =>
            {
                string name = _ttb.Text;
                _lm.ViewContainer.Children.Remove(_ttb);
                if (name == null) { return; }
                name = name.Trim();
                if (name.Length == 0) { return; }
                
                ECElement ece = new ECElement(_countainerL, _scaleLayout, _pm.AddGroup(name));
                _lm.ViewContainer.Children.Add(ece);
                _lm.ViewContainer.Children.Add(_addGroup);
                RootElement.Focus = ece;
            });
        }
    }
}
