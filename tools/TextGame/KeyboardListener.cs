using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TextGame
{
	class KeyboardListener
	{
		public KeyboardListener()
		{

		}

		private volatile bool mRunning;
		private Thread mThread;
		private Queue<ConsoleKeyInfo> mKeyQueue = new Queue<ConsoleKeyInfo>();

		public void Start()
		{
			if (mRunning)
				return;

			mRunning = true;

			mThread = new Thread(() =>
			{
				while (this.mRunning)
				{
					ConsoleKeyInfo key = System.Console.ReadKey(true);
					if( mRunning )
					{
						mKeyQueue.Enqueue(key);
					}
				}
			});

			mThread.IsBackground = true;
			mThread.Start();
		}

		public HashSet<char> UpdateKeysLatestMessages( HashSet<char> result = null )
		{
			if( result == null )
				result = new HashSet<char>();

			result.Clear();
			while( mKeyQueue.Count > 0 )
			{
				ConsoleKeyInfo info = mKeyQueue.Dequeue();
				result.Add(info.KeyChar);
			}

			return result;
		}

		public void Stop()
		{
			if(mThread != null)
			{
				mRunning = false;
				Process.GetCurrentProcess().StandardInput.Write('t');
				mThread.Join();
            }
		}
	}
}
