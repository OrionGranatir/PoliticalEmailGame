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
		
		private string ExpandTextWithPhrases(CCompiledWaveEmailData ed, string text, List<CEmailPhrase> usedPhrases)
		{
			int index1 = text.IndexOf("{{");
			int index2 = text.IndexOf("}}");

			if (index1 != -1 && index2 != -1)
			{
				int len = index2 - index1 - 2;
                string key = text.Substring(index1 + 2, len);

				CEmailPhraseGroup group = null;
				string replaceText;
				if( !ed.PhraseGroups.TryGetValue( key.ToLower(), out group))
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
				return ExpandTextWithPhrases(ed, outText, usedPhrases);
            }
			return text;
		}



		public void ValidateEmails(CCompiledWaveEmailData ed)
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
							if (!ed.PhraseGroups.ContainsKey(tag.ToLower()))
							{
								CBackendUtil.DebugPrint(string.Format("Warning: Undefined phrase tag '{0}' referenced in '{1}'.", tag, textName));
								CBackendUtil.DebugPrint(string.Format("Full Containing Text: {0}", text));
							}
						}
					}
					return true;
				};
				foreach (CEmailTemplate temp in ed.EmailTemplates)
				{
					checkText(temp.From, "Email From");
					checkText(temp.Subject, "Email Subject");
					checkText(temp.Body, "Email Body");
				}

				foreach (var kvp in ed.PhraseGroups)
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
			foreach (CEmailTemplate template in ed.EmailTemplates)
			{
				if (template.Weight != 0)
				{
					if (template.Category != CharacterType.Unknown)
						continue;

					// Add all the email strings together. It will make it easier to code
					string fullEmailText = template.From + template.Subject + template.Body;

					CheckPermutationsR(ed, template, fullEmailText);
				}
			}
		}

		void CheckPermutationsR(CCompiledWaveEmailData ed, CEmailTemplate template, string text, List<CEmailPhrase> phrases = null)
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
				if( !ed.PhraseGroups.ContainsKey(tagLower) )
				{
					// Warn that the tag is missing and stopt he permutations
					CBackendUtil.DebugPrint(string.Format("Warning! Couldn't resolve tag '{0}'!", tag));
					CBackendUtil.DebugPrint(string.Format("Email Template:\nFrom:{0}\nSubject:{1}\nBody: {2}", template.From, template.Subject, template.Body));
					CBackendUtil.DebugPrint("");
					return;
				}

				foreach( CEmailPhrase phrase in ed.PhraseGroups[tagLower].Phrases)
				{
					if (phrase.Category != CharacterType.Unknown)
						continue;

					phrases.Add(phrase);
					string augText = text.Replace("{{" + tag + "}}", phrase.Text);
					CheckPermutationsR(ed, template, augText, phrases);
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
			int categoryCount = Enum.GetNames(typeof(CharacterType)).Length;
			int[] tallyCount = new int[categoryCount];
			Func<CharacterType, bool> addCategoryTally = (category) =>
			{
				if (category != CharacterType.Unknown)
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
			context.Email.Category = (CharacterType)index;
		}

		private void BuildEmail(CCompiledWaveEmailData ed, CEmailBuildContext context )
		{
			// turn it into a CEmail
			CEmail email = new CEmail();
			context.Email.Body = ExpandTextWithPhrases(ed, context.Template.Body, context.Phrases);
			context.Email.Subject = ExpandTextWithPhrases(ed, context.Template.Subject, context.Phrases);
			context.Email.From = ExpandTextWithPhrases(ed, context.Template.From, context.Phrases);

			CalculateCategory( context );
        }

		public CEmail GenerateEmail(CCompiledWaveEmailData ed)
		{
			// pick a random template
			CEmailTemplate emailTemplate = ed.EmailTemplates.GetRandom(mRandom);

			CEmailBuildContext context = new CEmailBuildContext(emailTemplate);
			BuildEmail(ed, context);
			return context.Email;
		}
	}
}
