using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateBefore(typeof(Momentum_S))]
public class Join_S : JobComponentSystem
{
    public EntityCommandBufferSystem bufferSystem;
    [BurstCompile]
    struct JoinJob : IJobParallelFor
    {
        public double deltaTime;
        public EntityCommandBuffer.Concurrent concurrent;
        [ReadOnly] public NativeArray<Entity> entities;
        public NativeArray<Joiner_C> joiners;
        [ReadOnly] public NativeArray<Translation> translations;
        [ReadOnly] public NativeArray<Mover_C> movers;
        public NativeArray<NonUniformScale> scales;

        public void Execute(int index)
        {
            Entity entity = entities[index];
            Joiner_C joiner = joiners[index];
            Translation translation = translations[index];
            Mover_C mover = movers[index];
            NonUniformScale scale = scales[index];
            // 增加的体积
            double addVolume = 0;
            // 增加的质量
            double addMass = 0;
            // 增加的动量
            double3 addMomentum = double3.zero;
            for (int i = 0; i < joiners.Length; i++)
            {
                if (i == index) continue;
                var joiner2 = joiners[i];
                if (joiner.Volume <= 0 || joiner2.Volume <= 0) continue;
                if (joiner2.Range == joiner.Range && entity.Index > entities[i].Index) continue;
                var translation2 = translations[i];
                var dis = math.distance(translation.Value, translation2.Value);
                if (dis == 0) continue;

                if (joiner.Range + joiner2.Range > dis)
                {
                    //如果被吸收则减少当前体积，如果吸收则增加对方的体积
                    bool isOut = joiner2.Range > joiner.Range;
                    double v = isOut ? joiner.Volume : joiner2.Volume;
                    //最少吸收每秒吸收1体积
                    if (v > 0.2)
                    {
                        v = math.max(0.2, v * deltaTime * 2);
                    }
                    if (!isOut)
                    {
                        var m = UnitHelper.Volume2Mass(v);
                        addMass += m;
                        addMomentum += m * movers[i].direction;
                    }
                    addVolume += isOut ? -v : v;
                }
            }
            joiner.Volume += addVolume;

            //根据物质量显示物体
            if (joiner.Volume < 0.01)
            {
                concurrent.DestroyEntity(index, entity);
                return;
            }

            if (addVolume > 0)
            {
                //添加动量
                Momentum_C momentumIn = new Momentum_C();
                momentumIn.mass = addMass * 0.1;
                momentumIn.speed = addMomentum / (addMass + 1);
                momentumIn.target = entity;
                concurrent.AddComponent(index, concurrent.CreateEntity(index), momentumIn);
            }
            scale.Value = (float3)(new double3(1, 1, 1) * 2 * joiner.Range);
            concurrent.SetComponent(index, entity, scale);
            concurrent.SetComponent(index, entity, joiner);
        }
    }
    EntityQuery entityQuery;
    EntityQuery viewQuery;
    protected override void OnCreate()
    {
        bufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem>();
        viewQuery = GetEntityQuery(
            typeof(Joiner_C),
            typeof(Translation),
            typeof(NonUniformScale),
            typeof(Mover_C)
        );
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var deltaTime = math.min(0.05, Time.DeltaTime);
        // 计算准备
        var concurrent = bufferSystem.CreateCommandBuffer().ToConcurrent();
        NativeArray<Entity> entities = viewQuery.ToEntityArray(Allocator.Persistent);
        NativeArray<Translation> translations = new NativeArray<Translation>(entities.Length, Allocator.Persistent);
        NativeArray<Joiner_C> joiners = new NativeArray<Joiner_C>(entities.Length, Allocator.Persistent);
        NativeArray<NonUniformScale> scales = new NativeArray<NonUniformScale>(entities.Length, Allocator.Persistent);
        NativeArray<Mover_C> movers = new NativeArray<Mover_C>(entities.Length, Allocator.Persistent);

        for (int i = 0; i < entities.Length; i++)
        {

            translations[i] = EntityManager.GetComponentData<Translation>(entities[i]);
            joiners[i] = EntityManager.GetComponentData<Joiner_C>(entities[i]);
            scales[i] = EntityManager.GetComponentData<NonUniformScale>(entities[i]);
            movers[i] = EntityManager.GetComponentData<Mover_C>(entities[i]);
        }
        // 新建句柄
        var splitJob = new JoinJob();
        splitJob.deltaTime = deltaTime;
        splitJob.concurrent = concurrent;
        splitJob.entities = entities;
        splitJob.movers = movers;
        splitJob.joiners = joiners;
        splitJob.translations = translations;
        splitJob.scales = scales;
        // 挂载句柄
        inputDeps = splitJob.Schedule(entities.Length, entities.Length, inputDeps);
        inputDeps.Complete();
        movers.Dispose(inputDeps);
        entities.Dispose(inputDeps);
        translations.Dispose(inputDeps);
        joiners.Dispose(inputDeps);
        scales.Dispose(inputDeps);
        return inputDeps;
    }
}