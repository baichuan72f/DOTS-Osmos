using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

//物体按照速度移动
[UpdateInGroup(typeof(TransformSystemGroup))]
public class SimpleForce_S1 : JobComponentSystem {

    protected override JobHandle OnUpdate (JobHandle inputDeps) {
        //计算准备
        float deltaTime = Time.DeltaTime;

        //准备句柄
        var handle = Entities
            //筛选
            .WithName ("SimpleForce1System")
            .ForEach ((ref Translation translation, in Mover_C mover) => {
                translation.Value += deltaTime * mover.direction;
                translation.Value.z = 0;
            }).Schedule (inputDeps);
        //返回句柄
        return handle;
    }
}