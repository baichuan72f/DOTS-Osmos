using Unity.Entities;

[GenerateAuthoringComponent]
public struct GravtiySender_C : IComponentData
{
    //重力发出者的模拟质量
    public float GravityMass;
}
