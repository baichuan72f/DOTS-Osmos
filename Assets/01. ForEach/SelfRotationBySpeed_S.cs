using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
public class SelfRotationBySpeed_S : JobComponentSystem {
    //获取并返回任务句柄中的数据
    protected override JobHandle OnUpdate (JobHandle inputDeps) {
        //任务准备
        float deltaTime = Time.DeltaTime;
        //新建任务句柄
        var handle = Entities
            //筛选实体
            .WithName ("SelfRotationBySpeed_S")
            //执行计算
            .ForEach ((ref Rotation rotation, in RotationSpeed_C rotationSpeed) => {
                rotation.Value = math.mul (
                    math.normalize (rotation.Value),
                    quaternion.AxisAngle (math.up (), rotationSpeed.value * deltaTime)
                );
                //挂起任务句柄
            }).Schedule (inputDeps);
        //返回任务句柄
        return handle;
    }
}