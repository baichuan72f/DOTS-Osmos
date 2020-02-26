using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

//动量守恒系统
[UpdateAfter(typeof(Join_S))]
public class Momentum_S : JobComponentSystem
{
    public EntityCommandBufferSystem bufferSystem;

    struct MomentumJob : IJobParallelFor
    {
        public EntityCommandBuffer.Concurrent concurrent;
        [ReadOnly] public NativeArray<Momentum_C> MomentumArr; //动量
        [ReadOnly] public NativeArray<Entity> toArr; //接受者实体
        public NativeArray<Mover_C> movers; //接受者移动组件
        [ReadOnly] public NativeArray<MassPoint_C> masses; //接受者质量组件
        public void Execute(int index)
        {
            if (toArr[index].Equals(Entity.Null)) return;
            var momentum = MomentumArr[index];
            double3 direction = 0;
            //如果mass为0 代表增加单纯的动量(质量不变)
            if (momentum.mass == 0)
            {
                double3 speed = movers[index].direction * masses[index].Mass + momentum.speed;
                direction = speed / masses[index].Mass;
            }
            else
            {
                double3 speed = movers[index].direction * masses[index].Mass + momentum.mass * momentum.speed;
                direction = speed / (masses[index].Mass + momentum.mass);
            }
            Mover_C mover = new Mover_C() { direction = direction };
            movers[index] = mover;
        }
    }
    EntityQuery fromQuery;
    protected override void OnCreate()
    {
        bufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        fromQuery = GetEntityQuery(typeof(Momentum_C));
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        //计算准备
        var concurrent = bufferSystem.CreateCommandBuffer().ToConcurrent();

        var fromArr = fromQuery.ToEntityArray(Allocator.TempJob);
        NativeArray<Momentum_C> MomentumArr = new NativeArray<Momentum_C>(fromArr.Length, Allocator.Persistent);
        NativeArray<Entity> toArr = new NativeArray<Entity>(fromArr.Length, Allocator.Persistent);
        NativeArray<Mover_C> movers = new NativeArray<Mover_C>(fromArr.Length, Allocator.Persistent);
        NativeArray<MassPoint_C> masses = new NativeArray<MassPoint_C>(fromArr.Length, Allocator.Persistent);
        for (int i = 0; i < fromArr.Length; i++)
        {
            Momentum_C momentum = EntityManager.GetComponentData<Momentum_C>(fromArr[i]);
            MomentumArr[i] = momentum;
            toArr[i] = momentum.target;
            if (momentum.target.Equals(Entity.Null)) continue;
            movers[i] = EntityManager.GetComponentData<Mover_C>(momentum.target);
            masses[i] = EntityManager.GetComponentData<MassPoint_C>(momentum.target);
        }
        //实例句柄
        MomentumJob job = new MomentumJob();
        job.MomentumArr = MomentumArr;
        job.toArr = toArr;
        job.masses = masses;
        job.movers = movers;
        job.concurrent = concurrent;
        //挂载句柄
        inputDeps = job.Schedule(MomentumArr.Length, 1, inputDeps);
        inputDeps.Complete();
        for (int i = 0; i < movers.Length; i++)
        {
            EntityManager.SetComponentData(toArr[i], movers[i]);
        }
        //释放资源
        EntityManager.DestroyEntity(fromQuery);
        MomentumArr.Dispose(inputDeps);
        fromArr.Dispose(inputDeps);
        toArr.Dispose(inputDeps);
        movers.Dispose(inputDeps);
        masses.Dispose(inputDeps);

        //返回句柄
        return inputDeps;
    }
}