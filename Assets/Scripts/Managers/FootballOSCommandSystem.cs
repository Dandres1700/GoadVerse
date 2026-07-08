using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum CommandResult
{
    Perfect,
    Good,
    Bad,
    Miss
}

public class FootballOSCommandSystem : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float stepDuration = 1.1f;
    [SerializeField] private float targetProgress = 0.7f;
    [SerializeField] private float perfectWindow = 0.08f;
    [SerializeField] private float goodWindow = 0.18f;
    [SerializeField] private float badWindow = 0.32f;

    [Header("Slow Motion")]
    [SerializeField] private float commandTimeScale = 0.35f;

    private GameObject panel;
    private TMP_Text titleText;
    private TMP_Text sequenceText;
    private TMP_Text resultText;
    private RectTransform fillBar;

    public bool IsRunning { get; private set; }

    private void Awake()
    {
        BuildUI();
        HideUI();
    }

    public IEnumerator RunCommand(string commandName, KeyCode[] sequence, Action<CommandResult> onComplete)
    {
        IsRunning = true;

        float previousTimeScale = Time.timeScale;
        Time.timeScale = commandTimeScale;

        ShowUI();

        List<CommandResult> results = new List<CommandResult>();

        for (int i = 0; i < sequence.Length; i++)
        {
            KeyCode expectedKey = sequence[i];

            titleText.text = commandName;
            resultText.text = "SYNC INPUT";
            UpdateSequenceText(sequence, i);

            CommandResult stepResult = CommandResult.Miss;
            bool pressed = false;
            float elapsed = 0f;

            while (elapsed < stepDuration)
            {
                elapsed += Time.unscaledDeltaTime;

                float progress = Mathf.Clamp01(elapsed / stepDuration);
                UpdateBar(progress);

                if (Input.GetKeyDown(expectedKey))
                {
                    float difference = Mathf.Abs(progress - targetProgress);

                    if (difference <= perfectWindow)
                        stepResult = CommandResult.Perfect;
                    else if (difference <= goodWindow)
                        stepResult = CommandResult.Good;
                    else if (difference <= badWindow)
                        stepResult = CommandResult.Bad;
                    else
                        stepResult = CommandResult.Miss;

                    pressed = true;
                    break;
                }

                yield return null;
            }

            if (!pressed)
                stepResult = CommandResult.Miss;

            results.Add(stepResult);
            resultText.text = stepResult.ToString().ToUpper();

            yield return WaitUnscaled(0.25f);
        }

        CommandResult finalResult = CalculateFinalResult(results);

        resultText.text = "RESULT: " + finalResult.ToString().ToUpper();

        yield return WaitUnscaled(0.7f);

        Time.timeScale = previousTimeScale;
        HideUI();

        IsRunning = false;

        onComplete?.Invoke(finalResult);
    }

    private CommandResult CalculateFinalResult(List<CommandResult> results)
    {
        int perfect = 0;
        int good = 0;
        int bad = 0;
        int miss = 0;

        foreach (CommandResult result in results)
        {
            if (result == CommandResult.Perfect) perfect++;
            if (result == CommandResult.Good) good++;
            if (result == CommandResult.Bad) bad++;
            if (result == CommandResult.Miss) miss++;
        }

        if (miss > 0) return CommandResult.Miss;
        if (perfect == results.Count) return CommandResult.Perfect;
        if (bad > 0) return CommandResult.Bad;

        return CommandResult.Good;
    }

    private IEnumerator WaitUnscaled(float seconds)
    {
        float timer = 0f;

        while (timer < seconds)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private void BuildUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();

        if (canvas == null)
            return;

        panel = new GameObject("CommandPanel");
        panel.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.anchoredPosition = new Vector2(0f, 45f);
        panelRect.sizeDelta = new Vector2(680f, 150f);

        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color32(8, 14, 24, 230);

        titleText = CreateText(panel.transform, "CommandTitle", new Vector2(20, -15), new Vector2(640, 32), 22);
        sequenceText = CreateText(panel.transform, "CommandSequence", new Vector2(20, -52), new Vector2(640, 42), 28);
        resultText = CreateText(panel.transform, "CommandResult", new Vector2(20, -98), new Vector2(640, 28), 18);

        GameObject barBg = new GameObject("TimingBarBG");
        barBg.transform.SetParent(panel.transform, false);

        RectTransform bgRect = barBg.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0f, 0f);
        bgRect.anchorMax = new Vector2(0f, 0f);
        bgRect.pivot = new Vector2(0f, 0f);
        bgRect.anchoredPosition = new Vector2(20f, 15f);
        bgRect.sizeDelta = new Vector2(640f, 12f);

        Image bgImage = barBg.AddComponent<Image>();
        bgImage.color = new Color32(60, 70, 85, 255);

        GameObject barFill = new GameObject("TimingBarFill");
        barFill.transform.SetParent(barBg.transform, false);

        fillBar = barFill.AddComponent<RectTransform>();
        fillBar.anchorMin = new Vector2(0f, 0f);
        fillBar.anchorMax = new Vector2(0f, 1f);
        fillBar.pivot = new Vector2(0f, 0.5f);
        fillBar.anchoredPosition = Vector2.zero;
        fillBar.sizeDelta = new Vector2(0f, 0f);

        Image fillImage = barFill.AddComponent<Image>();
        fillImage.color = new Color32(0, 220, 255, 255);
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

    private void UpdateSequenceText(KeyCode[] sequence, int currentIndex)
    {
        string text = "";

        for (int i = 0; i < sequence.Length; i++)
        {
            string keyName = GetKeyName(sequence[i]);

            if (i == currentIndex)
                text += "<color=#00E5FF>[ " + keyName + " ]</color> ";
            else
                text += keyName + " ";
        }

        sequenceText.text = text;
    }

    private string GetKeyName(KeyCode key)
    {
        if (key == KeyCode.Space) return "SPACE";
        return key.ToString().ToUpper();
    }

    private void UpdateBar(float progress)
    {
        if (fillBar == null) return;

        fillBar.sizeDelta = new Vector2(640f * progress, 0f);
    }

    private void ShowUI()
    {
        if (panel != null)
            panel.SetActive(true);
    }

    private void HideUI()
    {
        if (panel != null)
            panel.SetActive(false);
    }
}