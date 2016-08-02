using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Backend
{
	class CEmailPhraseGroup
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

		public void AddPhrase( CEmailPhrase phrase )
		{
			mPhrases.Add(phrase);
		}
	}
}
