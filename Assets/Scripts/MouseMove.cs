using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;


public class MouseMove : MonoBehaviour
{

    public float moveSpeed = 8;

    public GameObject obj1;
    public GameObject obj2;
    public GameObject obj3;

    public AnimationCurve heightCurve;
    public AnimationCurve lopoCurve;

    Vector3 targetPos;
    NavMeshAgent smith;
    float currentTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        smith = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity))
            {
                //print(hitInfo.transform.name);
                targetPos = hitInfo.point + hitInfo.normal;

                // NavMeshAgent에게 목적지를 targetPos로 지정한다.
                smith.SetDestination(targetPos);
            }
        }

        // 오프메시 링크에 도달하면 지정된 경로대로 링크를 이동하게 하고 싶다.
        if(smith.isOnOffMeshLink)
        {
            //ParabolaType1(6, 1);
            // ParabolaType2(8);
            ParabolaType3();
        }


    }

    // 곡선 이동 방법 1 - 중력 계산법(이차원 곡선)
    void ParabolaType1(float velocity, float gravity)
    {
        // p = p0 + velocity * t - gravity * t * t
        currentTime = Time.deltaTime;
        if (currentTime < 1)
        {
            OffMeshLinkData linkData = smith.currentOffMeshLinkData;
            Vector3 moveRoute = Vector3.Lerp(linkData.startPos + Vector3.up, linkData.endPos + Vector3.up, currentTime);
            moveRoute.y += velocity * (currentTime - gravity * currentTime * currentTime);
            transform.position = moveRoute;

        }
        else
        {
            currentTime = 0;
            // 오프메시 링크 이동이 끝났음을 알린다.
            smith.CompleteOffMeshLink();
        }
    }


    // 곡선 이동 방법 2 - Bezier Curve 공식
    Vector3 BezierCurve(Vector3 p0, Vector3 p1, Vector3 p2, float percent)
    {
        Vector3 mid1 = Vector3.Lerp(p0, p1, percent);
        Vector3 mid2 = Vector3.Lerp(p1, p2, percent);
        Vector3 center = Vector3.Lerp(mid1, mid2, percent);

        return center;
    }

    void ParabolaType2(float height)
    {
        OffMeshLinkData linkData = smith.currentOffMeshLinkData;
        Vector3 start = linkData.startPos + Vector3.up;
        Vector3 end = linkData.endPos + Vector3.up;
        Vector3 middle = (start + end) / 2;
        middle.y += height;

        if (currentTime < 1.0f)
        {
            currentTime += Time.deltaTime;
            transform.position = BezierCurve(start, middle, end, currentTime);
        }
        else
        {
            currentTime = 0;
            smith.CompleteOffMeshLink();
            smith.ResetPath();
            smith.SetDestination(targetPos);
        }

       
    }

    void ParabolaType3()
    {
        OffMeshLinkData linkData = smith.currentOffMeshLinkData;
        Vector3 start = linkData.startPos + Vector3.up;
        Vector3 end = linkData.endPos + Vector3.up;

        if (currentTime < 1.0f)
        {
            currentTime += Time.deltaTime;
            Vector3 result = Vector3.Lerp(start, end, currentTime);
            result.y += heightCurve.Evaluate(currentTime);

            transform.position = result;
        }
        else
        {
            currentTime = 0;
            smith.CompleteOffMeshLink();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        List<Vector3> curves = new List<Vector3>();

        for(int i = 0; i < 50; i++)
        {
            float interval = 1.0f / 50.0f;
            Vector3 result = BezierCurve(obj1.transform.position, obj2.transform.position, obj3.transform.position, interval * (float)i);

            curves.Add(result);
        }

        for(int i = 0; i < curves.Count - 1; i++)
        {
            Gizmos.DrawLine(curves[i], curves[i + 1]);
        }
    }
}
