using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.AI;

public class navmesh : MonoBehaviour
{
    //PlayerController�Ăяo��
    [SerializeField] PlayerController PC = null;

    // �i�r���b�V��.
    NavMeshAgent navMeshAgent = null;

    void Start()
    {
        
    }
    void Update()
    {
        navMeshAgent.SetDestination(PC.transform.position);
    }
}
