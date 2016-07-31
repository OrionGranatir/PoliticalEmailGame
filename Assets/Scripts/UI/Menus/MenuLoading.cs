using UnityEngine;
using System.Collections;

public class MenuLoading : MonoBehaviour 
{
	private MenuManager menuManager;
	private DataManager dataManager;

	void Start() 
	{
		menuManager = GameObject.Find( "Menu Manager" ).GetComponent<MenuManager>();
		dataManager = GameObject.Find( "Data Manager" ).GetComponent<DataManager>();
	}

	void Update()
	{
		if( dataManager.IsReady() )
		{
			menuManager.ShowMainMenu();
		}
	}
}
