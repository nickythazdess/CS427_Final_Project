using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This class allows us to create multiple instances of a prefabs and reuse them.
/// It allows us to avoid the cost of destroying and creating objects.
/// </summary>
public class Pooler
{
	protected Stack<GameObject> pool = new Stack<GameObject>();
	protected GameObject prototype;

	public Pooler(GameObject original, int initialSize)
	{
		prototype = original;
		pool = new Stack<GameObject>(initialSize);

		for (int i = 0; i < initialSize; ++i)
		{
			GameObject obj = Object.Instantiate(original);
			obj.SetActive(false);
            pool.Push(obj);
		}
	}

	public GameObject Get() => Get(Vector3.zero, Quaternion.identity);

	public GameObject Get(Vector3 pos, Quaternion quat)
	{
	    GameObject ret = pool.Count > 0 ? pool.Pop() : Object.Instantiate(prototype);

		ret.SetActive(true);
		ret.transform.position = pos;
		ret.transform.rotation = quat;

		return ret;
	}

	public void Free(GameObject obj)
	{
		obj.transform.SetParent(null);
		obj.SetActive(false);
		pool.Push(obj);
	}
}
