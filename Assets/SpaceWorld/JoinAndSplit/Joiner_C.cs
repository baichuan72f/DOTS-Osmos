using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
//物体可以分离的组件标签
[GenerateAuthoringComponent]
public struct Joiner_C : IComponentData {
    //当前星体体积
    public float volume;
}

public struct Spliter_C : IComponentData {
    public float3 direction;
    public float volume;
}