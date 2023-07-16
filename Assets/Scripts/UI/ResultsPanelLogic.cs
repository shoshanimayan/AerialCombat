using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResultsPanelLogic : MonoBehaviour
{
    //  INSPECTOR VARIABLES      //

    [SerializeField]
    private TextMeshProUGUI _bestText;
    [SerializeField]
    private TextMeshProUGUI _lastText;
    [SerializeField]
    private GameObject _highScoreText;
    [SerializeField]
    private Animation _fadeInAnim;
    [SerializeField]
    private Animation _highScoreAnim;

    //  PRIVATE VARIABLES         //

    private float _clickTime = 0;
    protected GameBehaviour _game { get { return GameBehaviour.Instance; } }

    //  PRIVATE METHODS           //

    private void Update()
    {
        if (_game == null) { return; }

        if (Input.anyKey && _clickTime <= Time.time)
            _game.RestartGame();
    }

    public void UpdateScoreResults( int best, int last, bool newHighscore = false )
    {
        _fadeInAnim.Play();
        _bestText.text = "BEST: " + string.Format("{0:D8}", best);
        _lastText.text = "LAST: " + string.Format("{0:D8}", last);
        _clickTime = Time.time + 1.5f;

        if (newHighscore)
        {
            _highScoreText.SetActive(true);
            _highScoreAnim.Play();
        }
        
    }

    
}
