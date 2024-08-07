using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.AI;


public class EnemyFSM : ActorBase
{
    public enum EnemyState
    {
        Idle = 0,
        Patrol = 1,
        Trace = 2,
        MeleeAttack = 4,
        FarAttack = 8,
        AttackDelay = 16,
        Return = 32,
        Damaged = 64,
        Dead = 128
    }

    public EnemyState myState = EnemyState.Idle;
    public bool drawGizmos = true;

    [Header("�⺻ �Ӽ�")]
    public EnemyInitPreferences initPreferences;
    public EnemyStateBase myStatus;
    public Slider hpSlider;
    public Animator enemyAnim;

    public float attackType = 0;

    float currentTime = 0;
    float idleTime = 3.0f;
    Vector3 patrolCenter;
    CharacterController cc;
    Vector3 patrolNext;
    Vector3 hitDirection;
    float[] idleBlendValue = new float[] { 0, 0.5f, 1.0f };
    int idleNumber = 0;

    NavMeshAgent smith;
    
    [SerializeField]
    Transform target;

    void Start()
    {
        patrolCenter = transform.position;
        patrolNext = patrolCenter;
        cc = GetComponent<CharacterController>();
        //myStatus.Initialize(100, 9);
        //myStatus.patrolSpeed = 5;
        hpSlider.value = myStatus.currentHP / myStatus.maxHP;
        smith = GetComponent<NavMeshAgent>();
        if(smith != null)
        {
            smith.speed = myStatus.patrolSpeed;
            smith.angularSpeed = 300.0f;
            smith.acceleration = 35.0f;
            smith.autoBraking = true;
            smith.autoTraverseOffMeshLink = false;
            smith.stoppingDistance = 1.0f;
        }
    }

    void Update()
    {
        // ���� ���� ���¿� ���� ������ �Լ��� �����Ѵ�.
        switch (myState)
        {
            case EnemyState.Idle:
                Idle();
                break;
            case EnemyState.Patrol:
                Patrol();
                break;
            case EnemyState.Trace:
                TraceTarget();
                break;
            case EnemyState.MeleeAttack:
                //Attack();
                break;
            case EnemyState.FarAttack:
                break;
            case EnemyState.AttackDelay:
                AttackDelay();
                break;
            case EnemyState.Return:
                ReturnHome();
                break;
            case EnemyState.Damaged:
                OnDamaged();
                break;
            case EnemyState.Dead:
                Dead();
                break;
        }
    }

    private void Idle()
    {
        CheckSight(initPreferences.sightRange, initPreferences.sightDistance);

        currentTime += Time.deltaTime;
        if(currentTime > idleTime)
        {
            currentTime = 0;
            myState = EnemyState.Patrol;
            print("My State: Idle -> Patrol");
            enemyAnim.SetBool("PatrolStart", true);

            // Idle �ִϸ��̼��� 0, 1, 2�� ��ȯ�ؼ� ���õǵ��� �����Ѵ�.
            idleNumber = (idleNumber + 1) % 3;
            float selectedIdle = idleBlendValue[idleNumber];
            enemyAnim.SetFloat("SelectedIdle", selectedIdle);

            // �׺�޽� ������Ʈ�� �������� PatrolNext ������ �����Ѵ�.
            smith.SetDestination(patrolNext);
        }

    }

    private void Patrol()
    {
        CheckSight(initPreferences.sightRange, initPreferences.sightDistance);

        // ���õ� �������� �̵��Ѵ�.
        Vector3 dir = patrolNext - transform.position;
        //dir.y = 0;

        if (dir.magnitude > 1.0f)
        {
            //cc.Move(dir.normalized * myStatus.patrolSpeed * Time.deltaTime);
            // �̵��Ϸ��� �������� ȸ���Ѵ�.
            //transform.rotation = Quaternion.LookRotation(dir.normalized);
        }
        // �������� �����ϰ�, 2��~3�� ���̸�ŭ ����� ���� �ٸ� ������ ��÷�Ѵ�.
        else
        {
            // patrolRadius�� �ݰ����� �ϴ� ���� ������ ������ �����Ѵ�.
            #region 1. ���� �⺻ ���� ���
            //float h = Random.Range(-1.0f, 1.0f);
            //float v = Random.Range(-1.0f, 1.0f);
            //float distance = Random.Range(0, initPreferences.patrolRadius);
            //Vector3 newPos = new Vector3(h, 0, v).normalized * distance;
            //patrolNext = patrolCenter + newPos;
            #endregion

            #region 2. Random Ŭ������ �ִ� inside �Լ��� �̿��ؼ� �����ϴ� ���
            Vector2 newPos = Random.insideUnitCircle * initPreferences.patrolRadius;
            patrolNext = patrolCenter + new Vector3(newPos.x, 0, newPos.y);
            #endregion

            #region 3. �ﰢ �Լ��� �̿��� ����
            //float degree = Random.Range(-180.0f, 180.0f);
            //Vector3 newPos = new Vector3(Mathf.Cos(Mathf.Deg2Rad * degree), 0, Mathf.Sin(Mathf.Deg2Rad * degree));
            //float distance = Random.Range(0, initPreferences.patrolRadius);
            //patrolNext = patrolCenter + newPos * distance;
            #endregion

            myState = EnemyState.Idle;
            print("My State: Patrol -> Idle");
            idleTime = Random.Range(2.0f, 3.0f);

            enemyAnim.SetBool("PatrolStart", false);

            // ������ �׺�޽� ������Ʈ�� �����.
            smith.isStopped = true;
            smith.ResetPath();

        }
    }

    void CheckSight(float degree, float maxDistance)
    {
        // �þ� ���� �ȿ� ���� ����� �ִٸ� �� ����� Ÿ������ �����ϰ� �ʹ�.
        // �þ� ����(�þ߰�: �¿� 30��, ����, �ִ� �þ� �Ÿ�: 15����)
        // ��� ������ ���� �±�(Player) ����
        target = null;

        // 1. ���� �ȿ� ��ġ�� ������Ʈ �߿� Tag�� "Player"�� ������Ʈ�� ��� ã�´�.
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        // 2. ã�� ������Ʈ�� �߿��� �Ÿ��� maxDistance �̳��� ������Ʈ�� ã�´�.
        for(int i = 0; i < players.Length; i++)
        {
            float distance = Vector3.Distance(players[i].transform.position, transform.position);

            if(distance <= maxDistance)
            {
                // 3. ã�� ������Ʈ�� �ٶ󺸴� ���Ϳ� ���� ���� ���͸� �����Ѵ�.
                Vector3 lookVector = players[i].transform.position - transform.position;
                lookVector.Normalize();

                float cosTheta = Vector3.Dot(transform.forward, lookVector);
                float theta = Mathf.Acos(cosTheta) * Mathf.Rad2Deg;

                // 4-1. ����, ������ ��� ���� 0���� ũ��(������ ���ʿ� �ִ�)...
                // 4-2. ����, ���հ��� ���� 30���� ������(���� �¿� 30�� �̳�)...
                if(cosTheta > 0 && theta < degree)
                {
                    target = players[i].transform;

                    // ���¸� trace ���·� ��ȯ�Ѵ�.
                    myState = EnemyState.Trace;
                    print("My State: Idle/Patrol -> Trace");
                    enemyAnim.SetTrigger("Trace");
                    attackType = Random.Range(0, 1.0f);
                }
            }
        }
    }


    private void TraceTarget()
    {


        // ����, �ִ� �߰� �Ÿ��� ����ٸ�...
        if (Vector3.Distance(transform.position, patrolCenter) > initPreferences.maxTraceDistance)
        {
            // ���¸� Return ���·� ��ȯ�Ѵ�.
            myState = EnemyState.Return;
            print("My State: Trace -> Return");
            enemyAnim.SetBool("Returning", true);

            smith.speed = myStatus.speed;
            smith.SetDestination(patrolCenter);

            return;
        }


        // ���� ������
        float selectedRange = attackType > 0.5f ? initPreferences.attackRange : initPreferences.farAttackRange;
        Vector3 dir = target.position - transform.position;
        dir.y = 0;

        if (dir.magnitude > selectedRange)
        {
            // Ÿ���� ���� �߰� �̵��Ѵ�.
            //cc.Move(dir.normalized * myStatus.speed * Time.deltaTime);
            //transform.rotation = Quaternion.LookRotation(dir.normalized);
            smith.SetDestination(target.position);
            smith.speed = myStatus.speed;
        }
        else
        {
            // ���� ���� �̳��� ���� ���¸� Attack ���·� ��ȯ�Ѵ�.
            currentTime = 0;

            if (dir.magnitude > initPreferences.attackRange)
            {
                myState = EnemyState.FarAttack;
                print("My State: Trace -> FarAttack");
                enemyAnim.SetTrigger("FarAttack");
            }
            else
            {
                myState = EnemyState.MeleeAttack;
                print("My State: Trace -> MeleeAttack");
                enemyAnim.SetTrigger("Attack");
            }
            smith.isStopped = true;
            smith.ResetPath();
        }
    }

    public void Attack()
    {
        // ������ �Ѵ�
        target.GetComponent<PlayerMove>().TakeDamage(20, Vector3.zero, transform);

        // ���� �ִϸ��̼��� ������... 
        #region �ִϸ��̼� ���� �����κ��� ���� ��Ȳ�� ��������
        // ���� �������� �ִϸ��̼� ���� ������ �����´�.
        //AnimatorStateInfo stateInfo = enemyAnim.GetCurrentAnimatorStateInfo(0);
        // ���� ������ ���¸� �ؽ� �����ͷ� ��ȯ�Ѵ�.
        //int attackHash = Animator.StringToHash("Base Layer.AttackMelee");

        // ���� ���� �������� ���°� ���� �����̶��...
        //if(stateInfo.fullPathHash == attackHash)
        //{
        //print("Length: " + stateInfo.length);
        //print("Progress: " + stateInfo.normalizedTime);
        //if (stateInfo.normalizedTime > 1.0f)
        //{
        //        // ���� ��� ���·� ��ȯ�Ѵ�.
        //        myState = EnemyState.AttackDelay;
        //        print("My State: Attack -> AttackDelay");
        //    }
        //}
        #endregion


    }

    private void AttackDelay()
    {
        // ����, Ÿ�ٰ� �Ÿ��� ���� ������ ������ ����ٸ�...
        float dist = Vector3.Distance(transform.position, target.position);
        if (dist > initPreferences.farAttackRange)
        {
            // �ٽ� �߰� ���·� ��ȯ�Ѵ�.
            myState = EnemyState.Trace;
            print("My State: AttackDelay -> Trace");
            enemyAnim.SetTrigger("Trace");
            currentTime = 0;
            return;
        }
        // ���Ÿ� ���� ���·� ��ȯ�Ѵ�.
        else
        {
            // �����ð� ����Ѵ�.
            currentTime += Time.deltaTime;

            Vector3 dir = target.position - transform.position;
            dir.y = 0;
            dir.Normalize();

            transform.rotation = Quaternion.LookRotation(dir);
            if (dist > initPreferences.attackRange)
            {
                // ���� �ð��� �����ٸ� ���¸� �ٽ� ���Ÿ� ���� ���·� ��ȯ�Ѵ�.
                if (currentTime > 1.5f)
                {
                    currentTime = 0;
                    myState = EnemyState.FarAttack;
                    print("My State: AttackDelay -> FarAttack");
                    enemyAnim.SetTrigger("FarAttack");
                    
                }
            }
            else
            {
                // ���� �ð��� �����ٸ� ���¸� �ٽ� �ٰŸ� ���� ���·� ��ȯ�Ѵ�.
                if (currentTime > 1.5f)
                {
                    currentTime = 0;
                    myState = EnemyState.MeleeAttack;
                    print("My State: AttackDelay -> MeleeAttack");
                    enemyAnim.SetTrigger("Attack");
                }
            }

           
        }

    }

    private void ReturnHome()
    {
        // patrolCenter ��ġ�� �ٽ� ���ư���.
        Vector3 dir = patrolCenter - transform.position;
        dir.y = 0;

        // �������� �����ߴٸ�...
        if (dir.magnitude < 1.1f)
        {
            // ������̼� ����
            smith.isStopped = true;
            smith.ResetPath();

            transform.position = patrolCenter;

            // ���¸� Idle ���·� ��ȯ�Ѵ�.
            myState = EnemyState.Idle;
            print("My State: Return -> Idle");
            enemyAnim.SetBool("Returning", false);

            smith.speed = myStatus.patrolSpeed;
        }
        // �׷��� �ʾҴٸ�...
        //else
        //{
        //    cc.Move(dir.normalized * myStatus.speed * Time.deltaTime);
        //    transform.rotation = Quaternion.LookRotation(dir.normalized);
        //}

    }

    private void OnDamaged()
    {


        // ���� �ð� �ڷ� �������ٰ�(knock-back) ���¸� Trace ���·� ��ȯ�Ѵ�.
        transform.position = Vector3.Lerp(transform.position, hitDirection, 0.05f);

        if(Vector3.Distance(transform.position, hitDirection) < 0.1f)
        {
            myState = EnemyState.Trace;
            print("My State: Damaged -> Trace");
            //enemyAnim.SetTrigger("Trace");
            enemyAnim.SetBool("Hit", false);
            smith.speed = myStatus.speed;
        }
    }

    // ������ ������ �������� �ο��ϴ� �Լ�
    public override void TakeDamage(float atkPower, Vector3 hitDir, Transform attacker)
    {
        // �θ� ������ TakeDamage �Լ��� ���� �����Ѵ�.
        base.TakeDamage(atkPower, hitDir, attacker);

        if(myState == EnemyState.Dead || myState == EnemyState.Return || myState == EnemyState.Damaged)
        {
            return;
        }

        // 1. ���� ü�¿� ����� ���ݷ¸�ŭ�� ���ҽ�Ų��(min 0 ~ max 100).
        myStatus.currentHP = Mathf.Clamp(myStatus.currentHP - atkPower, 0, myStatus.maxHP);

        // 1-2. ü�� �����̴� UI�� ���� ü���� ǥ���Ѵ�.
        hpSlider.value = myStatus.currentHP / myStatus.maxHP;

        // 2. ����, �� ��� ���� ü���� 0 ���϶��...
        if (myStatus.currentHP <= 0)
        {
            // 2-1. ���� ���¸� ���� ���·� ��ȯ�Ѵ�.
            myState = EnemyState.Dead;
            print("My State: Any -> Dead");
            currentTime = 0;
            enemyAnim.SetTrigger("Die");

            // 2-2. �ݶ��̴� ������Ʈ�� ��Ȱ��ȭ ó���Ѵ�.
            GetComponent<CapsuleCollider>().enabled = false;
            GetComponent<CharacterController>().enabled = false;
        }
        // 3. �׷��� �ʴٸ�..
        else
        {
            // 3-1. ���� ���¸� ������ ���·� ��ȯ�Ѵ�.
            myState = EnemyState.Damaged;
            print("My State: Any -> Damaged");
            enemyAnim.SetBool("Hit", true);

            // 3-2. Ÿ�� �������� ���� �Ÿ���ŭ�� �˹� ��ġ�� �����Ѵ�.
            hitDir.y = 0;
            hitDirection = transform.position + hitDir * 2.5f;

            smith.isStopped = true;
            smith.ResetPath();

            // 3-3. �����ڸ� Ÿ������ �����Ѵ�.
            target = attacker;
            Vector3 dir = target.position - transform.position;
            dir.y = 0;
            transform.rotation = Quaternion.LookRotation(dir.normalized);
        }
    }


    private void Dead()
    {
        // Ŭ���� ������ �Ͻÿ� �ʱ�ȭ�ϱ�
        //initPreferences = new EnemyInitPreferences(10, 5, 10, 1.5f, 30, 10, 30);

        // 3�� �ڿ� ���ŵȴ�.
        currentTime += Time.deltaTime;
        if(currentTime > 4.0f)
        {
            Destroy(gameObject);
        }
    }

    // �� �׸���
    private void OnDrawGizmos()
    {
        if(!drawGizmos)
        {
            return;
        }

        Gizmos.color = new Color32(154, 14, 235, 255);

        #region �� �׸���
        //List<Vector3> points = new List<Vector3>();
        //for (int i = 0; i < 360; i += 5)
        //{
        //    Vector3 point = new Vector3(Mathf.Cos(i * Mathf.Deg2Rad), 0, Mathf.Sin(i * Mathf.Deg2Rad)) * 5;
        //    points.Add(transform.position + point);
        //}

        //for (int i = 0; i < points.Count - 1; i++)
        //{
        //    Gizmos.DrawLine(points[i], points[i + 1]);
        //}
        #endregion

        // �þ߰� �׸���
        float rightDegree = 90 - initPreferences.sightRange;
        float leftDegree = 90 + initPreferences.sightRange;

        Vector3 rightPos = (transform.right * Mathf.Cos(rightDegree * Mathf.Deg2Rad) + transform.forward * Mathf.Sin(rightDegree * Mathf.Deg2Rad)) * initPreferences.sightDistance
                                + transform.position;

        Vector3 leftPos =  (transform.right * Mathf.Cos(leftDegree * Mathf.Deg2Rad) + transform.forward * Mathf.Sin(leftDegree * Mathf.Deg2Rad)) * initPreferences.sightDistance + transform.position;

        Gizmos.DrawLine(transform.position, rightPos);
        Gizmos.DrawLine(transform.position, leftPos);

        // �ִ� �߰� �Ÿ� �׸���
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(patrolCenter, initPreferences.maxTraceDistance);
    }
}

// ����ȭ Ŭ����
[System.Serializable]
public class EnemyInitPreferences
{
    public float patrolRadius = 4.0f;
    public float attackRange = 1.5f;
    public float farAttackRange = 4.0f;
    [Range(0.0f, 90.0f)]
    public float sightRange = 30.0f;
    public float sightDistance = 15.0f;
    public float maxTraceDistance = 25.0f;

    // ������ �Լ�
    public EnemyInitPreferences(float patrolRadius, float attackRange, float sightRange, float sightDistance, float maxTraceDistance)
    {
        this.patrolRadius = patrolRadius;
        this.attackRange = attackRange;
        this.sightRange = Mathf.Clamp(sightRange, 0, 90.0f);
        this.sightDistance = sightDistance;
        this.maxTraceDistance = maxTraceDistance;
    }
}