using UnityEngine;
using Ubiq.Rooms;
using Ubiq.Messaging;

public class PlayerInteraction : MonoBehaviour
{
    private string networkIdString;
    private RoomClient roomClient;
    private AudioSource audioSource;
    public AudioClip interactionSound; // 在Unity Inspector中设置音效

    private void Start()
    {
        networkIdString = NetworkId.Create(this).ToString();
        roomClient = GetComponent<RoomClient>();
        audioSource = GetComponent<AudioSource>();
        
        // 如果没有AudioSource组件，添加一个
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // 当其他玩家点击这个玩家时调用
    public void OnPlayerClicked()
    {
        // 播放音效
        if (audioSource != null && interactionSound != null)
        {
            audioSource.PlayOneShot(interactionSound);
        }
        
        // 通过网络同步这个交互事件
        roomClient.Room[networkIdString + "_interaction"] = System.DateTime.Now.Ticks.ToString();
    }

    // 检测点击
    private void OnMouseDown()
    {
        // 找到所有PlayerInteraction组件
        var players = FindObjectsOfType<PlayerInteraction>();
        
        // 向被点击的玩家发送交互事件
        foreach (var player in players)
        {
            if (player != this) // 不给自己发送
            {
                player.OnPlayerClicked();
            }
        }
    }
} 