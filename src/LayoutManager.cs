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
        View
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
        
        public LayoutManager(RootElement rootElement, Xml xml)
        {
            _root = rootElement;
            _xml = xml;
            
            _inputLayout = new ElementManager(_root);
            _viewLayout = new ElementManager(_root);
            
            string folder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            
            _xml.LoadGUI(_inputLayout, File.ReadAllText(folder + "/Layouts/passwordInput.xml"));
            _xml.LoadGUI(_viewLayout, File.ReadAllText(folder + "/Layouts/passwordManage.xml"));
        }
        
        private Xml _xml;
        private RootElement _root;
        
        private ElementManager _inputLayout;
        private ElementManager _viewLayout;
        
        public IElement ViewContainer => _viewLayout[0];
        
        public void SelectLayout(LayoutSelect layout)
        {
            ListActions la = _root.Elements.StartGroupAction();
            la.Clear();
            
            ElementManager select = layout switch
            {
                LayoutSelect.Input => _inputLayout,
                LayoutSelect.View => _viewLayout,
                _ => _viewLayout
            };
            
            // if (layout == LayoutSelect.View)
            // {
            //     _root.AddChild(new Button(new TextLayout(5d, 5d, 100d, -50d, false)) { Text = "Add", TextSize = 20d });
            //     return;
            // }
            
            for (int i = 0; i < select.Length; i++)
            {
                la.Add(select[i]);
            }
            
            la.Apply();
        }
    }
}
