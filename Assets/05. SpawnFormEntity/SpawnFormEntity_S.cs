using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup (typeof (SimulationSystemGroup))]
public class SpawnFormEntity_S : JobComponentSystem {
    // 与主线程交互的系统
    BeginInitializationEntityCommandBufferSystem BufferSystem;
    [BurstCompile]
    struct SpawnEntityJob : IJobForEachWithEntity<SpawnFormEntity_C, LocalToWorld> {
        public EntityCommandBuffer.Concurrent commandBuffer;
        public void Execute (Entity entity, int index, [ReadOnly] ref SpawnFormEntity_C spawnFormEntity, [ReadOnly] ref LocalToWorld localToWorld) {
            for (int i = 0; i < spawnFormEntity.CountX; i++) {
                for (int j = 0; j < spawnFormEntity.CountY; j++) {
                    var instance = commandBuffer.Instantiate (index, spawnFormEntity.EntityPrefab);
                    var position = math.transform (localToWorld.Value, new float3 (i, noise.cnoise (new float2 (i, j) * 0.21f) * 1.5f, j) * 1.3f);
                    commandBuffer.SetComponent (index, instance, new Translation () { Value = position });
                }
            }
            commandBuffer.DestroyEntity (index, entity);
        }
    }
    // 获取与主线程交互的系统
    protected override void OnCreate () {
        BufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem> ();
    }
    protected override JobHandle OnUpdate (JobHandle inputDeps) {
        //新建任务
        var spawnEntityJob = new SpawnEntityJob ();
        //赋值任务,推入计划
        var concurrent = BufferSystem.CreateCommandBuffer ().ToConcurrent ();
        spawnEntityJob.commandBuffer = concurrent;
        var jobHandle = spawnEntityJob.Schedule (this, inputDeps);
        //将句柄加入bufferSystem
        BufferSystem.AddJobHandleForProducer (jobHandle);
        //返回句柄
        return jobHandle;
    }
}