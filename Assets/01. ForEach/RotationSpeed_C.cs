using Unity.Entities;

//使组件可以自动挂载到游戏物体上
[GenerateAuthoringComponent]
public struct RotationSpeed_C : IComponentData
{
    public float value;
}
