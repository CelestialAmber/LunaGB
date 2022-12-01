using System;

namespace LunaGB.Core.Debug
{
	//Might change this to not be a static class?
	public class Debugger
	{
		public static List<Breakpoint> breakpoints = new List<Breakpoint>();
		public static bool breakpointsEnabled = false;
		public static bool stepping = false;

		//Event for breakpoints
		public delegate void BreakpointEvent(Breakpoint breakpoint);
		public static event BreakpointEvent OnHitBreakpoint;

		public static void InitBreakpoints()
		{
			breakpoints.Clear();
			breakpoints.Add(new Breakpoint(0x0, 0x100, false, false, true));
			breakpoints[0].enabled = true;
		}

		//Checks whether one of the breakpoints was hit when the emulator reads a byte
		public static void OnMemoryRead(int address)
		{
			foreach (Breakpoint breakpoint in Debugger.breakpoints)
			{
				if (breakpoint.enabled && breakpoint.read && address >= breakpoint.minAddress && address <= breakpoint.maxAddress)
				{
					OnHitBreakpoint?.Invoke(breakpoint);
				}
			}
		}

		//Checks whether one of the breakpoints was hit when the emulator writes a byte
		public static void OnMemoryWrite(int address)
		{
			foreach (Breakpoint breakpoint in Debugger.breakpoints)
			{
				if (breakpoint.enabled && breakpoint.write && address >= breakpoint.minAddress && address <= breakpoint.maxAddress)
				{
					OnHitBreakpoint?.Invoke(breakpoint);
				}
			}
		}

		//Checks whether one of the breakpoints was hit when the emulator executes an instruction
		public static void OnExecute(int address)
		{
			foreach (Breakpoint breakpoint in Debugger.breakpoints)
			{
				if (breakpoint.enabled && breakpoint.execute && address >= breakpoint.minAddress && address <= breakpoint.maxAddress)
				{
					OnHitBreakpoint?.Invoke(breakpoint);
				}
			}
		}
	}
}

