using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class ViewManager : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs {

    public Dictionary<string, GameObject> PrefabDic; //展示集合
    public Dictionary<string, ComponentType> ComponentDic; //组件集合
    public Dictionary<string, Entity> EntityDic; //实体集合
    string[] prefabNames;
    public string rootUrl = "Prefabs/";
    public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        foreach (var prefab in PrefabDic) {
            ViewEntity viewEntity = new ViewEntity ();
            viewEntity.entity = conversionSystem.GetPrimaryEntity (prefab.Value);
            viewEntity.ViewIndex = System.Array.IndexOf (prefabNames, prefab.Key);
            dstManager.AddComponentData (entity, viewEntity);
        }
    }

    public void DeclareReferencedPrefabs (List<GameObject> referencedPrefabs) {
        referencedPrefabs.AddRange (PrefabDic.Values);
    }

    private void Start () {
        //星体，游戏的基本元素//镜子，场景道具
        prefabNames = new string[] { "Star", "Mirror" };
        PrefabDic = new Dictionary<string, GameObject> ();
        for (int i = 0; i < prefabNames.Length; i++) {
            PrefabDic.Add (prefabNames[i], Resources.Load<GameObject> (rootUrl + prefabNames[i]));
        }
    }
}