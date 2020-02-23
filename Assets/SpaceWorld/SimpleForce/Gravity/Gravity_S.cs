using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
public class Gravity_S : JobComponentSystem {
    //实体组
    EntityQuery SenderGroup;
    EntityQuery ReceiverGroup;

    EntityCommandBufferSystem commandBufferSystem;

    [BurstCompile]
    struct AddGravityHandle : IJobForEachWithEntity<MassPoint_C, Translation> {

        [ReadOnly] public double G;
        public EntityCommandBuffer.Concurrent concurrent;
        [ReadOnly] public NativeArray<Entity> fromEntities;

        //gravityReceivers 重力发送者
        [ReadOnly] public NativeArray<GravtiySender_C> gravtiySenders;
        [ReadOnly] public NativeArray<Translation> sendertranslations;

        public void Execute (Entity entity, int index, [ReadOnly] ref MassPoint_C mass, [ReadOnly] ref Translation translation) {
            for (int j = 0; j < gravtiySenders.Length; j++) {
                var same = sendertranslations[j].Value == translation.Value;
                if (same.x && same.y && same.z) continue;
                double3 dir = math.normalize (sendertranslations[j].Value - translation.Value);
                if (double.IsNaN (dir.x) || double.IsNaN (dir.y) || double.IsNaN (dir.z)) continue;
                double distance = math.distance (sendertranslations[j].Value, translation.Value);
                if (distance < 1) distance = 1;
                double power = mass.Mass * gravtiySenders[j].GravityMass * G * (1f / distance);
                // 添加受力情况
                Force_C force = new Force_C ();
                force.value = power * dir;
                force.type = ForceType.Gravity;
                force.time = double.MaxValue;
                force.from = fromEntities[j];
                force.to = entity;
                concurrent.AddComponent (index, concurrent.CreateEntity (index), force);
            }
        }
    }
    protected override void OnCreate () {
        commandBufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem> ();
        //接受者
        EntityQueryDesc receiverDescription = new EntityQueryDesc ();
        receiverDescription.All = new [] { ComponentType.ReadOnly<Translation> (), ComponentType.ReadOnly<MassPoint_C> () };
        ReceiverGroup = GetEntityQuery (receiverDescription);
        //发送者
        EntityQueryDesc senderDescription = new EntityQueryDesc ();
        senderDescription.All = new [] { ComponentType.ReadOnly<GravtiySender_C> (), ComponentType.ReadOnly<Translation> () };
        SenderGroup = GetEntityQuery (senderDescription);
    }
    protected override JobHandle OnUpdate (JobHandle inputDeps) {
        // 准备句柄
        AddGravityHandle bufferJob = new AddGravityHandle ();

        //发送者
        var senderArr = SenderGroup.ToEntityArray (Allocator.Persistent);
        NativeArray<Translation> Sendertranslations = new NativeArray<Translation> (senderArr.Length, Allocator.Persistent);
        NativeArray<GravtiySender_C> senders = new NativeArray<GravtiySender_C> (senderArr.Length, Allocator.Persistent);
        for (int i = 0; i < senderArr.Length; i++) {
            Sendertranslations[i] = EntityManager.GetComponentData<Translation> (senderArr[i]);
            senders[i] = EntityManager.GetComponentData<GravtiySender_C> (senderArr[i]);
        }

        //赋值任务
        bufferJob.concurrent = commandBufferSystem.CreateCommandBuffer ().ToConcurrent ();
        bufferJob.gravtiySenders = senders;
        bufferJob.fromEntities = senderArr;
        //bufferJob.toEntities = recevierArr;
        bufferJob.sendertranslations = Sendertranslations;
        //bufferJob.masses = masses;
        //bufferJob.receiverTranslations = translations;
        bufferJob.G = 6.67259f * math.pow (10, -11) * math.pow (10, 6);

        //计算与释放        
        // Native arrays must be disposed manually.执行计算任务
        inputDeps = bufferJob.Schedule (this, inputDeps);
        inputDeps.Complete ();

        //释放计算任务所需
        senderArr.Dispose (inputDeps);
        //recevierArr.Dispose (inputDeps);
        senders.Dispose ();
        Sendertranslations.Dispose ();
        //bufferJob.masses.Dispose ();
        //bufferJob.receiverTranslations.Dispose ();

        //返回句柄
        return inputDeps;
    }
}