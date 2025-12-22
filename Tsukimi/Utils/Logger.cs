using System;
using System.IO;

namespace Tsukimi.Utils
{
	public class Logger
	{
		string filename;

		public Logger(string filename)
		{
			this.filename = filename;
		}

		//Clears the log file.
		public void ClearFile()
		{
			File.WriteAllText(filename, string.Empty);
		}

		public void Log(string message)
		{
			StreamWriter sw = File.AppendText(filename);
			sw.WriteLine(message);
			sw.Flush();
		}
	}
}

