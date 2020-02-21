using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class Split_S : JobComponentSystem {
    public EntityCommandBufferSystem bufferSystem;

    struct SplitJob : IJobForEachWithEntity<Spliter_C, Translation, Joiner_C, Mover_C> {
        public Entity smallStar;
        public EntityCommandBuffer.Concurrent concurrent;
        public void Execute (Entity entity, int index, ref Spliter_C spliter, ref Translation translation, ref Joiner_C joiner, ref Mover_C mover) {
            //体积过小就无法分离子星体了
            if (joiner.volume < 3) {
                return;
            }
            //校验spliter数据合理性
            if (spliter.direction.Equals (float3.zero)) {
                return;
            }
            bool3 nanResult = math.isnan (math.normalize (spliter.direction));
            if (nanResult.x || nanResult.y || nanResult.z) {
                return;
            }
            // 根据split的方向分离小实体
            Entity subEntity = concurrent.Instantiate (index, smallStar);
            Joiner_C subJoiner = new Joiner_C () { volume = spliter.volume + 0.01f };
            MassPoint_C subMass = new MassPoint_C () { Mass = density.water.Volume2Mass (subJoiner.volume) };
            Mover_C subMover = new Mover_C ();
            Translation subTranslation = new Translation ();
            NonUniformScale subScale = new NonUniformScale ();
            var dirNormal = math.normalize (spliter.direction);
            var distance = UnitHelper.Volume2Range (spliter.volume) + UnitHelper.Volume2Range (joiner.volume);
            subTranslation.Value = translation.Value + dirNormal * distance;
            subScale.Value = 0.01f;
            concurrent.AddComponent (index, subEntity, subTranslation);
            concurrent.AddComponent (index, subEntity, subJoiner);
            concurrent.AddComponent (index, subEntity, subMass);
            concurrent.AddComponent (index, subEntity, subMover);
            concurrent.AddComponent (index, subEntity, subScale);

            //分离产生的力
            var spliterForce = math.normalize (spliter.direction) * subMass.Mass;

            // 添加子物体受力
            SimapleForceSender_C subForce = new SimapleForceSender_C () {
                value = -spliterForce*5,
                type = ForceType.external,
                to = subEntity,
                time = 0.3f
            };
            Entity fEntity = concurrent.CreateEntity (index);
            concurrent.AddComponent (index, fEntity, subForce);
            //添加子物体动量
            Momentum_C subMomentum = new Momentum_C () {
                mass = subMass.Mass,
                mover = subMover,
                speed = spliterForce * 4,
                target = subEntity
            };
            Entity mEntity = concurrent.CreateEntity (index);
            concurrent.AddComponent (index, mEntity, subMomentum);
            // 添加主物体动量
            Momentum_C momentum = new Momentum_C () {
                mass = subMass.Mass,
                mover = mover,
                speed = -spliterForce * 4,
                target = entity
            };
            Entity mainMEntity = concurrent.CreateEntity (index);
            concurrent.AddComponent (index, mainMEntity, momentum);

            // 添加主物体受力情况
            //SimapleForceSender_C mainForce = new SimapleForceSender_C () {
            //    value = -math.normalize (spliter.direction) * 2,
            //    type = ForceType.external,
            //    to = entity,
            //    time = 1f
            //};
            //Entity mainFEntity = concurrent.CreateEntity (index);
            //concurrent.AddComponent (index, mainFEntity, mainForce);
            // 减少朱物体mass的质量
            joiner.volume -= spliter.volume;
            concurrent.RemoveComponent (index, entity, typeof (Spliter_C));
        }
    }

    protected override void OnCreate () {
        bufferSystem = World.GetOrCreateSystem<BeginPresentationEntityCommandBufferSystem> ();
    }
    protected override JobHandle OnUpdate (JobHandle inputDeps) {
        // 计算准备
        var concurrent = bufferSystem.CreateCommandBuffer ().ToConcurrent ();
        // 新建句柄
        var splitJob = new SplitJob ();
        Entity entity;
        bool result = ViewFactory.ViewMap.TryGetValue ("Star".GetHashCode (), out entity);
        UnityEngine.Debug.Log ("Star".GetHashCode ());
        splitJob.smallStar = entity;
        splitJob.concurrent = concurrent;
        // 挂载句柄
        inputDeps = splitJob.Schedule (this, inputDeps);
        return inputDeps;
    }
}