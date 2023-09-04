using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameoverUI : MonoBehaviour
{

    private Canvas gameOverCanvas;

    private void Start()
    {
        gameOverCanvas= GetComponent<Canvas>();
    }
    private void OnEnable()
    {
        PlayerController.GameOverEvent += GameOver;

    }
    private void OnDisable()
    {
        PlayerController.GameOverEvent -= GameOver;

    }
    private void GameOver()
    {
        gameOverCanvas.enabled = true;
    }
}
