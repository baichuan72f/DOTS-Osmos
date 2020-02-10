using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[AddComponentMenu ("DOTSEx/SwanpFormGameObject")]
public class SpawnFromGameObject : MonoBehaviour {
    public GameObject Prefab;
    public int CountX = 100;
    public int CountY = 100;
    // Start is called before the first frame update
    void Start () {
        var setting = GameObjectConversionSettings.FromWorld (World.DefaultGameObjectInjectionWorld, null);
        var prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy (Prefab, setting);
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        for (int x = 0; x < CountX; x++) {
            for (int y = 0; y < CountY; y++) {
                var instance = entityManager.Instantiate (prefab);
                var position = transform.TransformPoint (new Vector3 (x, noise.cnoise (new Vector2 (x, y) * 1.5f), y) * 1.2f);
                entityManager.SetComponentData (instance, new Translation () { Value = position });
            }
        }
    }

    // Update is called once per frame
    void Update () {

    }
}