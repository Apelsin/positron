using System;
using System.Collections.Generic;

using OpenTK.Input;

namespace positron
{
	public class UIElementGroup : List<IUIElement>, IInputAccepter
	{
		protected int _ElementFocusedIndex = 0;
		public int ElementFocusedIndex { get { return _ElementFocusedIndex; } set { _ElementFocusedIndex = value; } }
		public IUIElement ElementFocused { get { return this [_ElementFocusedIndex]; } }
		public UIElementGroup ():
			base()
		{
		}
		public bool KeyDown (object sender, KeyboardKeyEventArgs e)
		{
			if (e.Key == Key.A || e.Key == Key.W) {
				_ElementFocusedIndex = (_ElementFocusedIndex + Count - 1) % Count;
				this.ForEach(element => element.OnRefresh(this, new EventArgs()));
			} else if (e.Key == Key.S || e.Key == Key.S) {
				_ElementFocusedIndex = (_ElementFocusedIndex + 1) % Count;
				this.ForEach(element => element.OnRefresh(this, new EventArgs()));
			}
			else if (e.Key == Key.Space) {
				ActuateFocused();
				this.ForEach(element => element.OnRefresh(this, new EventArgs()));
			}
			return true;
		}
		public void ActuateFocused ()
		{
			if(ElementFocused is UIButton)
			{
				((UIButton)ElementFocused).OnAction(this, new ActionEventArgs(true, ElementFocused));
			}
		}
		public bool KeyUp(object sender, KeyboardKeyEventArgs e)
		{
			return true;
		}
		public KeysUpdateEventArgs KeysUpdate(object sender, KeysUpdateEventArgs e)
		{
			//e.KeysPressedWhen.Clear();
			return e;
		}
	}
}

