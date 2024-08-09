using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{


    void Start()
    {
        
    }

    void Update()
    {
        if (Camera.main != null && Camera.main.gameObject.activeInHierarchy)
        {
            // ���� ���� ������ ���� ī�޶� ���� �ٶ󺸴� ����� ��ġ��Ų��.
            transform.forward = (Camera.main.transform.position - transform.position).normalized;
        }
    }
}
