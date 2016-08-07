using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Backend;

public class DropTarget : MonoBehaviour 
{
	public EmailCategory category;

	private bool init = false;
	private CCharacter character;
	private CCharacter.Mood mood;
	private Image image;

	private UIManager uiManager;

	void Start() 
	{
		uiManager = GameObject.Find( "UI Manager" ).GetComponent<UIManager>();
		image = GetComponent<Image>();
	}

	void Update()
	{
		// See if we need to initialize our data
		if( !init && uiManager.IsReady() )
		{
			init = true;
			character = uiManager.gameplayDriver.mGameplay.GetCharacter( category );
			mood = character.mood;
		}

		// Update our character
		if( init )
		{
			// Check if our mood has changed
			if( mood != character.mood )
			{
				// Animate the mood change
				// ...

				// Store our current mood
				mood = character.mood;
			}

			image.enabled = character.shown;
		}
	}

	public EmailCategory Category()
	{
		return character.catergory;
	}

	public bool CharacterShown()
	{
		return character.shown;
	}

	public void RecieveEmail( Email email )
	{
		
	}
}
