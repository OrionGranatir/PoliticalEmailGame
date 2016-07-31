using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MenuManager : MonoBehaviour 
{
	public GameObject menuContext;
	public Canvas canvasLoading;
	public Canvas canvaseMain;
	public Canvas canvasOptions;

	void Start () 
	{
		// Start on the loading screen
		ShowLoadingScreen();
	}

	public void ShowLoadingScreen()
	{
		HideAll();
		canvasLoading.gameObject.SetActive( true );
	}

	public void ShowMainMenu()
	{
		HideAll();
		canvaseMain.gameObject.SetActive( true );
	}

	public void ShowOptions()
	{
		HideAll();
		canvasOptions.gameObject.SetActive( true );
	}

	// Exit the main menu, this means we destory all the menu items
	public void Exit()
	{
		Destroy( menuContext );
	}

	private void HideAll()
	{
		canvasLoading.gameObject.SetActive( false );
		canvaseMain.gameObject.SetActive( false );
		canvasOptions.gameObject.SetActive( false );
	}
}
