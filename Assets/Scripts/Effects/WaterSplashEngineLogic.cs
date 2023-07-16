using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WaterSplashEngineLogic : MonoBehaviour
{
    //  PRIVATE VARIABLES         //

    private List<ParticleSystem> _splashes;
    private List<float> _splashVel;
    private float _sizeX = 1;
    private float _sizeY = 1;
    private Vector3 _direction;
    protected GameBehaviour _game { get { return GameBehaviour.Instance; } }
    //  PRIVATE METHODS           //
    private void Awake()
    {
        _splashVel = new List<float> { -5, 5 };
        _splashes = new List<ParticleSystem>();
        foreach (Transform child in transform)
        {
            _splashes.Add(child.GetComponent<ParticleSystem>());
        }
    }


    private void LateUpdate()
    {
        if (!_game) { return; }


        float rotation = GetComponentInParent<Rigidbody2D>().rotation;

        int index = 0;

        foreach (ParticleSystem p in _splashes)
        {
            var main = p.main;
            main.startSizeYMultiplier = _sizeY;

            var scale = p.transform.localScale;
            scale.x = _sizeX;
            p.transform.localScale = scale;

            var vel = p.velocityOverLifetime;
            vel.x = _splashVel[index] + _direction.x * -1 * (10 + _sizeX);
            index += 1;
        }

        var update_pos = transform.parent.position;
        update_pos.y = _game.GetWaterLevel();

        transform.position = update_pos;
        transform.rotation = Quaternion.identity;

        var scale2 = transform.localScale;
        scale2.x = _sizeX * 3;
        transform.localScale = scale2;

    }
    //  PUBLIC API               //

    public void SetPower(float pow)
    {
       
        _sizeX = Mathf.Lerp(_sizeX, 1 + (pow - 1) / 3, 2f * Time.deltaTime);
        _sizeY = Mathf.Lerp(_sizeY, pow * 1.5f, 2f * Time.deltaTime);
    }

    public void SetDirection(Vector3 dir)
    {
        _direction = dir;
    }
}
