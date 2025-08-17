using System;
using Zene.GUI;
using Zene.Windowing;

namespace Encryption
{
    public class PasswordEnter : TextInput
    {
        private string _passWordView = "";
        protected override string TextReference => _passWordView;
        
        public string GetPassword() => base.TextReference;
        
        public event EventHandler PasswordEntered;
        
        public void Clear()
        {
            base.TextReference = "";
            _passWordView = "";
        }
        
        protected override void OnTextInput(TextInputEventArgs e)
        {
            base.OnTextInput(e);
            UpdateView();
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e[Keys.Enter])
            {
                PasswordEntered?.Invoke(this, EventArgs.Empty);
                return;
            }
            
            base.OnKeyDown(e);
            UpdateView();
        }
        private void UpdateView()
        {
            if (base.TextReference.Length == _passWordView.Length) { return; }
            
            _passWordView = new string('*', base.TextReference.Length);
            TriggerChange();
        }
    }
}
