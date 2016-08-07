using UnityEngine;
using System.Collections;
using Backend;

public class CCharacter 
{
	public enum Mood
	{
		Neutral,
		Angry,
		Happy
	}

	public Mood mood;
	public EmailCategory catergory;
	public bool shown = false;

	public CCharacter() 
	{
		catergory = EmailCategory.Unknown;
		mood = Mood.Neutral;
	}

	public CCharacter(EmailCategory catergory ) 
	{
		this.catergory = catergory;
		mood = Mood.Neutral;

		// Start out showing Senator, Bill, Manager, and Trash
		if( catergory == EmailCategory.Senator 
		 || catergory == EmailCategory.Bill
		 || catergory == EmailCategory.Manager
		 || catergory == EmailCategory.Trash )
		{
			shown = true;
		}
	}
}
