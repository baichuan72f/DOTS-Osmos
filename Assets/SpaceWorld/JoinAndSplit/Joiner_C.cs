using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
//物体可以分离的组件标签
[GenerateAuthoringComponent]
public struct Joiner_C : IComponentData {
    //当前星体体积
    public double volume;
}

public struct JoinerView_C : IComponentData {
    public Joiner_C joiner;
    public MassPoint_C mass;
    public NonUniformScale nonUniformScale;
}

[MaterialProperty ("_FeelTime", MaterialPropertyFormat.Float)]
public struct ColorLevel : IComponentData {
    public float Value;
}