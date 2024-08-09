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
        // ����, �ó׸ӽ��� ���۵Ǿ��ٸ�...
        if(isStartCinema)
        {
            // ����, ���� ���� �ð��� ��ü ���� �ð��� �����ߴٸ�...
            if (director.time >= director.duration)
            {

                // �ó׸ӽ��� �����ϰ� �ʹ�.
                director.Stop();
               

                // �÷��̾�� ���ʹ̵��� ���¸� ��� Idle ���·� ��ȯ�Ѵ�.
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
        // �ó׸ӽ��� ������ ���� PlayableDireector ������Ʈ�� �˸���
        director.Play();
        isStartCinema = true;



        // �÷��̾�� ���ʹ̵��� ���¸� ��� ���׸� ���·� ��ȯ�Ѵ�.
        player.myMoveState = PlayerMove.PlayerMoveState.Cinematic;

        for(int i = 0; i < enemies.Count; i++)
        {
            enemies[i].myState = EnemyFSM.EnemyState.Cinematic;
        }
    }
}
