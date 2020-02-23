using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class mousePoition_S : JobComponentSystem {
    public float splitVolume = 1f;
    EntityCommandBufferSystem bufferSystem;
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
                if (player.index != mouseposition.index) continue;
                var translation = EntityManager.GetComponentData<Translation> (entity);
                var direction = math.normalize (mouseposition.value - translation.Value);
                if (math.isnan (direction).x || math.isnan (direction).y || math.isnan (direction).z) {
                    continue;
                }
                Spliter_C spliter = new Spliter_C ();
                spliter.direction = direction;
                spliter.volume = splitVolume;
                EntityManager.AddComponentData<Spliter_C> (entity, spliter);
            }
        }
        EntityManager.DestroyEntity (inEntitites);
        outArr.Dispose (inputDeps);
        inArr.Dispose (inputDeps);
        return inputDeps;
    }
}