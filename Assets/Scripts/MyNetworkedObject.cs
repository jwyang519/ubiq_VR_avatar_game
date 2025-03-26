using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Messaging;

public class MyNetworkedObject : MonoBehaviour
{
    private NetworkContext context;
    private Vector3 lastPosition;
    private Quaternion lastRotation;

    void Start()
    {
        context = NetworkScene.Register(this);
        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }

    void Update()
    {
        // Check world position and rotation for changes
        if (lastPosition != transform.position || lastRotation != transform.rotation)
        {
            lastPosition = transform.position;
            lastRotation = transform.rotation;
            context.SendJson(new Message()
            {
                position = transform.position,
                rotation = transform.rotation
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
        // Update world transform
        transform.position = m.position;
        transform.rotation = m.rotation;
        lastPosition = m.position;
        lastRotation = m.rotation;
    }
}