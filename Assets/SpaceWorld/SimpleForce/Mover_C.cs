using Unity.Entities;
using Unity.Mathematics;

//物体可以移动的组件标签
[GenerateAuthoringComponent]
public struct Mover_C : IComponentData {
    //速度的方向和大小
    public double3 direction;
}

