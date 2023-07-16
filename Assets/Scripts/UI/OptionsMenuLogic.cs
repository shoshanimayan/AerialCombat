using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using TMPro;
using UnityEngine.UI;

public class OptionsMenuLogic : MonoBehaviour
{
    //  INSPECTOR VARIABLES      //

    [SerializeField]
    private GameObject _mainMenu;
    [SerializeField]
    private GameObject _volumeText;
    [SerializeField]
    private GameObject _volumeSlider;
    //  PRIVATE VARIABLES         //

    protected GameBehaviour _game { get { return GameBehaviour.Instance; } }

    //  PRIVATE METHODS           //

    private void Start()
    {
        float vol = PlayerPrefs.GetFloat("MasterVolume", 0);
        _volumeSlider.GetComponent<Slider>().value = vol;
        SetVolume(vol);
    }

    private void SetVolumeNumber(int num)
    {
        _volumeText.GetComponent<TextMeshProUGUI>().text = "VOLUME: " + num.ToString();
    }
    //  PUBLIC API               //

    public void ReturnToMenu()
    {
        gameObject.SetActive(false);
        _mainMenu.SetActive(true);
    }

    public void SetVolume( float vol )
    {
        _game.MainMixer.SetFloat("masterVolume", vol);
        int volume = Mathf.RoundToInt( _volumeSlider.GetComponent<Slider>().normalizedValue * 100 );
        SetVolumeNumber( volume );
        PlayerPrefs.SetFloat("MasterVolume", vol);
    }

   
}
