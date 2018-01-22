using UnityEngine;
using System.Collections;

public class StarGenerator : MonoBehaviour {
	public GameObject[] StarGO;
    public int numberOnStartup = 30;

	void Start() {
        for (int i = 0; i < numberOnStartup; i++)
        {

			Vector2 min = Camera.main.ViewportToWorldPoint (new Vector2 (0, 0));
			Vector2 max = Camera.main.ViewportToWorldPoint (new Vector2 (1, 1));

			GameObject star = (GameObject)Instantiate (StarGO [0]);

			star.transform.position = new Vector2 (Random.Range (min.x, max.x), Random.Range (min.y, max.y));
			star.transform.parent = transform;

        } 
        ScheduleNextSpawn();

	}

	public void StartGenerating() {
		ScheduleNextSpawn ();
	}

	void spawnStar () {
		Vector2 min = Camera.main.ViewportToWorldPoint(new Vector2(0, 0));
		Vector2 max = Camera.main.ViewportToWorldPoint(new Vector2(1, 1));

		int indexToChoose = 0;

		GameObject star = (GameObject)Instantiate (StarGO[indexToChoose]);

		star.transform.position = new Vector2(min.x, Random.Range(min.y, max.y));
		star.transform.parent = transform;

		ScheduleNextSpawn ();
	}


	private void ScheduleNextSpawn()
	{
		Invoke("spawnStar", .5f);
	}

}
