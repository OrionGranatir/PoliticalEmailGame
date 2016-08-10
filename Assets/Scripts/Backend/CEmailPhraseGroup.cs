using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Backend
{
	public class CEmailPhraseGroup
	{
		private List<CEmailPhrase> mPhrases = new List<CEmailPhrase>();
		private string mTag;

		public List<CEmailPhrase> Phrases { get { return mPhrases; } }
		public string Tag { get { return mTag; } }

		public CEmailPhraseGroup(string tag)
		{
			mTag = tag;
		}
		public CEmailPhraseGroup()
		{

		}

		public CEmailPhraseGroup CloneForCharacters(List<CharacterType> characters)
		{
			CEmailPhraseGroup clone = new CEmailPhraseGroup();
			clone.mTag = mTag;
			foreach(CEmailPhrase phrase in mPhrases)
			{
				if( phrase.Category == CharacterType.Unknown || characters.Contains(phrase.Category))
					clone.mPhrases.Add(phrase);
			}
			return clone;
        }

		public void AddPhrase( CEmailPhrase phrase )
		{
			mPhrases.Add(phrase);
		}
	}
}
