using Avalonia.Input;
using System.Collections.Generic;
using LunaGB;

namespace Tsukimi.Avalonia {
	public class Config {
		//Keyboard button map
		public static Dictionary<Key,Input.Button> buttonKeys = new Dictionary<Key,Input.Button>{
			{ Key.I,Input.Button.Up },
			{ Key.K,Input.Button.Down },
			{ Key.J,Input.Button.Left },
			{ Key.L,Input.Button.Right },
			{ Key.X,Input.Button.Select },
			{ Key.C,Input.Button.Start },
			{ Key.A,Input.Button.B },
			{ Key.S,Input.Button.A },
		};

	}
}