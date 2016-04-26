using UnityEngine;
 
namespace RealisticEyeMovements {

	[System.Serializable]
	public struct SerializableQuaternion
	{
		public float x;
		public float y;
		public float z;
		public float w;
     
		public SerializableQuaternion(float rX, float rY, float rZ, float rW)
		{
			x = rX;
			y = rY;
			z = rZ;
			w = rW;
		}
     
		public override string ToString()
		{
			return string.Format("[{0}, {1}, {2}, {3}]", x, y, z, w);
		}
     
		public static implicit operator Quaternion(SerializableQuaternion rValue)
		{
			return new Quaternion(rValue.x, rValue.y, rValue.z, rValue.w);
		}
     
		public static implicit operator SerializableQuaternion(Quaternion rValue)
		{
			return new SerializableQuaternion(rValue.x, rValue.y, rValue.z, rValue.w);
		}
	}
}