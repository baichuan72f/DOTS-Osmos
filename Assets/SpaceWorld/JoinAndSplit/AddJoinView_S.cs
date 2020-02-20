using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateBefore(typeof(Join_S))]
public class AddJoinView_S : JobComponentSystem {
    EntityQuery viewQuery;
    protected override void OnCreate () {
        viewQuery = GetEntityQuery (
            typeof (Joiner_C),
            typeof (Translation)
        );
    }

    protected override JobHandle OnUpdate (JobHandle inputDeps) {
        // 计算准备
        NativeArray<Entity> entities = viewQuery.ToEntityArray (Allocator.Persistent);
        for (int i = 0; i < entities.Length; i++) {
            if (!EntityManager.HasComponent<NonUniformScale> (entities[i])) {
                EntityManager.AddComponent<NonUniformScale> (entities[i]);
            }
        }

        // 挂载句柄
        inputDeps.Complete ();
        entities.Dispose (inputDeps);
        return inputDeps;
    }
}