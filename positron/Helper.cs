using System;
using System.Collections.Generic;

using OpenTK.Input;

namespace positron
{
	public static class Helper
	{
		public static bool KeyPressedInTime (DateTime pressed, DateTime now)
		{
			return (now - pressed).TotalSeconds < Configuration.KeyPressTimeTolerance;
		}
	}
}

