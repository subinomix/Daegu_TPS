using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum WeaponType
{
    None,
    Pistol,
    SniperGun,
}

public class PlayerFire : MonoBehaviour
{
    public GameObject bulletFXObject;
    public GameObject grenadePrefab;
    public Vector3 direction;
    public float throwPower = 5;
    public Transform firePosition;
    public WeaponInfo myWeaponInfo;
    public GameObject[] weaponSockets = new GameObject[2];
    public GameObject[] weaponUI = new GameObject[3];

    // ����ź ���� �׸���� ����
    public float simulationTime = 5.0f;
    public float interval = 0.1f;
    public float mass = 5;
    public float grenadeRange = 5.0f;
    public GameObject targetTexture;
    public Animator myAnim;


    List<Vector3> trajectory = new List<Vector3>();
    ParticleSystem bulletEffect;
    LineRenderer line;
    FollowCamera followCam;

    void Start()
    {
        // Ŀ���� ���Ӻ� �ȿ� ���д�.
        Cursor.lockState = CursorLockMode.Locked;

        bulletEffect = bulletFXObject.GetComponent<ParticleSystem>();
        line = firePosition.GetComponent<LineRenderer>();
        if (targetTexture != null)
        {
            targetTexture.transform.localScale = new Vector3(grenadeRange, grenadeRange, 1);
            targetTexture.SetActive(false);
        }

        followCam = Camera.main.GetComponent<FollowCamera>();
    }

    void Update()
    {
        
        switch (myWeaponInfo.type)
        {
            case WeaponType.None:

                break;
            case WeaponType.Pistol:
                FireType1();
                FireType2();
                break;
            case WeaponType.SniperGun:
                FireType1();
                FireType3();
                break;
            default:
                break;
        }
    }

    
    void FireType1()
    {
        // ����, ���콺 ���� ��ư�� �����ٸ�, ���� ���� �������� �Ѿ��� �߻��ϰ� �ʹ�.
        // 1. ���콺 ���� ��ư �Է� üũ
        if (Input.GetMouseButtonDown(0))
        {
            if(myWeaponInfo.type == WeaponType.Pistol)
            {
                myAnim.SetTrigger("FirePistol");
            }
            else if(myWeaponInfo.type == WeaponType.SniperGun)
            {
                myAnim.SetTrigger("FireRifle");
            }


            // 2. ����, ���� ����, üũ �Ÿ�
            // 2-1. ���̸� �����.
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

            // 2-2. ���̰� �浹�� ����� ������ ��� ���� ����ü�� �����Ѵ�.
            RaycastHit hitInfo;

            // 2-3. ������� ���̸� ������ ����� �Ÿ���ŭ �߻��Ѵ�.
            bool isHit = Physics.Raycast(ray, out hitInfo, myWeaponInfo.range, ~(1<<7));

            // 2-4. ����, ���̰� �浹�� �ߴٸ� ���̰� ���� ��ġ�� �Ѿ� ����Ʈ�� ǥ���Ѵ�.
            if (isHit)
            {
                //print(hitInfo.transform.name);
                //GameObject go = Instantiate(bulletFXObject, hitInfo.point, Quaternion.identity);

                // ����, �浹�� ����� EnemyFSM ������Ʈ�� ������ �ִٸ�...
                EnemyFSM enemy = hitInfo.transform.GetComponent<EnemyFSM>();

                if (enemy != null)
                {
                    // EnemyFSM�� TakeDamage �Լ��� �����Ѵ�.
                    enemy.TakeDamage(myWeaponInfo.damagePower, ray.direction, transform);
                }
                // �׷��� �ʴٸ�...
                else
                {
                    bulletFXObject.transform.position = hitInfo.point;

                    // �浹 ������ ���� �������� ����Ʈ�� ȸ����Ų��.
                    bulletFXObject.transform.forward = hitInfo.normal;

                    bulletEffect.Play();
                }
            }
        }
    }

    void FireType2()
    {
        // ����, ���콺 ���� ��ư�� ������ �ִٸ�...
        if(Input.GetMouseButton(1))
        {
            // ����ź�� ���ư��� ������ �׸���.
            Vector3 startPos = firePosition.position;
            //Vector3 dir = transform.TransformDirection(direction);
            //dir.Normalize();
            Vector3 dir = Camera.main.transform.forward + Camera.main.transform.up;
            dir.Normalize();

            Vector3 gravity = Physics.gravity;
            int simulCount = (int)(simulationTime / interval);


            Vector3 hitNormal = Vector3.zero;
            trajectory.Clear();
            for(int i = 0; i < simulCount; i++)
            {
                float currentTime = interval * i;

                // p = p0 + vt - 0.5 * g * t^2 * m^2;
                Vector3 result = startPos + dir * throwPower * currentTime + 0.5f * gravity * currentTime * currentTime * MathF.Pow(mass, 2);

                // ���� result ��ġ�� ���� ��ġ ���̿� �浹�� ��ü�� �ִ��� Ȯ���Ѵ�.
                // Raycast�� �̿�
                if (trajectory.Count > 0)
                {
                    Vector3 rayDir = result - trajectory[trajectory.Count - 1];
                    Ray ray = new Ray(trajectory[trajectory.Count - 1], rayDir.normalized);
                    RaycastHit hitInfo;

                    // ����, �ε��� ����� �ִٸ�...
                    if (Physics.Raycast(ray, out hitInfo, rayDir.magnitude))
                    {
                        // �� ������ ����Ʈ�� �߰��ϰ� �ݺ����� �����Ѵ�.
                        trajectory.Add(hitInfo.point);
                        hitNormal = hitInfo.normal * 0.01f;
                        break;
                    }
                    // �׷��� �ʴٸ�...
                    else
                    {
                        // result ��ġ�� ����Ʈ�� �߰��Ѵ�.
                        trajectory.Add(result);
                    }
                }
                else
                {
                    trajectory.Add(result);
                }
            }

            // ���� �������� trajectory �������� ȭ�鿡 �׸���.
            line.positionCount = trajectory.Count;
            line.SetPositions(trajectory.ToArray());
            line.startWidth = 0.1f;
            line.endWidth = 0.1f;
            //line.startColor = Color.white;
            //line.endColor = Color.white;

            // ź�� ������ ���� �ؽ��� ������Ʈ�� ��ġ�ϰ� Ȱ��ȭ�Ѵ�.
            targetTexture.transform.position = trajectory[trajectory.Count - 1] + hitNormal;
            targetTexture.transform.forward = hitNormal * -100;
            targetTexture.SetActive(true);
        }
        // ����, ���콺�� ���� ��ư�� �����ٰ� ����...
        else if (Input.GetMouseButtonUp(1))
        {
            // ����ź �������� �����ϰ�, ���������� �߻��Ѵ�.
            GameObject bomb = Instantiate(grenadePrefab, firePosition.position, firePosition.rotation);

            // ����ź�� �������� �ڽ��� ����Ѵ�.
            bomb.GetComponent<GrenadeExplosion>().master = transform;

            Rigidbody rb = bomb.GetComponent<Rigidbody>();
            if(rb != null)
            {
                rb.mass = mass;

                //Vector3 dir = transform.TransformDirection(direction);
                //dir.Normalize();
                Vector3 dir = Camera.main.transform.forward + Camera.main.transform.up;
                dir.Normalize();

                // ���������� �߻��ϱ�
                rb.AddForce(dir * throwPower, ForceMode.Impulse);
            }

            targetTexture.SetActive(false);

            // ���� �������� �迭 ���̸� 0���� �ʱ�ȭ�Ѵ�.
            line.positionCount = 0;
        }
    }


    void FireType3()
    {
        //  ���콺 ���� ��ư�� ������...
        if (Input.GetMouseButtonDown(1))
        {
            // ī�޶� ����(Ȯ��) �ȴ�.
            followCam.ZoomIn(true);

            // �������� UI�� ǥ���Ѵ�.
        }
       // ���콺 ���� ��ư�� ����...
        else if (Input.GetMouseButtonUp(1))
        {

            // ī�޶��� ������ �ٽ� ������� �������´�.
            // �������� UI�� �����ϰ� ó���Ѵ�.
            followCam.ZoomIn(false);
        }
    }


    // ���� ���� �� �ִϸ��̼� ���� �Լ�
    public void ChangeWeapon(WeaponInfo weaponInfo)
    {
        // ���� ������ �����Ѵ�.
        myWeaponInfo = weaponInfo;

        // ��� ���� �𵨸��� UI�� ��Ȱ��ȭ�Ѵ�.
        for (int i = 0; i < weaponSockets.Length; i++)
        {
            weaponSockets[i].SetActive(false);
        }
        for (int i = 0; i < weaponUI.Length; i++)
        {
            weaponUI[i].SetActive(false);
        }
        weaponUI[0].SetActive(true);
        weaponUI[0].GetComponent<Text>().text = $"Ammo: <color=#FFFF00><size=60>{weaponInfo.bulletCount}</size></color>";


        // WeaponType�� �´� ���� �𵨸��� Ȱ��ȭ�Ѵ�.
        if (weaponInfo.type == WeaponType.Pistol)
        {
            weaponSockets[0].SetActive(true);
            weaponUI[1].SetActive(true);
        }
        else if(weaponInfo.type == WeaponType.SniperGun)
        {
            weaponSockets[1].SetActive(true);
            weaponUI[2].SetActive(true);
        }

        // �ִϸ������� SelectedIdle �Ķ������ ���� ���� Ÿ�Կ� �ִ� ������ �����Ѵ�.
        int typeNumber = (int)weaponInfo.type;
        myAnim.SetFloat("SelectedIdle", (float)typeNumber);
    }



    // Scene View�� ����� �׸��� �̺�Ʈ �Լ�
    private void OnDrawGizmos()
    {
        // trajectory ����Ʈ�� ���� ������ �Լ� ����
        if(trajectory.Count < 1)
        {
            return;
        }

        // ������ ������ ������� �����Ѵ�.
        Gizmos.color = Color.green;

        // trajectory ����Ʈ�� ��� ��ȣ�� �����Ͽ� ������ �׸���.
        for (int i = 0; i < trajectory.Count - 1; i++)
        {
            Gizmos.DrawLine(trajectory[i], trajectory[i + 1]);
        }

        
    }

}
