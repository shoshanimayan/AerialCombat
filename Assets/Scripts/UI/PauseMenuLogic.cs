using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuLogic : MonoBehaviour
{

    //  INSPECTOR VARIABLES      //

    [SerializeField]
    private GameObject _optionsMenu;

    //  PRIVATE VARIABLES         //

    protected GameBehaviour _game { get { return GameBehaviour.Instance; } }

    //  PUBLIC API               //

    public void ResumeGame()
    {
        _game.PauseGame(false);
    }

    public void OpenOptions()
    {
        gameObject.SetActive(false);
        _optionsMenu.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
