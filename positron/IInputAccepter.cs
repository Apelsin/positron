using System;
using System.Collections;
using System.Collections.Generic;
using OpenTK.Input;

namespace positron
{
	public interface IInputAccepter
	{
		bool KeyUp(object sender, KeyboardKeyEventArgs e);
		bool KeyDown(object sender, KeyboardKeyEventArgs e);
		KeysUpdateEventArgs KeysUpdate(object sender, KeysUpdateEventArgs e);
		// TODO: GamePad / Joystick support!
	}
}

