using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Backend
{
	class CBackendUtil
	{
		public delegate void DebugPrintDelegate(string text);
		public static DebugPrintDelegate DebugPrintCallback;

		public static void DebugPrint(string text)
		{
			if (DebugPrintCallback != null)
				DebugPrintCallback(text);
		}
	}
}
