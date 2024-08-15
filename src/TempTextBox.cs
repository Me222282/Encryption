using System;
using Zene.GUI;
using Zene.Windowing;

namespace Encryption
{
    public class TempTextBox : TextInput
    {
        public TempTextBox(TextLayout layout)
            : base(layout)
        {
            
        }
        
        public event EventHandler Entered;

        protected override void OnFocus(FocusedEventArgs e)
        {
            base.OnFocus(e);
            
            if (e.Focus) { return; }
            
            Parent.Children.Remove(this);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e[Keys.Enter])
            {
                Entered?.Invoke(this, EventArgs.Empty);
                return;
            }
            if (e[Keys.Escape])
            {
                Parent.Children.Remove(this);
                return;
            }
            
            base.OnKeyDown(e);
        }
    }
}
