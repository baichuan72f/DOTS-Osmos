using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class SpawnFromGameObject : MonoBehaviour {
    public GameObject Prefab;
    public int CountX;
    public int CountY;
    private void Start () {
        // Setting 
        var setting = GameObjectConversionSettings.FromWorld (World.DefaultGameObjectInjectionWorld, null); //BlobAssetStore
        // 将GameObject转换为Entity(准备实体)
        var prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy (Prefab, setting);
        // 实例实体管理类(准备管理实例)
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        // 复制实体
        for (int x = 0; x < CountX; x++) {
            for (int y = 0; y < CountY; y++) {
                //复制实体
                var instance = manager.Instantiate (prefab);
                //数据准备（Position）
                var position = transform.TransformPoint (new float3 (x * 1.3f, noise.cnoise (new float2 (x, y) * 0.23f) * 1.5f, y * 1.3f));
                //赋值组件
                manager.SetComponentData (instance, new Translation () { Value = position });
            }
        }
    }
}