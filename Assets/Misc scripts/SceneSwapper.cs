using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwapper : MonoBehaviour {
    public static SceneSwapper i;
    public string activeSceneName;

    private void Awake() {
        i = this;
        DontDestroyOnLoad(this);
    }

    public void LoadScene(string sceneName) {
        try {
            SceneManager.LoadScene(sceneName);
        }
        catch {
            Debug.LogError("Error loading scene: " + sceneName);
            return;
        }
        activeSceneName = sceneName;
    }
}
