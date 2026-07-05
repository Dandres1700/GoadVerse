using UnityEngine;


/// Colocar en la escena de CADA minijuego (un GameObject vacio, ej "MapManager").
/// Instancia uno de los mapas disponibles (prefabs de la carpeta Assets/Maps/...).
/// Funciona igual si el minijuego tiene 1 solo mapa o varios: si la lista tiene
/// un unico elemento, siempre carga ese; si tiene varios, elige uno.

public class MinigameSceneManager : MonoBehaviour
{
    [Header("Mapas disponibles para este minijuego")]
    [Tooltip("Arrastra aqui los prefabs de mapa desde Assets/Maps/<NombreMinijuego>/")]
    public GameObject[] mapPrefabs;

    [Tooltip("Punto donde se instancia el mapa. Si se deja vacio, usa (0,0,0).")]
    public Transform mapSpawnPoint;

    [Header("Configuracion de seleccion")]
    [Tooltip("Si esta activo, elige un mapa al azar entre los disponibles")]
    public bool chooseRandomMap = true;

    [Tooltip("Indice del mapa a usar si 'chooseRandomMap' esta desactivado")]
    public int fixedMapIndex = 0;

    private GameObject currentMapInstance;

    void Start()
    {
        LoadMap();
    }

    private void LoadMap()
    {
        if (mapPrefabs == null || mapPrefabs.Length == 0)
        {
            Debug.LogWarning("MinigameSceneManager: no hay mapas asignados en el array 'mapPrefabs'.");
            return;
        }

        int index = chooseRandomMap
            ? Random.Range(0, mapPrefabs.Length)
            : Mathf.Clamp(fixedMapIndex, 0, mapPrefabs.Length - 1);

        Vector3 spawnPos = mapSpawnPoint != null ? mapSpawnPoint.position : Vector3.zero;
        Quaternion spawnRot = mapSpawnPoint != null ? mapSpawnPoint.rotation : Quaternion.identity;

        currentMapInstance = Instantiate(mapPrefabs[index], spawnPos, spawnRot);
    }
}