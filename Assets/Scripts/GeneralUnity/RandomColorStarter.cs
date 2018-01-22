using UnityEngine;
using System.Collections;

public class RandomColorStarter : MonoBehaviour {

	Color[] starColors = {
		new Color(0.5f, 0.5f, 1.0f),
		new Color(0.0f, 1.0f, 1.0f),
		new Color(1.0f, 1.0f, 0.0f),
		new Color(1.0f, 0.0f, 0.0f),
		new Color(1.0f, 0.0f, 0.0f)
	};


	// Use this for initialization
	void Start () {
		GetComponent<SpriteRenderer> ().color = starColors [Random.Range(0, starColors.Length)];
	}

}
