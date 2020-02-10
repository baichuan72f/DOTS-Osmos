using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

//加速度定律(加速度结算)
public class SimpleForce_S2 : JobComponentSystem {

    EntityQuery entityGroup;

    [BurstCompile]
    struct moverChangeJob : IJobChunk {
        public float deltaTime;
        public ArchetypeChunkBufferType<Force_C> BufferType;
        public ArchetypeChunkComponentType<Mover_C> moverType;
        [ReadOnly] public ArchetypeChunkComponentType<MassPoint_C> massType;
        public void Execute (ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
            BufferAccessor<Force_C> buffers = chunk.GetBufferAccessor (BufferType);
            NativeArray<Mover_C> movers = chunk.GetNativeArray<Mover_C> (moverType);
            NativeArray<MassPoint_C> masses = chunk.GetNativeArray<MassPoint_C> (massType);

            for (int i = 0; i < chunk.Count; i++) {
                Mover_C mover = movers[i];
                MassPoint_C mass = masses[i];
                DynamicBuffer<Force_C> forces = buffers[i];
                float3 joinForce = float3.zero;
                // 计算受力对于速度的改变
                for (int j = 0; j < forces.Length; j++) {
                    var force = forces[j];
                    // 持续时间已过
                    if (force.direction.w <= 0) {
                        return;
                    }
                    // 当前计算的施力时间
                    var time = deltaTime;
                    // 间隔时间长于施力时间
                    if (deltaTime > force.direction.w) time = force.direction.w;
                    joinForce += new float3 (force.direction.x, force.direction.y, force.direction.z) * time;
                    // 施力时间减少
                    force.direction.w -= deltaTime;
                    forces[j] = force;
                }
                movers[i] = new Mover_C () {
                    direction = mover.direction + joinForce / mass.Mass
                };
            }
        }
    }
    protected override void OnCreate () {
        EntityQueryDesc queryDescription = new EntityQueryDesc ();
        queryDescription.All = new [] { ComponentType.ReadWrite<Force_C> (), ComponentType.ReadWrite<Mover_C> (), ComponentType.ReadOnly<MassPoint_C> () };
        entityGroup = GetEntityQuery (queryDescription);
    }

    protected override JobHandle OnUpdate (JobHandle inputDeps) {
        // 计算准备
        float deltaTime = Time.DeltaTime;

        // 准备句柄
        //Schedule the second job, which depends on the first
        moverChangeJob bufferJob = new moverChangeJob ();
        bufferJob.BufferType = GetArchetypeChunkBufferType<Force_C> ();
        bufferJob.moverType = GetArchetypeChunkComponentType<Mover_C> ();
        bufferJob.massType = GetArchetypeChunkComponentType<MassPoint_C> ();
        bufferJob.deltaTime = Time.DeltaTime;
        JobHandle mover_handle = bufferJob.Schedule (entityGroup, inputDeps);

        //返回句柄
        return mover_handle;
    }
}