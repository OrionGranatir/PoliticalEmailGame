using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Backend;

public class DropTarget : MonoBehaviour 
{
	public EmailCategory Category;
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
