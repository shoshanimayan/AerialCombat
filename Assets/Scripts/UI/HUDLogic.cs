using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HUDLogic : MonoBehaviour
{

    //  INSPECTOR VARIABLES      //

    [SerializeField]
    GameObject _startText;

    [SerializeField]
    GameObject _scoreText;

    [SerializeField]
    GameObject _tempScoreText;

    [SerializeField]
    GameObject _comboMeterText;

    private int _curScore = 0;
    private bool _playedComboSound = false;
    protected GameBehaviour _game { get { return GameBehaviour.Instance; } }

    //  PRIVATE METHODS           //

    private void Update()
    {
        if (!_game) { return; }

        _startText.SetActive(!_game.IsGameActive());

        UpdateScore();
        UpdateTempScore();
        UpdateComboMeter();
    }

    private void UpdateScore()
    {
        _scoreText.SetActive(_game.IsGameActive() && !_game.IsPlayerDead());

        if (!_scoreText.activeSelf)
            return;

        _curScore = Mathf.RoundToInt(Mathf.Clamp(_curScore + 5000 * Time.deltaTime, 0, _game.GetScore()));

        _scoreText.GetComponent<TextMeshProUGUI>().text = string.Format("{0:D6}", _curScore );
    }

    private void UpdateTempScore()
    {
        _tempScoreText.SetActive(_game.GetTempScore() > 0);

        if (!_tempScoreText.activeSelf)
            return;

        _tempScoreText.GetComponent<TextMeshProUGUI>().text = string.Format("{0:D6}", _game.GetTempScore());
    }

    private void UpdateComboMeter()
    {
        _comboMeterText.SetActive(_game.GetTempScore() > 0);

        if (!_comboMeterText.activeSelf)
            return;

        _comboMeterText.GetComponent<TextMeshProUGUI>().text = _game.GetCombo() >= 20 ? "MAX!" : "x" + _game.GetCombo();


        float offset_delta = _game.GetComboDelta() * 0.39f;
        _comboMeterText.GetComponent<TextMeshProUGUI>().materialForRendering.SetTextureOffset( "_FaceTex", new Vector2( 0, 0.42f - offset_delta ) );

        float white_flash;

        if (_game.GetComboDelta() >= 0.9)
        {
            if (!_playedComboSound)
            {
                var audio = _comboMeterText.GetComponent<AudioSource>();
                audio.Stop();
                audio.Play();
                _playedComboSound = true;
            }
            white_flash = Mathf.PingPong(Time.time * 15, 1);
        }
        else
        {
            _playedComboSound = false;
            white_flash = 1;
        }
            

        var col = _comboMeterText.GetComponent<TextMeshProUGUI>().color;
        col.a = white_flash;

        _comboMeterText.GetComponent<TextMeshProUGUI>().color = col;
    }

}
