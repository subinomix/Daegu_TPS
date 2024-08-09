using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessingManager : MonoBehaviour
{
    public static PostProcessingManager instance;

    

    Volume mainPost;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // ����Ʈ ���μ��� ���� ������Ʈ�� �����´�.
        mainPost = GetComponent<Volume>();
    }

    public void GrayScaleOn()
    {
        ColorAdjustments colorAdjustment;
        // ����, ������ ���Ͽ��� ColorAdjustments �Ӽ��� �����Դٸ�...
        if(mainPost.profile.TryGet<ColorAdjustments>(out colorAdjustment))
        {
            // ColorAdjustments �Ӽ��� Ȱ��ȭ�Ѵ�.
            colorAdjustment.active = true;

            // Saturation  ���� -100���� �����Ѵ�.
            colorAdjustment.saturation.value = -100;
        }
    }
}
