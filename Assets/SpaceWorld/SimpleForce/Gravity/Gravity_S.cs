using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
public class Gravity_S : JobComponentSystem {
    //实体组
    EntityQuery entityGroup;
    BeginSimulationEntityCommandBufferSystem beginSimulation;
    [BurstCompile]
    struct AddGravityHandle : IJobForEachWithEntity<MassPoint_C, Translation> {
        [ReadOnly] public float G;
        [ReadOnly] public NativeArray<Translation> translations;
        [ReadOnly] public NativeArray<MassPoint_C> masses;
        public EntityCommandBuffer.Concurrent CommandBuffer;
        public void Execute (Entity entity, int index, [ReadOnly] ref MassPoint_C mass, [ReadOnly] ref Translation translation) {

            float3 vector = float3.zero;
            for (int j = 0; j < masses.Length && j < translations.Length; j++) {
                if (j == index) continue;
                float3 dir = math.normalizesafe (translations[j].Value - translation.Value);
                if (float.IsNaN (dir.x) || float.IsNaN (dir.y) || float.IsNaN (dir.z)) continue;
                float distance = math.distance (translations[j].Value, translation.Value);
                if (distance < 1) distance = 1;
                float power = mass.Mass * masses[j].Mass * G * (math.pow (1 / distance, 2));
                vector += power * dir;
            }
            Force_C f = new Force_C ();
            f.type = ForceType.Gravity;
            f.direction = new float4 (vector, 100);
            var forces = CommandBuffer.AddBuffer<Force_C> (index, entity);
            forces.Add (f);
        }
    }
    struct hasMapStruct : IJobNativeMultiHashMapMergedSharedKeyIndices
    {
       public void ExecuteFirst(int index)
       {
           throw new System.NotImplementedException();
       }

       public void ExecuteNext(int firstIndex, int index)
       {
           throw new System.NotImplementedException();
       }
    }
    protected override void OnCreate () {
        EntityQueryDesc queryDescription = new EntityQueryDesc ();
        queryDescription.All = new [] { ComponentType.ReadOnly<MassPoint_C> (), ComponentType.ReadOnly<Translation> () };
        entityGroup = GetEntityQuery (queryDescription);

        beginSimulation = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem> ();
    }
    protected override JobHandle OnUpdate (JobHandle inputDeps) {
        // 计算准备

        // 准备句柄
        // 添加重力
        AddGravityHandle bufferJob = new AddGravityHandle ();
        bufferJob.CommandBuffer = beginSimulation.CreateCommandBuffer ().ToConcurrent ();
        var entityArr = entityGroup.ToEntityArray (Allocator.TempJob);

        NativeArray<MassPoint_C> masses = new NativeArray<MassPoint_C> (entityArr.Length, Allocator.TempJob);
        NativeArray<Translation> translations = new NativeArray<Translation> (entityArr.Length, Allocator.TempJob);
        for (int i = 0; i < entityArr.Length; i++) {
            masses[i] = EntityManager.GetComponentData<MassPoint_C> (entityArr[i]);
            translations[i] = EntityManager.GetComponentData<Translation> (entityArr[i]);
        }
        bufferJob.masses = masses;
        bufferJob.translations = translations;
        bufferJob.G = 6.67259f * math.pow (10, -11);
        JobHandle adderHandle = bufferJob.Schedule (entityGroup, inputDeps);

        ////计算重力
        //CompulteGravityHandle compulteJob = new CompulteGravityHandle ();
        //compulteJob.forceType = GetArchetypeChunkBufferType<Force_C> ();
        //compulteJob.massPointType = GetArchetypeChunkComponentType<MassPoint_C> ();
        //compulteJob.translationType = GetArchetypeChunkComponentType<Translation> ();
        //JobHandle compulteHandle = compulteJob.Schedule (entityGroup, adderHandle);
        //返回句柄
        return adderHandle;
    }
}