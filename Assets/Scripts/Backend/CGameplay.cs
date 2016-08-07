using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Backend
{
	public delegate void NewEmailDelegate(CEmail prevEmail, CEmail newEmail); 

	public class CGameplay
	{
		public CGameplay()
		{
			// Set up characters
			characters = new List<CCharacter>();
			foreach( EmailCategory value in Enum.GetValues( typeof(EmailCategory) ) )
			{
				CCharacter character = new CCharacter( value );
				characters.Add( character );
			}
		}

		public int TotalScore { get { return mTotalScore; } }
		public int WaveScore { get { return mWaveScore; } }
		public int InboxCount {  get { return mInboxCount; } }
		public int WaveEmail {  get { return mWaveEmailProcessed; } }
		public string JusticeDepartmentStanding {  get { return mJusticeDepartmentStanding; } }
		public CEmail ActiveEmail { get { return mActiveEmail; } }

		public float Time { get { return mTime; } }
		public float WaveDuration { get { return 120; } }
		
		private int mTotalScore;

		// Chracters
		List<CCharacter> characters;

		// This wave
		private int mWaveScore;
		private int mInboxCount;
		private CEmail mActiveEmail;
		private float mTime;
		private int mWaveEmailProcessed;
		private string mJusticeDepartmentStanding;

		public void StartWave()
		{
			mTime = 0.0f;
			mInboxCount = 0;
			mWaveScore = 0;
			mWaveEmailProcessed = 0;
			mJusticeDepartmentStanding = "Ok";
			NewEmail();
		}

		public void Update( float dt )
		{
			mTime += dt;
		}

		// Returns true when the game is over
		public bool GameOver()
		{
			return false;
		}

		// Returns true when the wave has started and the timer is running
		public bool WaveStarted()
		{
			return true;
		}

		// Returns true when the wave is complete
		public bool WaveComplete()
		{
			return false;
		}

		// Returns the character for the given email category
		public CCharacter GetCharacter( EmailCategory category )
		{
			CCharacter result = null;

			foreach( CCharacter character in characters )
			{
				if( character.catergory == category )
				{
					result = character;
					break;
				}
			}

			return result;
		}

		private void NewEmail()
		{
			CEmail prevEmail = mActiveEmail;
			mActiveEmail = CEmailGenerator.Instance.GenerateEmail();
			if( NewEmailEvent != null )
			{
				NewEmailEvent(prevEmail, mActiveEmail);
			}
		}

		public void CategoryChosen(EmailCategory category)
		{
			if( category == mActiveEmail.Category)
			{
				mWaveScore += 100;
			}
			NewEmail();
		}

		public event NewEmailDelegate NewEmailEvent;
	}
}
