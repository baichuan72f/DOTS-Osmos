using Unity.Entities;
using Unity.Mathematics;

public struct SimapleForceSender_C : IComponentData {
    public double time;
    public Entity to;//受力点所属
    public double3 value; //均匀力大小
    public ForceType type;//施力类型
}
