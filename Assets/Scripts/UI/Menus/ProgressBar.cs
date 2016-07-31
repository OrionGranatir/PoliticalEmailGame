using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ProgressBar : MonoBehaviour 
{
	public Image image;
	public float progress;

	private float anchorMinX;
	private float anchorMaxX;
	private float anchorDiff;

	DataManager dataManager;

	// Use this for initialization
	void Start () 
	{
		dataManager = GameObject.FindObjectOfType<DataManager>();
		anchorMinX = image.rectTransform.anchorMin.x;
		anchorMaxX = image.rectTransform.anchorMax.x;
		anchorDiff = anchorMaxX - anchorMinX;
		image.rectTransform.anchorMax = new Vector2( anchorMinX, image.rectTransform.anchorMax.y );
	}
	
	// Update is called once per frame
	void Update () 
	{
		progress = dataManager.progress;
		float x = Mathf.Lerp( image.rectTransform.anchorMax.x, anchorMinX + anchorDiff * progress, Time.deltaTime * 5.0f );
		image.rectTransform.anchorMax = new Vector2( x, image.rectTransform.anchorMax.y );
	}
}
