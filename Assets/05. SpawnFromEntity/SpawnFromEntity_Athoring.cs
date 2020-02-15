using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class SpawnFromEntity_Athoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs {
    public GameObject Prefab;
    public int CountX;
    public int CountY;

    public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        // 初始化组件
        var SpawnData = new SpawnFormEntity_C ();
        SpawnData.CountX = CountX;
        SpawnData.CountY = CountY;
        SpawnData.entity = conversionSystem.GetPrimaryEntity (Prefab);
        // 将组件赋值给实体
        dstManager.AddComponentData (entity, SpawnData);
    }

    // 将预制体添加到系统实例化列表中
    public void DeclareReferencedPrefabs (List<GameObject> referencedPrefabs) {
        referencedPrefabs.Add (Prefab);
    }
}