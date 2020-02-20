using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class Join_S : JobComponentSystem {
    public EntityCommandBufferSystem bufferSystem;

    struct JoinJob : IJobForEachWithEntity<Joiner_C, Translation, NonUniformScale> {
        public EntityCommandBuffer.Concurrent concurrent;
        [ReadOnly] public NativeArray<Entity> entities;
        [ReadOnly] public NativeArray<Joiner_C> joiners;
        [ReadOnly] public NativeArray<Translation> translations;
        //public NativeArray<Mover_C> movers;
        public void Execute (Entity entity, int index, ref Joiner_C joiner, ref Translation translation, ref NonUniformScale uniformScale) {
            //计算与当前星体接触的星体
            float outScale = 0.1f;
            float outVolume = 0;
            float3 addV = float3.zero;
            for (int i = 0; i < joiners.Length; i++) {
                if (i == index) continue;
                var joiner2 = joiners[i];
                if (joiner2.volume == joiner.volume && entity.Index > entities[i].Index) continue;

                var translation2 = translations[i];
                var dis = math.distance (translation.Value, translation2.Value);
                if (dis == 0) continue;
                var range1 = UnitHelper.Volume2Range (joiner.volume);
                var range2 = UnitHelper.Volume2Range (joiner2.volume);

                if (range1 + range2 > dis) {
                    bool isOut = joiner2.volume > joiner.volume;
                    float v = isOut?(-joiner.volume * outScale): (joiner2.volume * outScale);
                    outVolume += v;
                    if (!isOut)
                    {
                        //addV+=movers[index].direction;
                    }
                }
            }
            joiner.volume += outVolume;
            //根据物质量显示物体
            if (joiner.volume < 0.01f) {
                concurrent.DestroyEntity (index, entity);
            } else {
                uniformScale.Value = new float3 (1, 1, 1) * 2 * UnitHelper.Volume2Range (joiner.volume);
            }
        }
    }
    EntityQuery entityQuery;
    EntityQuery viewQuery;
    protected override void OnCreate () {
        bufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem> ();
        viewQuery = GetEntityQuery (
            typeof (Joiner_C),
            typeof (Translation),
            typeof (NonUniformScale)
        );
    }

    protected override JobHandle OnUpdate (JobHandle inputDeps) {
        // 计算准备
        var concurrent = bufferSystem.CreateCommandBuffer ().ToConcurrent ();
        NativeArray<Entity> entities = viewQuery.ToEntityArray (Allocator.Persistent);
        NativeArray<Translation> translations = new NativeArray<Translation> (entities.Length, Allocator.Persistent);
        NativeArray<Joiner_C> joiners = new NativeArray<Joiner_C> (entities.Length, Allocator.Persistent);
        for (int i = 0; i < entities.Length; i++) {
            translations[i] = EntityManager.GetComponentData<Translation> (entities[i]);
            joiners[i] = EntityManager.GetComponentData<Joiner_C> (entities[i]);
        }
        // 新建句柄
        var splitJob = new JoinJob ();
        splitJob.concurrent = concurrent;
        splitJob.entities = entities;
        splitJob.joiners = joiners;
        splitJob.translations = translations;
        // 挂载句柄
        inputDeps = splitJob.Schedule (viewQuery, inputDeps);
        inputDeps.Complete ();
        entities.Dispose (inputDeps);
        translations.Dispose (inputDeps);
        joiners.Dispose (inputDeps);
        return inputDeps;
    }
}