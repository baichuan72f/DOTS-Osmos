using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter (typeof (Join_S))]
public class StarView_S : JobComponentSystem {
    EntityCommandBufferSystem bufferSystem;
    EntityQuery playerQuery;
    EntityQuery joinerQuery;
    Dictionary<double, Material> materials;
    Dictionary<double, RenderMesh> renders;
    protected override void OnCreate () {
        bufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem> ();
        EntityQueryDesc PlayerDesc = new EntityQueryDesc ();
        PlayerDesc.All = new ComponentType[] { typeof (player_C), typeof (Joiner_C) };
        playerQuery = GetEntityQuery (PlayerDesc);
        joinerQuery = GetEntityQuery (new ComponentType[] { typeof (Joiner_C) });
    }
    protected override JobHandle OnUpdate (JobHandle inputDeps) {
        EntityCommandBuffer.Concurrent concurrent = bufferSystem.CreateCommandBuffer ().ToConcurrent ();
        // int playerIndex准备(取第一个Index为准)
        NativeHashMap<int, Mouseposition_C> indexes = new NativeHashMap<int, Mouseposition_C> (1, Allocator.TempJob);
        var indexWriter = indexes.AsParallelWriter ();
        var playerIndexHandle = Entities.WithName ("PlayerIndexHandle")
            .ForEach ((int entityInQueryIndex, in Mouseposition_C inputer) => {
                indexWriter.TryAdd (1, inputer);
            }).Schedule (inputDeps);

        //players准备
        NativeArray<Entity> tmpPlayers = playerQuery.ToEntityArray (Allocator.TempJob);
        NativeHashMap<int, Joiner_C> players = new NativeHashMap<int, Joiner_C> (tmpPlayers.Length, Allocator.TempJob);
        var plyaerWriter = players.AsParallelWriter ();
        var playersHandle = Entities.WithName ("playersHandle")
            .ForEach ((int entityInQueryIndex, in player_C player, in Joiner_C joiner) => {
                plyaerWriter.TryAdd (player.index, joiner);
            }).Schedule (inputDeps);

        inputDeps = JobHandle.CombineDependencies (playerIndexHandle, playersHandle);

        inputDeps.Complete ();
        var joiners = joinerQuery.ToEntityArray (Allocator.TempJob);
        Joiner_C playerJoiner;
        players.TryGetValue (1, out playerJoiner); // 暂时取1，默认玩家索引为1
        if (materials == null) materials = new Dictionary<double, Material> ();
        if (renders == null) renders = new Dictionary<double, RenderMesh> ();

        //显示View
        for (int i = 0; i < joiners.Length; i++) {
            Joiner_C joiner = EntityManager.GetComponentData<Joiner_C> (joiners[i]);
            var danger = math.round ((10 * joiner.volume / playerJoiner.volume)) * 0.05;
            if (!materials.ContainsKey (danger)) {
                var render = EntityManager.GetSharedComponentData<RenderMesh> (joiners[i]);
                materials.Add (danger, GameObject.Instantiate<Material> (render.material));
                materials[danger].SetFloat ("FreeTime", (float) danger);
                render.material = materials[danger];
                renders.Add (danger, render);
                Debug.Log (danger);
                
            }
            EntityManager.AddSharedComponentData (joiners[i], renders[danger]);
        }
        joiners.Dispose (inputDeps);
        tmpPlayers.Dispose (inputDeps);
        players.Dispose (inputDeps);
        indexes.Dispose (inputDeps);
        return inputDeps;
    }
}