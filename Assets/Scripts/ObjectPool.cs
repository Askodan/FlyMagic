using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class ObjectPool : Singleton<ObjectPool> {
	public GameObject[] objectPrefabs;
	List<GameObject>[] pooledObjects;
	public int[] amountToBuffer; 
	public int defaultBufferAmount = 3;
	protected GameObject containerObject;

	void  Start (){ 
		containerObject = new GameObject("ObjectPool"); 
		pooledObjects = new List<GameObject>[objectPrefabs.Length];
		int i=0;
		foreach(GameObject objectPrefab in objectPrefabs) 
		{
			pooledObjects[i] = new List<GameObject>();
			int bufferAmount;

			if(i < amountToBuffer.Length) 
				bufferAmount = amountToBuffer[i];
			else
				bufferAmount = defaultBufferAmount;

			for(int n = 0; n < bufferAmount; n++) 
			{
				GameObject newObj = Instantiate(objectPrefab) as GameObject;
				newObj.name = objectPrefab.name;
				PoolObject(newObj);
			}
			i++;
		}

	}

	public GameObject  GetObjectForType (  string objectType ,   bool onlyPooled  ){ 
		for(int i = 0; i < objectPrefabs.Length; i++) 
		{
			GameObject prefab = objectPrefabs[i];
			if(prefab.name == objectType) 
			{
				if(pooledObjects[i].Count > 0)
				{
					GameObject pooledObject = pooledObjects[i][0];
					pooledObjects[i].RemoveAt(0);
					pooledObject.transform.parent = null;
					pooledObject.SetActive(true);
					return pooledObject;
				}
				else
					if(!onlyPooled) 
					{
						return Instantiate(objectPrefabs[i]) as GameObject;
					}
			}
		}
		return null; 
	}

	public void  PoolObject ( GameObject obj  ){
		for (int i = 0; i < objectPrefabs.Length; i++) 
		{
			if (objectPrefabs[i].name == obj.name)
			{
				obj.SetActive(false);
				obj.transform.parent = containerObject.transform;
				pooledObjects[i].Add(obj);
				return;
			}
		}
	}
}