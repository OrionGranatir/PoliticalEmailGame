using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DropTarget : MonoBehaviour 
{
	void Start () 
	{
	}
	
	void Update () 
	{
		
	}

	public void RecieveEmail( Email email )
	{
		Debug.Log( "I got an email from my intern." );
	}
}
