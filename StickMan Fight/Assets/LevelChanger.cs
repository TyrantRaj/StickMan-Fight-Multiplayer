using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelChanger : MonoBehaviour
{
    public void ChangeScene(string Scenename)
    {
        SceneManager.LoadScene(Scenename);
    }

    public void QuitGame()
    {
        // Logs a message when quitting from the Unity Editor
        Debug.Log("Game is exiting...");

        // If running in the Unity Editor
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                // If running in a standalone build
                Application.Quit();
        #endif
    }
}
