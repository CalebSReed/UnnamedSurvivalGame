using TMPro;
using UnityEngine;

public class UI_HotbarTooltip : MonoBehaviour
{
    public static UI_HotbarTooltip Instance { get; private set; }
    [SerializeField] private TextMeshProUGUI textMesh;

    private void Awake()
    {
        Instance = this;
    }

    public void SetTooltip(string text)
    {
        if (text.Contains("LMB"))
        {
            text = text.Remove(0, 3);
            text = text.Insert(0, "<sprite name=\"LMB\">");
        }
        if (text.Contains("RMB"))
        {
            text = text.Remove(0, 3);
            text = text.Insert(0, "<sprite name=\"RMB\">");
        }
        if (text.Contains("MMB"))
        {
            text = text.Remove(0, 3);
            text = text.Insert(0, "<sprite name=\"MMB\">");
        }

        textMesh.text = text;
    }
}