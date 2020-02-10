using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class SpawnFormEntity_Authoring : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity {
    public int CountX;
    public int CountY;
    public GameObject Prefab;
    public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        var SpawnData = new SpawnFormEntity_C ();
        SpawnData.CountX = CountX;
        SpawnData.CountY = CountY;
        SpawnData.EntityPrefab = conversionSystem.GetPrimaryEntity(Prefab);
        dstManager.AddComponentData (entity, SpawnData);
    }

    public void DeclareReferencedPrefabs (List<GameObject> referencedPrefabs) {
        referencedPrefabs.Add (Prefab);
    }
}