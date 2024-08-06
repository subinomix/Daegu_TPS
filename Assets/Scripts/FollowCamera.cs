using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    //Transform player;


    void Start()
    {
        //player = GameObject.Find("Player").transform;

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
}
