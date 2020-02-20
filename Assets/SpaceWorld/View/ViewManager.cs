using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Hash128 = Unity.Entities.Hash128;

public class ViewManager : MonoBehaviour {
    public static GameObjectConversionSettings setting;
    string[] prefabNames;
    public string rootUrl = "Prefabs/";

    private void Start () {
        //星体，游戏的基本元素//镜子，场景道具
        prefabNames = new string[] { "Star", "Mirror" };
        setting = GameObjectConversionSettings.FromWorld (World.DefaultGameObjectInjectionWorld, null);
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        ViewFactory.ViewMap = new NativeHashMap<int, Entity> (0, Allocator.Persistent);
        for (int i = 0; i < prefabNames.Length; i++) {
            var obj = Resources.Load<GameObject> (rootUrl + prefabNames[i]);
            var prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy (obj, setting);
            bool result = ViewFactory.ViewMap.TryAdd (prefabNames[i].GetHashCode (), prefab);
            Debug.Log (prefabNames[i].GetHashCode () + " " + result);

        }
    }
    void OnDestroy () {
        ViewFactory.ViewMap.Dispose ();
    }
}