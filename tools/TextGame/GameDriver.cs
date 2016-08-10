using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backend;

namespace TextGame
{
	class GameDriver
	{
		CGameplay mGameplay;
		HashSet<char> mFrameInput;

		void PrintCharacters(StringBuilder builder)
		{
			foreach( CCharacter c in mGameplay.ActiveCharacters )
			{
				builder.AppendFormat("{0} ({1})  ", c.catergory, c.mood);
			}
			builder.AppendLine();
		}

		void PrintDialog(StringBuilder builder)
		{
			if( mGameplay.CurrentDialog != null)
			{
				builder.AppendFormat("{0}({1}): {2}\n", mGameplay.CurrentDialog.Character, mGameplay.CurrentDialog.Mood, mGameplay.CurrentDialog.Text);
			}
        }

		void PrintOptions(StringBuilder builder)
		{
			CEmail email = mGameplay.ActiveEmail;
			
			builder.AppendFormat("From: {0}\nSubject: {1}\n{2}\n\n", email.From, email.Subject, email.Body);
			builder.AppendFormat("Choose Category: ");
			for ( int i = 0; i < mGameplay.ActiveCharacters.Count(); i++)
			{
				builder.AppendFormat("{0}) {1}   ", i + 1, mGameplay.ActiveCharacters[i].catergory.ToString());
            }
		}

		void PrintGameStats(StringBuilder builder)
		{
			builder.AppendFormat("Inbox: {0}   Score: {1}    Time: {2}\n", mGameplay.InboxCount, mGameplay.WaveScore, (int)mGameplay.Time );
		}

		void PrintCountdown(StringBuilder builder)
		{
			builder.AppendFormat("Wave {0} Starting\n{1}\n", CGameState.Instance.WaveNumber, mGameplay.CountdownSeconds);
		}

		void PrintWaveSummary(StringBuilder builder)
		{
			builder.AppendLine("Wave Completed!");
			builder.AppendFormat("Wave Score: {0}\n", mGameplay.WaveScore);
		}

		void PrintGameOver(StringBuilder builder)
		{
			builder.AppendFormat("Game Over! Total Score: {0}, Wave: {1}\n", CGameState.Instance.Score, CGameState.Instance.WaveNumber );
		}


		void HandleEmailSelectionInput()
		{
			for (int i = 0; i < mGameplay.ActiveCharacters.Count(); i++)
			{
				char charValue = (char)((int)'1' + i);
                if (mFrameInput.Contains(charValue))
				{
					mGameplay.CategoryChosen(mGameplay.ActiveCharacters[i].catergory);
				}
            }
				
			//Console.Read
		}

		public void Run()
		{
			KeyboardListener listener = new KeyboardListener();
			mFrameInput = new HashSet<char>();
			listener.Start();

			
			mGameplay = CGameState.Instance.Gameplay;
			CGameState.Instance.NextWave();

			long lastTicks = DateTime.Now.Ticks;
			float fps = 30.0f;
			float fixedDT = 1.0f / fps;
			string lastText = string.Empty;
			// Game Loop
			while (true)
			{
				TimeSpan span = new TimeSpan(DateTime.Now.Ticks - lastTicks);
				if( span.TotalMilliseconds < (fixedDT * 1000.0f) )
					continue;
				lastTicks = DateTime.Now.Ticks;

				mGameplay.Update(fixedDT);
				
				listener.UpdateKeysLatestMessages(mFrameInput);

				StringBuilder builder = new StringBuilder();

				if( mGameplay.CurrentDialog != null )
				{
					PrintGameStats(builder);
					PrintCharacters(builder);
					PrintDialog(builder);

					if (mFrameInput.Count != 0)
					{
						mGameplay.CloseDialog();
					}
				}
				else if( mGameplay.WaveState == WaveState.Countdown)
				{
					PrintGameStats(builder);
					PrintCharacters(builder);
					PrintCountdown(builder);
				}
				else if (mGameplay.WaveState == WaveState.Playing)
				{
					PrintGameStats(builder);
					builder.AppendLine();
					PrintCharacters(builder);
					builder.AppendLine();
					PrintOptions(builder);
					HandleEmailSelectionInput();
				}
				else if( mGameplay.WaveState == WaveState.Finished )
				{
					if( mGameplay.WaveResults == WaveResult.Success )
					{
						PrintWaveSummary(builder);
						if (mFrameInput.Count != 0)
						{
							CGameState.Instance.WaveComplete();
						}
                    }
					else
					{
						PrintGameOver(builder);
						if (mFrameInput.Count != 0)
							break;
					}
				}

				string thisString = builder.ToString();
				if(lastText != thisString)
				{
					Console.Clear();
					Console.WriteLine(thisString);
					lastText = thisString;
				}

				//Thread.Yield();
			}

			listener.Stop();

		}
	}
}
