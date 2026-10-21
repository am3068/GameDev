
using UnityEngine;
using UnityEngine.InputSystem;

public class StartMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject gameplayUI;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Player")]
    [SerializeField] private PlayerController playerController;

    private bool gameStarted = false;

    private void Awake()
    {
        Time.timeScale = 0f;

        if (menuPanel != null)
            menuPanel.SetActive(true);

        if (gameplayUI != null)
            gameplayUI.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (playerController != null)
            playerController.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        if (gameStarted)
            return;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Start when the player clicks anywhere or presses P.
        bool clicked =
            Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame;


        if (clicked)
            StartGame();
    }

    public void StartGame()
    {
        if (gameStarted)
            return;

        gameStarted = true;

        if (menuPanel != null)
            menuPanel.SetActive(false);

        if (gameplayUI != null)
            gameplayUI.SetActive(true);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        Time.timeScale = 1f;

        if (playerController != null)
            playerController.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
