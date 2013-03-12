using System;
using System.Collections;
using System.Collections.Generic;
using OpenTK.Input;

namespace positron
{
	public interface IInputAccepter
	{
		void KeyUp(object sender, KeyboardKeyEventArgs e);
		void KeyDown(object sender, KeyboardKeyEventArgs e);
		void KeysUpdate(object sender, KeysUpdateEventArgs e);
		// TODO: GamePad / Joystick support!
	}
}

