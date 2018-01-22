using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetronomeController : MonoBehaviour {

    public GameObject metronome;
    
    public RangeValue rotationGuide;

    public float speed;
    
    private const int LEFT = -1;
    private const int RIGHT = 1;
    private int direction = RIGHT;
    
	void Start () {
		
	}
	
	void Update () {
        rotationGuide.current += speed * direction;     
        metronome.transform.eulerAngles = new Vector3(0,0,rotationGuide.current);
        if (rotationGuide.isAtMax() || rotationGuide.isAtMin()) {
            direction *= -1;
        }
	}
}
