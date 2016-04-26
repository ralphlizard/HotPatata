// Utils.cs
// Tore Knabe
// Copyright 2015 ioccam@ioccam.com

using UnityEngine;
using System.Collections.Generic;

namespace RealisticEyeMovements {


	public class Utils
	{

		static readonly Dictionary<string, GameObject> dummyObjects = new Dictionary<string, GameObject>();


		public static bool CanGetTransformFromPath(Transform startXform, string path)
		{
			if ( string.IsNullOrEmpty(path) )
				return true;
			
			if ( null != GetTransformFromPath(startXform, path) )
				return true;

			Debug.LogWarning(startXform.name + ": Cannot find path " + path, startXform.gameObject);

			return false;				
		}


		/*
		@t is the current time (or position) of the tween.
		@b is the beginning value of the property.
		@c is the change between the beginning and destination value of the property.
		@d is the total time of the tween.
		*/			
		public static float EaseSineIn( float t, float b, float c, float d )
		{
			return -c * Mathf.Cos( t / d * ( Mathf.PI / 2 ) ) + c + b;
		}
	
	
	
		public static GameObject FindChildInHierarchy(GameObject go, string name)
		{
			if (go.name == name)
				return go;
		
			foreach (Transform t in go.transform)
			{
				GameObject foundGO = FindChildInHierarchy(t.gameObject, name);
				if (foundGO != null)
					return foundGO;
			}
		
			return null;
		}
	
	
	
		public static Transform GetCommonAncestor( Transform xform1, Transform xform2 )
		{
			List<Transform> ancestors1 = new List<Transform> { xform1 };

			while ( xform1.parent != null )
			{
				xform1 = xform1.parent;
				ancestors1.Add( xform1 );
			}

			while ( xform2.parent != null && false == ancestors1.Contains( xform2 ))
				xform2 = xform2.parent;

			return ancestors1.Contains( xform2) ? xform2 : null;
		}



		public static string GetPathForTransform(Transform startXform, Transform targetXform)
		{
			List<string> path = new List<string>();
			Transform xform = targetXform;

			while ( xform != startXform && xform != null )
			{
				path.Add(xform.name);
				xform = xform.parent;
			}
			
			path.Reverse();
			
			string pathStr = string.Join("/", path.ToArray());

			return pathStr;
		}
		


		public static Transform GetTransformFromPath(Transform startXform, string path)
		{
			if ( string.IsNullOrEmpty(path) )
				return null;

			return startXform.Find(path);
		}



		// returns the angle in the range -180 to 180
		public static float NormalizedDegAngle ( float degrees )
		{
			int factor = (int) (degrees/360);
			degrees -= factor * 360;
			if ( degrees > 180 )
				return degrees - 360;
		
			if ( degrees < -180 )
				return degrees + 360;
		
			return degrees;
		}
	


		public static void PlaceDummyObject ( string name, Vector3 pos, float scale = 0.1f, Quaternion? rotation = null )
		{
			GameObject dummyObject;
		
			if ( dummyObjects.ContainsKey(name) )
				dummyObject = dummyObjects[ name ];
			else
			{
				dummyObject = GameObject.CreatePrimitive( PrimitiveType.Cube );
				dummyObject.transform.localScale = scale * Vector3.one;
				dummyObject.GetComponent<Renderer>().material = Resources.Load ("DummyObjectMaterial") as Material;
				Object.Destroy (dummyObject.GetComponent<Collider>());
				dummyObject.name = name;
			
				dummyObjects[ name ] = dummyObject;
			}
		
			dummyObject.transform.position = pos;
			dummyObject.transform.rotation = rotation ?? Quaternion.identity;
		}

	}

}
