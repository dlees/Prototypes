using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeTargetHistory : MonoBehaviour {

    public List<int> zone;
    public List<int> demand;
    public List<int> delusion;
    public List<int> distraction;


    public RangeValue zoneTotal;
    public RangeValue demandTotal;
    public RangeValue delusionTotal;
    public RangeValue distractionTotal;

	void Start () {
        zone = new List<int>();
        demand = new List<int>();
        delusion = new List<int>();
        distraction = new List<int>();
	}
	
    public void saveHistory(int zone, int demand, int delusion, int distraction) {
        this.zone.Add(zone);
        zoneTotal.current += zone;

        this.demand.Add(demand);
        demandTotal.current += demand;

        this.delusion.Add(delusion);
        delusionTotal.current += delusion;
        
        this.distraction.Add(distraction);
        distractionTotal.current += distraction;
    }

}
