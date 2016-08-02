using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Backend;

using SimpleJSON;

public class EmailData
{
	public int id;
	public string to;
	public string from;
	public string subject;
	public string body;
}

public enum DownloadDataType
{
	Email,
	Phrases,
}

public class DownloadDataInfo
{
	public string Filename; // Name of the data file. Not the full path
	public string URL;
	public string DebugName;
	public DownloadDataType DataType;
}

public class DataManager : MonoBehaviour 
{
	enum State
	{
		Init,
		Load,
		Ready
	}

	StateTemplate<State> state;

	public float progress { get; private set; }

	private int loadCount = -1;
	private List<EmailData> emails;

	private string wwwEmails;

	private List<DownloadDataInfo> mToLoad = new List<DownloadDataInfo>();
	private const string fileEmails = "emails.data";
	private const string filePhrases = "phrases.data";

	private const string urlEmails = "https://spreadsheets.google.com/feeds/list/1knOSeGd7ebLJtmIapm5fZCgOgF53Aq_bY1cg82UIW3E/od6/public/full?alt=json";
	private const string urlPhrases = "https://spreadsheets.google.com/feeds/list/1eW4kMHdFOG4C9sgWyuc_0wvfR4mflDZSasY9irrBa68/od6/public/full?alt=json";

#if (UNITY_STANDALONE_WIN || UNITY_WSA)
	private const string filePath = "file:///";
	#else
	private const string filePath = "file://";
	#endif

	void Start() 
	{
		// Hook up print callbacks
		CBackendUtil.DebugPrintCallback = (text) => Debug.Log(text);

		// Add the email request
		DownloadDataInfo emailInfo = new DownloadDataInfo();
		emailInfo.Filename = "emails.data";
		emailInfo.URL = "https://spreadsheets.google.com/feeds/list/1knOSeGd7ebLJtmIapm5fZCgOgF53Aq_bY1cg82UIW3E/od6/public/full?alt=json";
		emailInfo.DebugName = "Email Templates";
		emailInfo.DataType = DownloadDataType.Email;
		mToLoad.Add(emailInfo);

		// Add the phrase request
		DownloadDataInfo phraseInfo = new DownloadDataInfo();
		phraseInfo.Filename = "phrases.data";
		phraseInfo.URL = "https://spreadsheets.google.com/feeds/list/1eW4kMHdFOG4C9sgWyuc_0wvfR4mflDZSasY9irrBa68/od6/public/full?alt=json";
		phraseInfo.DebugName = "Phrases";
		phraseInfo.DataType = DownloadDataType.Phrases;
		mToLoad.Add(phraseInfo);

		state = new StateTemplate<State>( State.Init );

		// If we don't have data file, write them to the persistent storage
		/*
		if( !File.Exists( Application.persistentDataPath + "/" + fileEmails ) )
		{
			WriteInitialFiles();
		}
		*/

		// Determine if we want to load files from the internet or files
		if( Application.internetReachability != NetworkReachability.NotReachable || true )
		{
			wwwEmails = urlEmails;
		}
		else
		{
			wwwEmails = filePath + Application.persistentDataPath + "/" + fileEmails;
		}
	}

	private void LoadEmailData( bool success, string text)
	{
        if (!success)
			throw new System.Exception("Failed To Load Email Data!");
		CEmailGenerator.Instance.AddEmailTemplatesJSON(text);
	}
	private void LoadPhraseData(bool success, string text)
	{
		if (!success)
			throw new System.Exception("Failed To Load Phrase Data!");
		CEmailGenerator.Instance.AddPhrasesJSON(text);
	}

	// Update is called once per frame
	void Update() 
	{
		switch( state.value )
		{
			case State.Init:
			{
				// Check if we already have a DataManager
				DataManager[] existingData = GameObject.FindObjectsOfType<DataManager>();
				if( existingData.Length != 1 )
				{
					// We don't need two, so destory ourself
					Destroy( gameObject );
				}
				else
				{
					emails = new List<EmailData>();  // Must come first because it tests for a valid connection

					state.value = State.Load;
				}
				break;
			}

			case State.Load:
			{
				if( state.StateTriggered() )
				{
					loadCount = mToLoad.Count;
					foreach(DownloadDataInfo info in mToLoad )
					{
						StartCoroutine(DownloadData(info));
                    }
				}

				if( loadCount == 0 )
				{
					CEmailGenerator.Instance.PrintSummary();
					CEmailGenerator.Instance.ValidateEmails();
                    state.value = State.Ready;
				}

				progress = ( 5.0f - (float)loadCount )/ 5.0f;

				break;
			}

			case State.Ready:
			{
				// Do nothing...
				break;
			}
		}
	}

	public bool IsReady()
	{
		return ( state.value == State.Ready );
	}

	public EmailData GetEmail( int id )
	{
		EmailData result = null;

		for( int index = 0; index < emails.Count; index++ )
		{
			EmailData email = emails[ index ];
			if( email.id == id )
			{
				result = email;
				break;
			}
		}

		return result;
	}

	private delegate void DataReadyDelegate(bool result, string text);

	private string GetFullPersistentFilename( string filename )
	{
		return filePath + Application.persistentDataPath + "/" + filename;
	}

	private IEnumerator DownloadData(DownloadDataInfo info)
	{
		WWW www = null;
		int methodIndex = 0;
        for (methodIndex = 0; methodIndex < 2; methodIndex++)
		{
			string url = info.URL;
			if(methodIndex == 1)
			{
				url = GetFullPersistentFilename(info.Filename);
			}
			
			// Pull data from the web
			www = new WWW(url);

			// Set a short timeout for downloading vote data
			float startTime = Time.time;
			bool failed = false;

			while (!www.isDone)
			{
				if (Time.time - startTime > 3.25f)
				{
					failed = true;
					break;
				}
			}

			// Check if we failed to read the file online, and switch to loading files
			if (failed || !string.IsNullOrEmpty(www.error))
			{
				www.Dispose();
				www = null;
			}
			else
			{
				break;
			}
		}

		if (www == null)
		{
			loadCount--;
			throw new System.Exception("Failed Loading " + info.DebugName);
		}
		else
		{
			// Parse data
			var webData = JSON.Parse(www.text);
			if( info.DataType == DownloadDataType.Email)
			{
				CEmailGenerator.Instance.AddEmailTemplatesJSON(www.text);
			}
			else
			{
				CEmailGenerator.Instance.AddPhrasesJSON(www.text);
			}

			// Write the download content to a local file

			string filename = GetFullPersistentFilename(info.Filename);
            //File.WriteAllBytes(filename, www.bytes); // This is throwing exception?

			www.Dispose();
			loadCount--;
		}

		yield break;
	}

	private IEnumerator DownloadEmailData()
	{
		// Pull data from the web
		WWW www = new WWW( wwwEmails );

		// Set a short timeout for downloading vote data
		float startTime = Time.time; 
		bool failed = false;

		while( !www.isDone )
		{
			if( Time.time - startTime > 3.25f )
			{ 
				failed = true; 
				break; 
			}
		}

		// Check if we failed to read the file online, and switch to loading files
		if(failed || !string.IsNullOrEmpty(www.error))
		{
			www.Dispose();
			wwwEmails = filePath + Application.persistentDataPath + "/" + fileEmails;

			StartCoroutine( "DownloadEmailData" );
			yield break;
		}

		// Parse data
		var webData = JSON.Parse( www.text );

		// Check update date
		string date = webData["feed"]["updated"]["$t"];
		Debug.Log( "Email Date: " + date );

		// Load data
		int numEntries = webData["feed"]["entry"].Count;
		for( int index = 0; index < numEntries; index++ )
		{
			EmailData email = new EmailData();
			var entry = webData["feed"]["entry"][index];

			email.id      = entry["gsx$id"]["$t"].AsInt;
			email.to      = entry["gsx$to"]["$t"];
			email.from    = entry["gsx$from"]["$t"];
			email.subject = entry["gsx$subject"]["$t"];
			email.body    = entry["gsx$body"]["$t"];

			emails.Add( email );
		}

		// Write the download content to a local file
		string fullPath = Application.persistentDataPath + "/" + fileEmails;
		File.WriteAllBytes( fullPath, www.bytes );

		// Mark that we are done loading
		loadCount--;
	}

	private void WriteInitialFiles()
	{
		TextAsset data = Resources.Load( "Offline/" + fileEmails ) as TextAsset;
		string fullPath = Application.persistentDataPath + "/" + fileEmails;
		File.WriteAllBytes( fullPath, data.bytes );
	}
}
