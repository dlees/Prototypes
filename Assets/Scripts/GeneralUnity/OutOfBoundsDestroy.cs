using UnityEngine;
using System.Collections;

public class OutOfBoundsDestroy : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}

	// Update is called once per frame
	void Update () {
		Vector2 min = Camera.main.ViewportToWorldPoint(new Vector2(-0.25f, -0.25f));
		Vector2 max = Camera.main.ViewportToWorldPoint(new Vector2(1.25f, 1.25f));

		if (transform.position.y > max.y || transform.position.y < min.y || transform.position.x > max.x || transform.position.x < min.x)
		{
			Destroy(gameObject);
		}
	}
}
