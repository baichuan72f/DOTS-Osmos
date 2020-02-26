using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

//加速度定律(加速度结算)
[UpdateInGroup (typeof (TransformSystemGroup))]
[UpdateBefore (typeof (SimpleForce_S1))]
public class SimpleForce_S2 : JobComponentSystem {

    EntityQuery forceQuery;
    EntityCommandBufferSystem bufferSystem;

    [BurstCompile]
    struct ForceJob : IJobForEachWithEntity<Force_C,Mover_C,MassPoint_C> {
        public double deltaTime;

        public void Execute(Entity entity, int index, [ReadOnly]ref Force_C force, ref Mover_C mover, [ReadOnly] ref MassPoint_C mass)
        {
            mover.direction += force.value * deltaTime / mass.Mass;
        }
    }
    protected override void OnCreate () {
        bufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem> ();
        EntityQueryDesc queryDescription = new EntityQueryDesc ();
        queryDescription.All = new [] { ComponentType.ReadWrite<Force_C> (),ComponentType.ReadWrite<Mover_C>(),ComponentType.ReadWrite<MassPoint_C>() };
        forceQuery = GetEntityQuery (queryDescription);
    }

    protected override JobHandle OnUpdate (JobHandle inputDeps) {

        // 计算准备
        double deltaTime = math.min(0.05,Time.DeltaTime);;

        // 计算受力
        var forceJob = new ForceJob ();
        forceJob.deltaTime = deltaTime;
        inputDeps = forceJob.Schedule (forceQuery,inputDeps);

        inputDeps.Complete ();
        
        //返回句柄
        return inputDeps;
    }
}