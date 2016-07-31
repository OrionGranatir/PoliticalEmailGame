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

		}

		public int TotalScore { get { return mTotalScore; } }
		public int WaveScore { get { return mWaveScore; } }
		public int InboxCount {  get { return mInboxCount; } }
		public CEmail ActiveEmail { get { return mActiveEmail; } }

		public float Time { get { return mTime; } }
		public float WaveDuration { get { return 120; } }
		
		private int mTotalScore;

		// This wave
		private int mWaveScore;
		private int mInboxCount;
		private CEmail mActiveEmail;
		private float mTime;

		public void StartWave()
		{
			mTime = 0.0f;
			mWaveScore = 0;
			NewEmail();
		}

		public void Update( float dt )
		{
			mTime += dt;
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
