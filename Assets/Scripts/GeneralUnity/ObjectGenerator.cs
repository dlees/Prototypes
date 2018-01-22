using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectGenerator : MonoBehaviour {
    public ObjectFactory[] gameObjects;

    /** 
     * Generates the ith object in a random place between minPos and maxPos
     * */
    public GameObject generateObject(int indexToChoose, Vector2 minPos, Vector2 maxPos) {
        return generateObject(indexToChoose, new Vector2(Random.Range(minPos.x, maxPos.x), Random.Range(minPos.y, maxPos.y)));
    }

    public GameObject generateObject(Vector2 minPos, Vector2 maxPos) {
        return generateObject(Random.Range(0, gameObjects.Length), minPos, maxPos);
    }
    
    public GameObject generateObject(Vector2 pos) {
        return generateObject(Random.Range(0, gameObjects.Length), pos);
    }

    public GameObject generateObject(int indexToChoose, Vector2 pos) {
        GameObject newObject = gameObjects[indexToChoose].createObject();

        newObject.transform.position = pos;
        newObject.transform.parent = transform;

        return newObject;
    }

    public GameObject generateObjectAtTopOfScreen() {
        Vector2 min = new Vector2(0, 0);
        Vector2 max = new Vector2(Screen.width, Screen.height-50);
        return generateObject(new Vector2(min.x, max.y), new Vector2(max.x, max.y));
    }
}
