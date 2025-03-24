using UnityEngine;
using System.IO;

using UnityEngine.XR;

[RequireComponent(typeof(Renderer))]
public class CanvasPainter : MonoBehaviour
{
    [Header("Texture Settings")]
    public int textureWidth = 512;
    public int textureHeight = 512;
    public Color backgroundColor = Color.white;

    [Header("Drawing Settings")]
    public int brushSize = 5;
    public Color drawColor = Color.black; // Default drawing color

    [Header("VR Settings")]
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor rayInteractor; // If assigned, we'll use VR drawing

    private Texture2D drawingTexture;
    private Renderer rend;

    private bool canUseVR = false;   // True if we detect VR

    private void Start()
    {
        // 1. Setup the texture for drawing
        rend = GetComponent<Renderer>();
        drawingTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);

        // Fill the texture with the background color
        Color[] pixels = new Color[textureWidth * textureHeight];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = backgroundColor;
        }
        drawingTexture.SetPixels(pixels);
        drawingTexture.Apply();

        // Assign to the material
        rend.material.mainTexture = drawingTexture;

        // 2. If rayInteractor is not assigned, try to find it
        if (!rayInteractor)
        {
            // Attempt a naive search by name (adjust path if needed)
            var rightController = GameObject.Find("XR Origin/Camera Offset/Right Controller");
            if (rightController)
            {
                rayInteractor = rightController.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor>();
            }
        }

        // 3. Determine if VR is available (if we found an XRRayInteractor)
        if (rayInteractor)
        {
            canUseVR = true;
            Debug.Log("[CanvasPainter] VR mode enabled. Using XRRayInteractor for painting.");
        }
        else
        {
            canUseVR = false;
            Debug.Log("[CanvasPainter] Desktop mode enabled. Using mouse input for painting.");
        }
    }

    private void Update()
    {
        if (canUseVR && rayInteractor != null)
        {
            // --- VR Logic ---
            TryPaintVR();
        }
        else
        {
            // --- Desktop Logic ---
            TryPaintDesktop();
        }
    }

    private void TryPaintVR()
    {
        // Check if the ray is hitting something
        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            // IsSelectActive = user is pulling the trigger (in XR Interaction Toolkit).
            // Alternatively, you could check: rayInteractor.inputDevice.TryGetFeatureValue(CommonUsages.trigger, out float value)
            // if you want a more analog approach.
            if (rayInteractor.isSelectActive)
            {
                // If the ray hits this canvas
                if (hit.collider && hit.collider.gameObject == gameObject)
                {
                    DrawAtPoint(hit.textureCoord);
                }
            }
        }
    }

    private void TryPaintDesktop()
    {
        // If left mouse button is down, let's do a normal Raycast from Camera
        if (Input.GetMouseButton(0))
        {
            if (Camera.main)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (hit.collider && hit.collider.gameObject == gameObject)
                    {
                        DrawAtPoint(hit.textureCoord);
                    }
                }
            }
        }
    }

    // Draw on the texture at the given UV coordinate
    private void DrawAtPoint(Vector2 uv)
    {
        int x = (int)(uv.x * textureWidth);
        int y = (int)(uv.y * textureHeight);

        // Loop through a square of pixels based on brushSize
        for (int i = -brushSize; i <= brushSize; i++)
        {
            for (int j = -brushSize; j <= brushSize; j++)
            {
                int pixelX = x + i;
                int pixelY = y + j;

                // Check that we're within bounds of the texture
                if (pixelX >= 0 && pixelX < textureWidth && pixelY >= 0 && pixelY < textureHeight)
                {
                    drawingTexture.SetPixel(pixelX, pixelY, drawColor);
                }
            }
        }
        drawingTexture.Apply(); // Update the texture with the new pixels
    }

    // Public method to change the drawing color (can be called from UI buttons)
    public void SetDrawingColor(Color newColor)
    {
        drawColor = newColor;
    }

    // Optional: Save the drawing to a PNG file
    public void SaveDrawing()
    {
        byte[] bytes = drawingTexture.EncodeToPNG();
        string path = Application.persistentDataPath + "/MyDrawing.png";
        File.WriteAllBytes(path, bytes);
        Debug.Log("Drawing saved at: " + path);
    }
}