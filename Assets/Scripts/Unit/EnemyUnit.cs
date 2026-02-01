using UnityEngine;

public class EnemyUnit : Unit
{
    protected override void MoveState()
    {
        transform.position += Vector3.left * moveSpeed * Time.deltaTime;
    }
}
