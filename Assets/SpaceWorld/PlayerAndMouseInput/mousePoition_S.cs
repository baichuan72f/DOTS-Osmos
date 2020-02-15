using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class mousePoition_S : JobComponentSystem {

    [BurstCompile]
    struct MouseMoveJob : IJobParallelFor {
        public float time;
        [ReadOnly] public NativeArray<Forcer> forcers;
        [ReadOnly] public NativeArray<player_C> players;
        [ReadOnly] public NativeArray<Translation> translations;
        [ReadOnly] public NativeArray<Mouseposition_C> mousepositions;
        public void Execute (int index) {
            var mouseposition = mousepositions[index];
            for (int j = 0; j < players.Length; j++) {
                //筛选实体
                var player = players[j];
                if (player.Index != mouseposition.index) continue;
                //UnityEngine.Debug.Log ("PlayerIndex: " + player.Index);
                var translation = translations[j];

                // 添加受力情况
                var force = new Force_C ();
                force.type = ForceType.external;
                force.direction = new float4 (math.normalizesafe (mouseposition.value - translation.Value), time);
                UnityEngine.Debug.Log (force.direction);
                forcers[j].AddForce (force);
            }
        }
    }

    protected override JobHandle OnUpdate (JobHandle inputDeps) {
        //输入实体准备
        var inDesc = new EntityQueryDesc ();
        inDesc.All = new ComponentType[] { ComponentType.ReadOnly (typeof (Mouseposition_C)) };
        var inEntitites = GetEntityQuery (inDesc).ToEntityArray (Allocator.Persistent);
        var mousepositions = new NativeArray<Mouseposition_C> (inEntitites.Length, Allocator.Persistent);
        for (int i = 0; i < inEntitites.Length; i++) {
            mousepositions[i] = EntityManager.GetComponentData<Mouseposition_C> (inEntitites[i]);
        }

        //输出实体准备
        var outDect = new EntityQueryDesc ();
        outDect.All = new ComponentType[] {
            ComponentType.ReadOnly (typeof (player_C)),
            ComponentType.ReadOnly (typeof (Mover_C)),
            ComponentType.ReadOnly (typeof (Translation))
        };
        var outEntitites = GetEntityQuery (outDect).ToEntityArray (Allocator.Persistent);
        var playerTranslations = new NativeArray<Translation> (outEntitites.Length, Allocator.Persistent);
        var players = new NativeArray<player_C> (outEntitites.Length, Allocator.Persistent);
        for (int i = 0; i < outEntitites.Length; i++) {
            players[i] = EntityManager.GetComponentData<player_C> (outEntitites[i]);
            playerTranslations[i] = EntityManager.GetComponentData<Translation> (outEntitites[i]);
        }

        //赋值任务并开始计算
        MouseMoveJob job = new MouseMoveJob ();
        job.mousepositions = mousepositions;
        job.players = players;
        job.time = 1.5f;
        job.translations = playerTranslations;
        inputDeps = job.Schedule (mousepositions.Length, 64, inputDeps);

        //释放资源
        inputDeps.Complete ();
        EntityManager.DestroyEntity (inEntitites);
        inEntitites.Dispose (inputDeps);
        outEntitites.Dispose (inputDeps);
        mousepositions.Dispose (inputDeps);
        players.Dispose (inputDeps);
        playerTranslations.Dispose (inputDeps);
        return inputDeps;
    }
}