using System;
using System.IO;
using Zene.GUI;
using Zene.Structs;
using Zene.Windowing;

namespace Encryption
{
    class Program : GUIWindow
    {
        static void Main(string[] args)
        {
            Core.Init();
            
            Window w;
            
            if (args.Length > 0)
            {
                w = new Program(800, 500, "WORK", new FileStream(args[0], FileMode.Open));
            }
            else
            {
                w = new Program(800, 500, "WORK");
            }
            
            w.RunMultithread();
            
            Core.Terminate();
        }
        
        public Program(int width, int height, string title, Stream file)
            : base(width, height, title)
        {
            //RootElement.LayoutManager = new BlockLayout(10d);
            
            //AddChild();
            
            _file = file;
            _fileOpen = true;
            
            _lm = new LayoutManager(RootElement, new Xml());
            LoadLayout(LayoutSelect.Input);
        }
        
        public Program(int width, int height, string title)
            : base(width, height, title)
        {
            //RootElement.LayoutManager = new BlockLayout(10d);
            
            //AddChild();
            
            _file = new FileStream("passwords.aes", FileMode.Create);
            _fileOpen = false;
            
            _lm = new LayoutManager(RootElement, new Xml());
            LoadLayout(LayoutSelect.Input);
        }
        
        private readonly bool _fileOpen;
        private Stream _file;
        private PasswordManager _pm;
        private string _password;
        
        private LayoutManager _lm;
        
        private void LoadLayout(LayoutSelect layout) => _lm.SelectLayout(layout);
        
        private void OnPasswordEntered(object sender, EventArgs e)
        {
            PasswordEnter pe = sender as PasswordEnter;
            _password = pe.GetPassword();
            
            if (_fileOpen)
            {
                _pm = Encryption.Decrypt(_file, _password);
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
        private ScaleLayout _scaleLayout = new ScaleLayout(5d);
        private Layout _countainerL = new Layout(0d, 0d, 1.9d, 0d);
        private void LoadPMElements(IElement container)
        {
            foreach (EntryContainer ec in _pm)
            {
                AddContainer(container, ec);
            }
            
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
            c.Graphics.Colour = ColourF.Grey;
            c.LayoutManager = _scaleLayout;
            c.AddChild(new Label(new TextLayout(5d, 5d)) { Text = ec.Name, TextSize = 20d, BorderWidth = 0 });
            c.AddChild(new Button(new TextLayout(5d, 5d, 0d, 0d, 0.5, 0d)) { Text = "Add Entry", TextSize = 20d });
            
            parent.Children.Add(c);
        }
        
        private void AddGroupEvent(object sender, EventArgs e)
        {
            TempTextBox ttb = new TempTextBox(new TextLayout(5d, 5d, 0d, 0d, 1.9d, 0d, true))
            {
                TextSize = 30d,
            };
            ttb.Entered += PushGroup;
            _lm.ViewContainer.Children.Remove(_addGroup);
            _lm.ViewContainer.Children.Add(ttb);
            RootElement.Focus = ttb;
        }
        private void PushGroup(object sender, EventArgs e)
        {
            TempTextBox ttb = sender as TempTextBox;
            if (ttb == null) { return; }
            
            string name = ttb.Text;
            _lm.ViewContainer.Children.Remove(ttb);
            if (name == null) { return; }
            name = name.Trim();
            if (name.Length == 0) { return; }
            
            AddContainer(_lm.ViewContainer, _pm.AddGroup(name));
            _lm.ViewContainer.Children.Add(_addGroup);
        }
    }
}
