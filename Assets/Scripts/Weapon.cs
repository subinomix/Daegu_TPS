using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class WeaponInfo
{
    public float damagePower;
    public int bulletCount;
    public float range;
    public WeaponType type;
}

public class Weapon : MonoBehaviour
{
    public WeaponInfo weaponInfo;

    
    // Update is called once per frame
    void Update()
    {
        // y������ ȸ���� �Ѵ�.
        transform.Rotate(Vector3.up, 0.5f);
    }

    private void OnTriggerEnter(Collider other)
    {
        // ���� �浹�� ����� PlayerFire ������Ʈ�� ������ �ִٸ�...
        PlayerFire pFire = other.GetComponent<PlayerFire>();

        if (pFire != null)
        {
            // PlayerFire ������Ʈ�� weaponInfo �����͸� �ѱ��.
            pFire.ChangeWeapon(weaponInfo);

            // �ڽ��� �������.
            Destroy(gameObject);
        }
    }
}
