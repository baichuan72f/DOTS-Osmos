using Unity.Entities;

public struct ViewEntity : IComponentData {
    public int ViewIndex;
    public Entity entity;
}