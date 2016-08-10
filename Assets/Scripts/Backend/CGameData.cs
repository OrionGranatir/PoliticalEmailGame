using System;
using System.Collections.Generic;
using SimpleJSON;

namespace Backend
{
	class CDialogItem
	{
		public string Text;
		public string Character;
		public CharacterMood Mood;
	};

	class CCompiledWaveEmailData
	{
		public Dictionary<string, CEmailPhraseGroup> PhraseGroups = new Dictionary<string, CEmailPhraseGroup>();
		public WeightedList<CEmailTemplate> EmailTemplates = new WeightedList<CEmailTemplate>((item) => item.Weight);
	}

	class CWaveData
	{
		public string StartDialogText;
		public string EmailGroupsText;
		public string DifficultyText;
		public List<CEmailTemplateGroup> EmailGroups;
		public List<CharacterType> Characters;
		public CCompiledWaveEmailData CompiledEmailData;
		public List<CDialogItem> Dialog;
	};

	class CGameData
	{
		public static void ResetGameData()
		{
			DialogChains = new Dictionary<string, List<CDialogItem>>();
			WaveDataList = new List<CWaveData>();
			PhraseGroups = new Dictionary<string, CEmailPhraseGroup>();
			EmailTemplates = new WeightedList<CEmailTemplate>((item) => item.Weight);
			EmailGroups = new Dictionary<string, CEmailTemplateGroup>();
        }

		public static Dictionary<string, List<CDialogItem>> DialogChains;
		public static List<CWaveData> WaveDataList;
		public static Dictionary<string, CEmailPhraseGroup> PhraseGroups;
		public static WeightedList<CEmailTemplate> EmailTemplates;
		public static Dictionary<string, CEmailTemplateGroup> EmailGroups;

		private static string GetJSONValueSafe(JSONNode node, string text, string defaultValue)
		{
			if (node[text] != null && node[text]["$t"] != null)
			{
				return node[text]["$t"];
			}
			return defaultValue;
		}

		// Called after all the data files have been parsed
		public static void PostLoadLinking()
		{

			// Create email groups
			foreach( CEmailTemplate email in EmailTemplates )
			{
				foreach (string groupName in email.EmailGroupText.Split(','))
				{
					string text = groupName.Trim().ToLower();
					if (CBackendUtil.StringNullOfWhitespace(text))
						text = "default";
					if (!EmailGroups.ContainsKey(text))
						EmailGroups.Add(text, new CEmailTemplateGroup());
					EmailGroups[text].Emails.Add(email);
				}
			}

			// Link waves to email groups and dialog
			foreach (CWaveData wd in WaveDataList)
			{
				wd.EmailGroups = new List<CEmailTemplateGroup>();
				foreach (string groupText in wd.EmailGroupsText.Split(','))
				{
					string gt = groupText.ToLower().Trim();
					if (gt == "all")
					{
						wd.EmailGroups.AddRange(EmailGroups.Values);
					}
					else if (!EmailGroups.ContainsKey(gt))
					{
						CBackendUtil.DebugPrint(string.Format("Wave references unknown email group '{0}'", gt));
					}
					else
					{
						wd.EmailGroups.Add(EmailGroups[gt]);
					}
				}
				wd.Dialog = new List<CDialogItem>();
				foreach (string dialogText in wd.StartDialogText.Split(','))
				{
					string dt = dialogText.ToLower().Trim();
					if (!CBackendUtil.StringNullOfWhitespace(dt))
					{
						if (DialogChains.ContainsKey(dt))
						{
							wd.Dialog.AddRange(DialogChains[dt]);
						}
						else
						{
							CBackendUtil.DebugPrint(string.Format("WARNING! Unknown dialog text '{0}'", dt));
						}
					}
				}
			}

			// Compile email data for each wave
			foreach(CWaveData wd in WaveDataList)
			{
				CompileWaveEmailData(wd);
			}
		}

		private static void CompileWaveEmailData(CWaveData wd)
		{
			wd.CompiledEmailData = new CCompiledWaveEmailData();
			foreach ( CEmailTemplateGroup group in wd.EmailGroups )
			{
				foreach( CEmailTemplate temp in group.Emails )
				{
					if( temp.Category == CharacterType.Unknown || wd.Characters.Contains(temp.Category))
					{
						wd.CompiledEmailData.EmailTemplates.Add(temp);
					}
				}
				
            }

			wd.CompiledEmailData.PhraseGroups = new Dictionary<string, CEmailPhraseGroup>();

			// build phrase groups stripping out unwanted characters
			foreach ( string phraseName in PhraseGroups.Keys )
			{
				wd.CompiledEmailData.PhraseGroups.Add(phraseName, PhraseGroups[phraseName].CloneForCharacters(wd.Characters));
			}
			
        }

		public static void ParseDialogData(JSONNode node)
		{
			JSONNode dataNode = node["feed"]["entry"];

			// Check update date
			string date = node["feed"]["updated"]["$t"];
			CBackendUtil.DebugPrint("Parsing Dialog Data File Updated Date: " + date);

			for (int i = 0; i < dataNode.Count; i++)
			{
				CDialogItem item = new CDialogItem();
				JSONNode entry = dataNode[i];

				string tag = GetJSONValueSafe(entry, "gsx$tag", string.Empty).ToLower();
				if (tag == "" || tag == string.Empty)
					CBackendUtil.DebugPrint(string.Format("Warning! Line {0} of dialog spreadsheet has no tag", i));

				item.Character = GetJSONValueSafe(entry, "gsx$character", string.Empty);
				string mood = GetJSONValueSafe(entry, "gsx$mood", "Neutral");
				item.Mood = (CharacterMood)TextToEnum.Convert<CharacterMood>(mood, 0);
                item.Text = GetJSONValueSafe(entry, "gsx$text", string.Empty);
				
				if (!DialogChains.ContainsKey(tag))
					DialogChains.Add(tag, new List<CDialogItem>());
				DialogChains[tag].Add(item);
			}
		}
		public static void ParseDialogData(string text)
		{
			ParseDialogData(JSON.Parse(text));
		}

		public static void ParseWaveData(JSONNode node)
		{
			JSONNode dataNode = node["feed"]["entry"];

			// Check update date
			string date = node["feed"]["updated"]["$t"];
			CBackendUtil.DebugPrint("Parsing Wave Data File Updated Date: " + date);

			for (int i = 0; i < dataNode.Count; i++)
			{
				CWaveData item = new CWaveData();
				JSONNode entry = dataNode[i];

				item.StartDialogText = GetJSONValueSafe(entry, "gsx$startdialog", string.Empty);
				item.EmailGroupsText = GetJSONValueSafe(entry, "gsx$emailgroups", string.Empty);
				item.DifficultyText = GetJSONValueSafe(entry, "gsx$difficulty", string.Empty);

				item.Characters = new List<CharacterType>();
				foreach ( string cText in GetJSONValueSafe(entry, "gsx$characters", string.Empty).Split(','))
				{
					if( !CBackendUtil.StringNullOfWhitespace(cText) )
					{
						item.Characters.Add((CharacterType)TextToEnum.Convert<CharacterType>(cText.Trim().ToLower())); 
                    }
				}
				WaveDataList.Add(item);
            }
		}

		public static void ParseWaveData(string text)
		{
			ParseWaveData(JSON.Parse(text));
		}

		public static void ParsePhraseData(string text)
		{
			JSONNode node = JSON.Parse(text);
			ParsePhraseData(node);
		}

		public static void ParsePhraseData(JSONNode node)
		{
			JSONNode dataNode = node["feed"]["entry"];

			// Check update date
			string date = node["feed"]["updated"]["$t"];
			CBackendUtil.DebugPrint("Phrase File Updated Date: " + date);

			for (int i = 0; i < dataNode.Count; i++)
			{
				CEmailPhrase phrase = new CEmailPhrase();
				JSONNode entry = dataNode[i];

				string toText = GetJSONValueSafe(entry, "gsx$to", string.Empty);
				phrase.Category = (CharacterType)TextToEnum.Convert<CharacterType>(toText);
				phrase.Text = GetJSONValueSafe(entry, "gsx$text", string.Empty);
				string group = GetJSONValueSafe(entry, "gsx$group", string.Empty).ToLower().Trim();
				if (!PhraseGroups.ContainsKey(group))
				{
					PhraseGroups.Add(group, new CEmailPhraseGroup(group));
				}
				PhraseGroups[group].Phrases.Add(phrase);
			}
		}

		public static void ParseEmailTemplates(string text)
		{
			JSONNode node = JSON.Parse(text);
			ParseEmailTemplates(node);
		}
		
		public static void ParseEmailTemplates(JSONNode node)
		{
			JSONNode dataNode = node["feed"]["entry"];

			// Check update date
			string date = node["feed"]["updated"]["$t"];
			CBackendUtil.DebugPrint("Email File Updated Date: " + date);

			for (int i = 0; i < dataNode.Count; i++)
			{
				CEmailTemplate email = new CEmailTemplate();
				JSONNode entry = dataNode[i];

				string toText = GetJSONValueSafe(entry, "gsx$to", string.Empty);
				email.Category = (CharacterType)TextToEnum.Convert<CharacterType>(toText);
				email.From = GetJSONValueSafe(entry, "gsx$from", string.Empty);
				email.Body = GetJSONValueSafe(entry, "gsx$body", string.Empty);
				email.Subject = GetJSONValueSafe(entry, "gsx$subject", string.Empty);
				email.EmailGroupText = GetJSONValueSafe(entry, "gsx$emailgroups", string.Empty);

				string weightText = GetJSONValueSafe(entry, "gsx$weight", "1");
				int weight = 1;
				int.TryParse(weightText, out weight);
				email.Weight = weight;

				EmailTemplates.Add(email);
			}
		}

		public static void PrintSummary()
		{
			CBackendUtil.DebugPrint("----- Summary -------------");
			CBackendUtil.DebugPrint(string.Format("Email Templates: {0}", CGameData.EmailTemplates.Count));
			CBackendUtil.DebugPrint(string.Format("Phrase Groups: {0}", CGameData.PhraseGroups.Count));
			foreach (var pair in CGameData.PhraseGroups)
			{
				CBackendUtil.DebugPrint(string.Format(" {0}: {1}", pair.Key, pair.Value.Phrases.Count));
			}

			CBackendUtil.DebugPrint("");
		}
	};
}