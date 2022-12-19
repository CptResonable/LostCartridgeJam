using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour {
    [SerializeField] private GameObject goPausePanel;
    [SerializeField] private GameObject goGameOverPanel;

    [SerializeField] private TMP_Text txtSens;
    [SerializeField] private Slider sensSlider;

    public Player player;

    public enum GameState { Menu, Playing, Paused, GameOver }
    public GameState gameState;

    public static GameManager i;

    private void Awake() {
        i = this;
        gameState = GameState.Playing;
        Cursor.lockState = CursorLockMode.Locked;

        player.health.diedEvent += Health_diedEvent;

        sensSlider.value = Settings.MOUSE_SENSITIVITY;
        txtSens.text = "Mouse sensitivity: " + Settings.MOUSE_SENSITIVITY.ToString("0.0");
    }

    private void Start() {
    }

    private void Update() {
        if (Input.GetKeyUp(KeyCode.Escape)) {
            switch (gameState) {
                case GameState.Menu:
                    break;
                case GameState.Playing:
                    PauseGame();
                    break;
                case GameState.Paused:
                    UnpauseGame();
                    break;
                case GameState.GameOver:
                    break;
            }
        }
    }

    private void Health_diedEvent() {
        gameState = GameState.GameOver;
        Cursor.lockState = CursorLockMode.None;
        goGameOverPanel.SetActive(true);
    }


    private void PauseGame() {
        gameState = GameState.Paused;
        goPausePanel.SetActive(true);
        Time.timeScale = 0;
    }

    private void UnpauseGame() {
        gameState = GameState.Playing;
        goPausePanel.SetActive(false);
        Time.timeScale = 1;
    }

    public void OnBtn_Restart() {
        SceneManager.LoadScene("SampleScene");
    }

    public void OnSensChange() {
        Settings.MOUSE_SENSITIVITY = sensSlider.value;
        txtSens.text = "Mouse sensitivity: " + Settings.MOUSE_SENSITIVITY.ToString("0.0");
    }
}
