using Unity.Entities;

public struct ViewEntity : IComponentData
{
    public string name;
    public Entity entity;
}