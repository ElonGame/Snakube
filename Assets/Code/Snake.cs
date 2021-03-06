﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class Snake : MonoBehaviour
{
    public AudioClip failSound;
    public AudioClip pickupSound;
    public AudioClip themeSound;
    public TextMesh pressKey;
    public TextMesh counter;
    public float moveSpeed = 0.1f;
    public float moveSpeedAccel = 0.01f;
    public GameObject tailPrefab;
    public GameObject foodPrefab;

    bool _inputEnabled;
    int _score;
    bool _papu;
    Vector2 _snakeDir = Vector2.right;
    List<Transform> _tail = new List<Transform>();
    
    Vector3 _textScale;
    int _startTextIndex;
    string[] _startText = new[] {
        "[Press any key]",
        "I dare you!",
        "more than",
    };


    void Start() {

        _textScale = counter.transform.localScale;

        iTween.ScaleTo( pressKey.gameObject, new Hashtable {
            { "scale", 1.5f * pressKey.transform.localScale },
            { "time", 1f },
            { "looptype", iTween.LoopType.pingPong },
        } );

        iTween.ColorTo( pressKey.gameObject, new Hashtable {
            { "color", 1.5f * pressKey.color },
            { "time", 1f },
            { "looptype", iTween.LoopType.pingPong }
        } );

        Invoke( "ChangeStartText", 2f );
    }

    void ChangeStartText() {

        ++_startTextIndex;

        if ( _startTextIndex == _startText.Length ) {
            pressKey.text = PlayerPrefs.GetInt( "score" ) + " pts";
            _startTextIndex = -1;

        } else {
            pressKey.text = _startText[_startTextIndex];
        }

        Invoke( "ChangeStartText", 2f );
    }

    void OnTriggerEnter( Collider coll ) {

        if ( _inputEnabled ) {
            if ( coll.gameObject.tag == "Food" ) {

                OnFoodCollected( coll );
            }
        }
    }

    void OnCollisionEnter( Collision coll ) {

        if ( _inputEnabled ) {
            if ( coll.gameObject.tag == "Wall"
                || coll.gameObject.tag == "Tail" ) {

                OnFail();
            }
        }
    }

    void Update() {

        if ( _inputEnabled ) {

            if ( Input.GetKey( KeyCode.W ) || Input.GetKey( KeyCode.UpArrow ) ) {
                if ( _snakeDir != -Vector2.up ) {
                    _snakeDir = Vector2.up;
                }
            }

            if ( Input.GetKey( KeyCode.A ) || Input.GetKey( KeyCode.LeftArrow ) ) {
                if ( _snakeDir != Vector2.right ) {
                    _snakeDir = -Vector2.right;
                }
            }

            if ( Input.GetKey( KeyCode.D ) || Input.GetKey( KeyCode.RightArrow ) ) {
                if ( _snakeDir != -Vector2.right ) {
                    _snakeDir = Vector2.right;
                }
            }

            if ( Input.GetKey( KeyCode.S ) || Input.GetKey( KeyCode.DownArrow ) ) {
                if ( _snakeDir != Vector2.up ) {
                    _snakeDir = -Vector2.up;
                }
            }

        } else {

            if ( Input.anyKeyDown ) {
                _inputEnabled = true;
                OnStartGame();
            }
        }
    }

    void Move() {

        if ( _inputEnabled ) {

            Vector2 wektor = transform.position;
            transform.Translate( _snakeDir );

            if ( _papu ) {
                GameObject g = Instantiate( tailPrefab, wektor, Quaternion.identity ) as GameObject;
                _tail.Insert( 0, g.transform );
                _papu = false;

            } else if ( _tail.Count > 0 ) {
                _tail.Last().position = wektor;
                _tail.Insert( 0, _tail.Last() );
                _tail.RemoveAt( _tail.Count - 1 );
            }
        }

        Invoke( "Move", moveSpeed );
    }

    void SpawnFood() {

        Vector3 randomFoodPos = new Vector3(
            (int)( AdjustWalls.bounds.x - 3f ) * 2f * ( Random.value - 0.5f ),
            (int)( AdjustWalls.bounds.y - 3f ) * 2f * ( Random.value - 0.5f ),
            0f
        );

        GameObject newFood = Instantiate( foodPrefab, randomFoodPos, Quaternion.identity ) as GameObject;
        iTween.ScaleTo( newFood.gameObject, new Hashtable {
            { "scale", 1.5f * newFood.transform.localScale },
            { "time", 1f },
            { "looptype", iTween.LoopType.pingPong }
        } );

        iTween.ColorTo( newFood.gameObject, new Hashtable {
            { "color", 1.5f * pressKey.color },
            { "time", 1f },
            { "looptype", iTween.LoopType.pingPong }
        } );
    }

    void OnStartGame() {

        counter.transform.localScale *= 1.5f;
        counter.text = "0";
        pressKey.gameObject.SetActive( false );

        SpawnFood();
        Invoke( "Move", moveSpeed );

        AudioPlayer.Instance.PlayAtMainCamera( themeSound,
            volume: 0.7f,
            autoDestroy: false
        ).loop = true;
    }

    void OnFoodCollected( Collider coll ) {

        coll.enabled = false;

        _score++;
        counter.text = _score.ToString();
        moveSpeed -= moveSpeedAccel;
        _papu = true;

        SpawnFood();
        if ( Random.value > 0.99 ) {
            SpawnFood();
        }

        AudioPlayer.Instance.PlayAtMainCamera( pickupSound );
        CounterTextEffect();

        iTween.ScaleTo( coll.gameObject, new Hashtable {
            { "scale", 3f * coll.transform.localScale },
            { "time", 1f },
        } );

        iTween.ColorTo( coll.gameObject, new Hashtable {
            { "r", 2f * coll.gameObject.renderer.material.color.r },
            { "g", 2f * coll.gameObject.renderer.material.color.g },
            { "b", 2f * coll.gameObject.renderer.material.color.b },
            { "a", 0f },
            { "time", 0.4f }
        } );

        Destroy( coll.gameObject, 1f );
    }

    void CounterTextEffect() {

        GameObject textEffect = GameObject.Instantiate( counter.gameObject, counter.transform.position, Quaternion.identity ) as GameObject;
        TextMesh effectText = textEffect.GetComponent<TextMesh>();
        effectText.text = _score.ToString();

        iTween.ScaleTo( textEffect.gameObject, new Hashtable {
            { "scale", 3f * counter.transform.localScale },
            { "time", 0.4f },
        } );

        iTween.ColorTo( textEffect.gameObject, new Hashtable {
            //{ "r", 2f * pressKey.color.r },
            //{ "g", 2f * pressKey.color.g },
            //{ "b", 2f * pressKey.color.b },

            { "r", 1f },
            { "g", 1f },
            { "b", 1f },

            { "a", 0f },
            { "time", 0.4f }
        } );

        Destroy( textEffect, 0.4f );
    }

    void OnFail() {

        _inputEnabled = false;
        counter.text = "Fail";

        counter.transform.localScale = _textScale;
        int bestScore = PlayerPrefs.GetInt( "score" );
        if ( _score > bestScore ) {
            PlayerPrefs.SetInt( "score", _score );
        }

        AudioPlayer.Instance.PlayAtMainCamera( failSound );

        GameObject tmpHead = GameObject.Instantiate( tailPrefab, transform.position, Quaternion.identity ) as GameObject;

        iTween.ScaleTo( tmpHead, new Hashtable {
            { "scale", 5f  * transform.localScale },
            { "time", 1f },
        } );

        iTween.ColorTo( tmpHead, new Hashtable {
            { "r", 2f * tmpHead.gameObject.renderer.material.color.r },
            { "g", 2f * tmpHead.gameObject.renderer.material.color.g },
            { "b", 2f * tmpHead.gameObject.renderer.material.color.b },
            { "a", 0f },
            { "time", 0.5f }
        } );

        _tail.ForEach( t => iTween.ScaleTo( t.gameObject, new Hashtable {
            { "scale", 0.01f * Vector3.one },
            { "time", 1f },
        } ) );

        Invoke( "Reload", 1f );
    }

    void Reload() {

        Application.LoadLevel( Application.loadedLevel );
    }
}
