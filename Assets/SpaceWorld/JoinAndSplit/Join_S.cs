using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateBefore (typeof (Momentum_S))]
public class Join_S : JobComponentSystem {
    public EntityCommandBufferSystem bufferSystem;

    struct JoinJob : IJobParallelFor {
        public EntityCommandBuffer.Concurrent concurrent;
        [ReadOnly] public NativeArray<Entity> entities;
        public NativeArray<Joiner_C> joiners;
        [ReadOnly] public NativeArray<Translation> translations;
        [ReadOnly] public NativeArray<Mover_C> movers;
        public NativeArray<NonUniformScale> scales;

        public void Execute (int index) {
            Entity entity = entities[index];
            Joiner_C joiner = joiners[index];
            Translation translation = translations[index];
            Mover_C mover = movers[index];
            NonUniformScale scale = scales[index];
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
                    //物质转移量
                    float v = isOut?(-joiner.volume * outScale): (joiner2.volume * outScale);
                    outVolume += v;
                    float mass = UnitHelper.Volume2Mass (density.water, v);
                    if (!isOut) {
                        Momentum_C momentumIn = new Momentum_C ();
                        momentumIn.mass = mass;
                        //momentumIn.mover = mover;
                        momentumIn.speed = movers[i].direction;
                        momentumIn.target = entity;
                        concurrent.AddComponent (index, concurrent.CreateEntity (index), momentumIn);
                        Momentum_C momentumOut = new Momentum_C ();
                        momentumOut.mass = mass;
                        momentumOut.speed = -movers[i].direction;
                        momentumOut.target = entities[i];
                        //momentumOut.mover = movers[i];
                        concurrent.AddComponent (index, concurrent.CreateEntity (index), momentumOut);

                    }
                }
            }
            joiner.volume += outVolume;
            //根据物质量显示物体
            if (joiner.volume < 0.01f) {
                concurrent.DestroyEntity (index, entity);
            } else {
                scale.Value = new float3 (1, 1, 1) * 2 * UnitHelper.Volume2Range (joiner.volume);
                concurrent.SetComponent (index, entity, scale);
                concurrent.SetComponent (index, entity, joiner);
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
            typeof (NonUniformScale),
            typeof (Mover_C)
        );
    }

    protected override JobHandle OnUpdate (JobHandle inputDeps) {
        // 计算准备
        var concurrent = bufferSystem.CreateCommandBuffer ().ToConcurrent ();
        NativeArray<Entity> entities = viewQuery.ToEntityArray (Allocator.Persistent);
        NativeArray<Translation> translations = new NativeArray<Translation> (entities.Length, Allocator.Persistent);
        NativeArray<Joiner_C> joiners = new NativeArray<Joiner_C> (entities.Length, Allocator.Persistent);
        NativeArray<NonUniformScale> scales = new NativeArray<NonUniformScale> (entities.Length, Allocator.Persistent);
        NativeArray<Mover_C> movers = new NativeArray<Mover_C> (entities.Length, Allocator.Persistent);

        for (int i = 0; i < entities.Length; i++) {
            translations[i] = EntityManager.GetComponentData<Translation> (entities[i]);
            joiners[i] = EntityManager.GetComponentData<Joiner_C> (entities[i]);
            scales[i] = EntityManager.GetComponentData<NonUniformScale> (entities[i]);
            movers[i] = EntityManager.GetComponentData<Mover_C> (entities[i]);
        }
        // 新建句柄
        var splitJob = new JoinJob ();
        splitJob.concurrent = concurrent;
        splitJob.entities = entities;
        splitJob.movers = movers;
        splitJob.joiners = joiners;
        splitJob.translations = translations;
        splitJob.scales = scales;
        // 挂载句柄
        inputDeps = splitJob.Schedule (entities.Length, entities.Length, inputDeps);
        inputDeps.Complete ();
        movers.Dispose (inputDeps);
        entities.Dispose (inputDeps);
        translations.Dispose (inputDeps);
        joiners.Dispose (inputDeps);
        scales.Dispose (inputDeps);
        return inputDeps;
    }
}