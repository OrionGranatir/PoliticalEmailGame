using System;

namespace Backend
{
	// Global state: Scores, stats, wave number etc.
	// Also handles flow control from wave to wave
	class CGameState
	{
		static private CGameState mInstance=null;
		static public CGameState Instance { get { if (mInstance == null) mInstance = new CGameState(); return mInstance; } }
		static public void StaticInit() { mInstance = null; }

		private int mTotalScore;
		private int mWaveIndex;
		private CWaveData mCurrentWave;
		private CGameplay mGameplay;

		public int WaveNumber { get { return mWaveIndex + 1; } }
		public int Score { get { return mTotalScore; } }
		public CWaveData CurrentWave { get { return mCurrentWave; }	}
		public CGameplay Gameplay { get { return mGameplay; } }
		

		public CGameState()
		{
			mGameplay = new CGameplay();
		}

		public void BeginWave( CWaveData waveData )
		{
			mCurrentWave = waveData;
			mGameplay.LoadWave(mCurrentWave);
		}

		public void NextWave()
		{
			// Just cycle through the waves list for now.
			// eventually add a procedural wave generation
			BeginWave(CGameData.WaveDataList[mWaveIndex % CGameData.WaveDataList.Count]);
        }

		public void WaveComplete()
		{
			mTotalScore += mGameplay.WaveScore;
			mWaveIndex++;
			NextWave();
		}
	};
	
};