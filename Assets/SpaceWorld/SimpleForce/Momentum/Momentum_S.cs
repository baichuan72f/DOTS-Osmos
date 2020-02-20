using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

//动量守恒系统

public class Momantum_S : JobComponentSystem {
    public EntityCommandBufferSystem bufferSystem;

    struct MomantumJob : IJobParallelFor {
        public EntityCommandBuffer.Concurrent concurrent;
        public NativeArray<Entity> fromArr;
        [ReadOnly] public NativeArray<Momentum_C> MomentumArr; //动量
        [ReadOnly] public NativeArray<Entity> toArr; //接受者实体
        public NativeArray<Mover_C> movers; //接受者移动组件
        [ReadOnly] public NativeArray<MassPoint_C> masses; //接受者质量组件

        public void Execute (int index) {
            var momentum = MomentumArr[index];
            float3 speed = movers[index].direction * masses[index].Mass + momentum.mass * momentum.speed;
            Mover_C mover = new Mover_C () { direction = speed / (masses[index].Mass + momentum.mass) };
            concurrent.SetComponent (index, toArr[index], mover);
            //concurrent.DestroyEntity (index, fromArr[index]);
        }
    }
    EntityQuery fromQuery;
    protected override void OnCreate () {
        bufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem> ();
        fromQuery = GetEntityQuery (typeof (Momentum_C));
    }
    protected override JobHandle OnUpdate (JobHandle inputDeps) {
        //计算准备
        var concurrent = bufferSystem.CreateCommandBuffer ().ToConcurrent ();
        NativeArray<Entity> fromArr = fromQuery.ToEntityArray (Allocator.Persistent);
        NativeArray<Momentum_C> MomentumArr = new NativeArray<Momentum_C> (fromArr.Length, Allocator.Persistent);
        NativeArray<Entity> toArr = new NativeArray<Entity> (fromArr.Length, Allocator.Persistent);
        NativeArray<Mover_C> movers = new NativeArray<Mover_C> (fromArr.Length, Allocator.Persistent);
        NativeArray<MassPoint_C> masses = new NativeArray<MassPoint_C> (fromArr.Length, Allocator.Persistent);
        for (int i = 0; i < fromArr.Length; i++) {
            Momentum_C momentum = EntityManager.GetComponentData<Momentum_C> (fromArr[i]);
            MomentumArr[i] = momentum;
            toArr[i] = momentum.target;
            movers[i] = momentum.mover;
            masses[i] = EntityManager.GetComponentData<MassPoint_C> (momentum.target);
        }
        //实例句柄
        MomantumJob job = new MomantumJob ();
        job.fromArr = fromArr;
        job.MomentumArr = MomentumArr;
        job.toArr = toArr;
        job.masses = masses;
        job.movers = movers;
        job.concurrent = concurrent;
        //挂载句柄
        inputDeps = job.Schedule (fromArr.Length, 1, inputDeps);

        inputDeps.Complete ();
        //释放资源
        EntityManager.DestroyEntity (fromQuery);
        MomentumArr.Dispose (inputDeps);
        fromArr.Dispose (inputDeps);
        toArr.Dispose (inputDeps);
        movers.Dispose (inputDeps);
        masses.Dispose (inputDeps);

        //返回句柄
        return inputDeps;
    }
}