using UnityEngine;
using UnityEngine.XR;
using Unity.XR.CoreUtils;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("References")]
    public Animator animator;  // 改为 public 以便直接赋值
    
    [Header("Movement Settings")]
    [SerializeField] private float movementThreshold = 0.001f;
    [SerializeField] private float speedMultiplier = 1.5f;
    [SerializeField] private bool debugMode = true;

    // 使用输入系统来检测移动
    private Vector2 movementInput;
    private float currentSpeed = 0f;

    private void Update()
    {
        if (animator == null)
        {
            return;
        }

        // 获取输入
        float horizontal = Input.GetAxis("Horizontal"); // A/D 或 ←/→
        float vertical = Input.GetAxis("Vertical");     // W/S 或 ↑/↓

        // 计算移动量
        Vector2 movement = new Vector2(horizontal, vertical);
        float movementMagnitude = movement.magnitude;

        // 限制对角线移动速度
        movementMagnitude = Mathf.Min(movementMagnitude, 1.0f);

        if (debugMode)
        {
            Debug.Log($"Movement Input: {movement}, Magnitude: {movementMagnitude}");
        }

        // 更新动画速度
        if (movementMagnitude > movementThreshold)
        {
            currentSpeed = movementMagnitude * speedMultiplier;
            
            if (debugMode)
            {
                Debug.Log($"Setting speed to: {currentSpeed}");
            }
        }
        else
        {
            currentSpeed = 0f;
            
            if (debugMode)
            {
                Debug.Log("Setting speed to 0 (below threshold)");
            }
        }

        // 应用到动画器
        animator.SetFloat("Speed", currentSpeed);
    }

    private void OnGUI()
    {
        if (debugMode)
        {
            GUI.Label(new Rect(10, 10, 300, 20), $"Movement Magnitude: {currentSpeed:F3}");
            GUI.Label(new Rect(10, 30, 300, 20), $"Input: {movementInput}");
            GUI.Label(new Rect(10, 50, 300, 20), $"Has Animator: {animator != null}");
        }
    }
}