using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using Backend;

public class Email : MonoBehaviour 
{
	public delegate void CategoryChooseDelegate( CharacterType category );

	public Text toLine;
	public Text fromLine;
	public Text subjectLine;
	public Text body;
	public Image document;
	public Image icon;
	private Mask mask;

	private Vector3 defaultPos;
	private Vector3 startingPos;
	private Vector3 targetPos;
	private float minY;
	private float maxY;
	private float xThreshold;

	private bool showDocument = true;
	private bool scrollingEnabled = false;

	private const float timeToScale = 0.35f;
	private float timeScaling = timeToScale;
	private CEmail mEmail;

	public event CategoryChooseDelegate CategoryChosenEvent;

	// Use this for initialization
	void Start () 
	{
		mask = GetComponentInParent<Mask>();

		// Determine our min/max
		RectTransform maskRect = mask.GetComponent<RectTransform>();
		RectTransform documentRect = document.GetComponent<RectTransform>();
		if( maskRect.rect.height < documentRect.rect.height )
		{
			scrollingEnabled = true;
			minY = transform.position.y;
			maxY = documentRect.rect.yMax + (documentRect.rect.height - maskRect.rect.height);
		}
		else
		{
			minY = transform.position.y;
			maxY = transform.position.y;
		}

		// Determine movement left/right to change the email into an icon
		xThreshold = maskRect.rect.width * 0.1f;

		// Store our starting position
		defaultPos = transform.position;
		targetPos = startingPos;
	}

	public void SetEmailData( CEmail email )
	{
		mEmail = email;
		toLine.text = "";
		fromLine.text = email.From;
		subjectLine.text = email.Subject;
		body.text = email.Body;
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
	}

	public void OnStartDrag()
	{
		defaultPos = transform.position;
		startingPos = Input.mousePosition;
	}

	public void OnDrag()
	{
		// Update our position
		targetPos = Input.mousePosition;
		
		// Check if we should change to the email icon
		if( showDocument )
		{
			Vector3 delta = Input.mousePosition - startingPos;
			float deltaX = delta.x;
			float deltaY = delta.y; 

			// Check if we should change to the email icon
			float distanceFromCenter = Mathf.Abs( targetPos.x - startingPos.x );
			if( distanceFromCenter > xThreshold )
			{
				ShowIcon();
			}
			else
			{
				// Clamp x to the middle
				deltaX = 0.0f;
			}

			// Clamps y
			deltaY = ( defaultPos.y + deltaY < minY ) ? minY - defaultPos.y : deltaY;
			deltaY = ( defaultPos.y + deltaY > maxY ) ? maxY - defaultPos.y : deltaY;

			// Allow us to scroll, only if enabled
			if( !scrollingEnabled )
			{
				deltaX = 0.0f;
				deltaY = 0.0f;
			}

			targetPos = defaultPos + new Vector3( deltaX, deltaY, 0.0f );
		}
		else
		{
			// Just follow the mouse position directly
			targetPos = Input.mousePosition;
		}

		// Move us to the target position
		transform.position = targetPos;
	}
		
	public void OnEndDrag(BaseEventData data)
	{
		if( !showDocument )
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
						if(CategoryChosenEvent != null)
						{
							CategoryChosenEvent(target.Category());
						}
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
	}

	private void ShowIcon()
	{
		showDocument = false;
		icon.gameObject.SetActive( true );

		// Set up animation
		timeScaling = 0.0f;

		// Disable the mask so we can move out of the email panel
		mask.enabled = false;
	}

	private void ShowDocument()
	{
		showDocument = true;
		icon.gameObject.SetActive( false );

		// Set up animation
		targetPos = defaultPos;
		timeScaling = 0.0f;

		// Enable the mask so we are only show in the email panel
		mask.enabled = true;
	}
}
