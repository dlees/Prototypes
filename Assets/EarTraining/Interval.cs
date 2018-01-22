using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interval {

    public Interval(int intervalDiff_, string name_, SongClip helpSong_) {
        intervalDiff = intervalDiff_;
        name = name_;
        helpSong = helpSong_;
    }

    public int intervalDiff;
    public string name;
    public SongClip helpSong;

    public int numCorrect;
    public int numMissed;
    public Dictionary<int, int> intervalMixups = new Dictionary<int, int>();

    public void addAttempt(int guessedInterval) {
        if (guessedInterval == intervalDiff) {
            numCorrect++;

        } else {
            numMissed++;
            //intervalMixups[guessedInterval]++;
        }
    }

    public float getPercentageCorrect() {
        if (numMissed + numCorrect == 0) {
            return 0;
        }

        return 100*numCorrect / (numCorrect + numMissed);
    }

}
