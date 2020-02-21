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
    struct ForceJob : IJobParallelFor {
        public float deltaTime;
        public NativeArray<Force_C> forces;
        public NativeArray<Mover_C> movers;
        public NativeArray<MassPoint_C> masses;
        public void Execute (int index) {
            var mass = masses[index];
            var force = forces[index];
            var mover = movers[index];
            mover.direction += force.value * deltaTime / mass.Mass;
            movers[index] = mover;
        }
    }
    protected override void OnCreate () {
        bufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem> ();
        EntityQueryDesc queryDescription = new EntityQueryDesc ();
        queryDescription.All = new [] { ComponentType.ReadWrite<Force_C> () };
        forceQuery = GetEntityQuery (queryDescription);
    }

    protected override JobHandle OnUpdate (JobHandle inputDeps) {

        // 计算准备
        float deltaTime = Time.DeltaTime;
        var forcers = forceQuery.ToEntityArray (Allocator.Persistent);

        var forcerEnumerable = forcers.ToList ()
            .Where<Entity> ((v) => {
                var force = EntityManager.GetComponentData<Force_C> (v);
                bool hasTo = !force.to.Equals (Entity.Null);
                bool hasFrom = !force.from.Equals (Entity.Null);
                bool hasMover = EntityManager.HasComponent<Mover_C> (force.to);
                bool hasMass = EntityManager.HasComponent<MassPoint_C> (force.to);
                return hasTo && hasFrom && hasMover && hasMass;
            });
        if (forcerEnumerable.Count () == 0) {
            return inputDeps;
        }
        NativeArray<Force_C> forceArr = new NativeArray<Force_C> (
            forcerEnumerable.Select<Entity, Force_C> ((v) => {
                return EntityManager.GetComponentData<Force_C> (v);
            }).ToArray (), Allocator.Persistent);
//        UnityEngine.Debug.Log (forceArr.Length);
        NativeArray<Entity> toArr = new NativeArray<Entity> (
            forceArr.Select<Force_C, Entity> ((v) => {
                return v.to;
            }).ToArray (), Allocator.Persistent);
        NativeArray<Mover_C> movers = new NativeArray<Mover_C> (
            forceArr.Select<Force_C, Mover_C> ((v) => {
                return EntityManager.GetComponentData<Mover_C> (v.to);
            }).ToArray (), Allocator.Persistent);
        NativeArray<MassPoint_C> masses = new NativeArray<MassPoint_C> (
            forceArr.Select<Force_C, MassPoint_C> ((v) => {
                return EntityManager.GetComponentData<MassPoint_C> (v.to);
            }).ToArray (), Allocator.Persistent);
        // 计算受力
        var forceJob = new ForceJob ();
        forceJob.deltaTime = deltaTime;
        forceJob.masses = masses;
        forceJob.movers = movers;
        forceJob.forces = forceArr;
        inputDeps = forceJob.Schedule (forceArr.Length, 64, inputDeps);

        inputDeps.Complete ();
        for (int i = 0; i < toArr.Length; i++) {
            // UnityEngine.Debug.Log(movers[i].direction);
            EntityManager.SetComponentData (toArr[i], movers[i]);
        }
        forceArr.Dispose (inputDeps);
        EntityManager.DestroyEntity (forcers);
        forcers.Dispose (inputDeps);

        movers.Dispose (inputDeps);
        toArr.Dispose (inputDeps);
        masses.Dispose (inputDeps);
        //返回句柄
        return inputDeps;
    }
}