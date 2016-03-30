using UnityEngine;
using System.Collections;

/*
 * 
 * Note: This class must be attached to a Game Object with a rigidbody 
 */
public class PhysicsBodyPart : MonoBehaviour 
{

	public float m_blend; // 0 = full anim, 1 = full physics
	public HumanBodyBones m_animBone; //The bone in the mecanim rig which this part is tied to
	public Animator m_animator; // The animator running the animation we wish to blend with
	public Transform[] m_connectedParts; // The other parts this part is connected with.
    public float m_controlRegain = 0.7f; // how quickly the part regains control.
	public float m_defaultControlRegain; // the default regain amount (how much to use after a knock down)
	public float[] m_connectedPartsWeight; //The ratio an impact hitting this part will impact connected parts.
	public float m_balanceRatio; // how much does this body part infuluence the character's ability to maintain balance
    public float m_blendMax = 1000.0f; // this gives us the ability to set the maximum allowable physics ratio (useful for legs), Set this between 0 - 1 to 
									   // make the entire rig collapse if this part is blended partially - > fully to physics.
    public float m_rigidness = 10.0f; // the higher this is, the more force is required to make this part blend to physics rather than anim. For instance, a chest is much more rigid than a forearm
									  // there is more give (beyond the difference in mass) in the forearm.
    public float m_blendSpeed = 0.2f; // the maximum percentage to lerp if not 100% blended with anim

	public float mBounceFactor = 3.0f; // How much to scale the effect of the impact on balance when hitting something while moving. This makes the character collapse more easily when hitting an object while moving.

	// Use this for initialization
	void Start () 
	{
		m_defaultControlRegain = m_controlRegain;
	}

	void Update()
	{
		if(m_blend > 0.0f)
		{
			float reduction = (m_controlRegain * Time.deltaTime);
			m_blend -= reduction;
			m_blend = Mathf.Clamp(m_blend, 0.0f, 1.0f);
		}
	}


	
	// This function updates the blend ratio. It then interpolates the part between its current transform and the animation part's transform (based on the blend ratio).
	// Also, velocities are applied to the objects.
	void FixedUpdate () 
	{
		float lerpSpeed = m_blendSpeed * (1.0f - m_blend);
		if(m_blend <= 0.0f)
		{
			lerpSpeed = 1.0f;
		}

		Transform animBone = m_animator.GetBoneTransform(m_animBone);


		//interpolate from current to anim transform based on the current m_blend rate.
		Vector3 newPos = transform.position;
		newPos = Vector3.Lerp(transform.position, animBone.position,
		                      lerpSpeed);
		
		Quaternion lerped = Quaternion.Slerp(transform.rotation, 
		                                     animBone.rotation, lerpSpeed);


		if(!transform.GetComponent<Rigidbody>().isKinematic)
		{
			transform.GetComponent<Rigidbody>().MovePosition(newPos);
			transform.GetComponent<Rigidbody>().MoveRotation(lerped);
		}

		GetComponent<Rigidbody>().velocity = m_blend * GetComponent<Rigidbody>().velocity;
		GetComponent<Rigidbody>().angularVelocity = m_blend * GetComponent<Rigidbody>().angularVelocity;
	}

	//update body parts with new blend weights (the impact cascades accross rig)
	public void AdjustBlendWeight(float newBlend)
	{
		if(newBlend > 0.0f)
		{
			GetComponent<Rigidbody>().isKinematic = false;
		}

		if(m_blend < newBlend)
		{
			m_blend = newBlend;
		}
		for(int i = 0; i < m_connectedParts.Length; ++i)
		{
			PhysicsBodyPart bodyPart = m_connectedParts[i].GetComponent<PhysicsBodyPart>();
			if(bodyPart.m_blend < m_blend * m_connectedPartsWeight[i])
			{
				bodyPart.AdjustBlendWeight(m_blend * m_connectedPartsWeight[i]);
			}
		}
	}

	/*
	 * Detect any rigidbodies hitting this part and transfer force onto the body part. Also update attached parts so force impacts them.
	 */
	void OnCollisionEnter(Collision collision)
	{
      
        if (collision.transform.root != transform.root &&
           collision.collider.GetComponent<Rigidbody>() != null && m_blend < 0.8f)
        {
			//calculate the impact ratios based on the relative masses of the colliding objects. Also, calculate the force of the impact.
            ContactPoint contact = collision.contacts[0];
            Vector3 force = collision.collider.GetComponent<Rigidbody>().velocity;
            float totalMass = GetComponent<Rigidbody>().mass + collision.collider.GetComponent<Rigidbody>().mass;
            float thisRatio = GetComponent<Rigidbody>().mass / totalMass;
            float otherRatio = collision.collider.GetComponent<Rigidbody>().mass / totalMass;

            //apply equal / opposite force to this
            Vector3 physGOForce = (transform.root.GetComponent<BodyPhysicsController>().m_GOVelocity + GetComponent<Rigidbody>().velocity) * GetComponent<Rigidbody>().mass;

            transform.root.GetComponent<BodyPhysicsController>().m_currOffBalance += (physGOForce * (otherRatio) * mBounceFactor).magnitude * m_balanceRatio;

			//apply force to this part
            GetComponent<Rigidbody>().AddForceAtPosition(-physGOForce * otherRatio, contact.point);

			//change the blend ratio between physics and animation rigs.
            float blendChange = (physGOForce.magnitude * otherRatio * 2.0f) / m_rigidness;
            m_blend += blendChange;
			ApplyForce(force * otherRatio, contact.point);

			//apply force to the other game object.
            Vector3 toObject = collision.collider.transform.position - contact.point;
            collision.collider.GetComponent<Rigidbody>().AddForceAtPosition(toObject.normalized * thisRatio + physGOForce * thisRatio, contact.point);
        }
	}

	//updates blending ratio and balance based on a force being applied to the part at the position passed in. This can be usefull for 
	//applying impacts from non physics objects (raycasts).
	public void ApplyForce(Vector3 force, Vector3 pos)
	{
		transform.GetComponent<Rigidbody>().AddForceAtPosition(force, pos);
		
		//calculate blend amount based on force
		float blendChange = force.magnitude / m_rigidness;
		
		
		m_blend += blendChange;
		//when we have passed the maximum allowable blend ratio, make the whole rig collapse
		if (m_blend > m_blendMax)
		{
			transform.root.GetComponent<BodyPhysicsController>().m_currOffBalance +=
				transform.root.GetComponent<BodyPhysicsController>().m_balance;
		}
		
		//update the connected parts
		for(int i = 0; i < m_connectedParts.Length; ++i)
		{
			m_connectedParts[i].GetComponent<PhysicsBodyPart>().AdjustBlendWeight(m_connectedPartsWeight[i] * m_blend);
		}
		
		//apply force to root go to track balance
		transform.root.GetComponent<BodyPhysicsController>().m_currOffBalance += force.magnitude * m_balanceRatio;
	}
}
