/* This script is based off of the Unity Ragdoll script. I've changed it slightly to add more bones and add the componenets necessary for the phys-anim blender.
 * I've tweaked the values of the bone setup data to achieve a more realistic result given my models / tests.
 * 
 * Note: the majority of this file is the same as the one provided freely by Unity.
 */


using UnityEngine;
using UnityEditor;
using System.Collections;
using System;

public class AnimRagdollBlender : ScriptableWizard 
{

	public Transform m_root;
	public Transform m_spine;
	public Transform m_chest;
	
	public Transform m_leftHips;
	public Transform m_leftKnee;
	public Transform m_leftFoot;
	
	public Transform m_rightHips;
	public Transform m_rightKnee;
	public Transform m_rightFoot;

	public Transform m_leftShoulder;
	public Transform m_leftArm;
	public Transform m_leftElbow;
    public Transform m_leftHand;

	public Transform m_rightShoulder;
	public Transform m_rightArm;
	public Transform m_rightElbow;
    public Transform m_rightHand;
	

	public Transform m_head;
	public Transform m_neck;
	
	
	public float m_totalMass = 20;
	public float m_strength = 0.0F;

    //adjust this to make the colliders thicker (for thicker characters)
    public float m_thickness = 1.0f; 

    //this should be true to ensure that the animation object keeps animating even though it is not being rendered.
    public bool m_alwaysAnimate = true;
	
	Vector3 right = Vector3.right;
	Vector3 up = Vector3.up;
	Vector3 forward = Vector3.forward;
	
	Vector3 worldRight = Vector3.right;
	Vector3 worldUp = Vector3.up;
	Vector3 worldForward = Vector3.forward;
	public bool m_flipForward = false;

    private Animator animator_;
	
	class BoneInfo
	{
		public string name;
		
		public Transform anchor;
		public CharacterJoint joint;
		public BoneInfo parent;
		
		public float minLimit;
		public float maxLimit;
		public float swingLimit;
        public float swing2Limit;
		
		public Vector3 axis;
		public Vector3 normalAxis;
		
		public float radiusScale;
		public Type colliderType;
		
		public ArrayList children = new ArrayList();
		public float density;
		public float summedMass;// The mass of this and all children bodies
	}
	
	ArrayList bones;
	BoneInfo m_rootBone;
	
	string CheckConsistency ()
	{
		PrepareBones();
		Hashtable map = new Hashtable ();
		foreach (BoneInfo bone in bones)
		{
			if (bone.anchor)
			{
				if (map[bone.anchor] != null)
				{
					BoneInfo oldBone = (BoneInfo)map[bone.anchor];
					return String.Format("{0} and {1} may not be assigned to the same bone.", bone.name, oldBone.name);
				}
				map[bone.anchor] = bone;
			}
		}
		
		foreach (BoneInfo bone in bones)
		{
			if (bone.anchor == null)
				return String.Format("{0} has not been assigned yet.\n", bone.name);
		}
		
		return "";
	}
	
	void OnDrawGizmos ()
	{
		if (m_root)
		{
			Gizmos.color = Color.red;   Gizmos.DrawRay (m_root.position, m_root.TransformDirection(right));
			Gizmos.color = Color.green;	Gizmos.DrawRay (m_root.position, m_root.TransformDirection(up));
			Gizmos.color = Color.blue;	Gizmos.DrawRay (m_root.position, m_root.TransformDirection(forward));
		}
	}
	
	[MenuItem ("GameObject/Create Other/AnimRagdollBlender")]
	static void CreateWizard ()
	{
		ScriptableWizard.DisplayWizard("Create Ragdoll", typeof (AnimRagdollBlender));
	}
	
	void DecomposeVector(out Vector3 normalCompo, out Vector3 tangentCompo, Vector3 outwardDir, Vector3 outwardNormal)
	{
		outwardNormal = outwardNormal.normalized;
		normalCompo = outwardNormal * Vector3.Dot(outwardDir, outwardNormal);
		tangentCompo = outwardDir - normalCompo;
	}
	
	void CalculateAxes ()
	{
		if (m_head != null && m_root != null)
			up = CalculateDirectionAxis(m_root.InverseTransformPoint(m_head.position));
		if (m_rightElbow != null && m_root != null)
		{
			Vector3 removed, temp;
			DecomposeVector(out temp, out removed, m_root.InverseTransformPoint(m_rightElbow.position), up);
			right = CalculateDirectionAxis(removed);
		}
		
		forward = Vector3.Cross(right, up);
		if (m_flipForward)
			forward = -forward;	
	}	
	
	void OnWizardUpdate ()
	{
        if (m_root != null)
        {
            AddBonesThroughAvatar();
        }

		errorString = CheckConsistency ();
		CalculateAxes();
		
		if (errorString.Length != 0)
		{
			helpString = "Set the m_root GO first to automatically map bones. \n" + 
                "Drag all bones from the hierarchy into their slots if necessary.\nMake sure your character is in T-Stand.\n";
		}
		else
		{
			helpString = "Make sure your character is in T-Stand.\nMake sure the blue axis faces in the same direction the chracter is looking.\nUse m_flipForward to flip the direction";
		}
		
		isValid = errorString.Length == 0;

        
	}

    void AddBonesThroughAvatar()
    {
		m_head = m_root.transform.root.GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.Head);
		m_neck = m_root.transform.root.GetComponentInChildren<Animator> ().GetBoneTransform (HumanBodyBones.Neck);

        m_leftHips = m_root.transform.root.GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.LeftUpperLeg);
		m_leftKnee = m_root.transform.root.GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.LeftLowerLeg);
		m_leftFoot = m_root.transform.root.GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.LeftFoot);
	
		m_rightHips = m_root.transform.root.GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.RightUpperLeg);
		m_rightKnee = m_root.transform.root.GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.RightLowerLeg);
		m_rightFoot = m_root.transform.root.GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.RightFoot);
	
		m_leftShoulder = m_root.transform.root.GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.LeftShoulder);
		m_leftArm = m_root.transform.root.GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.LeftUpperArm);
		m_leftElbow = m_root.transform.root.GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.LeftLowerArm);
		m_leftHand = m_root.transform.root.GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.LeftHand);
	
		m_rightShoulder = m_root.transform.root.GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.RightShoulder);
		m_rightArm = m_root.transform.root.GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.RightUpperArm);
		m_rightElbow = m_root.transform.root.GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.RightLowerArm);
		m_rightHand = m_root.transform.root.GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.RightHand);

		m_spine = m_root.transform.root.GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.Spine);
		m_chest = m_root.transform.root.GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.Chest);
    }
	
	void PrepareBones ()
	{
		if (m_root)
		{
			worldRight = m_root.TransformDirection(right);
			worldUp = m_root.TransformDirection(up);
			worldForward = m_root.TransformDirection(forward);
		}
		
		bones = new ArrayList();
		
		m_rootBone = new BoneInfo ();
		m_rootBone.name = "root";
		m_rootBone.anchor = m_root;
		m_rootBone.parent = null;
		m_rootBone.density = 4F;
		bones.Add (m_rootBone);
		
		AddMirroredJoint ("Hips", m_leftHips, m_rightHips, "root", worldRight, worldForward, -90, 90, 50, 10, typeof(CapsuleCollider), 0.25F, 1.5F);
		AddMirroredJoint ("Knee", m_leftKnee, m_rightKnee, "Hips", worldRight, worldUp, -180, 10, 10, 10, typeof(CapsuleCollider), 0.20F, 1.5F);
		
		AddJoint ("Middle Spine", m_spine, "root", worldRight, worldForward, -50, 10, 15, 15, null, 0.8f, 2.5F);
		AddJoint ("Chest", m_chest, "Middle Spine", worldRight, worldForward, -50, 10, 15, 15, null, 0.8f, 2.5F);

		AddMirroredJoint("Shoulder", m_leftShoulder, m_rightShoulder, "Chest", worldUp, worldRight, -10, 10, 5, 5, typeof(CapsuleCollider), 0.25F, 1.0F);
		AddMirroredJoint ("Arm", m_leftArm, m_rightArm, "Shoulder", worldForward, worldUp, -130, 10, 80, 20, typeof(CapsuleCollider), 0.25F, 1.0F);
		AddMirroredJoint ("Elbow", m_leftElbow, m_rightElbow, "Arm", worldUp, worldRight, -180, 10, 15, 20, typeof(CapsuleCollider), 0.15F, 1.0F);

        AddMirroredJoint("Hand", m_leftHand, m_rightHand, "Elbow", worldForward, worldUp, -70, 70, 90, 5, typeof(CapsuleCollider), 0.15F, 1.0F);

		AddJoint ("Neck", m_neck, "Chest", worldRight, worldForward, -1, 1, 1, 1, typeof(CapsuleCollider), 0.5f, 1.0F);
		AddJoint ("Head", m_head, "Neck", worldUp, -worldForward, -30, 30, 25, 25, null, 0.3f, 1.0F);
	}

    GameObject animationGO;
    void AddAnimationObject()
    {
        GameObject templateGO = m_root.transform.root.gameObject;
        animationGO = (GameObject)GameObject.Instantiate(templateGO, templateGO.transform.position, templateGO.transform.rotation);
        animationGO.name = "AnimationGO";
        animator_ = animationGO.GetComponent<Animator>();
        if (m_alwaysAnimate)
        {
            animator_.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        }

        //make the animationGO invisible
        SkinnedMeshRenderer[] animRenderers = animationGO.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer renderer in animRenderers)
        {
            renderer.enabled = false;
        }

        MeshRenderer[] meshRenderers = animationGO.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in meshRenderers)
        {
            renderer.enabled = false;
        }
    }

    void AddPhysicsController()
    {
        GameObject m_rootGO = m_root.transform.root.gameObject;

        //turn off the animator on the physics side of things
        Animator[] animators = m_rootGO.GetComponentsInChildren<Animator>();
        foreach (Animator animator in animators)
        {
            animator.enabled = false;
        }

        GameObject master = new GameObject();
        master.name = "PhysicsAnimm_root";
        master.transform.position = m_rootGO.transform.position;
        master.transform.rotation = m_rootGO.transform.rotation;
        BodyPhysicsController physController = master.AddComponent<BodyPhysicsController>();

        m_rootGO.transform.parent = master.transform;
        animationGO.transform.parent = master.transform;

        physController.m_animGO = animationGO.transform;
        physController.m_physicsGO = m_rootGO.transform;
        physController.m_hips = m_root.GetComponent<Rigidbody>();
    }
	
	void OnWizardCreate ()
	{
		
		Cleanup();
        AddAnimationObject();
		BuildCapsules();	
		AddBreastColliders();
		Addm_headCollider();
		
		BuildBodies ();
		BuildJoints ();
        BuildPhysics();
		CalculateMass();
//		CalculateSpringDampers();
        AddPhysicsController();
	}
	
	BoneInfo FindBone (string name)
	{
		foreach (BoneInfo bone in bones)
		{
			if (bone.name == name)
				return bone;
		}
		return null;
	}
	
	void AddMirroredJoint (string name, Transform leftAnchor, Transform rightAnchor, string parent, Vector3 worldTwistAxis, Vector3 worldSwingAxis, float minLimit, float maxLimit, float swingLimit, float swing2Limit, Type colliderType, float radiusScale, float density)
	{
		AddJoint ("Left " + name, leftAnchor, parent, worldTwistAxis, worldSwingAxis, minLimit, maxLimit, swingLimit, swing2Limit, colliderType, radiusScale, density);

		//mirror if armed joints, if knee use same rotation values
		if (name != "Hips" && name != "Knee") 
		{
			AddJoint ("Right " + name, rightAnchor, parent, worldTwistAxis, worldSwingAxis, -maxLimit, -minLimit, swingLimit, swing2Limit, colliderType, radiusScale, density);
		}
		else
		{
			AddJoint ("Right " + name, rightAnchor, parent, worldTwistAxis, worldSwingAxis, minLimit, maxLimit, swingLimit, swing2Limit, colliderType, radiusScale, density);
		}
	}
	
	
	void AddJoint (string name, Transform anchor, string parent, Vector3 worldTwistAxis, Vector3 worldSwingAxis, float minLimit, float maxLimit, float swingLimit, float swing2Limit, Type colliderType, float radiusScale, float density)
	{
		BoneInfo bone = new BoneInfo();
		bone.name = name;
		bone.anchor = anchor;
		bone.axis = worldTwistAxis;
		bone.normalAxis = worldSwingAxis;
		bone.minLimit = minLimit;
		bone.maxLimit = maxLimit;
		bone.swingLimit = swingLimit;
        bone.swing2Limit = swing2Limit;
		bone.density = density;
		bone.colliderType = colliderType;
		bone.radiusScale = radiusScale * m_thickness;
		
		if (FindBone (parent) != null)
			bone.parent = FindBone (parent);
		else if (name.StartsWith ("Left"))
			bone.parent = FindBone ("Left " + parent);
		else if (name.StartsWith ("Right"))
			bone.parent = FindBone ("Right "+ parent);
		
		
		bone.parent.children.Add(bone);
		bones.Add (bone);
	}
	
	void BuildCapsules ()
	{
		foreach (BoneInfo bone in bones)
		{
			if (bone.colliderType != typeof (CapsuleCollider))
				continue;
			
			int direction;
			float distance;
			if (bone.children.Count == 1)
			{
				BoneInfo childBone = (BoneInfo)bone.children[0];
				Vector3 endPoint = childBone.anchor.position;
				CalculateDirection (bone.anchor.InverseTransformPoint(endPoint), out direction, out distance);
			}
			else
			{
				Vector3 endPoint = (bone.anchor.position - bone.parent.anchor.position) + bone.anchor.position;
				CalculateDirection (bone.anchor.InverseTransformPoint(endPoint), out direction, out distance);
				
				if (bone.anchor.GetComponentsInChildren(typeof(Transform)).Length > 1)
				{
					Bounds bounds = new Bounds();
					foreach (Transform child in bone.anchor.GetComponentsInChildren(typeof(Transform)))
					{
						bounds.Encapsulate(bone.anchor.InverseTransformPoint(child.position));
					}
					
					if (distance > 0)
						distance = bounds.max[direction];
					else
						distance = bounds.min[direction];
				}
			}
			
			CapsuleCollider collider = (CapsuleCollider)bone.anchor.gameObject.AddComponent <CapsuleCollider>();
			collider.direction = direction;
			
			Vector3 center = Vector3.zero;
			center[direction] = distance * 0.5F;
			collider.center = center;
			collider.height = Mathf.Abs (distance);
			collider.radius = Mathf.Abs (distance * bone.radiusScale);
		}
	}
	
	void Cleanup ()
	{
		foreach (BoneInfo bone in bones)
		{
			if (!bone.anchor)
				continue;
			
			Component[] joints = bone.anchor.GetComponentsInChildren(typeof(Joint));
			foreach (Joint joint in joints)
				DestroyImmediate(joint);
			
			Component[] bodies = bone.anchor.GetComponentsInChildren(typeof(Rigidbody));
			foreach (Rigidbody body in bodies)
				DestroyImmediate(body);
			
			Component[] colliders = bone.anchor.GetComponentsInChildren(typeof(Collider));
			foreach (Collider collider in colliders)
				DestroyImmediate(collider);
		}
	}
	
	void BuildBodies ()
	{
		foreach (BoneInfo bone in bones)
		{
			bone.anchor.gameObject.AddComponent<Rigidbody>();
			//			bone.anchor.rigidbody.SetDensity (bone.density);
			bone.anchor.GetComponent<Rigidbody>().mass = bone.density;
		}
	}
	
	void BuildJoints ()
	{
		foreach (BoneInfo bone in bones)
		{
			if (bone.parent == null)
				continue;
			
			CharacterJoint joint = (CharacterJoint)bone.anchor.gameObject.AddComponent <CharacterJoint>();
			bone.joint = joint;
			
			// Setup connection and axis
			joint.axis = CalculateDirectionAxis (bone.anchor.InverseTransformDirection(bone.axis));
			joint.swingAxis = CalculateDirectionAxis (bone.anchor.InverseTransformDirection(bone.normalAxis));
			joint.anchor = Vector3.zero;
			joint.connectedBody = bone.parent.anchor.GetComponent<Rigidbody>();

			// Setup limits			
			SoftJointLimit limit = new SoftJointLimit ();
			
			limit.limit = bone.minLimit;
			joint.lowTwistLimit = limit;
			
			limit.limit = bone.maxLimit;
			joint.highTwistLimit = limit;
			
			limit.limit = bone.swingLimit;
			joint.swing1Limit = limit;
			
			limit.limit = bone.swing2Limit;
			joint.swing2Limit = limit;


			//use the new joint projection functionality to avoid twitching. The values are just best guess initial values
			joint.enableProjection = true;
			joint.projectionDistance = 0.02f;
			joint.projectionAngle = 10;
		}
	}

    void BuildPhysics()
    {
        foreach (BoneInfo bone in bones)
        {
            PhysicsBodyPart part = (PhysicsBodyPart)bone.anchor.gameObject.AddComponent<PhysicsBodyPart>();
            part.m_animBone = GetAnimBone(bone.name);
            part.m_animator =  animator_;

            float[] connectedWeights;
            float balanceRatio;
            float maxBlend = part.m_blendMax;
            part.m_connectedParts = SetupPhysics(bone.name, out connectedWeights, out balanceRatio, ref maxBlend );
            part.m_connectedPartsWeight = connectedWeights;
            part.m_balanceRatio = balanceRatio;
            part.m_blendMax = maxBlend;
        }
    }
	
	void CalculateMassRecurse (BoneInfo bone)
	{
		float mass = bone.anchor.GetComponent<Rigidbody>().mass;
		foreach (BoneInfo child in bone.children)
		{
			CalculateMassRecurse (child);
			mass += child.summedMass;
		}
		bone.summedMass = mass;
	}
	
	void CalculateMass ()
	{
		// Calculate allChildMass by summing all bodies
		CalculateMassRecurse (m_rootBone);
		
		// Rescale the mass so that the whole character weights m_totalMass
		float massScale = m_totalMass / m_rootBone.summedMass;
		foreach (BoneInfo bone in bones)
			bone.anchor.GetComponent<Rigidbody>().mass *= massScale;
		
		// Recalculate allChildMass by summing all bodies
		CalculateMassRecurse(m_rootBone);
	}
	
	///@todo: This should take into account the inertia tensor.
	JointDrive CalculateSpringDamper (float frequency, float damping, float mass)
	{
		JointDrive drive = new JointDrive();
		drive.positionSpring = 9 * frequency * frequency * mass;
		drive.positionDamper = 4.5F * frequency * damping * mass;
		return drive;
	}

	
	static void CalculateDirection (Vector3 point, out int direction, out float distance)
	{
		// Calculate longest axis
		direction = 0;
		if (Mathf.Abs(point[1]) > Mathf.Abs(point[0]))
			direction = 1;
		if (Mathf.Abs(point[2]) >Mathf.Abs(point[direction]))
			direction = 2;
		
		distance = point[direction];
	}
	
	static Vector3 CalculateDirectionAxis (Vector3 point)
	{
		int direction = 0;
		float distance;
		CalculateDirection (point, out direction, out distance);
		Vector3 axis = Vector3.zero;
		if (distance > 0)
			axis[direction] = 1.0F;
		else
			axis[direction] = -1.0F;
		return axis;
	}
	
	static int SmallestComponent (Vector3 point)
	{
		int direction = 0;
		if (Mathf.Abs(point[1]) < Mathf.Abs(point[0]))
			direction = 1;
		if (Mathf.Abs(point[2]) < Mathf.Abs(point[direction]))
			direction = 2;
		return direction;
	}
	
	static int LargestComponent (Vector3 point)
	{
		int direction = 0;
		if (Mathf.Abs(point[1]) > Mathf.Abs(point[0]))
			direction = 1;
		if (Mathf.Abs(point[2]) > Mathf.Abs(point[direction]))
			direction = 2;
		return direction;
	}
	
	static int SecondLargestComponent (Vector3 point)
	{
		int smallest = SmallestComponent (point);
		int largest = LargestComponent (point);
		if (smallest < largest)
		{
			int temp = largest;
			largest = smallest;
			smallest = temp;
		}
		
		if (smallest == 0 && largest == 1)
			return 2;
		else if (smallest == 0 && largest == 2)
			return 1;
		else
			return 0;
	}
	
	Bounds Clip (Bounds bounds, Transform relativeTo, Transform clipTransform, bool below)
	{
		int axis = LargestComponent(bounds.size);
		
		if (Vector3.Dot (worldUp, relativeTo.TransformPoint(bounds.max)) > Vector3.Dot (worldUp, relativeTo.TransformPoint(bounds.min)) == below)
		{
			Vector3 min = bounds.min;
			min[axis] = relativeTo.InverseTransformPoint (clipTransform.position)[axis];
			bounds.min = min;
		}
		else
		{
			Vector3 max = bounds.max;
			max[axis] = relativeTo.InverseTransformPoint (clipTransform.position)[axis];
			bounds.max = max;
		}
		return bounds;
	}
	
	Bounds GetBreastBounds (Transform relativeTo)
	{
		// m_root bounds
		Bounds bounds = new Bounds ();
		bounds.Encapsulate (relativeTo.InverseTransformPoint (m_leftHips.position));
		bounds.Encapsulate (relativeTo.InverseTransformPoint (m_rightHips.position));
		bounds.Encapsulate (relativeTo.InverseTransformPoint (m_leftArm.position));
		bounds.Encapsulate (relativeTo.InverseTransformPoint (m_rightArm.position));
		Vector3 size = bounds.size;
		size[SmallestComponent (bounds.size)] = size[LargestComponent (bounds.size)] / 2.0F;
		bounds.size = size;
		return bounds;		
	}
	
	void AddBreastColliders ()
	{
		// Middle spine and m_root
		if (m_spine != null && m_root != null && m_chest != null)
		{
			Bounds bounds;
			BoxCollider box;
			
			// Middle spine bounds
			bounds = Clip (GetBreastBounds (m_root), m_root, m_spine, false);
			box = (BoxCollider)m_root.gameObject.AddComponent<BoxCollider>();
			box.center = bounds.center;
			box.size = bounds.size;
			
			bounds = Clip (GetBreastBounds (m_spine), m_spine, m_spine, true);
			box = (BoxCollider)m_spine.gameObject.AddComponent<BoxCollider>();
			box.center = bounds.center;
			box.size = bounds.size * m_thickness;

			bounds = Clip (GetBreastBounds (m_chest), m_chest, m_chest, true);
			box = (BoxCollider)m_chest.gameObject.AddComponent<BoxCollider>();
			box.center = bounds.center;
			box.size = bounds.size * m_thickness;
		}
		else if(m_spine != null && m_root != null)
		{
			Bounds bounds;
			BoxCollider box;
			
			// Middle spine bounds
			bounds = Clip (GetBreastBounds (m_root), m_root, m_spine, false);
			box = (BoxCollider)m_root.gameObject.AddComponent<BoxCollider>();
			box.center = bounds.center;
			box.size = bounds.size;
			
			bounds = Clip (GetBreastBounds (m_spine), m_spine, m_spine, true);
			box = (BoxCollider)m_spine.gameObject.AddComponent<BoxCollider>();
			box.center = bounds.center;
			box.size = bounds.size * m_thickness;
		}
		// Only m_root
		else
		{
			Bounds bounds = new Bounds ();
			bounds.Encapsulate (m_root.InverseTransformPoint (m_leftHips.position));
			bounds.Encapsulate (m_root.InverseTransformPoint (m_rightHips.position));
			bounds.Encapsulate (m_root.InverseTransformPoint (m_leftArm.position));
			bounds.Encapsulate (m_root.InverseTransformPoint (m_rightArm.position));
			
			Vector3 size = bounds.size;
			size[SmallestComponent (bounds.size)] = size[LargestComponent (bounds.size)] / 2.0F;
			
			BoxCollider box = (BoxCollider)m_root.gameObject.AddComponent<BoxCollider>();
			box.center = bounds.center;
			box.size = size;
		}
	}
	
	void Addm_headCollider ()
	{
		if (m_head.GetComponent<Collider>())
			Destroy (m_head.GetComponent<Collider>());
		
		float radius = Vector3.Distance(m_leftArm.transform.position, m_rightArm.transform.position);
		radius /= 2.5f;
        radius *= m_thickness;
		SphereCollider sphere = (SphereCollider)m_head.gameObject.AddComponent <SphereCollider>();
		sphere.radius = radius;
		Vector3 center = Vector3.zero;
		
		int direction;
		float distance;
		CalculateDirection (m_head.InverseTransformPoint(m_root.position), out direction, out distance);
		if (distance > 0)
			center[direction] = -radius;
		else
			center[direction] = radius;
		sphere.center = center;
	}


    HumanBodyBones GetAnimBone(string physBoneName)
    {
        switch (physBoneName)
        {
            case "root":
                return HumanBodyBones.Hips;

            case "Left Hips":
                return HumanBodyBones.LeftUpperLeg;
            
            case "Right Hips":
                return HumanBodyBones.RightUpperLeg;

            case "Left Knee":
                return HumanBodyBones.LeftLowerLeg;

            case "Right Knee":
                return HumanBodyBones.RightLowerLeg;

            case "Middle Spine":
                return HumanBodyBones.Spine;

			case "Chest":
				return HumanBodyBones.Chest;

			case "Left Shoulder":
				return HumanBodyBones.LeftShoulder;

            case "Left Arm":
                return HumanBodyBones.LeftUpperArm;

            case "Left Hand":
                return HumanBodyBones.LeftHand;

			case "Right Shoulder":
				return HumanBodyBones.RightShoulder;

            case "Right Arm":
                return HumanBodyBones.RightUpperArm;

            case "Left Elbow":
                return HumanBodyBones.LeftLowerArm;

            case "Right Elbow":
                return HumanBodyBones.RightLowerArm;

            case "Right Hand":
                return HumanBodyBones.RightHand;

            case "Head":
                return HumanBodyBones.Head;

			case "Neck":
				return HumanBodyBones.Neck;

            default:
                Debug.LogError("Incorrect Bone Name Requested");
                return HumanBodyBones.LastBone;
        }
    }



	/* This function helps set up a physics object for the phys-anim blender. It returns the connected children as an array of transforms. It creates the corresponding connected blend weight array. 
	 * It also returns the balance ratio for the part. It does all this based on the name of the bone passed in. This name should specify the corresponding bone in the mechanim avatar the part is matched to. 
	 * The way the names are related to actual transforms depend on the intitial setup via the ragdoll window.
	 * */
    Transform[] SetupPhysics(string physBoneName, out float[] connectedWeights, out float balanceRatio, ref float maxBlend)
    {
        Transform[] retval;
        switch (physBoneName)
        {
            case "root":
                retval = new Transform[3];
                retval[0] = m_leftHips;
                retval[1] = m_rightHips;
                retval[2] = m_spine;
                connectedWeights = new float[3];
                connectedWeights[0] = 0.1f;
                connectedWeights[1] = 0.1f;
                connectedWeights[2] = 0.7f;
                balanceRatio = 0.1f;
                return retval;

            case "Left Hips":
                retval = new Transform[2];
                retval[0] = m_leftKnee;
                retval[1] = m_root;
                connectedWeights = new float[2];
                connectedWeights[0] = 1.0f;
                connectedWeights[1] = 0.1f;
                balanceRatio = 1.0f;
                maxBlend = 1.0f;
                return retval;

            case "Right Hips":
                retval = new Transform[2];
                retval[0] = m_rightKnee;
                retval[1] = m_root;
                connectedWeights = new float[2];
                connectedWeights[0] = 1.0f;
                connectedWeights[1] = 0.1f;
                balanceRatio = 1.0f;
                maxBlend = 1.0f;
                return retval;

            case "Left Knee":
                retval = new Transform[1];
                retval[0] = m_leftHips;
                connectedWeights = new float[1];
                connectedWeights[0] = 0.7f;
                balanceRatio = 1.0f;
                maxBlend = 1.0f;
                return retval;

            case "Right Knee":
                retval = new Transform[1];
                retval[0] = m_rightHips;
                connectedWeights = new float[1];
                connectedWeights[0] = 0.7f;
                balanceRatio = 1.0f;
                maxBlend = 1.0f;
                return retval;

            case "Middle Spine":
                retval = new Transform[2];
                retval[0] = m_chest;
                retval[1] = m_root;
                connectedWeights = new float[2];
                connectedWeights[0] = 0.7f;
                connectedWeights[1] = 0.25f;
                balanceRatio = 0.75f;
                return retval;

			case "Chest":
				retval = new Transform[4];
				retval[0] = m_spine;
				retval[1] = m_neck;
				retval[2] = m_rightShoulder;
				retval[3] = m_leftShoulder;
				connectedWeights = new float[4];
				connectedWeights[0] = 0.7f;
				connectedWeights[1] = 1.0f;
				connectedWeights[2] = 1.0f;
				connectedWeights[3] = 1.0f;
				balanceRatio = 0.75f;
				return retval; 

			case "Left Shoulder":
				retval = new Transform[2];
				retval[0] = m_leftArm;
				retval[1] = m_chest;
				connectedWeights = new float[2];
				connectedWeights[0] = 1.0f;
				connectedWeights[1] = 0.5f;
				balanceRatio = 0.2f;
				return retval;

            case "Left Arm":
                retval = new Transform[2];
                retval[0] = m_leftElbow;
                retval[1] = m_leftShoulder;
                connectedWeights = new float[2];
                connectedWeights[0] = 1.0f;
                connectedWeights[1] = 0.5f;
                balanceRatio = 0.2f;
                return retval;

            case "Left Elbow":
                retval = new Transform[2];
                retval[0] = m_leftArm;
                retval[1] = m_leftHand;
                connectedWeights = new float[2];
                connectedWeights[0] = 0.5f;
                connectedWeights[1] = 1.0f;
                balanceRatio = 0.1f;
                return retval;

            case "Left Hand":
                retval = new Transform[1];
                retval[0] = m_leftElbow;
                connectedWeights = new float[1];
                connectedWeights[0] = 0.4f;
                balanceRatio = 0.1f;
                return retval;

			case "Right Shoulder":
				retval = new Transform[2];
				retval[0] = m_rightArm;
				retval[1] = m_chest;
				connectedWeights = new float[2];
				connectedWeights[0] = 1.0f;
				connectedWeights[1] = 0.5f;
				balanceRatio = 0.2f;
				return retval;

			case "Right Arm":
                retval = new Transform[2];
                retval[0] = m_rightElbow;
                retval[1] = m_rightShoulder;
                connectedWeights = new float[2];
                connectedWeights[0] = 1.0f;
                connectedWeights[1] = 0.5f;
                balanceRatio = 0.2f;
                return retval;

            case "Right Elbow":
                retval = new Transform[2];
                retval[0] = m_rightArm;
                retval[1] = m_rightHand;
                connectedWeights = new float[2];
                connectedWeights[0] = 0.5f;
                connectedWeights[1] = 1.0f;
                balanceRatio = 0.1f;
                return retval;

            case "Right Hand":
                retval = new Transform[1];
                retval[0] = m_rightElbow;
                connectedWeights = new float[1];
                connectedWeights[0] = 0.4f;
                balanceRatio = 0.1f;
                return retval;

			case "Neck":
				retval = new Transform[2];
				retval[0] = m_chest;
				retval[1] = m_head;
				connectedWeights = new float[2];
				connectedWeights[0] = 0.4f;
				connectedWeights[1] = 1.0f;
				balanceRatio = 0.5f;
				return retval;

            case "Head":
                retval = new Transform[1];
                retval[0] = m_neck;
                connectedWeights = new float[1];
                connectedWeights[0] = 0.4f;
                balanceRatio = 2.0f;
                return retval;

            default:
                Debug.LogError("Incorrect Bone Name Requested");
                retval = new Transform[0];
                connectedWeights = new float[0];
                balanceRatio = 0.0f;
                return retval;
        }
    }

}
