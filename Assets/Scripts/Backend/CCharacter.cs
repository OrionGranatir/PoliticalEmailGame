using System.Collections;
using Backend;

public class CCharacter 
{
	public CharacterMood mood;
	public CharacterType catergory;

	public CCharacter() 
	{
		catergory = CharacterType.Unknown;
		mood = CharacterMood.Neutral;
	}

	public CCharacter(CharacterType catergory ) 
	{
		this.catergory = catergory;
		mood = CharacterMood.Neutral;
	}
}
