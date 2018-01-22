using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardController : MonoBehaviour {

    public RangeValue points;
    public ObjectGenerator generator;
    
    GameObject curKeyGO;
    KeyCode curKeyCode;

    void Start() {
        curKeyGO = generator.generateObjectAtTopOfScreen();
        curKeyCode = curKeyGO.GetComponent<KeyCodeHolder>().keyCode;
    }

	void Update () {

        if (Input.GetKeyDown(curKeyCode)) {
            points.current += 1;
            curKeyGO = generator.generateObjectAtTopOfScreen();
            curKeyCode = curKeyGO.GetComponent<KeyCodeHolder>().keyCode;
        }

	}
}
