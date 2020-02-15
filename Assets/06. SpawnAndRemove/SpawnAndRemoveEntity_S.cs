using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class SpawnAndRemoveEntity_S : JobComponentSystem {
    EntityCommandBufferSystem bufferSystem;
    struct SpawnAndRemoveJob : IJobForEachWithEntity<SpawnAndRemoveEntity_C, LocalToWorld> {
        public EntityCommandBuffer.Concurrent concurrent;
        public void Execute (Entity entity, int index, ref SpawnAndRemoveEntity_C spawnData, ref LocalToWorld location) {
            var random = new Unity.Mathematics.Random (1);
            for (int x = 0; x < spawnData.CountX; x++) {
                for (int y = 0; y < spawnData.CountY; y++) {
                    var instance = concurrent.Instantiate (index, spawnData.entity);
                    var position = math.transform (location.Value, new float3 (x * 1.3F, noise.cnoise (new float2 (x, y) * 0.21F) * 2, y * 1.3F));
                    concurrent.AddComponent (index, instance, new Translation { Value = position });
                    concurrent.AddComponent (index, instance, new LifeTime { Value = random.NextFloat (3.0F, 30.0F) });
                    concurrent.AddComponent (index, instance, new RotationSpeed_SpawnAndRemove { RadiansPerSecond = math.radians (random.NextFloat (25.0F, 90.0F)) });
                }
            }
            concurrent.DestroyEntity (index, entity);
        }
    }
    protected override void OnCreate () {
        bufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem> ();
    }
    protected override JobHandle OnUpdate (JobHandle inputDeps) {
        SpawnAndRemoveJob job = new SpawnAndRemoveJob ();
        job.concurrent = bufferSystem.CreateCommandBuffer ().ToConcurrent ();
        var handle = job.Schedule (this, inputDeps);
        bufferSystem.AddJobHandleForProducer (handle);
        return handle;
    }
}