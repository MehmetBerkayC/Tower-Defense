using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class WarFactory : GameObjectFactory
{
    [SerializeField] Shell shellPrefab = default;

    public Shell Shell => Get(shellPrefab);

    T Get<T> (T prefab) where T : WarEntity
    {
        T instance = CreateGameObjectInstance(prefab);
        instance.OriginFactory = this;
        return instance;
    }

    public void Reclaim(WarEntity entity)
    {
        Debug.Assert(entity.OriginFactory == this, "Wrong factory claimed!");
        Destroy(entity.gameObject);
    }
}