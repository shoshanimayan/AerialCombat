using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScorePopupLogic : MonoBehaviour
{
    //  PRIVATE VARIABLES         //

    protected GameBehaviour _game { get { return GameBehaviour.Instance; } }
    private Vector3 _savePos;

    //  PRIVATE METHODS           //

    private void Start()
    {
        _savePos = transform.position;

        if (_game.GetCombo() >= 20)
            GetComponent<AudioSource>().Play();

        Destroy(gameObject, 3);
    }

    private void Update()
    {
        if (_game == null) { return; }

        _savePos.x = _game.calculateLoopingX(_savePos.x);
        transform.position = _game.ToScreenView(_savePos);
    }

}
