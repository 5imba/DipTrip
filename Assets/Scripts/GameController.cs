using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    [SerializeField] PlayerMovement player;
    [SerializeField] Canvas gameOverUI;
    [SerializeField] TunnelColorController colorController;

    PlayerPawnController playerPawn;

    private void Start()
    {
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.orientation = ScreenOrientation.Portrait;

        playerPawn = player.GetComponentInChildren<PlayerPawnController>();
        playerPawn.OnDeath += OnDeath;
        Messenger<bool>.AddListener(GameEvent.ON_PAUSE, OnPause);
    }

    private void OnDestroy()
    {
        Messenger<bool>.RemoveListener(GameEvent.ON_PAUSE, OnPause);
        playerPawn.OnDeath -= OnDeath;
    }

    void OnDeath()
    {
        player.IsMove = false;
        if (colorController != null) colorController.IsChanging = false;
        gameOverUI.gameObject.SetActive(true);
    }

    public void Restart()
    {
        SceneManager.LoadScene("Game");
    }

    public void ToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void OnPause(bool pause)
    {
        player.IsMove = !pause;
        if (colorController != null) colorController.IsChanging = !pause;
    }
}
