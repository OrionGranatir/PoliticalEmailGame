using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Backend
{
	class CBackendUtil
	{
		public delegate void DebugPrintDelegate(string text);
		public static DebugPrintDelegate DebugPrintCallback = null;

		public static void DebugPrint(string text)
		{
			if (DebugPrintCallback != null)
				DebugPrintCallback(text);
		}

		public static bool StringNullOfWhitespace( string text )
		{
			return text == null || text.Trim().Length == 0;
		}

#if true
		// Deveopment URLs
		public static string EmailURL = "https://spreadsheets.google.com/feeds/list/1knOSeGd7ebLJtmIapm5fZCgOgF53Aq_bY1cg82UIW3E/od6/public/full?alt=json";
		public static string PhraseURL = "https://spreadsheets.google.com/feeds/list/1eW4kMHdFOG4C9sgWyuc_0wvfR4mflDZSasY9irrBa68/od6/public/full?alt=json";
		public static string DialogURL = "https://spreadsheets.google.com/feeds/list/1jpdjA-7ScFasBMpreQd4GMTHuZDXZQCfKiYYSEV5sxM/od6/public/full?alt=json";
		public static string WaveURL = "https://spreadsheets.google.com/feeds/list/1jWE4GzVx_DUAF-mJ0U7kJhegLR7LPX4pfV_vYVzQhA0/od6/public/full?alt=json";
#else
		// Production URLs TODO
		public static string EmailURL = "https://spreadsheets.google.com/feeds/list/1knOSeGd7ebLJtmIapm5fZCgOgF53Aq_bY1cg82UIW3E/od6/public/full?alt=json";
		public static string PhraseURL = "https://spreadsheets.google.com/feeds/list/1eW4kMHdFOG4C9sgWyuc_0wvfR4mflDZSasY9irrBa68/od6/public/full?alt=json";
		public static string DialogURL = "https://spreadsheets.google.com/feeds/list/1jpdjA-7ScFasBMpreQd4GMTHuZDXZQCfKiYYSEV5sxM/od6/public/full?alt=json";
		public static string WaveURL = "https://spreadsheets.google.com/feeds/list/1jWE4GzVx_DUAF-mJ0U7kJhegLR7LPX4pfV_vYVzQhA0/od6/public/full?alt=json";
#endif

		public static List<DataRequestDesc> GetDataRequests()
		{
			List<DataRequestDesc> requests = new List<DataRequestDesc>();
			requests.Add(new DataRequestDesc("emails", EmailURL, "email.txt", false));
			requests.Add(new DataRequestDesc("phrases", PhraseURL, "phrases.txt", false));
			requests.Add(new DataRequestDesc("dialog", DialogURL, "dialog.txt", false));
			requests.Add(new DataRequestDesc("waves", WaveURL, "waves.txt", false));
			return requests;
		}

		public static void PopulateData(DataFetcher fetcher)
		{
			if( fetcher.FetchSuccessful() )
			{
				CGameData.ResetGameData();
				CGameData.ParseDialogData(fetcher.GetResultText("dialog"));
				CGameData.ParseWaveData(fetcher.GetResultText("waves"));
				CEmailGenerator.ResetInstance();
				CGameData.ParseEmailTemplates(fetcher.GetResultText("emails"));
				CGameData.ParsePhraseData(fetcher.GetResultText("phrases"));
				CGameData.PostLoadLinking();
            }
		}
	}
}
