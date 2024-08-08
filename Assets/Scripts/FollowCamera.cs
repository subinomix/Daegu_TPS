using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FollowCamera : MonoBehaviour
{
    // ī�޶��� ���� Ÿ�� ������ ���
    public enum FollowType
    {
        SoftType,
        HardType,
        ShakingPositionType,
        ShakingRotationType,
    }

    public FollowType followType = FollowType.HardType;


    // Ÿ������ ������ ��ġ�� ���� �̵���Ų��.
    public Transform target;
    public bool dynamicCam = true;
    public float followSpeed = 3.0f;
    public Image zoomUI;

    Coroutine zoom;
    float rate;
    //Transform player;

    CameraController camCon;


    void Start()
    {
        //player = GameObject.Find("Player").transform;
        camCon = FindObjectOfType<CameraController>();
    }

    void Update()
    {

        if (target != null)
        {
            // 1. ��ġ ����
            // ī�޶��� ��ġ�� Ÿ�� Ʈ�������� ��ġ�� �����Ѵ�.
            if (followType == FollowType.HardType || followType == FollowType.ShakingRotationType)
            {
                transform.position = target.position;
            }
            else if (followType == FollowType.SoftType)
            {
                transform.position = Vector3.Lerp(transform.position, target.position, Time.deltaTime * followSpeed);
            }
            else if (followType == FollowType.ShakingPositionType)
            {

            }

            // 2. ȸ�� ����
            if (followType != FollowType.ShakingRotationType)
            {
                transform.rotation = target.rotation;
            }
            

            // ī�޶��� ���� ������ �÷��̾��� ���� �������� �����Ѵ�.
            //Vector3 dir = (player.position - transform.position).normalized;
            // transform.forward = player.forward;

            // ������� ���콺 ���� ȸ�� ���� x�� ȸ������ �ִ´�.
            // transform.eulerAngles = new Vector3(-rotX, transform.eulerAngles.y, transform.eulerAngles.z);
        }
    }


    public void ZoomIn(bool isIn)
    {
        if (zoom == null)
        {
            print(isIn);
            zoom = StartCoroutine(ZoomInCoroutine(isIn));
        }
    }

    IEnumerator ZoomInCoroutine(bool isIn)
    {
        float currentTime = 0;
        if (isIn)
        {
            rate = camCon.currentRate;
        }

        while(currentTime < 0.5f)
        {
            // �ٸ� ��� ������Ʈ �Լ��� ���������� ��ٸ���.
            yield return new WaitForEndOfFrame();
            // yield return new WaitForFixedUpdate();

            currentTime += Time.deltaTime;
            // �þ߰��� 30���� ���
            float startFOV = isIn ? 60 : 20;
            float endFOV = isIn ? 20 : 60;
            Camera.main.fieldOfView = Mathf.Lerp(startFOV, endFOV, currentTime * 2);

            // �� UI ������ ������ 1�� �����Ѵ�.
            float zoomRate = isIn ? currentTime : 0.5f - currentTime;
            Color zoomAlpha = new Color(zoomUI.color.r, zoomUI.color.g, zoomUI.color.b, zoomRate * 2);
            zoomUI.color = zoomAlpha;

            // 1��Ī ���·� �ٲ۴�.
            camCon.currentRate = Mathf.Lerp(rate, 0, zoomRate * 2);
        }

        zoom = null;
    }
}
