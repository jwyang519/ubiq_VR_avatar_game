using UnityEngine;
using Ubiq.Messaging;
using UnityEngine.InputSystem;

public class GlobalPingTest : MonoBehaviour
{
    private NetworkContext context;

    private void Start()
    {
        context = NetworkScene.Register(this);
        Debug.Log($"[GlobalPingTest] Registered with context id: {context.GetHashCode()}");
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
        {
            var ping = new { text = "global ping" };
            string json = JsonUtility.ToJson(ping);
            Debug.Log($"[GlobalPingTest] Sending global ping: {json}");
            context.SendJson(ping);
        }
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        Debug.Log("[GlobalPingTest] Received message: " + message.ToString());
    }
}