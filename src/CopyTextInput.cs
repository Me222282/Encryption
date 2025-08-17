using System;
using Zene.GUI;
using Zene.Windowing;

namespace Encryption
{
    public class CopyTextInput : TextInput
    {
        public CopyTextInput(TextLayout layout, Action<string> set)
            : base(layout)
        {
            Set = set;
        }
        
        public Action<string> Set { get; set; }

        protected override string TextReference
        {
            get
            {
                string text = base.TextReference;
                Set(text);
                return text;
            }
            set
            {
                Set(value);
                base.TextReference = value;
            }
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e[Keys.Enter])
            {
                // reset focus
                Handle.Focus = null;
                return;
            }
            
            base.OnKeyDown(e);
        }
    }
}