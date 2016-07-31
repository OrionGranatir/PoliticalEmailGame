using UnityEngine;
using System.Collections;

public class MenuOptions : MonoBehaviour 
{
	private MenuManager menuManager;

	void Start () 
	{
		menuManager = GameObject.Find( "Menu Manager" ).GetComponent<MenuManager>();
	}

	public void MainMenu()
	{
		menuManager.ShowMainMenu();
	}
}
