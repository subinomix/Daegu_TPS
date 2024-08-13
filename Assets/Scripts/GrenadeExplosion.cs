using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// Attribute�� �ʿ� ������Ʈ �ڵ� �߰��ϱ�
[RequireComponent(typeof(Rigidbody))]
public class GrenadeExplosion : MonoBehaviour
{
    [Header("���� �ݰ�")]
    [Range(0.0f, 10.0f)]
    public float radius = 5.0f;
    public float damagePower = 50.0f;
    public Transform master;
    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
  
    // ����, �ٴڿ� ��Ҵٸ�...
    private void OnCollisionEnter(Collision collision)
    {
        // ���� ���� ��ġ�� �������� �ݰ� radius ���� ���� ��� ���� ������Ʈ�� ã�´�(�迭).
        Collider[] cols = Physics.OverlapSphere(transform.position, radius, 1<<6);

        // ����, ã���� ���� �ִٸ�
        if (cols.Length > 0)
        {
            // ã�� ������Ʈ�� ��� �ı��Ѵ�.
            for(int i = 0; i < cols.Length; i++)
            {
                //Destroy(cols[i].gameObject);
                Rigidbody rb = cols[i].gameObject.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    rb.AddExplosionForce(1000, transform.position, radius, 300);
                }

                // ����, ã�� ������Ʈ�� EnemyFSM ������Ʈ�� �ִٸ�...
                EnemyFSM enemy = cols[i].transform.GetComponent<EnemyFSM>();
                if (enemy != null)
                {
                    // EnemyFSM ������Ʈ�� TakeDamage �Լ��� �����Ѵ�.
                    Vector3 hitDir = cols[i].transform.position - transform.position;
                    hitDir.Normalize();
                    enemy.TakeDamage(damagePower, hitDir, master);
                }
            }
        }

        Destroy(gameObject);
    }
}
