using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateArrow : MonoBehaviour {
    void FixedUpdate() {
        var mouse = (Vector2)Camera.main.ScreenToWorldPoint((Vector2)Input.mousePosition);//鼠标位置
        var pos = (Vector2)transform.position;//中心位置
        transform.up = pos - mouse;//旋转
    }
}
