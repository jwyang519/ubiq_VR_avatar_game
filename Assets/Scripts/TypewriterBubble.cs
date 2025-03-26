using System.Collections;
using UnityEngine;
using TMPro;

public class SpeechBubble : MonoBehaviour
{
    public TextMeshProUGUI textDisplay;
    public string[] lines;
    public float typingSpeed = 0.05f;
    public float delayBetweenLines = 2f;

    private int index = 0;

    void Start()
    {
        StartCoroutine(PlaySpeechLoop());
    }

    IEnumerator PlaySpeechLoop()
    {
        while (true)
        {
            yield return StartCoroutine(TypeLine(lines[index]));
            yield return new WaitForSeconds(delayBetweenLines);
            index = (index + 1) % lines.Length;
        }
    }

    IEnumerator TypeLine(string line)
    {
        textDisplay.text = "";
        foreach (char letter in line.ToCharArray())
        {
            textDisplay.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}