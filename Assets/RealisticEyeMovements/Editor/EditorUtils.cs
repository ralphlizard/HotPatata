using System.Reflection;
using UnityEditor;



namespace RealisticEyeMovements {

	public class EditorUtils 
	{
		public static T GetBaseProperty<T>(SerializedProperty prop)
		{
			string[] separatedPaths = prop.propertyPath.Split('.');
			System.Object reflectionTarget = prop.serializedObject.targetObject;
			foreach (var path in separatedPaths)
			{
				FieldInfo fieldInfo = reflectionTarget.GetType().GetField(path);
				reflectionTarget = fieldInfo.GetValue(reflectionTarget);
			}
			return (T) reflectionTarget;
		}

	}

}