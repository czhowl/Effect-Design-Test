using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingBall : MonoBehaviour
{
    [SerializeField]
    GameObject sparkPrefab;
    public GameObject SparkPrefab
    {
        get => sparkPrefab;
        set => sparkPrefab = value;
    }

    void OnCollisionEnter(Collision col) {
        ContactPoint contact = col.contacts[0];
        Vector3 pos = contact.point;
        Instantiate(sparkPrefab, pos, Quaternion.identity);     // spawn star spark
        if(col.gameObject.CompareTag("Eyebrow"))     // ball held propagation
        {
            gameObject.tag = "Eyebrow";
        }
    }
}
