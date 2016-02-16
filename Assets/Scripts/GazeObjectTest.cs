using UnityEngine;
using System.Collections;

public class GazeObjectTest : MonoBehaviour {
	public GazeController gazeController;
	public GazeObjectTest otherFlower;
	public int textTag;
	public float textDelay;
	public float activeBuffer;
	public float lookBuffer;

	Animator anim;
	string text = "";
	string curText = "";
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
		UpdateText (text);
		anim = GetComponentInChildren<Animator> ();
		fullyInactive = true;
		textTag = 11;	
	}
	
	// Update is called once per frame
	void Update () {
		anim.SetBool ("lookedAt", lookedAt);
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
			curText = "";
			curTextLength = 0;
			transform.GetComponentInChildren<TextMesh>().text = curText;
		}

		fullyActive = visibility == 1;
		fullyInactive = visibility == 0;

		if (fullyActive) 
		{
			if (textTag == 11)
			{
				UpdateText("All men are mortal.");
				otherFlower.textTag = 12;
			}
			if (textTag == 12)
			{
				UpdateText("Socrates is a man.");
				otherFlower.textTag = 13;
			}
			if (textTag == 13)
				UpdateText("Socrates is mortal.");
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
	}

	void UpdateText(string newText)
	{
		text = newText;
		fullTextLength = text.Length;
	}
}