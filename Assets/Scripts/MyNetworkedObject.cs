using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Messaging;

public class MyNetworkedObject : MonoBehaviour
{
    NetworkContext context;
    Vector3 lastPosition;
    Quaternion lastRotation;

    void Start()
    {
        context = NetworkScene.Register(this);
        lastPosition = transform.localPosition;
        lastRotation = transform.localRotation;
    }

    void Update()
    {
        // Check if either position or rotation changed significantly
        if (lastPosition != transform.localPosition || lastRotation != transform.localRotation)
        {
            lastPosition = transform.localPosition;
            lastRotation = transform.localRotation;
            context.SendJson(new Message()
            {
                position = transform.localPosition,
                rotation = transform.localRotation
            });
        }
    }

    private struct Message
    {
        public Vector3 position;
        public Quaternion rotation;
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var m = message.FromJson<Message>();
        transform.localPosition = m.position;
        transform.localRotation = m.rotation;
        lastPosition = m.position;
        lastRotation = m.rotation;
    }
}