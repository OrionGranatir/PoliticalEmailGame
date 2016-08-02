using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SimpleJSON;
using System.Text.RegularExpressions;

namespace Backend
{
	partial class CEmailGenerator
	{
		private WeightedList<CEmailTemplate> mEmailTemplates = new WeightedList<CEmailTemplate>((item)=> item.Weight);
		private Dictionary<string, CEmailPhraseGroup> mGroups = new Dictionary<string, CEmailPhraseGroup>();
		public Random mRandom = new Random();

		class CEmailBuildContext
		{
			public List<CEmailPhrase> Phrases = new List<CEmailPhrase>();
			public CEmailTemplate Template;
			public CEmail Email = new CEmail();

			public CEmailBuildContext(CEmailTemplate template)
			{
				Template = template;
			}
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
		public static void ResetInstance()
		{
			mInstance = null;
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

				string weightText = GetJSONValueSafe(entry, "gsx$weight", "1");
				int weight = 1;
				int.TryParse(weightText, out weight);
				email.Weight = weight;

				mEmailTemplates.Add(email);
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
				string group = GetJSONValueSafe(entry, "gsx$group", string.Empty).ToLower().Trim();
				if (!this.mGroups.ContainsKey(group))
				{
                    mGroups.Add(group, new CEmailPhraseGroup(group));
				}
				mGroups[group].Phrases.Add(phrase);
			}
		}
		
		private string ExpandTextWithPhrases(string text, List<CEmailPhrase> usedPhrases)
		{
			int index1 = text.IndexOf("{{");
			int index2 = text.IndexOf("}}");

			if (index1 != -1 && index2 != -1)
			{
				int len = index2 - index1 - 2;
                string key = text.Substring(index1 + 2, len);

				CEmailPhraseGroup group = null;
				string replaceText;
				if( !mGroups.TryGetValue( key.ToLower(), out group))
				{
					replaceText = string.Format("[[{0}]]", key);
				}
				else
				{
					int index = mRandom.Next(0, group.Phrases.Count);
					CEmailPhrase phrase = group.Phrases[index];
					replaceText = phrase.Text;
					usedPhrases.Add(phrase);
                }
				string outText = text.Replace("{{" + key + "}}", replaceText);
				return ExpandTextWithPhrases(outText, usedPhrases);
            }
			return text;
		}

		public void PrintSummary()
		{
			CBackendUtil.DebugPrint("----- Summary -------------");
			CBackendUtil.DebugPrint(string.Format("Email Templates: {0}", mEmailTemplates.Count));
			CBackendUtil.DebugPrint(string.Format("Phrase Groups: {0}", mGroups.Count));
			foreach (var pair in mGroups)
			{
				CBackendUtil.DebugPrint(string.Format(" {0}: {1}", pair.Key, pair.Value.Phrases.Count));
			}

			CBackendUtil.DebugPrint("");
		}

		public void ValidateEmails()
		{
			Regex regex = new Regex(@"{{(?<TextInsideBrackets>\w+)}}");

			/*string testText = "ffff{{asdfasdf}} {{TestText}}aa";
			MatchCollection c = regex.Matches(testText);
			foreach (Match m in c)
			{
				if (m.Success)
				{
					CBackendUtil.DebugPrint(string.Format("Match Found {0}", m.Groups["TextInsideBrackets"].Value));
				}
			}*/

			// 1) Print warnings for unmatched group tags
			{
				CBackendUtil.DebugPrint("------ Checking for missing referenced phrase groups ------- ");

				Func<string, string, bool> checkText = (text, textName) =>
				{
					foreach (Match m in regex.Matches(text))
					{
						if (m.Success)
						{
							string tag = m.Groups["TextInsideBrackets"].Value;
							if (!this.mGroups.ContainsKey(tag.ToLower()))
							{
								CBackendUtil.DebugPrint(string.Format("Warning: Undefined phrase tag '{0}' referenced in '{1}'.", tag, textName));
								CBackendUtil.DebugPrint(string.Format("Full Containing Text: {0}", text));
							}
						}
					}
					return true;
				};
				foreach (CEmailTemplate temp in mEmailTemplates)
				{
					checkText(temp.From, "Email From");
					checkText(temp.Subject, "Email Subject");
					checkText(temp.Body, "Email Body");
				}

				foreach (var kvp in mGroups)
				{
					foreach(CEmailPhrase phrase in kvp.Value.Phrases)
					{
						checkText(phrase.Text, string.Format("Phrase Group {0}", kvp.Key));
                    }
                }
			}

			CBackendUtil.DebugPrint("");
			CBackendUtil.DebugPrint("------ Checking all permutations of emails ------- ");
			// 2) Do checks on every possible email permutation.
			//    a) Make sure they have a defined category
			foreach (CEmailTemplate template in mEmailTemplates)
			{
				if (template.Weight != 0)
				{
					if (template.Category != EmailCategory.Unknown)
						continue;

					// Add all the email strings together. It will make it easier to code
					string fullEmailText = template.From + template.Subject + template.Body;

					CheckPermutationsR(template, fullEmailText);
				}
			}
		}

		void CheckPermutationsR(CEmailTemplate template, string text, List<CEmailPhrase> phrases = null)
		{
			Regex regex = new Regex(@"{{(?<TextInsideBrackets>\w+)}}");

			if (phrases == null)
				phrases = new List<CEmailPhrase>();

			int phraseCount = phrases.Count;

			Match m = regex.Match(text);
			if (m.Success)
			{
				string tag = m.Groups["TextInsideBrackets"].Value;
				string tagLower = tag.ToLower();
				if( !mGroups.ContainsKey(tagLower) )
				{
					// Warn that the tag is missing and stopt he permutations
					CBackendUtil.DebugPrint(string.Format("Warning! Couldn't resolve tag '{0}'!", tag));
					CBackendUtil.DebugPrint(string.Format("Email Template:\nFrom:{0}\nSubject:{1}\nBody: {2}", template.From, template.Subject, template.Body));
					CBackendUtil.DebugPrint("");
					return;
				}

				foreach( CEmailPhrase phrase in mGroups[tagLower].Phrases)
				{
					if (phrase.Category != EmailCategory.Unknown)
						continue;

					phrases.Add(phrase);
					string augText = text.Replace("{{" + tag + "}}", phrase.Text);
					CheckPermutationsR(template, augText, phrases);
                    phrases.RemoveAt(phrases.Count - 1);
				}

			}
			else // Resolved all the tag and still haven't found a category
			{
				CBackendUtil.DebugPrint("Warning! Found combination that has no category!");
				CBackendUtil.DebugPrint(string.Format("Email Template:\nFrom:{0}\nSubject:{1}\nBody: {2}", template.From, template.Subject, template.Body));
				CBackendUtil.DebugPrint("Phrases:");
				foreach (CEmailPhrase phrase in phrases)
					CBackendUtil.DebugPrint(phrase.Text);
				CBackendUtil.DebugPrint("");
			}

		}

		private void CalculateCategory( CEmailBuildContext context )
		{
			int categoryCount = Enum.GetNames(typeof(EmailCategory)).Length;
			int[] tallyCount = new int[categoryCount];
			Func<EmailCategory, bool> addCategoryTally = (category) =>
			{
				if (category != EmailCategory.Unknown)
				{
					int catIndex = (int)category;
					tallyCount[catIndex]++;
				}
				return true;
			};

			// count the category attached to the email template 
			addCategoryTally(context.Template.Category);

			// count all phrase categories
			context.Phrases.ForEach(phrase => addCategoryTally(phrase.Category));

			int maxValue = tallyCount.Max();
			int index = Array.IndexOf(tallyCount, maxValue);
			context.Email.Category = (EmailCategory)index;
		}

		private void BuildEmail( CEmailBuildContext context )
		{
			// turn it into a CEmail
			CEmail email = new CEmail();
			context.Email.Body = ExpandTextWithPhrases(context.Template.Body, context.Phrases);
			context.Email.Subject = ExpandTextWithPhrases(context.Template.Subject, context.Phrases);
			context.Email.From = ExpandTextWithPhrases(context.Template.From, context.Phrases);

			CalculateCategory( context );
        }

		public CEmail GenerateEmail()
		{
			// pick a random template
			CEmailTemplate emailTemplate = mEmailTemplates.GetRandom(mRandom);

			CEmailBuildContext context = new CEmailBuildContext(emailTemplate);
			BuildEmail(context);
			return context.Email;
		}
	}
}
