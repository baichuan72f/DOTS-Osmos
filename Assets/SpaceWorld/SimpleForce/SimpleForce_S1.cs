using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

//物体按照速度移动
[UpdateInGroup(typeof(TransformSystemGroup))]
public class SimpleForce_S1 : JobComponentSystem
{
    bool3 fixation = new bool3(false, false, true);
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        //计算准备
        var deltaTime = math.min(0.05, Time.DeltaTime);
        var fixXYZ = this.fixation;
        //准备句柄
        inputDeps = Entities
                    //筛选
                    .WithName("BlockerSystem")
                    .ForEach((ref Mover_C mover, in Blocker_C blocker) =>
                    {
                        var direction = mover.direction;
                        if (blocker.x) direction.x = 0;
                        if (blocker.y) direction.y = 0;
                        if (blocker.z) direction.z = 0;
                        mover.direction = direction;
                    }).Schedule(inputDeps);

        inputDeps = Entities
         //筛选
         .WithName("SimpleForce1System")
         .ForEach((ref Translation translation, in Mover_C mover) =>
         {
             translation.Value += (float3)(deltaTime * mover.direction);
             var tran = translation.Value;
             if (fixXYZ.x) tran.x = 0;
             if (fixXYZ.y) tran.y = 0;
             if (fixXYZ.z) tran.z = 0;
             translation.Value = tran;
         }).Schedule(inputDeps);


        //返回句柄
        return inputDeps;
    }
}