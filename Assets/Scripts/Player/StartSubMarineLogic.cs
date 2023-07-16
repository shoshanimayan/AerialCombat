using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartSubMarineLogic : MonoBehaviour
{

    //  INSPECTOR VARIABLES      //


    [SerializeField]
    private AudioSource _launchSound;

    //  PRIVATE VARIABLES         //

    protected GameBehaviour _game { get { return GameBehaviour.Instance; } }
    private float _moveTime = 0;
    private float _moveDur = 1;
    private bool _moveUp = false;
    private bool _shouldHide = false;

    //  PRIVATE METHODS           //

    private void Start()
    {
        MoveSubMarine( true, 1);
    }


    private void Update()
    {
        if (!_game) { return; }
        if (!_game.PlayerEntity) { return;}


            if (_game.PlayerEntity.activeSelf && !_shouldHide)
            {
                _shouldHide = true;
                MoveSubMarine(false, 3);
                Invoke("DoDestroy", 5);
                _launchSound.Play();
            }

            float cur_y = Mathf.Clamp(1 - (_moveTime - Time.time) / _moveDur, 0, 1);

            if (!_moveUp)
                cur_y = 1 - cur_y;

            gameObject.transform.position = new Vector3(0, _game.GetWaterLevel() + cur_y * 4 - 4, 0);
        
    }

    private void DoDestroy()
    {
        Destroy(gameObject);
    }

    //  PUBLIC API               //

    public void MoveSubMarine( bool up, float dur )
    {
        _moveDur = dur;
        //moveY = up ? 1 : 0;
        _moveUp = up;
        _moveTime = Time.time + dur;
    }

  
}
