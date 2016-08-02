using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backend;

namespace EmailGeneratorConsole
{
	class Program
	{

		static string FetchURL(string url)
		{
			Console.WriteLine(string.Format("Fetching {0}", url));
			return new System.Net.WebClient().DownloadString(url);
		}

		static void Main(string[] args)
		{
			string emailURL = "https://spreadsheets.google.com/feeds/list/1knOSeGd7ebLJtmIapm5fZCgOgF53Aq_bY1cg82UIW3E/od6/public/full?alt=json";
			string phraseURL = "https://spreadsheets.google.com/feeds/list/1eW4kMHdFOG4C9sgWyuc_0wvfR4mflDZSasY9irrBa68/od6/public/full?alt=json";

			CBackendUtil.DebugPrintCallback = (text) => { Console.WriteLine(text); };

			string emailJsonText = FetchURL(emailURL);
			CEmailGenerator.Instance.AddEmailTemplatesJSON(emailJsonText);


			string phraseJsonText = FetchURL(phraseURL);
			CEmailGenerator.Instance.AddPhrasesJSON(phraseJsonText);

			// Print Summary
			CEmailGenerator.Instance.PrintSummary();

			CEmailGenerator.Instance.ValidateEmails();
		}
	}
}
