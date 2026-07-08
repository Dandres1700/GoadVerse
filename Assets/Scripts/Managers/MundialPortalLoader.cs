using UnityEngine;
using UnityEngine.SceneManagement;

public class MundialPortalLoader : MonoBehaviour
{
    [Header("Escena a cargar")]
    [SerializeField] private string matchSceneName = "minijuego1";

    [Header("Opcional")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private bool playerInside;

    private void OnMouseDown()
    {
        LoadMatchScene();
    }

    private void Update()
    {
        if (playerInside && Input.GetKeyDown(interactKey))
        {
            LoadMatchScene();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
        }
    }

    private void LoadMatchScene()
    {
        SceneManager.LoadScene(matchSceneName);
    }
}