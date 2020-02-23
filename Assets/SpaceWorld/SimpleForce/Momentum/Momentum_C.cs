using Unity.Entities;
using Unity.Mathematics;

public struct Momentum_C : IComponentData {
    //质量
    public double mass;
    //速度
    public double3 speed;
    ////动量增加给的mover
    //public Mover_C mover;
    // mover所在实体
    public Entity target;
}