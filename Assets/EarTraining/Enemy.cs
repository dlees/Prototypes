using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {
    enum EnemyState {
        GoingToDance,
        Dancing,
        Moving,
        OnStaff,
        Dying
    }

    public Behaviour dancingMovement;
    public Transform dancingStartPos;
    
    public Sprite deadSprite;

    private EnemyState state = EnemyState.GoingToDance;

    // Moving
    private Vector3 positionToMoveTowards;
    private EnemyState nextState;

    void Start() {
        dancingMovement.enabled = false;
    }

    void Update() {

        switch (state) {
            case EnemyState.GoingToDance:
                transform.position = Vector3.MoveTowards(transform.position, dancingStartPos.position, 10.0f * Time.deltaTime);
                if (transform.position == dancingStartPos.position) {
                    dancingMovement.enabled = true;
                    state = EnemyState.Dancing;
                }
                break;

            case EnemyState.Dancing:
                break;

            case EnemyState.Moving:
                transform.position = Vector3.MoveTowards(transform.position, positionToMoveTowards, 10.0f * Time.deltaTime);
                if (transform.position == positionToMoveTowards) {
                    state = nextState;
                }
                break;

            case EnemyState.Dying:
                Destroy(gameObject);
                // Destroy(gameObject) 
                // kills the object. In this case, 
                // I'm just gonna change sprites and stop this behavior
                break;


            case EnemyState.OnStaff:
                break;
        }
    }

    public void diveBomb(Vector3 positionToHit) {
        state = EnemyState.Moving;
        nextState = EnemyState.GoingToDance;
        positionToMoveTowards = positionToHit;
        dancingMovement.enabled = false;
    }

    public void defeat(Vector3 positionToDie) {
        state = EnemyState.Moving;
        nextState = EnemyState.Dying;
        positionToMoveTowards = positionToDie;
        dancingMovement.enabled = false;
    }
    
    public void goToStaff(Vector3 positionOnStaff) {
        state = EnemyState.Moving;
        nextState = EnemyState.OnStaff;
        positionToMoveTowards = positionOnStaff;
        dancingMovement.enabled = false;
    }

}
