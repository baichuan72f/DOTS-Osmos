using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateBefore (typeof (Join_S))]
public class AddJoinView_S : JobComponentSystem {
    EntityQuery viewQuery;
    EntityCommandBufferSystem bufferSystem;
    protected override void OnCreate () {
        bufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem> ();
        viewQuery = GetEntityQuery (
            typeof (Joiner_C),
            typeof (Translation)
        );
    }
    protected override JobHandle OnUpdate (JobHandle inputDeps) {
        var concurrent = bufferSystem.CreateCommandBuffer ().ToConcurrent ();
        // 计算准备
        NativeArray<Entity> entities = viewQuery.ToEntityArray (Allocator.Persistent);
        foreach (var item in entities) {
            if (!EntityManager.HasComponent<NonUniformScale> (item)) {
                EntityManager.AddComponent<NonUniformScale> (item);
            }
        }
        entities.Dispose(inputDeps);
        return inputDeps;
    }
}