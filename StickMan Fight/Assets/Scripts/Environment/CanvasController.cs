using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CanvasController : MonoBehaviour
{
    [SerializeField] Button[] playerControlBtn; // Array of button

    private void Awake()
    {
        DontDestroyOnLoad(gameObject); // Keep the canvas persistent across scenes
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded; // Register the callback for scene load
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // Unregister the callback when the object is disabled/destroyed
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //ResetButtonState(); // Reset buttons when the new scene loads
    }

    // Reset button state
    private void ResetButtonState()
    {
        foreach (var button in playerControlBtn)
        {
            button.interactable = true;  // Re-enable the button for the new scene
            button.onClick.RemoveAllListeners();  // Clear previous listeners
        }

        Debug.Log("Buttons Reset for the new scene");
    }
}
