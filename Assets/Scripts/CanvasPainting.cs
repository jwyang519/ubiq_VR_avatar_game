using UnityEngine;
using System.IO;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;

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
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor rayInteractor; // Reference to the XR Ray Interactor

    private Texture2D drawingTexture;
    private Renderer rend;
    private bool isDrawing = false;

    void Start()
    {
        // Get the Renderer component on the canvas (Quad)
        rend = GetComponent<Renderer>();

        // Create a new blank texture with RGBA32 format
        drawingTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);

        // Fill the texture with the background color
        Color[] pixels = new Color[textureWidth * textureHeight];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = backgroundColor;
        }
        drawingTexture.SetPixels(pixels);
        drawingTexture.Apply();

        // Set the material's main texture to the newly created texture
        rend.material.mainTexture = drawingTexture;

        // If ray interactor reference is not set, try to find it
        if (rayInteractor == null)
        {
            rayInteractor = GameObject.Find("XR Origin/Camera Offset/Right Controller")?.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor>();
            if (rayInteractor == null)
            {
                Debug.LogError("XR Ray Interactor not found! Please assign it in the inspector.");
            }
        }
    }

    void Update()
    {
        if (rayInteractor == null) return;

        // Check if the ray is hitting something
        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            // Check if the trigger is being pressed on the right controller
            if (Input.GetKey(KeyCode.Mouse0) || Input.GetButton("Fire1") || Input.GetAxis("XRI_Right_Trigger") > 0.1f)
            {
                // Check if the ray hit this canvas
                if (hit.collider.gameObject == gameObject)
                {
                    DrawAtPoint(hit.textureCoord);
                }
            }
        }
    }

    // Draw on the texture at the given UV coordinate
    void DrawAtPoint(Vector2 uv)
    {
        int x = (int)(uv.x * textureWidth);
        int y = (int)(uv.y * textureHeight);

        // Loop through a square of pixels based on brushSize
        for (int i = -brushSize; i < brushSize; i++)
        {
            for (int j = -brushSize; j < brushSize; j++)
            {
                int pixelX = x + i;
                int pixelY = y + j;

                // Check that we are within bounds of the texture
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