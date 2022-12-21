using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour {
    [SerializeField] public EnemySpawnManager enemySpawnManager;

    [SerializeField] private GameObject goStartPanel;
    [SerializeField] private GameObject goControlsPanel;
    [SerializeField] private GameObject goPausePanel;
    [SerializeField] private GameObject goGameOverPanel;

    [SerializeField] private TMP_Text txtSens;
    [SerializeField] private Slider sensSlider;

    [SerializeField] private TMP_Text txtKills;
    [SerializeField] private TMP_Text txtEnemies;
    [SerializeField] private TMP_Text txtWave;
    [SerializeField] private TMP_Text txtInfo;

    [SerializeField] private UI_bar barBulletTime;

    public bool usingBulletTime;

    public Player player;

    public enum GameState { Menu, Playing, Paused, GameOver }
    public GameState gameState;

    public int kills;
    public int enemiesAlive;
    [HideInInspector] public int wave = 1;

    public static GameManager i;

    private void Awake() {
        i = this;
        gameState = GameState.Menu;
  
        player.health.diedEvent += Player_diedEvent;

        sensSlider.value = Settings.MOUSE_SENSITIVITY;
        txtSens.text = "Mouse sensitivity: " + Settings.MOUSE_SENSITIVITY.ToString("0.0");
        Time.timeScale = 0;

        wave = 1;
        StartCoroutine(StartWaveCorutine());
        //enemySpawnManager.SpawnWave(6, 2, 15);
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

        if (gameState == GameState.Playing) {
            if (Input.GetKeyDown(KeyCode.LeftShift)) {
                if (!usingBulletTime && barBulletTime.fillAmount > 0)
                    StartBulletTime();
                else if (usingBulletTime)
                    StopBulletTime();
            }

            //if (Input.GetKey(KeyCode.LeftShift))
            //    Time.timeScale = 0.35f;
            //else
            //    Time.timeScale = 1;

            if (usingBulletTime) {
                barBulletTime.fillAmount -= Time.unscaledDeltaTime * 0.15f;

                if (barBulletTime.fillAmount <= 0) {
                    barBulletTime.fillAmount = 0;
                    StopBulletTime();
                }
            }
        }
    }

    private void Player_diedEvent() {
        gameState = GameState.GameOver;
        Cursor.lockState = CursorLockMode.None;
        goControlsPanel.SetActive(true);
        goGameOverPanel.SetActive(true);
    }

    private void StartBulletTime() {
        usingBulletTime = true;
        Time.timeScale = 0.35f;
    }

    private void StopBulletTime() {
        usingBulletTime = false;
        Time.timeScale = 1f;
    }

    private void PauseGame() {
        gameState = GameState.Paused;
        Cursor.lockState = CursorLockMode.None;
        goPausePanel.SetActive(true);
        goControlsPanel.SetActive(true);
        Time.timeScale = 0;
    }

    private void UnpauseGame() {
        gameState = GameState.Playing;
        Cursor.lockState = CursorLockMode.Locked;
        goPausePanel.SetActive(false);
        goControlsPanel.SetActive(false);
        Time.timeScale = 1;
    }

    public void OnBtn_Start() {
        gameState = GameState.Playing;
        Cursor.lockState = CursorLockMode.Locked;
        goControlsPanel.SetActive(false);
        goStartPanel.SetActive(false);
        Time.timeScale = 1;
    }

    public void OnBtn_Continue() {
        UnpauseGame();
    }

    public void OnBtn_Restart() {
        SceneManager.LoadScene("SampleScene");
    }

    public void OnSensChange() {
        Settings.MOUSE_SENSITIVITY = sensSlider.value;
        txtSens.text = "Mouse sensitivity: " + Settings.MOUSE_SENSITIVITY.ToString("0.0");
    }

    public void EnemySpawned(Character character) {
        character.health.diedEvent += Enemy_diedEvent;
        enemiesAlive++;
        txtEnemies.text = "ALIVE ENEMIES: " + enemiesAlive.ToString();
    }

    private void Enemy_diedEvent() {
        enemiesAlive--;
        kills++;

        txtKills.text = "KILLS: " + kills.ToString();
        txtEnemies.text = "ALIVE ENEMIES: " + enemiesAlive.ToString();

        if (enemiesAlive == 0 && enemySpawnManager.spawningComplete)
            WaveCompleted();

        barBulletTime.fillAmount += 0.07f;
        if (barBulletTime.fillAmount > 1)
            barBulletTime.fillAmount = 1;
    }

    private void WaveCompleted() {
        wave++;
        txtWave.text = "WAVE: " + wave.ToString();

        txtInfo.text = "WAVE " + (wave - 1).ToString() + " DEFEATED";
        player.health.HP = player.health.maxHP;
        StartCoroutine(StartWaveCorutine());
    }

    private IEnumerator StartWaveCorutine() {
        yield return new WaitForSeconds(2);

        txtInfo.text = "WAVE " + wave + " STARTS IN 3...";
        yield return new WaitForSeconds(1);
        txtInfo.text = "WAVE " + wave + " STARTS IN 2...";
        yield return new WaitForSeconds(1);
        txtInfo.text = "WAVE " + wave + " STARTS IN 1...";
        yield return new WaitForSeconds(1);
        txtInfo.text = "WAVE " + wave + " STARTS NOW!!";
        enemySpawnManager.SpawnWave(wave - 1);
        yield return new WaitForSeconds(1);
        txtInfo.text = "";
    }
}
