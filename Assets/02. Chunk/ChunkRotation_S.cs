using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class ChunkRotation_S : JobComponentSystem {
    EntityQuery entityGroup;
    [BurstCompile]
    struct chunkRotationHandle : IJobChunk {
        public float deltaTime;
        public ArchetypeChunkComponentType<Rotation> rotationType;
        [ReadOnly] public ArchetypeChunkComponentType<ChunkRotation_SpeedC> speedType;
        public void Execute (ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
            var rotations = chunk.GetNativeArray<Rotation> (rotationType);
            var speeds = chunk.GetNativeArray<ChunkRotation_SpeedC> (speedType);
            for (int i = 0; i < chunk.Count; i++) {
                var rotation = rotations[i];
                var speed = speeds[i];
                rotations[i] = new Rotation () {
                    Value = math.mul (
                    math.normalize (rotation.Value),
                    quaternion.AxisAngle (math.up (), speed.RadiansPerSecond * deltaTime))
                };
            }
        }
    }
    protected override void OnCreate () {
        entityGroup = GetEntityQuery (typeof (Rotation), ComponentType.ReadOnly (typeof (ChunkRotation_SpeedC)));
    }
    protected override JobHandle OnUpdate (JobHandle inputDeps) {
        var rotationType = GetArchetypeChunkComponentType<Rotation> ();
        var speedType = GetArchetypeChunkComponentType<ChunkRotation_SpeedC> ();
        var deltaTime = Time.DeltaTime;
        var handle = new chunkRotationHandle () {
            rotationType = rotationType,
            speedType = speedType,
            deltaTime = deltaTime
        };
        return handle.Schedule (entityGroup);
    }
}