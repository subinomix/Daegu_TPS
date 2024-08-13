using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class CineTrigger : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    private void OnTriggerEnter(Collider other)
    {

        // �浹�� ����� �÷��̾���...
        if(other.transform.tag == "Player")
        {
            // ���׸ӽ��� �÷����Ѵ�.
            //CinemaManager.instance.StartCineMachine();

            if (videoPlayer != null)
            {
                // ������ �÷����Ѵ�.
                videoPlayer.Play();
                //videoPlayer.frame = 300;
                videoPlayer.time = 60;
            }

            gameObject.SetActive(false);
        }
    }
}
