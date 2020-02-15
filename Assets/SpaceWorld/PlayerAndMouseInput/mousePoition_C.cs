using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct Mouseposition_C : IComponentData
{
    //鼠标点击位置
    public float3 value;
    public int index;
}
