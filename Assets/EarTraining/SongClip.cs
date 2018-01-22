using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SongClip {
    public string name;
    public List<int> intervals;
    public List<int> timings;

    public SongClip(string name_, List<int> intervals_, List<int> timings_) {
        name = name_;
        intervals = intervals_;
        timings = timings_;
    }
}
