 using UnityEngine;

namespace RealisticEyeMovements {

	[System.Serializable]
	public struct SerializableVector3
	{
		public float x;
		public float y;
		public float z;
     
		public SerializableVector3(float rX, float rY, float rZ)
		{
			x = rX;
			y = rY;
			z = rZ;
		}
     
		public override string ToString()
		{
			return string.Format("[{0}, {1}, {2}]", x, y, z);
		}
     
		public static implicit operator Vector3(SerializableVector3 rValue)
		{
			return new Vector3(rValue.x, rValue.y, rValue.z);
		}
     
		public static implicit operator SerializableVector3(Vector3 rValue)
		{
			return new SerializableVector3(rValue.x, rValue.y, rValue.z);
		}
	}
}