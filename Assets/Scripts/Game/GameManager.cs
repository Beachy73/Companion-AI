using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private GameObject player;

    private void Start()
    {
        player = FindObjectOfType<PlayerController>().gameObject;
    }

    private void Update()
    {
        if (player.activeInHierarchy == false)
        {
            EndGame();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene("GameScene");
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    public void EndGame()
    {
        SceneManager.LoadScene("GameOver");
    }

    public void Restart()
    {

    }
}
