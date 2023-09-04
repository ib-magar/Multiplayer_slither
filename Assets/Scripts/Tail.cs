using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Tail : MonoBehaviour
{
    public Transform networkedOwner;
    public Transform followTransform;

    [SerializeField] float delayTime=.1f;
    [SerializeField] float distance = .3f;
    [SerializeField] float moveStep = 10f;

    private Vector3 targetPosition;

    private void Update()
    {
        targetPosition = followTransform.position - followTransform.forward * distance;
        targetPosition += (transform.position - targetPosition) * delayTime;
        targetPosition.z = 0f;

        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveStep);
    }

}
