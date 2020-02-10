using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class PrefabManager : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs {
    //星体，游戏的基本元素
    public GameObject Star;
    //镜子，场景道具
    public GameObject Mirror;

    public Dictionary<string, GameObject> PrefabDic;

    public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        foreach (var prefab in PrefabDic) {
            ViewEntity viewEntity = new ViewEntity ();
            viewEntity.name = prefab.Key;
            viewEntity.entity = conversionSystem.GetPrimaryEntity (prefab.Value);
            dstManager.AddComponentData (entity, viewEntity);
        }
    }

    public void DeclareReferencedPrefabs (List<GameObject> referencedPrefabs) {
        referencedPrefabs.AddRange (PrefabDic.Values);
    }

    private void Start () {
        PrefabDic = new Dictionary<string, GameObject> ();
        PrefabDic.Add ("Star", Star);
        PrefabDic.Add ("Mirror", Mirror);
    }
}