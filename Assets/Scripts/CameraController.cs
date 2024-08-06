using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Vector3 cameraOffset = new Vector3(0.3f, 0.3f, -4.0f);

    // �迭(Array)
    //Transform[] camPositions = new Transform[2];

    // ����Ʈ(List)
    public List<Transform> camList = new List<Transform>();

    float currentRate = 0;
    float my = 0;


   
    void Start()
    {
        // �ڽ� ���ӿ�����Ʈ �߿��� �ι�°(Near)�� ����°(Far) ������Ʈ�� ã�Ƽ� �迭�� �ִ´�.
        //camPositions[0] = transform.GetChild(0);
        //camPositions[1] = transform.GetChild(1);

        camList.Add(transform.GetChild(0));
        camList.Add(transform.GetChild(1));

        camList[1].localPosition = camList[0].localPosition + cameraOffset;
        
        // �ʱ� ī�޶�� Near ī�޶�(1��Ī)�� �Ѵ�.
        ChangeCamTarget(1, false);
    }

    void Update()
    {
        // ����, ���� Ű 1���� ������ NearPos�� Ÿ������ �����ϰ�,
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            ChangeCamTarget(0, false);
        }
        // 2���� ������ FarPos�� Ÿ������ �����Ѵ�.
        else if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            ChangeCamTarget(1, true);
        }


        // ���콺 �ٿ� ���� farPos�� ��ġ�� �����Ѵ�.
        currentRate -= Input.GetAxis("Mouse ScrollWheel") * 0.5f;
        //currentRate += Time.deltaTime *0.5f;
        currentRate = Mathf.Clamp(currentRate, 0.0f, 1.0f);

        Vector3 startPos = camList[0].localPosition;
        Vector3 endPos = startPos + cameraOffset;

        // ����, startPos���� endPos���� ���̿� ��ֹ� �ִٸ�...
        Vector3 dir = endPos - startPos;
        dir = transform.TransformDirection(dir);

        // nearPos�κ��� endPos�������� Ray�� �߻��Ѵ�.
        Ray ray = new Ray(camList[0].position, dir.normalized);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, dir.magnitude, ~(1 << 7)))
        {
            // print(hitInfo.transform.name);
            // endPos�� ��ġ�� ��ֹ� �������� 10cm ���� ������ �����Ѵ�.
            Vector3 hitPosition = hitInfo.point + hitInfo.normal * 0.1f;
            endPos = hitPosition - transform.position;

            // ������ ����� endPos (����)���͸� �÷��̾ ȸ���� ��ŭ ȸ����Ų��.
            endPos = Quaternion.Inverse(transform.rotation) * endPos;
        }


        camList[1].localPosition = Vector3.Lerp(startPos, endPos, currentRate);

        // ���콺�� ���� �巡�׿� ���� farPos�� ���� ȸ���� �ϵ��� �Ѵ�.
        my += Input.GetAxis("Mouse Y") * 200 * Time.deltaTime;
        my = Mathf.Clamp(my, -30, 30);
        camList[1].localEulerAngles = new Vector3(-my, 0, 0);


    }

    void ChangeCamTarget(int targetNum, bool isDynamic)
    {
        // Far Pos ������Ʈ�� ��ġ�� �����Ѵ�.

        if (camList[1] != null)
        {
            if(targetNum == 0)
            {
                // farPos�� ��ġ�� nearPos�� ��ġ�� ��ġ��Ų��.
                camList[1].position = camList[0].position;

            }
            else if(targetNum == 1)
            {
                // farPos�� ��ġ�� nearPos�� ��ġ�κ��� offset��ŭ �̵���Ų��.

                camList[1].localPosition = camList[0].localPosition + cameraOffset;
            }
        }
    }
}
