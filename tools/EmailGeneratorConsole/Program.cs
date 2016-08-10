using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend
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
			CEmailGenerator.ResetInstance();

			CBackendUtil.DebugPrintCallback = (text) => { Console.WriteLine(text); };

			DataFetcherWin df = new DataFetcherWin();
			df.SetRequests(CBackendUtil.GetDataRequests());
			df.FetchAll(false);
			CBackendUtil.PopulateData(df);

			CEmailGenerator.Instance.ValidateEmails( CGameData.WaveDataList[0].CompiledEmailData );
		}

		static void Main(string[] args)
		{
			InitData();

			Random seedGenerator = new Random();

			int curSeed = 0;
			while( true )
			{
				Console.WriteLine("-----------------------------------------------------");
				Console.WriteLine("Options (1) Generate one email   (2) Generate 10 emails   (3) Generate with last seed   (4) Choose Seed   (5) Reload data" );
				Console.WriteLine("-----------------------------------------------------");
				Console.WriteLine("");

				Func<int, bool> GenAndPrintEmail = (seed) =>
				{
					CEmailGenerator.Instance.mRandom = new Random(seed);
					CEmail email = CEmailGenerator.Instance.GenerateEmail(CGameData.WaveDataList[0].CompiledEmailData );
					Console.WriteLine("-------------- Seed {0}", seed);
					Console.WriteLine(string.Format("Category: {0}", email.Category.ToString()));
					Console.WriteLine(string.Format("From: {0}", email.From));
					Console.WriteLine(string.Format("Subject: {0}", email.Subject));
					Console.WriteLine("Body:");
					Console.WriteLine(email.Body);
					Console.WriteLine("");
					return true;
				};

				ConsoleKeyInfo info = Console.ReadKey(true);
				switch (info.KeyChar)
				{
					case '1':
					{
						curSeed = seedGenerator.Next();
						GenAndPrintEmail(curSeed);
                    }
					break;
					case '2':
					{
						for (int i = 0; i < 10; i++)
						{
							curSeed = seedGenerator.Next();
							GenAndPrintEmail(curSeed);
						}
					}
					break;
					case '3':
					{
						GenAndPrintEmail(curSeed);
					}
					break;
					case '4':
					{
						Console.WriteLine("Enter Seed Value:");
						string line = Console.ReadLine();
						int lineInt;
                        if (int.TryParse(line, out lineInt))
						{
							GenAndPrintEmail(lineInt);
						}
						else
						{
							Console.WriteLine("Invalid Input");
						}
					}
					break;
					case '5':
					{
						Console.Clear();
						InitData();
					}
					break;
				}
			}
		}
	}
}
