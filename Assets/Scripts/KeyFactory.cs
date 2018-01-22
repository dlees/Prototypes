using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyFactory : ObjectFactory {

    public KeyCode keyCode;
    public string text;

    public GameObject template;
    
    public override GameObject createObject() {
        GameObject key = (GameObject)Instantiate(template);
        key.GetComponent<KeyCodeHolder>().keyCode = keyCode;
        key.GetComponentInChildren<Text>().text = text;

        return key;
    }

}
