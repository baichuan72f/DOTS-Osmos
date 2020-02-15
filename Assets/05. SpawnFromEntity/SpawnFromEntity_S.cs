using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class SpawnFromEntity_S : JobComponentSystem {
    EntityCommandBufferSystem bufferSystem;
    //构建Job
    public struct SpawnJob : IJobForEachWithEntity<SpawnFormEntity_C, LocalToWorld> {
        public EntityCommandBuffer.Concurrent concurrent;

        public void Execute (Entity entity, int index, ref SpawnFormEntity_C spawnData, ref LocalToWorld localToWorld) {
            //复制实体
            for (int x = 0; x < spawnData.CountX; x++) {
                for (int y = 0; y < spawnData.CountY; y++) {
                    var instance = concurrent.Instantiate (index, spawnData.entity);
                    var position = math.transform (localToWorld.Value, new float3 (x * 1.3f, noise.cnoise (new float2 (x, y) * 0.21f) * 1.5f, y * 1.3f));
                    concurrent.AddComponent (index, instance, new Translation () { Value = position });
                }
            }
            //销毁数据实体以阻止反复复制
            concurrent.DestroyEntity (index, entity);
        }
    }
    protected override void OnCreate () {
        bufferSystem = World.GetOrCreateSystem<EntityCommandBufferSystem> ();
    }
    protected override JobHandle OnUpdate (JobHandle inputDeps) {
        //计算准备
        var concurrent = bufferSystem.CreateCommandBuffer ().ToConcurrent ();
        //给Job赋值
        var spawnJob = new SpawnJob ();
        spawnJob.concurrent = concurrent;
        //挂载句柄
        var handle = spawnJob.Schedule (this, inputDeps);
        bufferSystem.AddJobHandleForProducer (handle);
        return handle;
    }
}