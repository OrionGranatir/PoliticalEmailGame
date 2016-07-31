using UnityEngine;
using System.Collections;
using Backend;

public class GameplayDriver : MonoBehaviour {

	public Email UIEmailPrefab;

	private CGameplay mGameplay;
	private Email mCurrentEmailUI;

	// Use this for initialization
	void Start () {

		// disable the email object. We'll use it as a prefab for generating future emails
		// and for positional data
		UIEmailPrefab.gameObject.SetActive(false); 

		mGameplay = new CGameplay();
		mGameplay.NewEmailEvent += MGameplay_NewEmailEvent;

		mGameplay.StartWave();
	}

	private void MGameplay_NewEmailEvent(CEmail prevEmail, CEmail newEmail)
	{
		if( mCurrentEmailUI != null )
		{
			Destroy(mCurrentEmailUI);
		}
		mCurrentEmailUI = Instantiate<Email>(UIEmailPrefab);
		mCurrentEmailUI.transform.SetParent(UIEmailPrefab.transform.parent.transform, false);
		mCurrentEmailUI.CategoryChosenEvent += MCurrentEmailUI_CategoryChosenEvent;
		mCurrentEmailUI.SetEmailData(newEmail);
        mCurrentEmailUI.gameObject.SetActive(true);
	}

	private void MCurrentEmailUI_CategoryChosenEvent(EmailCategory category)
	{
		mGameplay.CategoryChosen(category);
	}

	// Update is called once per frame
	void Update () {
		mGameplay.Update(Time.deltaTime);
	}
}
