using UnityEngine;
using System.Collections;


public class TestPhysMover : MonoBehaviour 
{
	public float mSpeed = 3.0f;
	public float startingY_;
	public BodyPhysicsController bodyPhysics;

	// Use this for initialization
	void Start () 
	{
		startingY_ = transform.position.y;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(bodyPhysics.CurrState != BodyPhysicsController.State.DEFAULT)
		{	
			return;
		}

		Vector3 directionVector = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

		if (directionVector != Vector3.zero) {
			// Get the length of the directon vector and then normalize it
			// Dividing by the length is cheaper than normalizing when we already have the length anyway
			var directionLength = directionVector.magnitude;
			directionVector = directionVector / directionLength;
			
			// Make sure the length is no bigger than 1
			directionLength = Mathf.Min(1, directionLength);
			
			// Make the input vector more sensitive towards the extremes and less sensitive in the middle
			// This makes it easier to control slow speeds when using analog sticks
			directionLength = directionLength * directionLength;
			
			// Multiply the normalized direction vector by the modified length
			directionVector = directionVector * directionLength;
		}
		
		// Apply the direction to the CharacterMotor
		//motor.inputMoveDirection = transform.rotation * directionVector;
		//motor.inputJump = Input.GetButton("Jump");
		//transform.position += transform.rotation * directionVector * mSpeed * Time.deltaTime;
		//transform.position = new Vector3(transform.position.x, startingY_, transform.position.z);
		Vector3 movement = transform.rotation * directionVector * mSpeed;
		movement.y = 0.0f;
		bodyPhysics.m_GOVelocity = movement;
		movement *= Time.deltaTime;
		GetComponent<CharacterController>().Move(movement);
	}
}


// Use this for initialization
