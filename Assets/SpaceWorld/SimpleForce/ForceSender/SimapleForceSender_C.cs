using Unity.Entities;
using Unity.Mathematics;

public struct SimapleForceSender_C : IComponentData {
    public float time;
    public Entity to;//受力点所属
    public float3 value; //均匀力大小
    public ForceType type;//施力类型
}
