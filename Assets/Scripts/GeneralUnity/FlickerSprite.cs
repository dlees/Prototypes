using UnityEngine;
using System.Collections;

public class FlickerSprite : MonoBehaviour {

    public SpriteRenderer sprite;

	void Start () {
        InvokeRepeating("flickerOff", 0.5f, 1.0f);
        InvokeRepeating("flickerOn", 0.6f, 1.0f);
	}

    // To accelerate, we need to change this whole thing:
    // http://answers.unity3d.com/questions/22889/random-range-spawntime.html#answer-22891
    public void flickerOff() {
        sprite.enabled = false;
    }

    public void flickerOn()
    {
        sprite.enabled = true;
    }
}
