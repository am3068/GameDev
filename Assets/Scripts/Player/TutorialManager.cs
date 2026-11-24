
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    public GameObject tutorialPanel;
    public GameObject crosshair;
    public PlayerController playerMovement;
    public PlayerInput playerInput;

    private IEnumerator Start()
    {
        Time.timeScale = 0f;
        // disable player input and mobement
        playerInput.enabled = false;
        playerMovement.enabled = false;

        // display tutorial and hide crosshair
        tutorialPanel.SetActive(true);
        crosshair.SetActive(false);

        yield return null;

        // unlock and display mouse cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void StartGame()

    {
        Time.timeScale = 1f;
        //Debug.Log("Start Game button clicked!");

        // hide tutorial and display crosshair
        tutorialPanel.SetActive(false);
        crosshair.SetActive(true);

        // restore player movement and input
        playerMovement.enabled = true;
        playerInput.enabled = true;

        // lock and hide mouse cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}

