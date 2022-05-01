using UnityEngine;

public abstract class Character : MonoBehaviour {
    public Bullet bulletPrefab;
    public float moveSpeed;
    public float bulletSpeed;
    public int shootCD;
    private int cd;

    protected void FixedUpdate() {
        if (cd > 0)
            --cd; 
    }

    public void Move(Vector2 dire) {//以dire为方向进行移动
        GetComponent<Rigidbody2D>().velocity=dire;
    }
    public void Shoot(Vector2 dire) {//向dire发射子弹
        if (cd > 0)
            return;
        cd = shootCD;
        Bullet bullet= Instantiate(bulletPrefab, transform) as Bullet;
        bullet.transform.position = transform.position;
        bullet.GetComponent<Rigidbody2D>().velocity = dire;
        bullet.gameObject.layer = LayerMask.NameToLayer("Bullet_" + LayerMask.LayerToName(gameObject.layer).Split('_')[1]);
    }
}
