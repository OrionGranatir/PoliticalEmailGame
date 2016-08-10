using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend
{
	class DataFetcherWin : DataFetcher
	{
		protected override bool FetchURL(string URL, out string outResult)
		{
			try
			{
				outResult = new System.Net.WebClient().DownloadString(URL);
			}
			catch
			{
				outResult = string.Empty;
				return false;
			}
			return true;
		}

		protected override bool ReadLocalFile(string filename, out string result)
		{
			throw new NotImplementedException();
		}

		protected override bool WriteLocalFile(string filename, string text)
		{
			throw new NotImplementedException();
		}
	}
}
