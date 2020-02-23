using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class SimapleForceSender_S : JobComponentSystem {
    EntityCommandBufferSystem bufferSystem;
    struct senderJob : IJobForEachWithEntity<SimapleForceSender_C> {
        public EntityCommandBuffer.Concurrent concurrent;
        public double deltime;
        public void Execute (Entity entity, int index, ref SimapleForceSender_C sender) {
            double t = deltime;
            if (sender.time < t) t = sender.time;
            sender.time -= t;

            if (sender.time <= 0) {
                concurrent.RemoveComponent (index, entity, typeof (SimapleForceSender_C));
                concurrent.DestroyEntity (index, entity);
                return;
            }
            Force_C f = new Force_C ();
            f.from = entity;
            f.time = t;
            f.to = sender.to;
            f.value = sender.value;
            f.type = sender.type;
            var forceClone = concurrent.CreateEntity (index);
            concurrent.AddComponent (index, forceClone, f);
            //var senderClone = concurrent.Instantiate (index, entity);
            //concurrent.SetComponent (index, senderClone, sender);
            //concurrent.DestroyEntity (index, entity);
        }
    }
    protected override void OnCreate () {
        bufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem> ();
    }
    protected override JobHandle OnUpdate (JobHandle inputDeps) {
        var deltime = Time.DeltaTime;
        var senderJob = new senderJob ();
        senderJob.concurrent = bufferSystem.CreateCommandBuffer ().ToConcurrent ();
        senderJob.deltime = Time.DeltaTime;
        inputDeps = senderJob.Schedule (this, inputDeps);
        return inputDeps;
    }
}