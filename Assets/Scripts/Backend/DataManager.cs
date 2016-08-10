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
	Wave,
	Dialog
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

	private List<DownloadDataInfo> mToLoad = new List<DownloadDataInfo>();
	private const string fileEmails = "emails.data";
	private const string filePhrases = "phrases.data";
	private const string fileWaves = "waves.data";
	private const string fileDialog = "dialog.data";

#if (UNITY_STANDALONE_WIN || UNITY_WSA)
	private const string filePath = "file:///";
	#else
	private const string filePath = "file://";
	#endif

	void Start() 
	{
		// Hook up print callbacks
		CBackendUtil.DebugPrintCallback = (text) => Debug.Log(text);

		CGameData.ResetGameData();

		// Add the email request
		DownloadDataInfo emailInfo = new DownloadDataInfo();
		emailInfo.Filename = "emails.data";
		emailInfo.URL = CBackendUtil.EmailURL;
		emailInfo.DebugName = "Email Templates";
		emailInfo.DataType = DownloadDataType.Email;
		mToLoad.Add(emailInfo);

		// Add the phrase request
		DownloadDataInfo phraseInfo = new DownloadDataInfo();
		phraseInfo.Filename = "phrases.data";
		phraseInfo.URL = CBackendUtil.PhraseURL;
		phraseInfo.DebugName = "Phrases";
		phraseInfo.DataType = DownloadDataType.Phrases;
		mToLoad.Add(phraseInfo);

		// Add the phrase request
		DownloadDataInfo waveInfo = new DownloadDataInfo();
		waveInfo.Filename = "waves.data";
		waveInfo.URL = CBackendUtil.WaveURL;
		waveInfo.DebugName = "Waves";
		waveInfo.DataType = DownloadDataType.Wave;
		mToLoad.Add(waveInfo);

		// Add the phrase request
		DownloadDataInfo dialogInfo = new DownloadDataInfo();
		dialogInfo.Filename = "dialog.data";
		dialogInfo.URL = CBackendUtil.DialogURL;
		dialogInfo.DebugName = "Dialog";
		dialogInfo.DataType = DownloadDataType.Dialog;
		mToLoad.Add(dialogInfo);

		state = new StateTemplate<State>( State.Init );

		// If we don't have data file, write them to the persistent storage
		/*
		if( !File.Exists( Application.persistentDataPath + "/" + fileEmails ) )
		{
			WriteInitialFiles();
		}
		*/

	}

	private void LoadEmailData( bool success, string text)
	{
        if (!success)
			throw new System.Exception("Failed To Load Email Data!");
		CGameData.ParseEmailTemplates(text);
	}
	private void LoadPhraseData(bool success, string text)
	{
		if (!success)
			throw new System.Exception("Failed To Load Phrase Data!");
		CGameData.ParsePhraseData(text);
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
					CGameData.PostLoadLinking();
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
			else
			{
				//if (Application.internetReachability != NetworkReachability.NotReachable)
				//	continue;
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
				CGameData.ParseEmailTemplates(www.text);
			}
			else if( info.DataType == DownloadDataType.Phrases)
			{
				CGameData.ParsePhraseData(www.text);
			}
			else if (info.DataType == DownloadDataType.Dialog)
			{
				CGameData.ParseDialogData(www.text);
			}
			else if (info.DataType == DownloadDataType.Wave)
			{
				CGameData.ParseWaveData(www.text);
			}

			// Write the download content to a local file

			string filename = GetFullPersistentFilename(info.Filename);
            //File.WriteAllBytes(filename, www.bytes); // This is throwing exception?

			www.Dispose();
			loadCount--;
		}

		yield break;
	}

	private void WriteInitialFiles()
	{
		TextAsset data = Resources.Load( "Offline/" + fileEmails ) as TextAsset;
		string fullPath = Application.persistentDataPath + "/" + fileEmails;
		File.WriteAllBytes( fullPath, data.bytes );
	}
}
