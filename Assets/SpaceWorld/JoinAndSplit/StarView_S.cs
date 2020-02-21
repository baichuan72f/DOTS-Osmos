using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
public class StarView_S : JobComponentSystem {

    struct StarViewJob : IJobForEachWithEntity<Joiner_C, NonUniformScale> {
        EntityCommandBuffer.Concurrent concurrent;

        public void Execute (Entity entity, int index, ref Joiner_C c0, ref NonUniformScale c1) {
            throw new System.NotImplementedException ();
        }
    }

    protected override JobHandle OnUpdate (JobHandle inputDeps) {
        return inputDeps;
    }
}