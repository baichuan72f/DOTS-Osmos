using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class SpawnAndRemoveEntity_Athoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs {
    public GameObject Prefab;
    public int CountX;
    public int CountY;
    public float MinLifeTime;
    public float MaxLifeTime;
    public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        var spawnData = new SpawnAndRemoveEntity_C ();
        spawnData.CountX = CountX;
        spawnData.CountY = CountY;
        spawnData.MinLifetime = MinLifeTime;
        spawnData.MaxLifetime = MaxLifeTime;
        spawnData.entity = conversionSystem.GetPrimaryEntity (Prefab);
        dstManager.AddComponentData (entity, spawnData);
    }

    public void DeclareReferencedPrefabs (List<GameObject> referencedPrefabs) {
        referencedPrefabs.Add (Prefab);
    }
}