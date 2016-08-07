using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIGame : MonoBehaviour 
{
	public GameObject desktop;

	public GameObject inboxCount;
	public GameObject timer;
	public GameObject score;
	public Text inboxCountText;
	public Text timerText;
	public Text scoreText;

	enum State
	{
		Init,
		NewWave,
		Tutorial,
		StartWave,
		Working,
		Waiting
	}

	private StateTemplate<State> state;
	private UIManager uiManager;

	void Start () 
	{
		state = new StateTemplate<State>( State.Init );
		uiManager = GameObject.Find( "UI Manager" ).GetComponent<UIManager>();

		// Hide the desktop until the game has finished loading
		desktop.SetActive( false );
	}

	public void Configure()
	{
		state.value = State.NewWave;
	}
	
	void Update() 
	{
		switch( state.value )
		{
			case State.Init:
			{
				if( uiManager.IsReady() )
				{
					state.value = State.NewWave;
					desktop.SetActive( true );
				}
				break;
			}

			case State.NewWave:
			{
				if( uiManager.ShowTutorial() )
				{
					// Load the tutorial
					// ...

					// Start the tutorial
					state.value = State.Tutorial;
				}
				else
				{
					// Load email for the day
					// ...

					// Start the day
					state.value = State.StartWave;
				}
				break;
			}

			case State.Tutorial:
			{
				Tutorial();
				break;
			}

			case State.StartWave:
			{
				StartWave();
				break;
			}

			case State.Working:
			{
				Working();
				break;
			}

			case State.Waiting:
			{
				// Waiting for the UI Manager to move us into a new state
				break;
			}
		}
	}

	private void Tutorial()
	{
		if( state.StateTriggered() )
		{
			inboxCount.SetActive( true );
			timer.SetActive( false );
			score.SetActive( false );
		}

		// Update our inbox, timer, and score
		inboxCountText.text = uiManager.gameplayDriver.mGameplay.InboxCount.ToString();

		state.value = State.StartWave;
	}

	private void StartWave()
	{
		if( state.StateTriggered() )
		{
			inboxCount.SetActive( true );
			timer.SetActive( false );
			score.SetActive( false );
		}

		// Update our inbox, timer, and score
		inboxCountText.text = uiManager.gameplayDriver.mGameplay.InboxCount.ToString();

		if( uiManager.gameplayDriver.mGameplay.WaveStarted() )
		{
			state.value = State.Working;
		}
	}

	private void Working()
	{
		if( state.StateTriggered() )
		{
			inboxCount.SetActive( true );
			timer.SetActive( true );
			score.SetActive( true );
		}

		// Update our inbox, timer, and score
		inboxCountText.text = uiManager.gameplayDriver.mGameplay.InboxCount.ToString();
		timerText.text = uiManager.gameplayDriver.mGameplay.WaveDuration.ToString();
		scoreText.text = uiManager.gameplayDriver.mGameplay.WaveScore.ToString();

		// Check if the day is complete
		if( uiManager.gameplayDriver.mGameplay.WaveComplete() )
		{
			uiManager.ShowSummary();
			state.value = State.Waiting;
		}

		// Check if it's game over, man...
		if( uiManager.gameplayDriver.mGameplay.GameOver() )
		{
			uiManager.ShowGameOver();
			state.value = State.Waiting;
		}
	}
}
