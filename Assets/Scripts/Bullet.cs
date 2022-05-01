using UnityEngine;

public class Bullet : MonoBehaviour {
    public int lifeTime;//寿命
    public void OnTriggerEnter2D(Collider2D collider) {//触发器触发函数
        Destroy(this.gameObject);//直接销毁
    }
    public void OnCollisionEnter2D(Collision2D collision) {//碰撞器触发函数
        Destroy(this.gameObject);//直接销毁
    }

    public void FixedUpdate() {
        if (--lifeTime < 0)//寿终正寝
            Destroy(this.gameObject);
    }
}
