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

    // 수류탄 궤적 그리기용 변수
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
        // 커서를 게임뷰 안에 가둔다.
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
        // 만일, 마우스 왼쪽 버튼을 눌렀다면, 나의 정면 방향으로 총알을 발사하고 싶다.
        // 1. 마우스 왼쪽 버튼 입력 체크
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


            // 2. 방향, 레이 생성, 체크 거리
            // 2-1. 레이를 만든다.
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

            // 2-2. 레이가 충돌한 대상의 정보를 담기 위한 구조체를 생성한다.
            RaycastHit hitInfo;

            // 2-3. 만들어진 레이를 지정된 방향과 거리만큼 발사한다.
            bool isHit = Physics.Raycast(ray, out hitInfo, myWeaponInfo.range, ~(1<<7));

            // 2-4. 만일, 레이가 충돌을 했다면 레이가 닿은 위치에 총알 이펙트를 표시한다.
            if (isHit)
            {
                //print(hitInfo.transform.name);
                //GameObject go = Instantiate(bulletFXObject, hitInfo.point, Quaternion.identity);

                // 만일, 충돌한 대상이 EnemyFSM 컴포넌트를 가지고 있다면...
                EnemyFSM enemy = hitInfo.transform.GetComponent<EnemyFSM>();

                if (enemy != null)
                {
                    // EnemyFSM의 TakeDamage 함수를 실행한다.
                    enemy.TakeDamage(myWeaponInfo.damagePower, ray.direction, transform);
                }
                // 그렇지 않다면...
                else
                {
                    bulletFXObject.transform.position = hitInfo.point;

                    // 충돌 지점의 법선 방향으로 이펙트를 회전시킨다.
                    bulletFXObject.transform.forward = hitInfo.normal;

                    bulletEffect.Play();
                }
            }
        }
    }

    void FireType2()
    {
        // 만일, 마우스 우측 버튼을 누르고 있다면...
        if(Input.GetMouseButton(1))
        {
            // 수류탄이 날아가는 궤적을 그린다.
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

                // 계산된 result 위치와 직전 위치 사이에 충돌할 물체가 있는지 확인한다.
                // Raycast를 이용
                if (trajectory.Count > 0)
                {
                    Vector3 rayDir = result - trajectory[trajectory.Count - 1];
                    Ray ray = new Ray(trajectory[trajectory.Count - 1], rayDir.normalized);
                    RaycastHit hitInfo;

                    // 만일, 부딪힌 대상이 있다면...
                    if (Physics.Raycast(ray, out hitInfo, rayDir.magnitude))
                    {
                        // 그 지점을 리스트에 추가하고 반복문을 종료한다.
                        trajectory.Add(hitInfo.point);
                        hitNormal = hitInfo.normal * 0.01f;
                        break;
                    }
                    // 그렇지 않다면...
                    else
                    {
                        // result 위치를 리스트에 추가한다.
                        trajectory.Add(result);
                    }
                }
                else
                {
                    trajectory.Add(result);
                }
            }

            // 라인 렌더러로 trajectory 예측선을 화면에 그린다.
            line.positionCount = trajectory.Count;
            line.SetPositions(trajectory.ToArray());
            line.startWidth = 0.1f;
            line.endWidth = 0.1f;
            //line.startColor = Color.white;
            //line.endColor = Color.white;

            // 탄착 지점에 범위 텍스쳐 오브젝트를 배치하고 활성화한다.
            targetTexture.transform.position = trajectory[trajectory.Count - 1] + hitNormal;
            targetTexture.transform.forward = hitNormal * -100;
            targetTexture.SetActive(true);
        }
        // 만일, 마우스의 우측 버튼을 눌렀다가 떼면...
        else if (Input.GetMouseButtonUp(1))
        {
            // 수류탄 프리팹을 생성하고, 물리적으로 발사한다.
            GameObject bomb = Instantiate(grenadePrefab, firePosition.position, firePosition.rotation);

            // 수류탄의 주인으로 자신을 등록한다.
            bomb.GetComponent<GrenadeExplosion>().master = transform;

            Rigidbody rb = bomb.GetComponent<Rigidbody>();
            if(rb != null)
            {
                rb.mass = mass;

                //Vector3 dir = transform.TransformDirection(direction);
                //dir.Normalize();
                Vector3 dir = Camera.main.transform.forward + Camera.main.transform.up;
                dir.Normalize();

                // 물리적으로 발사하기
                rb.AddForce(dir * throwPower, ForceMode.Impulse);
            }

            targetTexture.SetActive(false);

            // 라인 렌더러의 배열 길이를 0으로 초기화한다.
            line.positionCount = 0;
        }
    }


    void FireType3()
    {
        //  마우스 우측 버튼을 누르면...
        if (Input.GetMouseButtonDown(1))
        {
            // 카메라가 줌인(확대) 된다.
            followCam.ZoomIn(true);

            // 스나이퍼 UI를 표시한다.
        }
       // 마우스 우측 버튼을 떼면...
        else if (Input.GetMouseButtonUp(1))
        {

            // 카메라의 배율을 다시 원래대로 돌려놓는다.
            // 스나이퍼 UI를 투명하게 처리한다.
            followCam.ZoomIn(false);
        }
    }


    // 무기 소켓 및 애니메이션 변경 함수
    public void ChangeWeapon(WeaponInfo weaponInfo)
    {
        // 웨폰 정보를 변경한다.
        myWeaponInfo = weaponInfo;

        // 모든 무기 모델링과 UI를 비활성화한다.
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


        // WeaponType에 맞는 무기 모델링만 활성화한다.
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

        // 애니메이터의 SelectedIdle 파라미터의 값을 무기 타입에 있는 값으로 변경한다.
        int typeNumber = (int)weaponInfo.type;
        myAnim.SetFloat("SelectedIdle", (float)typeNumber);
    }



    // Scene View에 기즈모를 그리는 이벤트 함수
    private void OnDrawGizmos()
    {
        // trajectory 리스트에 값이 없으면 함수 종료
        if(trajectory.Count < 1)
        {
            return;
        }

        // 라인의 색상은 녹색으로 설정한다.
        Gizmos.color = Color.green;

        // trajectory 리스트의 모든 번호를 연결하여 라인을 그린다.
        for (int i = 0; i < trajectory.Count - 1; i++)
        {
            Gizmos.DrawLine(trajectory[i], trajectory[i + 1]);
        }

        
    }

}
