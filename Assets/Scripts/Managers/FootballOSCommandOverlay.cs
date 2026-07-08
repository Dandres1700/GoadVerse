using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FootballOSCommandOverlay : MonoBehaviour
{
    private GameObject panel;
    private TMP_Text titleText;
    private TMP_Text sequenceText;
    private TMP_Text resultText;
    private RectTransform fillBar;

    private void Awake()
    {
        BuildUI();
        HideCommand();
    }

    private void BuildUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();

        if (canvas == null) return;

        panel = new GameObject("FootballOS_CommandOverlay");
        panel.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.anchoredPosition = new Vector2(0f, 35f);
        panelRect.sizeDelta = new Vector2(760f, 150f);

        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color32(5, 12, 22, 235);

        titleText = CreateText(panel.transform, "CommandTitle", new Vector2(20f, -14f), new Vector2(720f, 30f), 22);
        sequenceText = CreateText(panel.transform, "CommandSequence", new Vector2(20f, -50f), new Vector2(720f, 42f), 30);
        resultText = CreateText(panel.transform, "CommandResult", new Vector2(20f, -96f), new Vector2(720f, 26f), 18);

        GameObject barBackground = new GameObject("TimingBarBackground");
        barBackground.transform.SetParent(panel.transform, false);

        RectTransform bgRect = barBackground.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0f, 0f);
        bgRect.anchorMax = new Vector2(0f, 0f);
        bgRect.pivot = new Vector2(0f, 0f);
        bgRect.anchoredPosition = new Vector2(20f, 14f);
        bgRect.sizeDelta = new Vector2(720f, 12f);

        Image bgImage = barBackground.AddComponent<Image>();
        bgImage.color = new Color32(55, 65, 80, 255);

        GameObject barFill = new GameObject("TimingBarFill");
        barFill.transform.SetParent(barBackground.transform, false);

        fillBar = barFill.AddComponent<RectTransform>();
        fillBar.anchorMin = new Vector2(0f, 0f);
        fillBar.anchorMax = new Vector2(0f, 1f);
        fillBar.pivot = new Vector2(0f, 0.5f);
        fillBar.anchoredPosition = Vector2.zero;
        fillBar.sizeDelta = new Vector2(0f, 0f);

        Image fillImage = barFill.AddComponent<Image>();
        fillImage.color = new Color32(0, 225, 255, 255);
    }

    private TMP_Text CreateText(Transform parent, string objectName, Vector2 position, Vector2 size, int fontSize)
    {
        GameObject obj = new GameObject(objectName);
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        TMP_Text text = obj.AddComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Bold;

        return text;
    }

    public void ShowCommand(string commandName)
    {
        if (panel != null)
        {
            panel.SetActive(true);
        }

        if (titleText != null)
        {
            titleText.text = commandName;
        }

        if (resultText != null)
        {
            resultText.text = "SYNC INPUT";
        }

        SetProgress(0f);
    }

    public void UpdateCommand(KeyCode[] sequence, int currentIndex, float syncPercent)
    {
        if (sequenceText != null)
        {
            sequenceText.text = BuildSequence(sequence, currentIndex);
        }

        if (resultText != null)
        {
            resultText.text = "SYNC: " + Mathf.RoundToInt(syncPercent) + "%";
        }

        SetProgress(syncPercent / 100f);
    }

    public void ShowResult(string result)
    {
        if (resultText != null)
        {
            resultText.text = "RESULT: " + result;
        }

        SetProgress(1f);
    }

    public void HideCommand()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    private string BuildSequence(KeyCode[] sequence, int currentIndex)
    {
        string output = "";

        for (int i = 0; i < sequence.Length; i++)
        {
            string keyName = GetKeyName(sequence[i]);

            if (i == currentIndex)
            {
                output += "<color=#00E5FF>[ " + keyName + " ]</color> ";
            }
            else
            {
                output += keyName + " ";
            }
        }

        return output;
    }

    private string GetKeyName(KeyCode key)
    {
        if (key == KeyCode.Space) return "SPACE";
        return key.ToString().ToUpper();
    }

    private void SetProgress(float progress)
    {
        if (fillBar == null) return;

        progress = Mathf.Clamp01(progress);
        fillBar.sizeDelta = new Vector2(720f * progress, 0f);
    }
}