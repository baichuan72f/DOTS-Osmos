using Unity.Entities;
using Unity.Mathematics;

//禁止物体位移的组件
[GenerateAuthoringComponent]
public struct Blocker_C : IComponentData {
    //速度的方向和大小
    public bool x;
    public bool y;
    public bool z;
}