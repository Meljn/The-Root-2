using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public bool PauseGame;
    public GameObject pauseGameMenu;
    public GameObject ledgeDetected;
    public PlayerLook playerLookScript;
    public LedgeGrabbing grabbingScript;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (PauseGame)
            {
                Resume();
            }

            else
            {
                Pause();
            }
        }

        if (grabbingScript.ledgeDetected || grabbingScript.holding)
        {
            ledgeDetected.SetActive(true);
        }

        else ledgeDetected.SetActive(false);
    }

    public void Resume()
    {
        playerLookScript.enabled = true;
        pauseGameMenu.SetActive(false);
        Time.timeScale = 1f;
        PauseGame = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Pause()
    {
        playerLookScript.enabled = false;
        pauseGameMenu.SetActive(true);
        Time.timeScale = 0f;
        PauseGame = true;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    public void LoadMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Menu");
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 0);
        playerLookScript.enabled = true;
        pauseGameMenu.SetActive(false);
        Time.timeScale = 1f;
        PauseGame = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
