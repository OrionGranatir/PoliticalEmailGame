using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class UISummary : MonoBehaviour 
{
	public Text waveScore;
	public Text emailsCollected;
	public Text justiceDepartmentStanding;
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

	void Update() 
	{
		switch( state.value )
		{
			case State.Init:
			{
				// Load stats
				waveScore.text = uiManager.gameplayDriver.mGameplay.WaveScore.ToString();
				emailsCollected.text = uiManager.gameplayDriver.mGameplay.WaveEmail.ToString();
				justiceDepartmentStanding.text = uiManager.gameplayDriver.mGameplay.JusticeDepartmentStanding;
				totalScore.text = uiManager.gameplayDriver.mGameplay.TotalScore.ToString();

				state.value = State.Waiting;
				break;
			}

			case State.Waiting:
			{
				// Do nothing and wait for the player to start a new wave
				break;
			}
		}
	}

	public void StartNewWave()
	{
		uiManager.StartNewWave();
	}
}
