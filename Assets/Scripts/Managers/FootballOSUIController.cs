using TMPro;
using UnityEngine;

public class FootballOSUIController : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text subtitleText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text eventLogText;
    [SerializeField] private TMP_Text controlText;

    private void Start()
    {
        SetStaticUI();
    }

    private void SetStaticUI()
    {
        if (titleText != null)
            titleText.text = "FOOTBALL OS";

        if (subtitleText != null)
            subtitleText.text = "Tactical Simulation Interface";

        if (statusText != null)
            statusText.text =
                "STATUS: CONNECTED\n" +
                "MODE: AUTO PLAY SIMULATION\n" +
                "MATCH: ECUADOR vs RIVAL\n" +
                "ENGINE: ACTIVE";

        if (eventLogText != null)
            eventLogText.text = "- Esperando simulación...";

        if (controlText != null)
            controlText.text =
                "PLAYER IN CONTROL: NONE\n" +
                "NEXT ACTION: WAITING";
    }

    public void SetEventLog(string text)
    {
        if (eventLogText != null)
            eventLogText.text = text;
    }

    public void SetControlData(string playerInControl, string nextAction)
    {
        if (controlText != null)
        {
            controlText.text =
                "PLAYER IN CONTROL: " + playerInControl + "\n" +
                "NEXT ACTION: " + nextAction;
        }
    }
}