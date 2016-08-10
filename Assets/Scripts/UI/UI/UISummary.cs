using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using Backend;

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

				SummaryScreenInfo info = CGameState.Instance.Gameplay.SummaryScreenInfo;
				waveScore.text = info.WaveScore.ToString();
				emailsCollected.text = info.SortedCount.ToString();
				justiceDepartmentStanding.text = info.JusticeDepartmentStanding.ToString();
				totalScore.text = info.TotalScore.ToString();

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
