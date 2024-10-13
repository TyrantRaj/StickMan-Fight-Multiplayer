using UnityEngine;
using UnityEngine.SceneManagement;

public class canvas : MonoBehaviour
{
    [SerializeField] public GameObject PlayerControlCanvas;
    [SerializeField] public GameObject Lobbycodecanvas;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject); // Keep the canvas persistent across scenes
    }

    private void  OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded; // Subscribe to sceneLoaded event
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // Unsubscribe to avoid memory leaks
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

        if (PlayerControlCanvas != null) {
            if (scene.name == "Menu")
            {
                
                    PlayerControlCanvas.SetActive(false);
                
            }
            else
            {
                
                    PlayerControlCanvas.SetActive(true);
                
            }
        }
        

        if(Lobbycodecanvas != null)
        {
            if (scene.name == "Lobby")
            {

                Lobbycodecanvas.SetActive(true);
            }
            else
            {
                Lobbycodecanvas.SetActive(false);
            }
        }

        
    }
}
