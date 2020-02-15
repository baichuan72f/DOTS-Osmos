using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class mousePoition_Authoring : MonoBehaviour {
    EntityManager manager;
    public int index;
    // Start is called before the first frame update
    void Start () {
        manager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    // Update is called once per frame
    void Update () {
        if (Input.GetMouseButtonDown (0)) {
            Vector3 pos = Camera.main.ScreenToWorldPoint (Input.mousePosition + Vector3.forward * 10);
            Debug.Log (pos);
            //new GameObject ().transform.position = pos;
            var mousePos = new Mouseposition_C ();
            mousePos.value = pos;
            mousePos.index = index;
            manager.AddComponentData (manager.CreateEntity (), mousePos);
        }
    }
}