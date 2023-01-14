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
            
            Stream stream;
            
            if (args.Length > 0)
            {
                stream = new FileStream(args[0], FileMode.Open);
            }
            else
            {
                stream = new FileStream("passwords.aes", FileMode.Create);
            }
            
            Window w = new Program(800, 500, "WORK", stream);
            w.Run();
            
            Core.Terminate();
        }
        
        public Program(int width, int height, string title, Stream file)
            : base(width, height, title)
        {
            //RootElement.LayoutManager = new BlockLayout(10d);
            
            //AddChild();
            
            _file = file;
        }
        
        private Stream _file;
        private PasswordManager _pm;
    }
}
