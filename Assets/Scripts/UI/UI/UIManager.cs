using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour 
{
	public Canvas canvasGame;
	public Canvas canvasGameOver;
	public Canvas canvasSummary;

	public GameplayDriver gameplayDriver { get; private set; }

	private UIGame     uiGame;
	private UIGameOver uiGameOver;
	private UISummary  uiSummary;

	private bool tutorial = true;

	void Start () 
	{
		gameplayDriver = GameObject.Find( "GameplayDriver" ).GetComponent<GameplayDriver>();

		uiGame = canvasGame.GetComponent<UIGame>();
		uiGameOver = canvasGameOver.GetComponent<UIGameOver>();
		uiSummary = canvasSummary.GetComponent<UISummary>();

		ShowGame();
	}

	// Returns true if the UI Manager is ready
	public bool IsReady()
	{
		return gameplayDriver.IsReady();
	}

	// Returns true if the tutorial should be shown
	public bool ShowTutorial()
	{
		bool result = tutorial;
		tutorial = false;
		return result;
	}

	// Triggers the start of a new wave
	public void StartNewWave()
	{
		// Setup the game UI for a new day
		uiGame.Configure();
		
		// Tell the gameplay drive to start a new wave
		gameplayDriver.mGameplay.StartWave();
	}

	public void ShowGame()
	{
		HideAll();
		canvasGame.gameObject.SetActive( true );
	}

	public void ShowGameOver()
	{
		HideAll();
		canvasGameOver.gameObject.SetActive( true );
		uiGameOver.Configure();
	}

	public void ShowSummary()
	{
		HideAll();
		canvasSummary.gameObject.SetActive( true );
		uiSummary.Configure();
	}

	private void HideAll()
	{
		canvasGame.gameObject.SetActive( false );
		canvasGameOver.gameObject.SetActive( false );
		canvasSummary.gameObject.SetActive( false );
	}
}
