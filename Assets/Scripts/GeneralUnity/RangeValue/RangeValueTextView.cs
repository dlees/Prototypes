using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RangeValueTextView : MonoBehaviour {
    
    public RangeValue value;
    public Text text;
    public string formatString = "{0}";

	void Update () {
        text.text = String.Format(formatString, value.current);
	}
}
