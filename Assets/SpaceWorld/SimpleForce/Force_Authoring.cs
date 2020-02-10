using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class Force_Authoring : MonoBehaviour, IConvertGameObjectToEntity {
    public float4 direction_default = float4.zero;
    public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        var data = new Force_C () {
            direction = direction_default
        };
        dstManager.AddBuffer<Force_C> (entity).Add (data);
    }
}

//物体可以移动的组件标签
public struct Force_C : IBufferElementData {
    //作用力的大小和持续时间(x,y,z,w)
    public float4 direction;
    public ForceType type;
}
public enum ForceType {
    external,
    Gravity
}