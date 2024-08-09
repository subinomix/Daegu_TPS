using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

public class CinemaManager : MonoBehaviour
{
    public static CinemaManager instance;
    public PlayableDirector director;

    PlayerMove player;

    List<EnemyFSM> enemies = new List<EnemyFSM>();

    bool isStartCinema = false;

    GameObject mainCam;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<PlayerMove>();
        enemies = FindObjectsOfType<EnemyFSM>().ToList();
    }

    // Update is called once per frame
    void Update()
    {
        // 만일, 시네머신이 시작되었다면...
        if(isStartCinema)
        {
            // 만일, 현재 진행 시간이 전체 진행 시간에 도달했다면...
            if (director.time >= director.duration)
            {

                // 시네머신을 중지하고 싶다.
                director.Stop();
               

                // 플레이어와 에너미들의 상태를 모두 Idle 상태로 전환한다.
                player.myMoveState = PlayerMove.PlayerMoveState.Normal;

                for (int i = 0; i < enemies.Count; i++)
                {
                    enemies[i].myState = EnemyFSM.EnemyState.Idle;
                }

                isStartCinema = false;
            }
        }
    }

    public void StartCineMachine()
    {
        // 시네머신을 시작할 것을 PlayableDireector 컴포넌트에 알리기
        director.Play();
        isStartCinema = true;



        // 플레이어와 에너미들의 상태를 모두 씨네마 상태로 전환한다.
        player.myMoveState = PlayerMove.PlayerMoveState.Cinematic;

        for(int i = 0; i < enemies.Count; i++)
        {
            enemies[i].myState = EnemyFSM.EnemyState.Cinematic;
        }
    }
}
