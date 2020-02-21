using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Hash128 = Unity.Entities.Hash128;

public class ViewManager : MonoBehaviour {
    public static GameObjectConversionSettings setting;
    public string rootUrl = "Prefabs/";

    public struct ChunckConfig {
        public Entity entity;
        public int count;
        public RangeInt volumeRange;
        public float3 center;
        public float3 size;
        public float3 minSpeed;
        public float3 maxSpeed;
    }
    private void Start () {
        //星体，游戏的基本元素//镜子，场景道具
        var prefabNames = new string[] { "Star", "Mirror" };
        setting = GameObjectConversionSettings.FromWorld (World.DefaultGameObjectInjectionWorld, null);
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        //初始化配置
        InitConfig (manager, setting, prefabNames);
        //初始化场景
        ChunckConfig chunckConfig = fastChunkConfig (new float3 (-6, 6, 0), Vector3.right);
        InitScene (manager, chunckConfig);
        chunckConfig = fastChunkConfig (new float3 (6, 6, 0), Vector3.down);
        InitScene (manager, chunckConfig);
        chunckConfig = fastChunkConfig (new float3 (6, -6, 0), Vector3.left);
        InitScene (manager, chunckConfig);
        chunckConfig = fastChunkConfig (new float3 (-6, -6, 0), Vector3.up);
        InitScene (manager, chunckConfig);
    }

    private static ChunckConfig fastChunkConfig (float3 position, float3 direction) {
        Entity entity;
        ViewFactory.ViewMap.TryGetValue ("Star".GetHashCode (), out entity);
        ChunckConfig chunckConfig = new ChunckConfig ();
        chunckConfig.entity = entity;
        chunckConfig.center = position;
        chunckConfig.count = 100;
        chunckConfig.maxSpeed = direction * 0.9f;
        chunckConfig.minSpeed = direction * 0.7f;
        chunckConfig.size = new float3 (5, 5, 0);
        chunckConfig.volumeRange = new RangeInt (2, 4);
        return chunckConfig;
    }

    public void InitConfig (EntityManager manager, GameObjectConversionSettings settings, string[] prefabNames) {
        ViewFactory.ViewMap = new NativeHashMap<int, Entity> (0, Allocator.Persistent);
        for (int i = 0; i < prefabNames.Length; i++) {
            var obj = Resources.Load<GameObject> (rootUrl + prefabNames[i]);
            var prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy (obj, setting);
            bool result = ViewFactory.ViewMap.TryAdd (prefabNames[i].GetHashCode (), prefab);
            Debug.Log (prefabNames[i].GetHashCode () + " " + result);
        }
    }

    public void InitScene (EntityManager manager, ChunckConfig config) {
        var random = new Unity.Mathematics.Random (1);
        for (int i = 0; i < config.count; i++) {
            var position = random.NextFloat3 (config.center - config.size, config.center + config.size);
            var volume = random.NextFloat (config.volumeRange.start, config.volumeRange.start + config.volumeRange.length);
            var speed = random.NextFloat3 (config.minSpeed, config.maxSpeed);
            var entity = manager.Instantiate (config.entity);
            Joiner_C joiner = new Joiner_C () { volume = volume };
            Mover_C mover = new Mover_C () { direction = speed };
            Translation translation = new Translation () { Value = position };
            manager.SetComponentData (entity, joiner);
            manager.SetComponentData (entity, mover);
            manager.SetComponentData (entity, translation);
        }
    }
    void OnDestroy () {
        ViewFactory.ViewMap.Dispose ();
    }
}