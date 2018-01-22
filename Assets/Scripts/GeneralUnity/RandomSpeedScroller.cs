using UnityEngine;
using System.Collections;

public class RandomSpeedScroller : MonoBehaviour {

	public Vector2 minVelo;
	public Vector2 maxVelo;

	private Vector2 velocity;

	void Start() {
		velocity = new Vector2(Random.Range(minVelo.x, maxVelo.x), Random.Range(minVelo.y, maxVelo.y));;
	}
	
	// Update is called once per frame
	void Update () {
		Vector2 pos = transform.position;
		pos += velocity;
		transform.position = pos;       
	}
}
