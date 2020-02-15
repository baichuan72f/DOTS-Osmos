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

    EntityQuery entityGroup;

    [BurstCompile]
    struct moverChangeJob : IJobForEach<Forcer, Mover_C, MassPoint_C> {
        public float deltaTime;
        public void Execute (ref Forcer forcePoint, ref Mover_C mover, ref MassPoint_C mass) {
            var forces = forcePoint.GetForces();
            float3 joinForce = float3.zero;
            // 计算受力对于速度的改变
            for (int j = 0; j < forces.Length; j++) {
                // 筛选还在持续的力
                var force = forces[j];
                if (force.direction.w <= 0 || force.type == ForceType.Gravity) {
                    forcePoint.RemoveForce (j);
                    continue;
                }

                // 施力时间不足一帧
                var time = deltaTime;
                if (deltaTime > force.direction.w) time = force.direction.w;
                joinForce += new float3 (force.direction.x, force.direction.y, force.direction.z) * time;

                // 施力时间减少
                force.direction.w -= deltaTime;
                forces[j] = force;
                forcePoint.ReplaceForce(j,force);
            }

            mover = new Mover_C () {
                direction = mover.direction + joinForce / mass.Mass
            };

        }
    }
    protected override void OnCreate () {
        EntityQueryDesc queryDescription = new EntityQueryDesc ();
        queryDescription.All = new [] { ComponentType.ReadWrite<Forcer> (), ComponentType.ReadWrite<Mover_C> (), ComponentType.ReadOnly<MassPoint_C> () };
        entityGroup = GetEntityQuery (queryDescription);
    }

    protected override JobHandle OnUpdate (JobHandle inputDeps) {
        // 计算准备
        float deltaTime = Time.DeltaTime;

        // 准备句柄
        moverChangeJob bufferJob = new moverChangeJob ();
        bufferJob.deltaTime = Time.DeltaTime;
        JobHandle mover_handle = bufferJob.Schedule (entityGroup, inputDeps);

        //返回句柄
        return mover_handle;
    }
}