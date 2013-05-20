using System;
using System.Collections;
using System.Collections.Generic;
using OpenTK.Input;
using positron.Input;

namespace positron
{
	public interface IInputAccepter
	{
		bool KeyUp(object sender, KeyEventArgs e);
		bool KeyDown(object sender, KeyEventArgs e);
		KeysUpdateEventArgs KeysUpdate(object sender, KeysUpdateEventArgs e);
		// TODO: GamePad / Joystick support!
	}
}

