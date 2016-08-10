using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Backend;

public class DropTarget : MonoBehaviour
{

	public int DropTargetIndex;

	private CCharacter mCharacter;
	private CharacterMood mMood;
	private Image image;

	public bool HasCharacter  { get { return mCharacter != null; } }

	private UIManager uiManager;

	void Start() 
	{
		uiManager = GameObject.Find( "UI Manager" ).GetComponent<UIManager>();
		image = GetComponent<Image>();
	}

	void Update()
	{
		// Lazy update character image
		if( uiManager.IsReady() )
		{
			CCharacter lastCharacter = mCharacter;

			if( DropTargetIndex < CGameState.Instance.Gameplay.ActiveCharacters.Count )
			{
				mCharacter = CGameState.Instance.Gameplay.ActiveCharacters[DropTargetIndex];
				image.enabled = true;
			}
			else
			{
				mCharacter = null;
				image.enabled = false;
			}
			if( lastCharacter != mCharacter )
			{
				// new character arrived
			}
			if( mCharacter != null && mCharacter.mood != mMood )
			{
				// animate to mood
				mMood = mCharacter.mood;
            }
		}
	}

	public CharacterType Category()
	{
		return mCharacter != null ? mCharacter.catergory : CharacterType.Unknown;
	}

	public void RecieveEmail( Email email )
	{
		
	}
}
