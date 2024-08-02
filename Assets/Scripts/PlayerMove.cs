using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMove : ActorBase
{
    enum PlayerMoveState
    {
        Normal,
        Jump,
    }

    public PlayerStateBase myStatus;
    public float rotSpeed = 200.0f;
    public float yVelocity = 2;
    public float jumpPower = 4;
    public int maxJumpCount = 1;
    public Image img_hitUI;
    public Animator playerAnim;

    // ȸ�� ���� �̸� ����ϱ� ���� ȸ����(x, y) ����
    float rotX;
    float rotY;
    float yPos;
    int currentJumpCount = 0;
    float currentTime = 0;
    bool timerStart = false;
    float[] idleAnims = new float[4] { 0.0f, 0.3f, 0.6f, 1.0f };

    CharacterController cc;
    PlayerMoveState myMoveState = PlayerMoveState.Normal;

    // �߷��� �����ϰ� �ʹ�.
    // �ٴڿ� �浹�� ���� �������� �Ʒ��� ��� �������� �ϰ� �ʹ�.
    // ����: �Ʒ�, ũ��: �߷�
    Vector3 gravityPower;

    void Start()
    {
        // ������ ȸ�� ���´�� ������ �ϰ� �ʹ�(���� ���� �ʱ�ȭ).
        rotX = transform.eulerAngles.x;
        rotY = transform.eulerAngles.y;

        // ĳ���� ��Ʈ�ѷ� ������Ʈ�� ������ ��Ƴ��´�.
        cc = GetComponent<CharacterController>();

        // �߷� ���� �ʱ�ȭ�Ѵ�.
        gravityPower = Physics.gravity;

        // 2�ʿ� �� ���� Idle �ִϸ��̼��� �����ϴ� �ڷ�ƾ �Լ��� �����Ѵ�.
        StartCoroutine(SelectIdleMotion(2.0f));

    }

    IEnumerator SelectIdleMotion(float changeTime)
    {
        while (true)
        {
            // ������ �ð����� 0~3 ������ ������ ���� �̴´�.
            int num = Random.Range(0, 4);

            // idleAnims �迭�� ���� �ε����� �ش��ϴ� ���� �ִϸ������� SelectedIdle ������ ������ �־��ش�.
            playerAnim.SetFloat("SelectedIdle", idleAnims[num]);

            yield return new WaitForSeconds(changeTime);
        }
    }


    void Update()
    {
        Move();
        Rotate();

        // ��Ʈ UI Ÿ�̸�
        //if(timerStart)
        //{
        //    currentTime -= Time.deltaTime;
        //    if(currentTime < 0)
        //    {
        //        timerStart = false;
        //        img_hitUI.gameObject.SetActive(false);
        //    }
        //}    
    }

    // "Horizontal"�� "Vertical" �Է��� �̿��ؼ� ��������� �̵��ϰ� �ϰ� �ʹ�.
    // 1. ������� �Է��� �޴´�.
    // 2. ����, �ӷ��� ����Ѵ�.
    // 3. �� �����Ӹ��� ���� �ӵ��� �ڽ��� ��ġ�� �����Ѵ�.
    void Move()
    {
        float h = 0;
        float v = 0;

        if (myMoveState == PlayerMoveState.Normal)
        {
            // 1. ���� �̵� ���
            h = Input.GetAxis("Horizontal");
            v = Input.GetAxis("Vertical");

            playerAnim.SetFloat("MoveHorizontal", h);
            playerAnim.SetFloat("MoveVertical", v);
        }
        // ���� ���� ���⿡ ���� �̵��ϵ��� �����Ѵ�.
        // 1-1. ���� ���� ���͸� �̿��ؼ� ����ϴ� ���
        //Vector3 dir = transform.forward * v + transform.right * h;
        //dir.Normalize();

        // 1-2. ���� ȸ�� ���� ���� ���� ���� ���͸� ���� ������ ���ͷ� ��ȯ�ϴ� �Լ��� �̿��ϴ� ���
        Vector3 dir = new Vector3(h, 0, v); // ���� ���� ����
        playerAnim.SetFloat("DirLength", dir.magnitude);
        dir = transform.TransformDirection(dir);
        dir.Normalize();


        // 2. ���� �̵� ���

        // �߷� ����
        yPos += gravityPower.y * yVelocity * Time.deltaTime;

        // ������ ���� �ʾҴµ� ĳ���� ��Ʈ�ѷ��� �浹�� ���� ���
        Ray ray = new Ray(transform.position, transform.up * -1);
        RaycastHit hitInfo;
        bool isHit = Physics.Raycast(ray, out hitInfo, (cc.height + cc.skinWidth + 0.4f) * 0.5f, ~(1 << 7));

        if(myMoveState != PlayerMoveState.Jump && !isHit)
        {
            // ���� ���� �ִϸ��̼� ��ȣ�� �ش�.
            playerAnim.SetTrigger("IsFalling");
        }


        // �ٴڿ� ������� ������ yPos�� ���� 0���� �ʱ�ȭ�Ѵ�.
        //if (cc.collisionFlags == CollisionFlags.CollidedBelow)
        if(isHit)
        {
            // ���� �ߴٰ� ���� �������� �� 1.25�� �ڿ� �̵��� �� �ִ� ���·� ��ȯ�Ѵ�.
            if (myMoveState == PlayerMoveState.Jump)
            {
                playerAnim.SetBool("Jump", false);
                currentTime += Time.deltaTime;
                if (currentTime > 0.3f)
                {
                    currentTime = 0;
                    myMoveState = PlayerMoveState.Normal;
                }
            }

            yPos = 0;
            currentJumpCount = 0;

        }

        // Ű������ �����̽��ٸ� ������ �������� ������ �ϰ� �ϰ� �ʹ�.
        if (Input.GetButtonDown("Jump") && currentJumpCount < maxJumpCount)
        {
            // ������ ó�� ������ �� ���� ���·� ��ȯ�Ѵ�.
            if (myMoveState == PlayerMoveState.Normal)
            {
                myMoveState = PlayerMoveState.Jump;
            }

            yPos = jumpPower;
            currentJumpCount++;
            playerAnim.SetBool("Jump", true);
        }

        dir.y = yPos;

        //transform.position += dir * moveSpeed * Time.deltaTime;
        cc.Move(dir * myStatus.speed * Time.deltaTime);
        //cc.SimpleMove(dir * moveSpeed);
    }

    // ������� ���콺 �巡�� ���⿡ ���� ���� �����¿� ȸ���� �ǰ� �ϰ� �ʹ�.
    // 1. ������� ���콺 �巡�� �Է��� �޴´�.
    // 2. ȸ�� �ӷ�, ȸ�� ������ �ʿ��ϴ�.
    // 3. �� �����Ӹ��� ���� �ӵ��� �ڽ��� ȸ������ �����Ѵ�.
    void Rotate()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // �� �� ���� ȸ�� ���� �̸� ����Ѵ�(R = R0 + vt).
        rotX += mouseY * rotSpeed * Time.deltaTime;
        rotY += mouseX * rotSpeed * Time.deltaTime;

        // ���� ȸ���� -60�� ~ +60�������� �����Ѵ�.
        if (rotX > 60.0f)
        {
            rotX = 60.0f;
        }
        else if (rotX < -60.0f)
        {
            rotX = -60.0f;
        }

        // ���� ȸ�� ���� ���� Ʈ������ ȸ�� ������ �����Ѵ�.
        transform.eulerAngles = new Vector3(0, rotY, 0);
        Camera.main.transform.GetComponent<FollowCamera>().rotX = rotX;
    }

    // ������ �޾��� ���� ������ �Լ�
    public override void TakeDamage(float atkPower, Vector3 hitDir, Transform attacker)
    {
        base.TakeDamage(atkPower, hitDir, attacker);

        myStatus.currentHP = Mathf.Clamp(myStatus.currentHP - atkPower, 0, myStatus.maxHP);
        //print("�� ü��: " + myStatus.currentHP);

        // img_hitUI ������Ʈ�� Ȱ��ȭ�ߴٰ�, 0.5�� �ڿ� �ٽ� ��Ȱ��ȭ�Ѵ�.
        StartCoroutine(DeActivateHitUI(0.5f));
    }


    // �ڷ�ƾ �Լ�
    IEnumerator DeActivateHitUI(float delayTime)
    {
        //float addValue = 0.05f;
        for (int i = 0; i < 100; i++)
        {
            Color colorVector = img_hitUI.color;
            //print(colorVector);
            float addValue = 0.05f;
            if (i > 49)
            {
                addValue *= -1;
            }
            colorVector.a += addValue;
            img_hitUI.color = colorVector;
            //yield return new WaitForSeconds(delayTime);
            yield return null;
        }

    }



    //private void OnControllerColliderHit(ControllerColliderHit hit)
    //{

    //}

}
