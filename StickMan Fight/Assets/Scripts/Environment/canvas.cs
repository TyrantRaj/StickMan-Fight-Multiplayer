using UnityEngine;
using UnityEngine.SceneManagement;

public class canvas : MonoBehaviour
{


    private void Awake()
    {
        DontDestroyOnLoad(gameObject); // Keep the canvas persistent across scenes

    }
}