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
            subtitleText.text = "Controles, acciones y marcador";

        if (statusText != null)
            statusText.text =
                "MOVIMIENTO: WASD / Flechas\n" +
                "CAMBIAR JUGADOR: E / Tab\n" +
                "PATEAR / PASAR: O\n" +
                "CORRER: Shift";

        if (eventLogText != null)
            eventLogText.text =
                "MARCADOR: Team Player 0 - 0 Team Rival\n" +
                "- Esperando inicio del partido...";

        if (controlText != null)
            controlText.text =
                "JUGADOR: Ninguno\n" +
                "ACCION: Esperando\n" +
                "O: patea cerca del arco, pasa si hay companero";
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
                "JUGADOR: " + playerInControl + "\n" +
                "ACCION: " + nextAction + "\n" +
                "WASD mueve | E cambia | O patea/pasa";
        }
    }
}
