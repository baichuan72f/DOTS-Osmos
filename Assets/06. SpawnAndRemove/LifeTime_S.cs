using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class LifeTime_S : JobComponentSystem {
    EntityCommandBufferSystem commandBufferSystem;

    struct lifetimeHandle : IJobForEachWithEntity<LifeTime_C> {
        public float deltaTime;
        public EntityCommandBuffer.Concurrent concurrent;
        public void Execute (Entity entity, int index, ref LifeTime_C lifetime) {
            lifetime.value -= deltaTime;
            if (lifetime.value < 0) {
                concurrent.DestroyEntity (index, entity);
            }
        }
    }

    protected override void OnCreate () {
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem> ();
    }
    protected override JobHandle OnUpdate (JobHandle inputDeps) {
        float deltaTime = Time.DeltaTime;
        var concurrent = commandBufferSystem.CreateCommandBuffer ().ToConcurrent ();
        var job = new lifetimeHandle ();
        job.concurrent = concurrent;
        job.deltaTime = Time.DeltaTime;
        var handle = job.Schedule (this, inputDeps);
        commandBufferSystem.AddJobHandleForProducer (handle);
        return handle;
    }
}