using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonActivator : MonoBehaviour {

    public ListHolder indiciesToActivate;
    public List<Button> buttons;

	void Update () {
        HashSet<int> indiciesSet = new HashSet<int>(indiciesToActivate.list);

        for (int i = 0; i < buttons.Count; i++ ) {
            if (buttons[i] != null) {
                buttons[i].gameObject.SetActive(indiciesSet.Contains(i));
            }
        }
	}
}
