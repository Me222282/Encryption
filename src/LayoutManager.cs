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
        private class ElementManager : IElementManager
        {
            public ElementManager(IElementManager real)
            {
                _handle = real;
            }
            
            private readonly List<Element> _elements = new List<Element>();
            public Element this[int index] => _elements[index];
            
            public int Length => _elements.Count;
            
            private readonly IElementManager _handle;
            public Window Handle => _handle.Handle;
            
            public void AddChild(Element e) => _elements.Add(e);
            public void ClearChildren() => _elements.Clear();
            public bool RemoveChild(Element e) => _elements.Remove(e);
        }
        
        public LayoutManager(RootElement rootElement, Xml xml)
        {
            _root = rootElement;
            _xml = xml;
            
            _inputLayout = new ElementManager(_root);
            _viewLayout = new ElementManager(_root);
            
            _xml.LoadGUI(_inputLayout, File.ReadAllText("Layouts/passwordInput.xml"));
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
