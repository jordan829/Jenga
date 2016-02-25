using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using UnityEngine;

namespace zSpace.Common
{
	public static class Utility
	{
		public delegate GameObject ObjectResolver (GameObject collidedObject);
		
		//
		// Static Fields
		//
		public static float Epsilon;
		
		//
		// Static Methods
		//
		public static float ClampAngle (float angle, float min, float max)
		{
			angle = Utility.NormalizeAngle (angle);
			min = Utility.NormalizeAngle (min);
			max = Utility.NormalizeAngle (max);
			if (max < min)
			{
				min -= 360f;
				if (angle > 180f)
				{
					angle -= 360f;
				}
			}
			float angle2 = Mathf.Clamp (angle, min, max);
			return Utility.NormalizeAngle (angle2);
		}
		
		public static Vector3 ComputeNearestVertexToPoint (GameObject go, Vector3 sourcePoint)
		{
			Vector3 result;
			if (go == null)
			{
				result = sourcePoint;
			}
			else
			{
				float num = float.PositiveInfinity;
				Vector3 vector = sourcePoint;
				MeshFilter[] componentsInChildren = go.GetComponentsInChildren<MeshFilter> ();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					MeshFilter meshFilter = componentsInChildren [i];
					Vector3 vector2 = meshFilter.transform.InverseTransformPoint (sourcePoint);
					Mesh mesh = meshFilter.mesh;
					Vector3[] vertices = mesh.vertices;
					for (int j = 0; j < vertices.Length; j++)
					{
						Vector3 vector3 = vertices [j];
						float sqrMagnitude = (vector2 - vector3).sqrMagnitude;
						if (sqrMagnitude < num)
						{
							num = sqrMagnitude;
							vector = meshFilter.transform.TransformPoint (vector3);
						}
					}
				}
				result = vector;
			}
			return result;
		}
		
		public static int ComputeNextPowerOfTwo (int input)
		{
			input--;
			for (int i = 0; i < 5; i++)
			{
				input |= input >> (1 << i);
			}
			input++;
			return input;
		}
		
		public static Quaternion[] ComputeSplineRotations (Vector3[] positions, Vector3 up)
		{
			Quaternion[] array = new Quaternion[positions.Length];
			Quaternion[] result;
			if (positions.Length == 0)
			{
				result = array;
			}
			else
			{
				if (array.Length == 1)
				{
					array [0] = Quaternion.identity;
				}
				else
				{
					array [0] = Quaternion.LookRotation (positions [1] - positions [0], up);
					array [array.Length - 1] = Quaternion.LookRotation (positions [array.Length - 1] - positions [array.Length - 2], up);
					for (int i = 0; i < array.Length - 2; i++)
					{
						Vector3 vector = positions [i + 2] - positions [i + 1];
						Vector3 vector2 = array [i] * Vector3.up;
						Vector3 vector3 = Vector3.Cross (vector2, vector);
						Vector3 vector4 = Vector3.Cross (vector, vector3);
						array [i + 1] = Quaternion.LookRotation (vector, vector4);
					}
				}
				result = array;
			}
			return result;
		}
		
		public static IEnumerator Delay (float waitTime, Action action)
		{
			for (float num = 0f; num < waitTime; num += Time.deltaTime)
			{
				yield return null;
			}
			action ();
			yield break;
		}
		
		public static Mesh extrudePolygon (Vector3[] polygon, float height)
		{
			Mesh mesh = new Mesh ();
			Vector3 normalized = Vector3.Cross (polygon [1] - polygon [0], polygon [2] - polygon [1]).normalized;
			Vector3 vector = height * normalized;
			List<Vector3> list = new List<Vector3> ();
			List<int> list2 = new List<int> ();
			for (int i = 0; i < polygon.Length; i++)
			{
				Vector3 item = polygon [i] + vector;
				list.Add (item);
				list.Add (polygon [i]);
			}
			list2.Add (0);
			list2.Add (3);
			list2.Add (2);
			list2.Add (0);
			list2.Add (1);
			list2.Add (3);
			list2.Add (list.Count - 2);
			list2.Add (1);
			list2.Add (0);
			list2.Add (list.Count - 2);
			list2.Add (list.Count - 1);
			list2.Add (1);
			for (int i = 1; i < polygon.Length - 1; i++)
			{
				list2.Add (0);
				list2.Add (2 * i);
				list2.Add (2 * i + 2);
				list2.Add (2 * i + 3);
				list2.Add (2 * i + 1);
				list2.Add (1);
				list2.Add (2 * i);
				list2.Add (2 * i + 3);
				list2.Add (2 * i + 2);
				list2.Add (2 * i);
				list2.Add (2 * i + 1);
				list2.Add (2 * i + 3);
			}
			mesh.vertices = list.ToArray ();
			mesh.triangles = list2.ToArray ();
			mesh.RecalculateNormals ();
			mesh.RecalculateBounds ();
			mesh.Optimize ();
			return mesh;
		}
		
		public static GameObject FindRigidBodyResolver (GameObject collidedObject)
		{
			Transform transform = collidedObject.transform;
			while (transform != null && transform.GetComponent<Rigidbody>() == null && transform.parent != null)
			{
				transform = transform.parent;
			}
			GameObject result;
			if (transform != null && transform.GetComponent<Rigidbody>() != null)
			{
				result = transform.gameObject;
			}
			else
			{
				result = null;
			}
			return result;
		}
		
		public static Type FindType (string typeName)
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies ();
			Type result;
			for (int i = 0; i < assemblies.Length; i++)
			{
				Assembly assembly = assemblies [i];
				try
				{
					Type type = assembly.GetType (typeName, false, true);
					if (type != null)
					{
						result = type;
						return result;
					}
				}
				catch (Exception arg)
				{
					Debug.LogWarning ("Ignoring exception: " + arg);
				}
			}
			result = null;
			return result;
		}
		
		public static bool IsEmpty (Bounds bounds)
		{
			return bounds.extents.x == 0f || bounds.extents.y == 0f || bounds.extents.z == 0f;
		}
		
		public static float NormalizeAngle (float angle)
		{
			float num = angle % 360f;
			if (num < 0f)
			{
				num += 360f;
			}
			return num;
		}
		
		public static GameObject ParentResolver (GameObject collidedObject)
		{
			return collidedObject.transform.parent ? collidedObject.transform.parent.gameObject : null;
		}
		
		public static void RecursivelyScale (GameObject go, Vector3 scale)
		{
			if (go.GetComponent<Rigidbody>() != null || go.GetComponent<Collider>() != null || go.GetComponent<Renderer>() != null)
			{
				go.transform.localScale = Vector3.Scale (scale, go.transform.localScale);
			}
			else
			{
				foreach (Transform transform in go.transform)
				{
					transform.localPosition = Vector3.Scale (scale, transform.localPosition);
					Utility.RecursivelyScale (transform.gameObject, scale);
				}
			}
		}
		
		public static void RenderMeshes (GameObject go, Material mat)
		{
			if (!(go == null) && !(mat == null))
			{
				MeshFilter[] componentsInChildren = go.GetComponentsInChildren<MeshFilter> ();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					MeshFilter meshFilter = componentsInChildren [i];
					if (meshFilter.GetComponent<Renderer>() != null && meshFilter.GetComponent<Renderer>().enabled)
					{
						for (int j = 0; j < mat.passCount; j++)
						{
							mat.SetPass (j);
							Graphics.DrawMeshNow (meshFilter.sharedMesh, meshFilter.transform.localToWorldMatrix);
						}
					}
				}
			}
		}

		public static IEnumerator RunInBackground (Action action)
		{
			bool isDone = false;
			WaitCallback callBack = delegate (object ctx)
			{
				action ();
				isDone = true;
			};
			ThreadPool.QueueUserWorkItem (callBack);
			while (!isDone)
			{
				yield return null;
			}
			yield break;
		}
		
		//
		// Nested Types
		//
		public class GraphHeightResolver
		{
			public int height = 0;
			public GameObject objectResolver (GameObject gameObject)
			{
				GameObject result;
				if (gameObject == null)
				{
					result = null;
				}
				else
				{
					Transform transform = gameObject.transform;
					int num = 0;
					while (num < this.height && transform != null && transform.parent != null && transform.parent.name != "VisualSceneNode")
					{
						transform = transform.parent;
						num++;
					}
					result = ((transform == null) ? null : transform.gameObject);
				}
				return result;
			}
		}
		
	}	
}


