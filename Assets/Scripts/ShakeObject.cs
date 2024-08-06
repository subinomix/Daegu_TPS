using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeObject : MonoBehaviour
{
    public float positionFrequency;
    public Vector3 positionAmplitude;
    public float positionDuration;

    public float rotationFrequency;
    public Vector3 rotationAmplitude;
    public float rotationDuration;

    bool shaking = false;
    Vector3 originPos;
    Quaternion originRot;
    FollowCamera followCamComp;

    float px = 0;
    float py = 0;

    void Start()
    {
        followCamComp = GetComponent<FollowCamera>();
    }

    //void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.P) && !shaking)
    //    {
    //        ShakePos();
    //    }

    //    if (Input.GetKeyDown(KeyCode.R) && !shaking)
    //    {
    //        ShakeRot();
    //    }
    //}


    public void ShakePos()
    {
        if(!shaking)
        {
            StartCoroutine(ShakePosition(positionDuration, positionFrequency, positionAmplitude));
        }
    }

    public void ShakeRot()
    {
        if (!shaking)
        {
            StartCoroutine(ShakeRotation(rotationDuration, rotationFrequency, rotationAmplitude));
        }
    }

    // ������ ����
    // ������ �ð� ���� ������ ���� �ȿ��� ������ �ð� �������� ��ġ�� �����Ѵ�.
    // �ʿ� ���: ��ü �ð�, ���� Ƚ��, ����
    IEnumerator ShakePosition(float duration, float frequency, Vector3 amplitude)
    {
        float interval = 1.0f / frequency;

        originPos = transform.position;

        FollowCamera.FollowType currentType = followCamComp.followType;

        shaking = true;
        followCamComp.followType = FollowCamera.FollowType.ShakingPositionType;
        int testNumber = 0;
        for (float i = 0; i < duration; i += interval)
        {
            // ���� �Լ��� �̿��� ���� ���
            Vector2 randomPos = Random.insideUnitCircle;



            randomPos.x *= amplitude.x;
            randomPos.y *= amplitude.y;

            // originPos�� �������� ������ ��ġ ���� ����ؼ� �������� ��ġ�� �����Ѵ�.
            //Vector3 worldPos = followCamComp.target.position
            transform.position = followCamComp.target.position + new Vector3(randomPos.x, randomPos.y, 0);

            yield return new WaitForSeconds(interval);
            testNumber++;
            print("�ݺ� Ƚ��: " + testNumber.ToString());
            print("���� �ð�: " + i.ToString());
        }
        
        shaking = false;

        transform.position = originPos;

        followCamComp.followType = currentType;
    }

    // �����̼� ����
    // ������ �ð� ���� ������ ���� �ȿ��� ������ �ð� �������� ȸ�� ���� �����Ѵ�.
    // �ʿ� ���: ��ü �ð�, ���� Ƚ��, ����
    IEnumerator ShakeRotation(float duration, float frequency, Vector3 amplitude)
    {
        float interval = 1.0f / frequency;

        // originRot = transform.rotation;
        // Vector3 eulerOrigin = transform.eulerAngles;

        FollowCamera.FollowType currentType = followCamComp.followType;

        shaking = true;
        followCamComp.followType = FollowCamera.FollowType.ShakingRotationType;

        for (float i = 0; i < duration; i += interval)
        {
            //Vector2 randomPos = Random.insideUnitCircle;
            
            // Perline Noise�� �̿��� ���� ���
            px += 0.1f;
            py += 0.1f;
            if (px >= 1.0f)
            {
                px = 0;
                if (py >= 1.0f)
                {
                    py = 0;
                }
            }

            Vector2 randomPos = new Vector2(Mathf.PerlinNoise(px, 0) - 0.5f, Mathf.PerlinNoise(0, py) - 0.5f);

            randomPos.x *= amplitude.x;
            randomPos.y *= amplitude.y;

            // originPos�� �������� ������ ȸ�� ���� ����ؼ� �������� ȸ�� ���� �����Ѵ�.
            transform.eulerAngles = followCamComp.target.eulerAngles + new Vector3(randomPos.x, randomPos.y, 0);

            yield return new WaitForSeconds(interval);
        }

        shaking = false;

        //transform.rotation = originRot;
        followCamComp.followType = currentType;
    }
}
