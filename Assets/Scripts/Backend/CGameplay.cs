using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Backend
{
	public delegate void NewEmailDelegate(CEmail prevEmail, CEmail newEmail);

	enum WaveState
	{
		Briefing,
		Countdown,
		Playing,
		Finished
	};

	enum WaveResult
	{
		Success,
		Failed,
	};

	class SummaryScreenInfo
	{
		public int WaveNumber { get; set; }
		public int WaveScore { get; set; }
		public int TotalScore { get; set; }
		public int CorrectCount { get; set; }
		public int MissedCount { get; set; }
		public int SortedCount { get; set; }
		public int JusticeDepartmentStanding { get; set; }
    }

	class CGameplay
	{
		public CGameplay()
		{
        }

		public int WaveScore { get { return mWaveScore; } }
		public int InboxCount { get { return mInboxCount; } }
		public int WaveEmail { get { return mWaveEmailProcessed; } }
		public string JusticeDepartmentStanding { get { return mJusticeDepartmentStanding; } }
		public CEmail ActiveEmail { get { return mActiveEmail; } }
		public List<CCharacter> ActiveCharacters { get { return mActiveCharacters; } }

		public float Time { get { return (float)mWaveDuration - mPlayingTime; } }
		public float WaveDuration { get { return 120; } }

		public WaveState WaveState { get { return mWaveState; } }
		
		public SummaryScreenInfo SummaryScreenInfo { get { return mSummaryScreenInfo; } }

		public CWaveData CurrentWave { get { return mStaticWaveData; } }

		public CDialogItem CurrentDialog { get	{ return mActiveDialog.Count == 0 ? null : mActiveDialog.Peek(); } }
		public WaveResult WaveResults { get { return mWaveResult; } }
		public int CountdownSeconds { get { return kCountdownTime - (int)mTimeInState; } }

		// This Wave
		private int mWaveScore;
		private int mInboxCount;
		private CEmail mActiveEmail;
		private float mPlayingTime;
		private int mWaveEmailProcessed;
		private string mJusticeDepartmentStanding;
		private List<CCharacter> mActiveCharacters;
		SummaryScreenInfo mSummaryScreenInfo;
		private CWaveData mStaticWaveData;
		private WaveResult mWaveResult;
		private int mWaveDuration = 30;

		private WaveState mWaveState;
		private float mTimeInState;

		// queue of dialog to display
		private Queue<CDialogItem> mActiveDialog;

		// Countdown State
		const int kCountdownTime = 3;
		

		public void CloseDialog()
		{
			if (mActiveDialog.Count != 0)
				mActiveDialog.Dequeue();
        }

		private void SetWaveState( WaveState state, bool force = false )
		{
			if( state != mWaveState || force)
			{
				mTimeInState = 0.0f;
				mWaveState = state;
            }
		}

		public void LoadWave(CWaveData data)
		{
			mPlayingTime = 0.0f;
			mInboxCount = 0;
			mWaveScore = 0;
			mWaveEmailProcessed = 0;
			mJusticeDepartmentStanding = "Ok";

			SetWaveState(WaveState.Briefing);

			// Add them all for now
			mStaticWaveData = data;

			// Set up characters
			mActiveCharacters = new List<CCharacter>();
			foreach ( CharacterType characterType in mStaticWaveData.Characters )
			{
				CCharacter character = new CCharacter(characterType);
				mActiveCharacters.Add(character);
			}

			mActiveDialog = new Queue<CDialogItem>(mStaticWaveData.Dialog);

			NewEmail();
		}

		public void Update( float dt )
		{
			if (mWaveState == WaveState.Briefing)
			{
				if( mActiveDialog.Count == 0 )
				{
					SetWaveState( WaveState.Countdown );
				}
			}
			else if (mWaveState == WaveState.Countdown)
			{
				if( (int)mTimeInState > kCountdownTime)
				{
					SetWaveState(WaveState.Playing);
				}
			}
			else if (mWaveState == WaveState.Playing)
			{
				mPlayingTime += dt;
				// Successfully completed the round
                if (mPlayingTime > mWaveDuration)
				{
					mWaveResult = WaveResult.Success;
					SetWaveState(WaveState.Finished);
					mSummaryScreenInfo = new SummaryScreenInfo();
				}
			}
			mTimeInState += dt;
        }

		private void NewEmail()
		{
			CEmail prevEmail = mActiveEmail;
			mActiveEmail = CEmailGenerator.Instance.GenerateEmail( mStaticWaveData.CompiledEmailData );
			if( NewEmailEvent != null )
			{
				NewEmailEvent(prevEmail, mActiveEmail);
			}
		}

		public void CategoryChosen(CharacterType category)
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
