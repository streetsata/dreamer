using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CollisionEventSystem : MonoBehaviour
{
    public bool DebugOn = false;
    public UnityEvent onEnterEvent, onExitEvent;

    private void Start()
    {
        PhysicsBody myBody = GetComponent<PhysicsBody>();
        if(myBody != null)
        {
            myBody.onEnter += OnEnterBody;
            myBody.onExit += OnExitBody;
        }
    }

    void OnEnterBody(List<CollisionData> physicsBodies)
    {
        if(DebugOn) Debug.Log(physicsBodies[0].other.gameObject.name + " enter");

        onEnterEvent.Invoke();
    }

    void OnExitBody(List<CollisionData> physicsBodies)
    {
        if (DebugOn)
        {
            if (physicsBodies.Count > 0) Debug.Log(physicsBodies[physicsBodies.Count - 1].other.gameObject.name + " exit");
            else Debug.Log("physicsBodies null (exit)");
        }

        onExitEvent.Invoke();
    }
}
