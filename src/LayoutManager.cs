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
            
            _xml.LoadGUI(_inputLayout, File.ReadAllText("Layouts/passwordInput.xml"));
            _xml.LoadGUI(_viewLayout, File.ReadAllText("Layouts/passwordManage.xml"));
        }
        
        private Xml _xml;
        private RootElement _root;
        
        private ElementManager _inputLayout;
        private ElementManager _viewLayout;
        
        public void SelectLayout(LayoutSelect layout)
        {
            _root.ClearChildren();
            
            ElementManager select = layout switch
            {
                LayoutSelect.Input => _inputLayout,
                LayoutSelect.View => _viewLayout,
                _ => _viewLayout
            };
            
            for (int i = 0; i < select.Length; i++)
            {
                _root.AddChild(select[i]);
            }
        }
    }
}
