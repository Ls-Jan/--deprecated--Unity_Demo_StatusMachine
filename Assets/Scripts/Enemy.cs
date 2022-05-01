using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Character {
    public Character target;
    public Enemy_Property property;
    [SerializeField]
    private XJ_StatusMachine machine;

    void Start() {
        machine = ScriptableObject.Instantiate<Enemy_Property>(property).machine;//要用复制的
    }


    new void FixedUpdate() {
        base.FixedUpdate();
        machine.UpdateStatus();//刷新状态机状态
        var dire = (Vector2)target.transform.position - (Vector2)transform.position;//目标所在方位
        var stat = machine.GetStatus();//当前状态

        if (stat == "Slow")
            Move(dire.normalized*moveSpeed/4f);
        else if(stat=="Fast")
            Move(dire.normalized * moveSpeed);
        else if (stat == "Shoot") {
            machine.SetParameter("shootCD", shootCD+(int)Random.Range(0, 2*shootCD));
//            Shoot(dire.normalized * bulletSpeed);//一般射击
            Shoot(Forecast((Vector2)target.transform.position, (Vector2)transform.position, target.GetComponent<Rigidbody2D>().velocity, bulletSpeed));//预判射击
        }
        machine.SetParameter("dist", (int)dire.sqrMagnitude);
        machine.SetParameter("shootCD", machine.GetParameter("shootCD") - 1);

    }


    private static Vector2 Forecast(Vector2 posA, Vector2 posB, Vector2 vector_A, float speed_B) {//子弹预测，返回子弹B的速度矢量(如果为0说明子弹追不上)
        double dX = posA.x - posB.x;
        double dY = posA.y - posB.y;
        double vX = vector_A.x;
        double vY = vector_A.y;

        //一元二次方程组At^2+Bt+C=0，t为时间。求解出时间t
        double A = vX * vX + vY * vY - speed_B * speed_B;
        double B = 2 * (vX * dX + vY * dY);
        double C = dX * dX + dY * dY;
        double D = B * B - 4 * A * C;
        if (D > 0) {//delta值小于0则无解
            D = System.Math.Sqrt(D);
            A = 2 * A;
            double t1 = (-B + D) / A;
            double t2 = (-B - D) / A;
            double t = 0;//符合条件的t值
            if (t1 > 0 && t2 > 0)
                t = System.Math.Min(t1, t2);
            else
                t = System.Math.Max(t1, t2);
            if (t > 0 && t < 10000)
                return new Vector2((float)(dX / t + vX), (float)(dY / t + vY));
        }
        return new Vector2(0, 0);
    }


}
