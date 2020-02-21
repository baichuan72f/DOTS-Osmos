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
            if (!EntityManager.HasComponent<MassPoint_C> (item)) {
                EntityManager.AddComponent<MassPoint_C> (item);
            }
            MassPoint_C mass = new MassPoint_C ();
            var joiner = EntityManager.GetComponentData<Joiner_C> (item);
            mass.Mass = UnitHelper.Volume2Mass (density.water, joiner.volume);
            EntityManager.SetComponentData<MassPoint_C> (item, mass);
        }
        entities.Dispose (inputDeps);
        return inputDeps;
    }
}