using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateBefore (typeof (Join_S))]
public class AddJoinView_S : JobComponentSystem {
    EntityCommandBufferSystem bufferSystem;
    struct AddMassJob : IJobForEachWithEntity<Joiner_C> {
        public EntityCommandBuffer.Concurrent concurrent;

        public void Execute (Entity entity, int index, ref Joiner_C joiner) {
            MassPoint_C mass = new MassPoint_C ();
            mass.densityType = density.water;
            mass.Mass = UnitHelper.Volume2Mass (mass.densityType, joiner.volume);
            concurrent.AddComponent (index, entity, mass);
        }
    }
    struct AddScaleJob : IJobForEachWithEntity<Joiner_C> {
        public EntityCommandBuffer.Concurrent concurrent;

        public void Execute (Entity entity, int index, ref Joiner_C joiner) {
            NonUniformScale scale = new NonUniformScale ();
            scale.Value = new float3 (1, 1, 1) * 2 * UnitHelper.Volume2Range (joiner.volume);
            concurrent.AddComponent (index, entity, scale);
        }
    }
    struct AddMoverJob : IJobForEachWithEntity<Joiner_C> {
        public EntityCommandBuffer.Concurrent concurrent;
        public void Execute (Entity entity, int index, ref Joiner_C joiner) {
            Mover_C mover = new Mover_C ();
            //scale.Value = new float3 (1, 1, 1) * 2 * UnitHelper.Volume2Range (joiner.volume);
            concurrent.AddComponent (index, entity, mover);
        }
    }
    EntityQuery nonMassQuery, nonMoverQuery, nonScaleQuery;
    protected override void OnCreate () {
        bufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem> ();
        EntityQueryDesc addMass = new EntityQueryDesc ();
        addMass.All = new ComponentType[] { typeof (Joiner_C) };
        addMass.None = new ComponentType[] { typeof (MassPoint_C) };
        nonMassQuery = GetEntityQuery (addMass);
        EntityQueryDesc addMover = new EntityQueryDesc ();
        addMover.All = new ComponentType[] { typeof (Joiner_C) };
        addMover.None = new ComponentType[] { typeof (Mover_C) };
        nonMoverQuery = GetEntityQuery (addMover);
        EntityQueryDesc addScale = new EntityQueryDesc ();
        addScale.All = new ComponentType[] { typeof (Joiner_C) };
        addScale.None = new ComponentType[] { typeof (NonUniformScale) };
        nonScaleQuery = GetEntityQuery (addScale);
    }
    protected override JobHandle OnUpdate (JobHandle inputDeps) {
        var concurrent = bufferSystem.CreateCommandBuffer ().ToConcurrent ();
        var addMassJob = new AddMassJob ();
        addMassJob.concurrent = concurrent;
        inputDeps = addMassJob.Schedule (nonMassQuery, inputDeps);
        var addMoverJob = new AddMoverJob ();
        addMoverJob.concurrent = concurrent;
        inputDeps = addMoverJob.Schedule (nonMoverQuery, inputDeps);
        var addScaleJob = new AddScaleJob ();
        addScaleJob.concurrent = concurrent;
        inputDeps = addScaleJob.Schedule (nonScaleQuery, inputDeps);
        return inputDeps;
    }
}