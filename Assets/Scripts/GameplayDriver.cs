using UnityEngine;
using System.Collections;
using Backend;

public class GameplayDriver : MonoBehaviour {

	public Email UIEmailPrefab;

	public CGameplay mGameplay { get; private set; }
	private Email mCurrentEmailUI;

	private bool init = false;
	private DataManager dataManager;

	// Use this for initialization
	void Start () 
	{
		dataManager = GameObject.Find( "Data Manager" ).GetComponent<DataManager>();
	}

	void Init()
	{
		init = true;

		// disable the email object. We'll use it as a prefab for generating future emails
		// and for positional data
		UIEmailPrefab.gameObject.SetActive(false); 

		mGameplay = new CGameplay();
		mGameplay.NewEmailEvent += MGameplay_NewEmailEvent;

		mGameplay.StartWave();
	}

	// Returns true when the game backend has fully loaded
	public bool IsReady()
	{
		return init;
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
	void Update () 
	{
		if( !init )
		{
			if( dataManager.IsReady() )
			{
				Init();
			}
		}
		else
		{
			mGameplay.Update(Time.deltaTime);
		}
	}
}
