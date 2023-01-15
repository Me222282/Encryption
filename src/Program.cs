using System;
using System.IO;
using Zene.GUI;
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
            
            LoadLayout(LayoutSelect.View);
        }
    }
}
