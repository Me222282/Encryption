using System;
using System.Collections.Generic;
using System.IO;
using Zene.GUI;
using Zene.Windowing;

namespace Encryption
{
    public enum LayoutSelect
    {
        Input,
        View,
        Empty
    }
    
    public class LayoutManager
    {
        private class ElementManager : ElementList
        {
            public ElementManager(IElement source)
                : base(source)
            {
                
            }
            
            private readonly List<IElement> _elements = new List<IElement>();
            public new IElement this[int index] => _elements[index];
            
            public new int Length => _elements.Count;
            
            public override void Add(IElement e) => _elements.Add(e);
            public override void Clear() => _elements.Clear();
            public override void RemoveAt(int index) => _elements.RemoveAt(index);
        }
        
        public LayoutManager(RootElement rootElement, Program p)
        {
            _root = rootElement;
            
            _inputLayout = new ElementManager(_root);
            _viewLayout = new ElementManager(_root);
            _emptyLayout = new ElementManager(_root);
#if DEBUG
            Xml xml = new Xml();
            string folder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            xml.LoadGUI(_inputLayout, File.ReadAllText(folder + "/Layouts/passwordInput.xml"));
            xml.LoadGUI(_viewLayout, File.ReadAllText(folder + "/Layouts/passwordManage.xml"));
            xml.LoadGUI(_emptyLayout, File.ReadAllText(folder + "/Layouts/empty.xml"));
#else
            passwordInput.LoadGUI(_inputLayout, p);
            passwordManage.LoadGUI(_viewLayout, p);
            empty.LoadGUI(_emptyLayout);
#endif
        }
        
        private RootElement _root;
        
        private ElementManager _inputLayout;
        private ElementManager _viewLayout;
        private ElementManager _emptyLayout;
        
        public IElement ViewContainer => _viewLayout[0].Children[2];
        public TextElement PathLabel => _inputLayout[0] as TextElement;
        private IElement _errorLabel => _inputLayout[2];
        
        public void SelectLayout(LayoutSelect layout)
        {
            ListActions la = _root.Elements.StartGroupAction();
            la.Clear();
            
            ElementManager select = layout switch
            {
                LayoutSelect.Input => _inputLayout,
                LayoutSelect.View => _viewLayout,
                LayoutSelect.Empty => _emptyLayout,
                _ => _viewLayout
            };
            
            // if (layout == LayoutSelect.View)
            // {
            //     _root.AddChild(new Button(new TextLayout(5d, 5d, 100d, -50d, false)) { Text = "Add", TextSize = 20d });
            //     return;
            // }
            
            _errorLabel.Properties.Visable = false;
            
            for (int i = 0; i < select.Length; i++)
            {
                la.Add(select[i]);
            }
            
            la.Apply();
        }
        
        public void ShowError()
        {
            _errorLabel.Properties.Visable = true;
            _root.LayoutElement(_root);
        }
    }
}
