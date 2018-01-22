using UnityEngine;
using System.Collections;

public class RangeValueBarListener : RangeValueListener {
    public RectTransform background;
    public RectTransform foreground;

    override public void updateValue(RangeValue value) {
        foreground.sizeDelta = new Vector2(value.getCurrentInNewRange(0, background.sizeDelta.x), foreground.sizeDelta.y);
    }
}
