using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character {
    new void FixedUpdate() {
        base.FixedUpdate();

        var x = Input.GetAxisRaw("Horizontal");
        var y = Input.GetAxisRaw("Vertical");
        var shoot = Input.GetButton("Fire1");

        Move(new Vector2(x, y)*moveSpeed);
        if (shoot) {
            var mouse = (Vector2)Camera.main.ScreenToWorldPoint((Vector2)Input.mousePosition);//鼠标位置
            var pos = (Vector2)transform.position;//中心位置
            Shoot((mouse-pos).normalized*bulletSpeed);
        }
    }
}
