using UnityEngine;
using Ubiq.Messaging;

public class NetworkObject : MonoBehaviour
{
    private NetworkContext context;
    private bool owner;

    private void Start()
    {
        context = NetworkScene.Register(this);
        owner = true; // 让所有客户端都能发送位置更新
    }

    private void Update()
    {
        // ... existing code ...
    }

    // ... existing code ...
} 