using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

[UpdateBefore (typeof (Join_S))]
public class AddJoinNeed_S : JobComponentSystem {
    EntityCommandBufferSystem bufferSystem;
    //public static NativeHashMap<double, RenderMesh> renderDic;

    struct AddCommponentJob : IJobForEachWithEntity<Joiner_C> {
        public EntityCommandBuffer.Concurrent concurrent;

        public void Execute (Entity entity, int index, ref Joiner_C joiner) {
            //添加质量组件
            MassPoint_C mass = new MassPoint_C ();
            mass.densityType = density.water;
            mass.Mass = UnitHelper.Volume2Mass (mass.densityType, joiner.volume);
            concurrent.AddComponent (index, entity, mass);
            //添加缩放组件
            NonUniformScale scale = new NonUniformScale ();
            scale.Value = (float3) (new double3 (1, 1, 1) * 2 * UnitHelper.Volume2Range (joiner.volume));
            concurrent.AddComponent (index, entity, scale);
            //添加缩放组件
            ColorLevel color = new ColorLevel ();
            color.Value = 0.5f;
            concurrent.AddComponent (index, entity, color);
        }
    }
    struct AddMoverJob : IJobForEachWithEntity<Joiner_C> {
        public EntityCommandBuffer.Concurrent concurrent;
        public void Execute (Entity entity, int index, ref Joiner_C joiner) {
            //添加移动组件(mover会记录实体信息，AddComponent会覆盖原有组件,所以只能给没有mover的实体单独添加)
            Mover_C mover = new Mover_C ();
            concurrent.AddComponent (index, entity, mover);
        }
    }
    struct AddViewJob : IJobForEachWithEntity<Joiner_C, MassPoint_C, NonUniformScale> {
        public EntityCommandBuffer.Concurrent concurrent;
        public void Execute (Entity entity, int index, ref Joiner_C joiner, ref MassPoint_C mass, ref NonUniformScale scale) {
            JoinerView_C view_C = new JoinerView_C ();
            view_C.joiner = joiner;
            view_C.mass = mass;
            view_C.nonUniformScale = scale;

            concurrent.AddComponent (index, entity, view_C);
        }
    }
    EntityQuery nonMassQuery, nonMoverQuery, nonScaleQuery, nonViewQuery, nonColorQuery;
    protected override void OnCreate () {
        bufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem> ();
        //待添加质量组件
        EntityQueryDesc addMass = new EntityQueryDesc ();
        addMass.All = new ComponentType[] { typeof (Joiner_C) };
        addMass.None = new ComponentType[] { typeof (MassPoint_C) };
        nonMassQuery = GetEntityQuery (addMass);
        //待添加缩放组件
        EntityQueryDesc addScale = new EntityQueryDesc ();
        addScale.All = new ComponentType[] { typeof (Joiner_C) };
        addScale.None = new ComponentType[] { typeof (NonUniformScale) };
        nonScaleQuery = GetEntityQuery (addScale);
        // 待添加颜色组件
        EntityQueryDesc addColor = new EntityQueryDesc ();
        addColor.All = new ComponentType[] { typeof (Joiner_C) };
        addColor.None = new ComponentType[] { typeof (NonUniformScale) };
        nonColorQuery = GetEntityQuery (addColor);
        //待添加移动组件
        EntityQueryDesc addMover = new EntityQueryDesc ();
        addMover.All = new ComponentType[] { typeof (Joiner_C) };
        addMover.None = new ComponentType[] { typeof (Mover_C) };
        nonMoverQuery = GetEntityQuery (addMover);
        EntityQueryDesc addView = new EntityQueryDesc ();
        addView.All = new ComponentType[] { typeof (Joiner_C), typeof (MassPoint_C), typeof (NonUniformScale) };
        //addView.None = new ComponentType[] { typeof (JoinerView_C) };
        nonViewQuery = GetEntityQuery (addView);
    }
    protected override JobHandle OnUpdate (JobHandle inputDeps) {
        var concurrent = bufferSystem.CreateCommandBuffer ().ToConcurrent ();
        //添加View所需组件
        var addJob1 = new AddCommponentJob ();
        addJob1.concurrent = concurrent;
        var addMassHandle = addJob1.Schedule (nonMassQuery, inputDeps);

        var addJob2 = new AddCommponentJob ();
        addJob2.concurrent = concurrent;
        var addScaleHandle = addJob2.Schedule (nonScaleQuery, addMassHandle);
        var addJob3 = new AddCommponentJob ();
        addJob3.concurrent = concurrent;
        var addColorHandle = addJob3.Schedule (nonColorQuery, addScaleHandle);
        //添加mover
        var addMoverJob = new AddMoverJob ();
        addMoverJob.concurrent = concurrent;
        inputDeps = addMoverJob.Schedule (nonMoverQuery, addColorHandle);
        //NativeArray<Entity> viewes = nonViewQuery.ToEntityArray (Allocator.TempJob);
        //if (!renderDic.IsCreated && viewes.Length != 0) {
        //    renderDic = new NativeHashMap<double, RenderMesh> (40, Allocator.Persistent);
        //    Entity entity = viewes[0];
        //    for (double i = 0; i <= 2; i += 0.05) {
        //        renderDic.Add (i, EntityManager.GetSharedComponentData<RenderMesh> (entity));
        //    }
        //}
        //viewes.Dispose (inputDeps);
        return inputDeps;
    }
}