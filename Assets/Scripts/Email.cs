using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class Email : MonoBehaviour 
{
	public Text toLine;
	public Text fromLine;
	public Text subjectLine;
	public Text body;
	public Image document;

	private Vector3 startingPos;
	private Vector3 targetPos;

	private bool showDocument = true;
	private const float timeToScale = 0.35f;
	private float timeScaling = timeToScale;

	// Use this for initialization
	void Start () 
	{
		startingPos = transform.position;
		targetPos = startingPos;
	}

	// Update is called once per frame
	void Update () 
	{
		// Adjust our scale and position, as needed
		if( timeScaling < timeToScale )
		{
			timeScaling += Time.deltaTime;
			if( showDocument )
			{
				document.transform.localScale = Vector3.Lerp( Vector3.zero, Vector3.one, timeScaling / timeToScale );
			}
			else
			{
				document.transform.localScale = Vector3.Lerp( Vector3.one, Vector3.zero, timeScaling / timeToScale );
			}
			transform.position = Vector3.Lerp( transform.position, targetPos, timeScaling / timeToScale );
		}
		else
		{
			transform.position = targetPos;
		}
	}

	public void OnStartDrag()
	{
		ShowIcon();
	}

	public void OnDrag()
	{
		targetPos = Input.mousePosition;
	}
		
	public void OnEndDrag(BaseEventData data)
	{
		ShowDocument();

		// Check where we have dropped...
		PointerEventData eventData = (PointerEventData)data;
		List<RaycastResult> hits = new List<RaycastResult>();
		EventSystem.current.RaycastAll( eventData, hits );

		bool foundTarget = false;
		if( hits.Count > 0 )
		{
			for( int index = 0; index < hits.Count; index++ )
			{
				DropTarget target = hits[ index ].gameObject.GetComponent<DropTarget>();
				if( target )
				{
					target.RecieveEmail( this );
					foundTarget = true;
					break;
				}
			}
		}

		// Check if we have found a target
		if( foundTarget )
		{
			// Destory ourselves
			Destroy( gameObject );
		}
	}

	private void ShowIcon()
	{
		showDocument = false;
		timeScaling = 0.0f;
	}

	private void ShowDocument()
	{
		showDocument = true;
		targetPos = startingPos;
		timeScaling = 0.0f;
	}
}
