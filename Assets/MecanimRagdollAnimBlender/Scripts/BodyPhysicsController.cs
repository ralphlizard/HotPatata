using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BodyPhysicsController: MonoBehaviour 
{
	public float KNOCKOUT_RECOVER_RATE = 0.1f; // used to set how fast parts recover from being knocked out

	//the states the rig may be in
	public enum State
	{
		DEFAULT, // normal blending between animation and physics
		FALLING, // when falling after a collision has caused the rig to lose balance
		RECOVERING, // when regaining composure after being knocked down
        GETTING_UP // when getting up after getting knocked down (after recovering)
	}

	List<Rigidbody> physRigidbodies_ = new List<Rigidbody>();

	private State currState_ = State.DEFAULT;

	private float knockoutTimer_ = 0.0f;

	public Transform m_animGO; //the animated game object without physics (rigidbodies etc...)
	public Transform m_physicsGO; // the game object with physics (all the Physics Body Parts) attached
	public float m_currOffBalance = 0.0f; // The current amount the char is off balance
	public float m_balance = 100.0f; // The maximum amount the char can be off balance without falling. Higher = better balance.
	public float m_balanceRecovery = 0.75f; //ratio per second
	public float m_knockoutTime = 2.0f; // how long to stay on the ground before recoverying (in seconds)
    public float m_getupTime = 2.5f; //how long it takes to get up in seconds
	public Rigidbody m_hips; // root bone rigidbody
    public Vector3 m_GOVelocity = Vector3.zero; //the overall speed for the entire game object. This should be set by the movement script
	public float m_getUpSpeed = 0.25f;//This can be used to slow down the getup animation if the character isn't blending back quickly enough. In other words, 
									 // if the character is knocked down and doesn't keep up with the get up animations, lower this.

    public string m_getupBackAnimFlag = "GetUpBack"; //the name of the flag in the controller to use to play the get up from back animation
	public string m_getupStomachAnimFlag = "GetUpStomach"; //the name of the flag in the controller to use to play the get up from stomach animation

    public float m_GOSpeedMod = 15.0f; // this modifies how much game object overall speed is maintained by the body parts. Use this to have momentum more impact the rig after collapsing.

	// Use this for initialization
	void Start () 
	{
		//Temp
		//Find all the transforms in the character, assuming that this script is attached to the root
		Component[] components= m_physicsGO.GetComponentsInChildren(typeof(Transform));
		
		//For each of the transforms, create a BodyPart instance and store the transform 
		foreach (Component c in components)
		{
			if(m_hips == null && c.name == "Hips")
			{
				m_hips = c.GetComponent<Rigidbody>();
			}
			if(c.GetComponent<Rigidbody>())
			{
				physRigidbodies_.Add(c.GetComponent<Rigidbody>());
			}
		}

        ToggleGravity(false);
	}

	public float blendAmount = 1.0f;
	// Update is called once per frame
	void Update () 
	{
		//check to see if the entity has lost balance
		if (m_currOffBalance >= m_balance && (currState_ == State.DEFAULT || currState_ == State.GETTING_UP))
		{
			Fall();
		}

		//update the current balance based on the recovery rate;
		m_currOffBalance *= 1.0f - (m_balanceRecovery * Time.deltaTime);

        

		switch(currState_)
		{

		case State.FALLING:
			if(knockoutTimer_ <= 0.0f)
			{
				//if we have regained our balance, then recover
				if(m_currOffBalance <= m_balance)
				{
					Recover();
				}
			}
			else 
			{
				knockoutTimer_ -= Time.deltaTime;
			}

			break;

		//this state handles how the rig recovers from being knocked down
		case State.RECOVERING:

			//when finished recovering, go to getting up state (make sure we dont play the get up anims again)
            if (CheckIfBlended(0.7f))
            {
                currState_ = State.GETTING_UP;
                m_animGO.GetComponent<Animator>().speed = 1.0f;
                m_animGO.GetComponent<Animator>().SetBool(m_getupBackAnimFlag, false);
                m_animGO.GetComponent<Animator>().SetBool(m_getupStomachAnimFlag, false);
                ChangeRecoveryToDefaults();
            }
			break;

		//wait the time required before going back to the defualt state
        case State.GETTING_UP:
            
            knockoutTimer_ += Time.deltaTime;
            if (knockoutTimer_ > m_getupTime)
            {
                knockoutTimer_ = 0.0f;
                currState_ = State.DEFAULT;
            }

            break;
		}
	}


	//change the gravity on all the physics part rigidbodies
	void ToggleGravity(bool on)
	{
		foreach(Rigidbody r in physRigidbodies_)
		{
			r.useGravity = on;
		}
	}


	//change the blend ratio of all the physics parts. This can be used to set all parts to blend to physics rather than animation.
	void ChangeAllBlends(float newWeight)
	{
		foreach(Rigidbody r in physRigidbodies_)
		{
			r.GetComponent<PhysicsBodyPart>().m_blend = newWeight;
		}
	}

	//change the recovery rate of all the physics parts.
	void ChangeAllRecoveryRates(float newRate)
	{
		foreach(Rigidbody r in physRigidbodies_)
		{
			r.GetComponent<PhysicsBodyPart>().m_controlRegain = newRate;
		}
	}

	//change the recover rate back to default for all physics parts
    void ChangeRecoveryToDefaults()
    {
        foreach (Rigidbody r in physRigidbodies_)
        {
            r.GetComponent<PhysicsBodyPart>().m_controlRegain = r.GetComponent<PhysicsBodyPart>().m_defaultControlRegain;
        }
    }

	/*
	 * Goes through the body parts checking if they are still not amount blended with anim. Returns
	 * false if they are not amount blended with animation.
	 */
	bool CheckIfBlended(float amount)
	{
		foreach(Rigidbody r in physRigidbodies_)
		{
			//r.GetComponent<PhysicsBodyPart>().CONTROL_REGAIN = newRate;
			if(r.GetComponent<PhysicsBodyPart>().m_blend > amount)
			{
				return false;
			}
		}

		return true;
	}

	/* 
	 * The functionality required for the rig to fall. Turns gravity on for all rig parts. Sets all physics body parts to blend to physics. Sets all physics body parts to not recover(for now)
	 * Set the physics controller's state to falling. Applies momentum to the rig based on the object's current velocity.
	 */
	public void Fall()
	{
		ToggleGravity(true);
		ChangeAllBlends(1.0f);
		ChangeAllRecoveryRates(0.0f);
		currState_ = State.FALLING;
		knockoutTimer_ = m_knockoutTime;

        for (int i = 0; i < physRigidbodies_.Count; ++i)
        {
			physRigidbodies_[i].AddForce(m_GOVelocity * m_GOSpeedMod);
        }
	}

	/*
	 * The functionality required for the recover state. This includes playing the get up animation, turning gravity off, setting the animation speed, repositioning the game object to the physics
	 * rig's position.
	 */
	public void Recover()
	{
		currState_ = State.RECOVERING;
		ToggleGravity(false);
		ChangeAllRecoveryRates(KNOCKOUT_RECOVER_RATE);
		Quaternion tempRot;
		if(m_hips.transform.forward.y >= 0.0f)
		{
            m_animGO.GetComponent<Animator>().SetBool(m_getupBackAnimFlag, true);

            Vector3 look = -m_hips.transform.up;
            look.y = 0.0f;

			tempRot = Quaternion.LookRotation(look, Vector3.up);
		}
		else
		{
            m_animGO.GetComponent<Animator>().SetBool(m_getupStomachAnimFlag, true);

            Vector3 look = m_hips.transform.up;
            look.y = 0.0f;

			tempRot = Quaternion.LookRotation(look, Vector3.up);
		}

		m_animGO.GetComponent<Animator>().speed = m_getUpSpeed;

		List<Transform> physObjects = new List<Transform>();
		for(int i = 0; i < m_physicsGO.childCount; ++i)
		{
			physObjects.Add(m_physicsGO.GetChild(i));
		}

		//set the parent GO position to the physics rig's hips position
		m_physicsGO.DetachChildren();

		Vector3 temp = m_physicsGO.transform.root.position;
		temp.x = m_hips.transform.position.x;  
		temp.z = m_hips.transform.position.z;
		m_physicsGO.transform.root.position = temp;
        m_animGO.transform.position = temp;

		m_physicsGO.transform.root.rotation = tempRot;
        m_animGO.transform.rotation = tempRot;
		
		for(int i = 0; i < physObjects.Count; ++i)
		{
			physObjects[i].parent = m_physicsGO;
		}
	}

    public State CurrState
    {
        get
        {
            return currState_;
        }
    }
}
