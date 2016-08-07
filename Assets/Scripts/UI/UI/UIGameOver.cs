using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIGameOver : MonoBehaviour 
{
	public Text waveScore;
	public Text totalScore;

	enum State
	{
		Init,
		Waiting
	}

	private StateTemplate<State> state;
	private UIManager uiManager;

	void Awake() 
	{
		state = new StateTemplate<State>( State.Init );
		uiManager = GameObject.Find( "UI Manager" ).GetComponent<UIManager>();
	}

	public void Configure()
	{
		state.value = State.Init;
	}
	
	void Update () 
	{
		switch( state.value )
		{
			case State.Init:
			{
				// Load stats
				waveScore.text = uiManager.gameplayDriver.mGameplay.WaveScore.ToString();
				totalScore.text = uiManager.gameplayDriver.mGameplay.TotalScore.ToString();

				state.value = State.Waiting;
				break;
			}

			case State.Waiting:
			{
				// After a delay, check if the player has tapped...
				if( state.TimeInState() > 0.75f && Input.GetMouseButtonUp(0) )
				{
					// Load the main menu
					SceneManager.LoadScene( "MainMenu" );
				}
				break;
			}
		}
	}
}
