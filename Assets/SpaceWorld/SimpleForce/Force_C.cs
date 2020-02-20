using Unity.Entities;
using Unity.Mathematics;

public struct Force_C : IComponentData {
    public float3 value;
    public float time;
    public ForceType type;
    public Entity from; //施力点
    public Entity to; //受力点
}

//力的类型
public enum ForceType {
    external, //外力
    Gravity //重力
}