using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoSingleton<ObjectPool>
{
    [Serializable]
    public class PoolModel
    {
        public GameObject gameObject = null;
        public int count = 0;
    }

    private readonly Dictionary<GameObject, List<GameObject>> ObjectMap
        = new Dictionary<GameObject, List<GameObject>>();

    public List<PoolModel> prefabList = null;

    public Transform poolObjectParent = null;

    private void Awake()
    {
        foreach (var model in prefabList)
        {
            ObjectMap[model.gameObject] = new List<GameObject>();

            for (int i = 0; i < model.count; ++i)
            {
                var go = Instantiate(model.gameObject, poolObjectParent);
                go.SetActive(false);
                ObjectMap[model.gameObject].Add(go);
            }
        }
    }

    public bool TryGet(GameObject key, out GameObject value)
    {
        if (ObjectMap.TryGetValue(key, out var objectList))
        {
            var firstInactiveObject = objectList.Find(x => !x.activeSelf);
            firstInactiveObject.SetActive(true);
            value = firstInactiveObject;
            return true;
        }

        value = null;
        return false;
    }
}
