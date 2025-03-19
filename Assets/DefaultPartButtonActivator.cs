using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DefaultPartButtonActivator : MonoBehaviour
{
    // Delay before trying to simulate button clicks; adjust if necessary.
    public float delay = 3f;

    void Start()
    {
        StartCoroutine(SimulateDefaultButtonPress());
    }

    IEnumerator SimulateDefaultButtonPress()
    {
        // Wait until UI buttons are created (adjust delay as needed).
        yield return new WaitForSeconds(delay);

        // Find all AvatarCustomizationUI components in the scene.
        AvatarCustomizationUI[] controllers = FindObjectsOfType<AvatarCustomizationUI>();
        if (controllers == null || controllers.Length == 0)
        {
            Debug.LogWarning("DefaultPartButtonActivator: No AvatarCustomizationUI components found in the scene.");
            yield break;
        }

        foreach (var controller in controllers)
        {
            if (controller.buttonContainer.childCount > 0)
            {
                // Get the first button in the container.
                Button firstButton = controller.buttonContainer.GetChild(0).GetComponent<Button>();
                if (firstButton != null)
                {
                    // Simulate the click on the button.
                    firstButton.onClick.Invoke();
                    Debug.Log("DefaultPartButtonActivator: Simulated click for category: " + controller.category);
                }
                else
                {
                    Debug.LogWarning("DefaultPartButtonActivator: No Button component found on first child in " + controller.category);
                }
            }
            else
            {
                Debug.LogWarning("DefaultPartButtonActivator: Button container is empty for category: " + controller.category);
            }
        }
    }
}