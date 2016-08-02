using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SimpleJSON;

namespace Backend
{
	class CEmailGenerator
	{

		private List<CEmailTemplate> mEmailTemplates = new List<CEmailTemplate>();
		private Dictionary<string, CEmailPhraseGroup> mGroups = new Dictionary<string, CEmailPhraseGroup>();
		private Random mRandom = new Random();

		class CEmailBuildContext
		{
			public List<CEmailPhrase> Phrases = new List<CEmailPhrase>();
			public CEmailTemplate TemplateEmail;
		}

		private CEmailGenerator()
		{
        }

		#region Singleton
		private static CEmailGenerator mInstance = null;
		public static CEmailGenerator Instance
		{
			get
			{
				if (mInstance == null)
					mInstance = new CEmailGenerator();
				return mInstance;
			}
		}
		#endregion

		public void AddEmailTemplatesJSON(string text)
		{
			JSONNode node = JSON.Parse(text);
			AddEmailTemplatesJSON(node);
		}
		private string GetJSONValueSafe(JSONNode node, string text, string defaultValue)
		{
			if( node[text] != null && node[text]["$t"] != null)
			{
				return node[text]["$t"];
			}
			return defaultValue;
		}
		public void AddEmailTemplatesJSON(JSONNode node)
		{
			JSONNode dataNode = node["feed"]["entry"];

			// Check update date
			string date = node["feed"]["updated"]["$t"];
			CBackendUtil.DebugPrint("Email File Updated Date: " + date);

			for ( int i = 0; i < dataNode.Count; i++ )
			{
				CEmailTemplate email = new CEmailTemplate();
				JSONNode entry = dataNode[i];

				string toText = GetJSONValueSafe(entry, "gsx$to", string.Empty);
				EmailCategory category = EmailCategory.Unknown;
                if ( !CEnumUtility.EmailCategoryFromString( toText, out category))
				{
					CBackendUtil.DebugPrint(string.Format("Unknown Category {0}", toText));
				}
				email.Category = category;
                email.From = GetJSONValueSafe(entry, "gsx$from", string.Empty);
				email.Body = GetJSONValueSafe(entry, "gsx$body", string.Empty);
				email.Subject = GetJSONValueSafe(entry, "gsx$subject", string.Empty);
				mEmailTemplates.Add(email);
			}
		}

		public void PrintSummary()
		{
			CBackendUtil.DebugPrint("Summary -------------");
			CBackendUtil.DebugPrint("---------------------");
			CBackendUtil.DebugPrint(string.Format("Email Templates: {0}", mEmailTemplates.Count));
			CBackendUtil.DebugPrint(string.Format("Phrase Groups: {0}", mGroups.Count));
			foreach( var pair in mGroups )
			{
				CBackendUtil.DebugPrint(string.Format(" {0}: {1}", pair.Key, pair.Value.Phrases.Count));
			}
		}

		public void AddPhrasesJSON(string text)
		{
			JSONNode node = JSON.Parse(text);
			AddPhrasesJSON(node);
		}

		public void AddPhrasesJSON(JSONNode node)
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
				EmailCategory category = EmailCategory.Unknown;
				if (!CEnumUtility.EmailCategoryFromString(toText, out category))
				{
					CBackendUtil.DebugPrint(string.Format("Unknown Category {0}", toText));
				}
				phrase.Category = category;
				phrase.Text = GetJSONValueSafe(entry, "gsx$text", string.Empty);
				string group = GetJSONValueSafe(entry, "gsx$group", string.Empty);
				if (!this.mGroups.ContainsKey(group))
				{
                    mGroups.Add(group, new CEmailPhraseGroup(group));
				}
				mGroups[group].Phrases.Add(phrase);
			}
		}

		private T RandomEnum<T>()
		{
			Array values = Enum.GetValues(typeof(T));
			Random random = new Random();
			T obj = (T)values.GetValue(random.Next(values.Length));
			return obj;
		}

		/*private int GetTags(string text, List<string> outTags)
		{

		}*/

		private string ExpandTextWithPhrases(string text)
		{
			int index1 = text.IndexOf("{{");
			int index2 = text.IndexOf("}}");

			if (index1 != -1 && index2 != -1)
			{
				string key = text.Substring(index1 + 2, index2 - 2);

				CEmailPhraseGroup group = null;
				string replaceText;
				if( !mGroups.TryGetValue( key, out group))
				{
					replaceText = string.Format("[[0]]", key);
				}
				else
				{
					int index = mRandom.Next(0, group.Phrases.Count);
					CEmailPhrase phrase = group.Phrases[index];
					replaceText = phrase.Text;
				}
				string outText = text.Replace("{{" + key + "}}", replaceText);
				return ExpandTextWithPhrases(outText);
            }
			return text;
		}

		public static void DebugPrintEmail(CEmail email)
		{
			CBackendUtil.DebugPrint(string.Format("Category: {0}", email.Category.ToString()));
			CBackendUtil.DebugPrint(string.Format("From: {0}", email.From));
			CBackendUtil.DebugPrint(string.Format("Subject: {0}", email.Subject));
			CBackendUtil.DebugPrint("Body:");
			CBackendUtil.DebugPrint(email.Body);
        }

		public void ValidateEmails()
		{
			for (int i = 0; i < 100; i++)
			{
				CEmail email = GenerateEmail();
				DebugPrintEmail(email);
				CBackendUtil.DebugPrint("");
            }
		}

		private CEmail CreateCEmailFromTemplate( CEmailTemplate emailTemplate )
		{
			// turn it into a CEmail
			CEmail email = new CEmail();
			email.Category = emailTemplate.Category;
			email.Body = ExpandTextWithPhrases(emailTemplate.Body);
			email.Subject = ExpandTextWithPhrases(emailTemplate.Subject);
			email.From = ExpandTextWithPhrases(emailTemplate.From);
			return email;
        }

		public CEmail GenerateEmail()
		{
			// pick a random template
			int emailIndex = mRandom.Next(0, mEmailTemplates.Count - 1);
			CEmailTemplate emailTemplate = mEmailTemplates[emailIndex];

			return CreateCEmailFromTemplate(emailTemplate);
		}
	}
}
