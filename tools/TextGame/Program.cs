using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backend;

namespace TextGame
{
	class Program
	{

		static string FetchURL(string url)
		{
			Console.WriteLine(string.Format("Fetching {0}", url));
			return new System.Net.WebClient().DownloadString(url);
		}

		static void InitData()
		{
			CBackendUtil.DebugPrintCallback = (text) => { Console.WriteLine(text); };

			DataFetcherWin df = new DataFetcherWin();
			df.SetRequests(CBackendUtil.GetDataRequests());
			df.FetchAll(false);
			CBackendUtil.PopulateData(df);
		}

		static void Main(string[] args)
		{
			InitData();
			GameDriver driver = new GameDriver();
			driver.Run();
		}
	}
}
