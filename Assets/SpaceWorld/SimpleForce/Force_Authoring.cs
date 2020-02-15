using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class Force_Authoring : MonoBehaviour, IConvertGameObjectToEntity {
    public float4[] direction_default;
    public void Convert (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        //BlobBuilder builder = new BlobBuilder (Allocator.Temp);
        //ref var forces = ref builder.ConstructRoot<Forces> ();
        //var blobArray = builder.Allocate (ref forces.ForceArray, direction_default.Length);
        var blobArray = new Force_C[direction_default.Length];
        var forcer = new Forcer ();
        for (int i = 0; i < direction_default.Length; i++) {
            blobArray[i] = new Force_C () { direction = direction_default[i] };
            forcer.AddForce (blobArray[i]);
        }

        for (int i = 0; i < forcer.forces.Value.ForceArray.Length; i++) {
            Debug.Log (forcer.forces.Value.ForceArray[i].direction);
        }
        dstManager.AddComponentData (entity, forcer);
        //builder.Dispose ();
    }
}

public struct Forcer : IComponentData {
    public BlobAssetReference<Forces> forces;
    public void AddForce (Force_C force) {
        BlobBuilder builder = new BlobBuilder (Allocator.Temp);
        BlobBuilderArray<Force_C> builderArray;
        if (forces.IsCreated) {
            ref var f = ref builder.ConstructRoot<Forces> ();
            var arr = forces.Value.ForceArray.ToArray ();
            builderArray = builder.Allocate (ref f.ForceArray, arr.Length + 1);
            for (int i = 0; i < arr.Length; i++) {
                builderArray[i] = arr[i];
            }
        } else {
            ref var f = ref builder.ConstructRoot<Forces> ();
            builderArray = builder.Allocate (ref f.ForceArray, 1);
        }
        builderArray[builderArray.Length - 1] = force;
        forces = builder.CreateBlobAssetReference<Forces> (Allocator.Persistent);
    }
    public void RemoveForce (int index) {
        BlobBuilder builder = new BlobBuilder (Allocator.Persistent);
        Force_C[] fromArr = forces.Value.ForceArray.ToArray ();
        Force_C[] toArr = new Force_C[fromArr.Length - 1];
        Array.ConstrainedCopy (fromArr, 0, toArr, 0, index);
        Array.ConstrainedCopy (fromArr, index + 1, toArr, index, toArr.Length - index);
        var builderArray = builder.Allocate (ref forces.Value.ForceArray, forces.Value.ForceArray.Length - 1);
        for (int i = 0; i < builderArray.Length; i++) {
            builderArray[i] = toArr[i];
        }
    }
    public void ReplaceForce (int index, Force_C node) {
        BlobBuilder builder = new BlobBuilder (Allocator.Temp);
        ref var f = ref builder.ConstructRoot<Forces> ();
        var arr = forces.Value.ForceArray.ToArray ();
        var builderArray = builder.Allocate (ref f.ForceArray, arr.Length + 1);
        for (int i = 0; i < arr.Length; i++) {
            builderArray[i] = arr[i];
        }
        builderArray[index] = node;
    }
    public Force_C[] GetForces () {
        return forces.Value.ForceArray.ToArray ();
    }
}

public struct Forces {
    public BlobArray<Force_C> ForceArray;
}

//物体可以移动的组件标签
public struct Force_C : IComponentData {
    //作用力的大小和持续时间(x,y,z,w)
    public float4 direction;
    public ForceType type;
    public int senderIndex;
    public int ReciverIndex;
}
public enum ForceType {
    external,
    Gravity
}