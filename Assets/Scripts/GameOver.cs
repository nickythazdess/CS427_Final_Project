using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOver : MonoBehaviour
{
    public GameObject gameOverPanel;
    public void MainMenu()
    {
        gameOverPanel.SetActive(false);
        //back to Main menu Scene
    }

    public void RunGame()
    {
        gameOverPanel.SetActive(false);
        //Restart game
    }
}
