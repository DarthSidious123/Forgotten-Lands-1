using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Zombie : MonoBehaviour
{

    [Header("Links")]

    public NavMeshAgent navMesh;
    public PlayerController player;


    void Awake()
    {
        navMesh = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("MainPlayer").GetComponent<PlayerController>();
    }

    void Start()
    {

    }

    void Update()
    {
        if (player != null)
        {
            navMesh.destination = player.transform.position;
        }

    }
}
