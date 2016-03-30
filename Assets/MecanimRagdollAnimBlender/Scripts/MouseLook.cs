//#define RAYCAST
#define BALL_PROJECTILE
using UnityEngine;
using System.Collections;

/// MouseLook rotates the transform based on the mouse delta.
/// Minimum and Maximum values can be used to constrain the possible rotation

/// To make an FPS style character:
/// - Create a capsule.
/// - Add the MouseLook script to the capsule.
///   -> Set the mouse look to use LookX. (You want to only turn character but not tilt it)
/// - Add FPSInputController script to the capsule
///   -> A CharacterMotor and a CharacterController component will be automatically added.

/// - Create a camera. Make the camera a child of the capsule. Reset it's transform.
/// - Add a MouseLook script to the camera.
///   -> Set the mouse look to use LookY. (You want the camera to tilt up and down like a head. The character already turns.)
[AddComponentMenu("Camera-Control/Mouse Look")]
public class MouseLook : MonoBehaviour {

	public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
	public RotationAxes axes = RotationAxes.MouseXAndY;
	public float sensitivityX = 15F;
	public float sensitivityY = 15F;

	public float minimumX = -360F;
	public float maximumX = 360F;

	public float minimumY = -60F;
	public float maximumY = 60F;

	float rotationY = 0F;

	public float SHOT_COOLDOWN = 0.1F;
	private float shotTimer_ = 0.0f;

    private float rayTime_ = 0.0f;
    private float rayTotalTime_ = 0.2f; // how long a ray lasts for

	public BodyPhysicsController mPhysController;

	void Update ()
	{
		if(Input.GetMouseButton(1) &&
		   mPhysController.CurrState == BodyPhysicsController.State.DEFAULT)
		{
			if (axes == RotationAxes.MouseXAndY)
			{
				float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivityX;
				
				rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
				rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);
				
				transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
			}
			else if (axes == RotationAxes.MouseX)
			{
				transform.Rotate(0, Input.GetAxis("Mouse X") * sensitivityX, 0);
			}
			else
			{
				rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
				rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);
				
				transform.localEulerAngles = new Vector3(-rotationY, transform.localEulerAngles.y, 0);
			}
		}


	}

    public bool mRaycast = false;
	public GameObject mBulletTemplate;
	public float BULLET_SPEED = 100.0f;
	public float BULLET_FORWARD_OFFSET = 1.0f;
	public float BULLET_UP_OFFSET = 1.0f;
	void LateUpdate()
	{
		if(shotTimer_ > 0.0f)
		{
			shotTimer_ -= Time.deltaTime;
		}

        if (rayTime_ > 0.0f)
        {
            rayTime_ -= Time.deltaTime;
        }
        else
        {
            LineRenderer line = GetComponent<LineRenderer>();
            line.GetComponent<Renderer>().enabled = false;
        }

		if(Input.GetMouseButton (0))
		{
            if (mRaycast)
            {
                LineRenderer line = GetComponent<LineRenderer>();
                line.GetComponent<Renderer>().enabled = true;
                line.SetPosition(0, transform.position);// + new Vector3(0.0f, 0.5f, 0.0f));
                RaycastHit info;
                Physics.Raycast(new Ray(Camera.main.transform.position, Camera.main.transform.forward), out info);
                if (info.collider)
                {
                    line.SetPosition(1, info.point);
                    if (info.collider.gameObject.GetComponent<PhysicsBodyPart>() != null)
                        info.collider.gameObject.GetComponent<PhysicsBodyPart>().ApplyForce(Camera.main.transform.forward * 10.0f,
                                                                                        info.point);
                }
                else
                {
                    line.SetPosition(1, transform.position + Camera.main.transform.forward * 1000.0f);
                }
                rayTime_ = rayTotalTime_;
            }
            else
            {
                if (shotTimer_ <= 0)
                {
                    shotTimer_ += SHOT_COOLDOWN;
                    Object temp = GameObject.Instantiate(mBulletTemplate, transform.position + transform.forward * BULLET_FORWARD_OFFSET + transform.up * BULLET_UP_OFFSET,
                                                        transform.rotation);
                    ((GameObject)temp).GetComponent<Rigidbody>().AddForce((transform.forward + transform.up * 0.5f).normalized * BULLET_SPEED);
                }
            }
		}

        if (Input.GetKeyDown(KeyCode.E))
        {
            mRaycast = !mRaycast;
        }
	}

    float reticalWidth = 10.0f;
    float reticalHeight = 10.0f;
    public Texture2D reticalTexture;
    void OnGUI()
    {
        GUI.Label(new Rect(0, 0, 300, 100), "Press e to toggle raycast / projectiles"); 

        GUI.Box(new Rect(Screen.width / 2.0f - reticalWidth / 2.0f, Screen.height / 2.0f - reticalHeight / 2.0f, reticalWidth, reticalHeight), 
            reticalTexture);
    }
	
	void Start ()
	{
		// Make the rigid body not change rotation
		if (GetComponent<Rigidbody>())
			GetComponent<Rigidbody>().freezeRotation = true;
	}
}