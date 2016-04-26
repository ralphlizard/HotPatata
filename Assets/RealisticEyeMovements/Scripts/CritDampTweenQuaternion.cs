// Critically damped smoothing tween.
// From Keijiro Takahashi's https://github.com/keijiro/SmoothingTest

using System.Runtime.InteropServices;
using UnityEngine;


namespace RealisticEyeMovements
{
    struct CritDampTweenQuaternion
    {
        [StructLayout(LayoutKind.Explicit)]
        struct QVUnion
        {
            [FieldOffset(0)] public Vector4 v;
            [FieldOffset(0)] public Quaternion q;
        }

        static Vector4 q2v(Quaternion q)
        {
            return new Vector4(q.x, q.y, q.z, q.w);
        }

        QVUnion _rotation;

        public Quaternion rotation {
            get { return _rotation.q; }
            set { _rotation.q = value; }
        }

        public Vector4 velocity;
        public float omega;
		readonly float maxSpeed;

        public CritDampTweenQuaternion(Quaternion rotation, float omega, float maxSpeed)
        {
            _rotation.v = Vector4.zero; // needed for suppressing warnings
            _rotation.q = rotation;
            velocity = Vector4.zero;
            this.omega = omega;
			this.maxSpeed = maxSpeed;
        }



        public void Step(Quaternion target)
        {
            var vtarget = q2v(target);
            // We can use either of vtarget/-vtarget. Use closer one.
            if (Vector4.Dot(_rotation.v, vtarget) < 0) vtarget = -vtarget;
            var dt = Time.deltaTime;
            var n1 = velocity - (_rotation.v - vtarget) * (omega * omega * dt);
            var n2 = 1 + omega * dt;
            velocity = n1 / (n2 * n2);
			float speed = velocity.magnitude;
			velocity = (Mathf.Min(speed, maxSpeed)/speed) * velocity;
            _rotation.v = (_rotation.v + velocity * dt).normalized;

        }

        public static implicit operator Quaternion(CritDampTweenQuaternion m)
        {
            return m.rotation;
        }
    }}
