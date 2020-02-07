using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingBallManager : MonoBehaviour
{
    public int eat = 0;    // eating detection
    public int score = 0;    // balls holding by eyebrow
    public Gradient gradient;     // TODO: customized color gradient
    public Color ballColor = new Color(0.1895693f, 0.5660378f, 0.5438926f, 1.0f);     // current color of ball eaten
    EyebrowTracker aRHead;     // ARhead
    Vector3 headPos;
    Vector3 mouthPos;
    Vector3 eyePos;

    [SerializeField]
    GameObject dropingBallPrefab;
    public GameObject DropingBallPrefab
    {
        get => dropingBallPrefab;
        set => dropingBallPrefab = value;
    }
    [SerializeField]
    int numberOfDroppingBall;
    public int NmberOfDroppingBall
    {
        get => numberOfDroppingBall;
        set => numberOfDroppingBall = value;
    }

    [SerializeField]
    GameObject disappearPrefab;
    public GameObject DisappearPrefab
    {
        get => disappearPrefab;
        set => disappearPrefab = value;
    }
    GameObject[] dropingBalls;
    void Start()
    {
        dropingBalls = new GameObject[numberOfDroppingBall];
    }

    void Update()
    {
        int count = 0;     // holder for checking how many balls held
        if(aRHead == null){
            aRHead = FindObjectOfType<EyebrowTracker>();     // get ARhead
        }else{
            headPos = aRHead.HeadPos;
            mouthPos = headPos - new Vector3(0.0f, 1f, 0.0f);
            eyePos = aRHead.EyePos;
        }
        if(Vector3.Distance(headPos, Vector3.zero) != 0.0f){
            for(int i = 0; i < dropingBalls.Length; i++){     // loop through balls for updating
                if(dropingBalls[i] == null){     // spawn balls
                    Vector3 spawnPos = headPos + new Vector3(Random.Range(-3f, 3f), Random.Range(14f, 18f), Random.Range(-3f, 1f));
                    dropingBalls[i] = Instantiate(dropingBallPrefab, spawnPos, Random.rotation);
                    dropingBalls[i].GetComponent<Renderer>().material.color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
                    // dropingBalls[i].transform.parent = transform;
                    ConstantForce gravity = dropingBalls[i].AddComponent<ConstantForce>();
                    gravity.force = new Vector3(0.0f, -3f, 0.0f);
                }
                dropingBalls[i].SetActive(aRHead.IsHeadDetected);
                if(Vector3.Distance(dropingBalls[i].transform.position, headPos) > 30.0f){     // cycle balls
                    resetPos(dropingBalls[i], headPos);
                }
                if(Vector3.Distance(dropingBalls[i].transform.position, mouthPos) < 3.3f && aRHead.mouthClose > 0.1f){     // eat balls
                    ballColor = dropingBalls[i].GetComponent<Renderer>().material.color;
                    GameObject ps = Instantiate(disappearPrefab, dropingBalls[i].transform.position, Quaternion.identity);
                    var main = ps.GetComponent<ParticleSystem>().main;
                    main.startColor = ballColor;
                    resetPos(dropingBalls[i], headPos);
                    eat++;
                }
                if(dropingBalls[i].transform.position.y < eyePos.y){     // reset fallen balls
                    dropingBalls[i].tag = "FallingBall";
                }
                if(dropingBalls[i].CompareTag("Eyebrow")){     // count balls held
                    count++;
                }
            }
        }
        score = count;     // update score
    }

    static void resetPos(GameObject ball, Vector3 headPos){     // reset ball position
        Rigidbody rg = ball.GetComponent<Rigidbody>();
        rg.velocity = Vector3.zero;
        rg.angularVelocity = Vector3.zero;
        Vector3 spawnPos = headPos + new Vector3(Random.Range(-3f, 3f), Random.Range(14f, 18f), Random.Range(-3f, 1f));
        ball.transform.position = spawnPos;
    }
}
