using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class LookPlayer_Authoring : MonoBehaviour {
    public int index;
    public GameObject Author;
    EntityManager manager;
    EntityQuery entityQuery;
    private void Start () {
        manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        entityQuery = manager.CreateEntityQuery (ComponentType.ReadOnly (typeof (player_C)));
    }
    private void Update () {
        NativeArray<Entity> entities = entityQuery.ToEntityArray (Allocator.TempJob);
        for (int i = 0; i < entities.Length; i++) {
            if (manager.GetComponentData<player_C> (entities[i]).index == index) {
                var position = manager.GetComponentData<Translation> (entities[i]).Value;
                Author.transform.position = position;
            }
        }
        entities.Dispose ();
    }
}