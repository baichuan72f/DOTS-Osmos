using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class mousePoition_S : JobComponentSystem {
    EntityCommandBufferSystem bufferSystem;
    // [BurstCompile]
    // struct MouseMoveJob : IJobParallelFor {
    //     public EntityCommandBuffer.Concurrent concurrent;
    //     [ReadOnly] public NativeArray<Entity> outEntitites;
    //     [ReadOnly] public NativeArray<player_C> players;
    //     [ReadOnly] public NativeArray<Translation> translations;
    //     [ReadOnly] public NativeArray<Mouseposition_C> mousepositions;

    //     public void Execute (int index) {
    //         var mouseposition = mousepositions[index];
    //         for (int j = 0; j < players.Length; j++) {
    //             //筛选实体
    //             var player = players[j];
    //             if (player.Index != mouseposition.index) continue;
    //             var translation = translations[j];

    //             // 添加受力情况
    //             var entity = concurrent.CreateEntity (index);
    //             var force = new SimapleForceSender_C ();
    //             force.type = ForceType.external;
    //             force.time = 2;
    //             force.value = math.normalizesafe (translation.Value - mouseposition.value);
    //             force.to = outEntitites[index];
    //             concurrent.AddComponent (index, entity, force);
    //         }
    //     }
    // }
    EntityQuery inEntitites;
    EntityQuery outEntitites;
    protected override void OnCreate () {
        bufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem> ();
        var inDesc = new EntityQueryDesc ();
        inDesc.All = new ComponentType[] {
            ComponentType.ReadOnly (typeof (Mouseposition_C))
        };
        inEntitites = GetEntityQuery (inDesc);
        var outDect = new EntityQueryDesc ();
        outDect.All = new ComponentType[] {
            ComponentType.ReadOnly (typeof (player_C)),
            ComponentType.ReadOnly (typeof (Translation))
        };
        outEntitites = GetEntityQuery (outDect);
    }

    protected override JobHandle OnUpdate (JobHandle inputDeps) {
        //输入实体准备
        var inArr = inEntitites.ToEntityArray (Allocator.Persistent);
        //输出实体准备
        var outArr = outEntitites.ToEntityArray (Allocator.Persistent);
        for (int i = 0; i < inArr.Length; i++) {
            var mouseposition = EntityManager.GetComponentData<Mouseposition_C> (inArr[i]);
            for (int j = 0; j < outArr.Length; j++) {
                Entity entity = outArr[j];
                var player = EntityManager.GetComponentData<player_C> (entity);
                if (player.Index != mouseposition.index) continue;
                var translation = EntityManager.GetComponentData<Translation> (entity);
                var direction = math.normalize (mouseposition.value - translation.Value);
                if (math.isnan (direction).x || math.isnan (direction).y || math.isnan (direction).z) {
                    continue;
                }
                Spliter_C spliter = new Spliter_C ();
                spliter.direction = direction;
                spliter.volume = 0.2f;
                EntityManager.AddComponentData<Spliter_C> (entity, spliter);
            }
        }
        EntityManager.DestroyEntity (inEntitites);
        outArr.Dispose (inputDeps);
        inArr.Dispose (inputDeps);
        return inputDeps;
    }
}