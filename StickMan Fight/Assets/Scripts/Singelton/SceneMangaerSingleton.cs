using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneMangaerSingleton : MonoBehaviour
{
    public static SceneMangaerSingleton Instance { get; private set; }

    void Awake()
    {
        // Check if an instance of NetworkManager already exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroy any duplicate instance
            return;
        }

        // Set this instance as the active instance
        Instance = this;

        // Ensure it persists between scene changes
        DontDestroyOnLoad(gameObject);
    }
}