using UnityEngine;
using System.Collections;

public class Note : MonoBehaviour {
	public float textDelay = 0.1f;
	public float activeBuffer = 1;
	public float lookBuffer = 0.1f;
	public string text = "This thing is really odd. It's fast thin and fat.\n" +
				"When fat it flies high, when thin it lands down.\n" +
			"Don’t forget my 3 brothers.\n";
	
	GazeController gazeController;
	string curText;
	float lookedAtDuration;
	float prevLookTime;
	float lastTextUpdateTime;
	float visibility;
	int fullTextLength;
	int curTextLength;
	bool lookedAt;
	bool fullyActive;
	bool fullyInactive;

	// Use this for initialization
	void Start () {
		gazeController = GameObject.FindGameObjectWithTag("Player").GetComponent<GazeController>();
		fullyInactive = true;
		fullTextLength = text.Length;
		curText = "";
	}
	
	// Update is called once per frame
	void Update () {
		ControlState ();
		
		GetComponentInChildren<TextMesh> ().color = new Color (1f, 1f, 1f, visibility);
		
	}
	
	void ControlState()
	{
		if (Time.time - prevLookTime < lookBuffer &&
		    !lookedAt) {
			lookedAt = true;
		} 
		else if (Time.time - prevLookTime >= lookBuffer &&
		         lookedAt)
		{
			lookedAt = false;
		}
		
		if (!fullyActive && lookedAt) {
			visibility += 1 * Time.deltaTime;
		} 
		else if (!fullyActive && !lookedAt) 
		{
			visibility -= 1 * Time.deltaTime;
		}
		else if (fullyActive && Time.time - prevLookTime >= activeBuffer)
		{
			visibility -= 1 * Time.deltaTime;
		}
		visibility = Mathf.Clamp(visibility, 0, 1);
		
		if (!fullyInactive && visibility == 0)
		{
			gazeController.GazeRelease();
		}
		
		fullyActive = visibility == 1;
		fullyInactive = visibility == 0;
		
		if (fullyActive) 
		{
			DisplayText (curText);
		}
		
	}
	
	void LookedAt() 
	{
		prevLookTime = Time.time;
	}
	
	void DisplayText(string curText)
	{
		if (fullTextLength > curTextLength &&
		    Time.time >= lastTextUpdateTime + textDelay) 
		{
			curTextLength++;
			if (text[curTextLength - 1] == ' ')
			{
				curTextLength++;
			}
			lastTextUpdateTime = Time.time;
			curText = text.Substring(0, curTextLength);
			transform.GetComponentInChildren<TextMesh>().text = curText;
		}
		else if (gazeController.solved == 0);
			gazeController.solved++;
	}
	
}