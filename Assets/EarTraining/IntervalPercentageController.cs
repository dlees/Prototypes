using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntervalPercentageController : MonoBehaviour {

    public Text stringToDisplay;
    public Interval interval;

    void Update() {
        stringToDisplay.text = interval.intervalDiff.ToString() + ": " + interval.getPercentageCorrect() + " % " /*+ interval.helpSong.name*/;
    }
}
