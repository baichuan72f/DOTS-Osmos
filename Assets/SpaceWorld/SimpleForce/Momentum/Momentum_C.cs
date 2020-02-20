using Unity.Entities;
using Unity.Mathematics;

public struct Momentum_C : IComponentData {
    public float mass;
    public float3 speed;
    public Mover_C mover;
    public Entity target;
}