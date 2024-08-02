using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyActionEvents : MonoBehaviour
{
    EnemyFSM enemyState;

    private void Start()
    {
        // �θ� ������Ʈ���� EnemyFSM ������Ʈ�� �����´�.
        enemyState = transform.parent.GetComponent<EnemyFSM>();
    }

    // ���� ���¸� AttackDelay ���·� ��ȯ�Ѵ�.
    public void ChangeToAttackDelay()
    {
        enemyState.myState = EnemyFSM.EnemyState.AttackDelay;
        print("My State: Attack -> AttackDelay");
        enemyState.attackType = Random.Range(0, 1.0f);
    }

    // Ÿ���� ���� �������� �ο��Ѵ�.
    public void AttackHit()
    {
        enemyState.Attack();
    }
}
