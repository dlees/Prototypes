using UnityEngine.UI;
using UnityEngine;
using System.Collections;

public class GameTimer : MonoBehaviour {

    public Text timeSinceStartText;

    private float startTime;

	void Start () {
        startTime = Time.time;
        InvokeRepeating("updateTimerText", 0.0f, 1.0f);
	}

    public void restart()
    {
        CancelInvoke();
        startTime = Time.time;
        InvokeRepeating("updateTimerText", 0.0f, 1.0f);
    }


    private void updateTimerText()
    {
        float timeSoFar = getSecondsSoFar();

        string minutes =  Mathf.Clamp(Mathf.Floor(timeSoFar / 60), 0, 60).ToString("00");
        string seconds = Mathf.Clamp(Mathf.Floor(timeSoFar % 60), 0, 60).ToString("00");

        timeSinceStartText.text = minutes + ":" + seconds;
    }

    public float getSecondsSoFar()
    {
        return Time.time - startTime;
    }
}
