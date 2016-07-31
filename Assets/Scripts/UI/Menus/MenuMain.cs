using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MenuMain : MonoBehaviour 
{
	public Button continueButton;
	private MenuManager menuManager;

	void Start () 
	{
		menuManager = GameObject.Find( "Menu Manager" ).GetComponent<MenuManager>();

		// Only enable the continue button if we have an existing game...
		continueButton.interactable = false;
	}

	public void NewGame()
	{
		// Load the game
		SceneManager.LoadScene( "Game", LoadSceneMode.Additive );

		// Tell the menu manager that we are exiting
		menuManager.Exit();
	}

	public void ContinueGame()
	{
	}

	public void Options()
	{
		menuManager.ShowOptions();
	}
}
