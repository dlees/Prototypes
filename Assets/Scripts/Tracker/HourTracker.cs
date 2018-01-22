using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HourTracker : MonoBehaviour {

    public RangeValue timeLeftInHour;

    public RangeValue zone;
    public RangeValue demand;
    public RangeValue delusion;
    public RangeValue distraction;

    public TimeTargetHistory history;
    
    public int curHour = 0;
    
	void Start () {
		
	}
	
	void Update () {
        if (timeLeftInHour.current == timeLeftInHour.min) {

            saveTimeTarget();
            resetTimeTargets();

            curHour++;
            timeLeftInHour.current = timeLeftInHour.max;
        }
	}

    private void resetTimeTargets() {

        zone.current = 0;
        demand.current = 0;
        delusion.current = 0;
        distraction.current = 0;
    }

    private void saveTimeTarget() {
        history.saveHistory((int)zone.current, (int)demand.current, (int)delusion.current, (int)distraction.current);
    }

}
