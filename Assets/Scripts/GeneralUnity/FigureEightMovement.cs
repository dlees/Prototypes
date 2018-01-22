using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FigureEightMovement : MonoBehaviour {
    
    public float speed = 1;
    public float xScale = 1;
    public float yScale = 1;

    private float timeSinceStart; 
    private Vector3 startPos;

    void OnEnable() {
        startPos = transform.position;
        timeSinceStart = 0.0f;
    }

    void Update() {
        timeSinceStart += Time.deltaTime;
        transform.position = startPos + (Vector3.right * Mathf.Sin(timeSinceStart / 2 * speed) * xScale -
            Vector3.up * Mathf.Sin(timeSinceStart * speed) * yScale);
    }
}
