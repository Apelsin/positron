using System;
using System.Collections;
using System.Collections.Generic;
using OpenTK.Input;
using Positron.Input;

namespace Positron
{
	public interface IInputAccepter
	{
		bool KeyUp(object sender, KeyEventArgs e);
		bool KeyDown(object sender, KeyEventArgs e);
		KeysUpdateEventArgs KeysUpdate(object sender, KeysUpdateEventArgs e);
		// TODO: GamePad / Joystick support!
	}
}

