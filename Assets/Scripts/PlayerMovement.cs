using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private TunnelControler tunnel;
    [SerializeField] private float speed = 50f;
    [SerializeField] private bool isMove = false;

    private float distanceTravelled = 15f;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = tunnel.GetPointAtDistance(distanceTravelled);
        transform.rotation = tunnel.GetRotationAtDistance(distanceTravelled);
    }

    // Update is called once per frame
    void Update()
    {
        if (isMove)
        {
            float zPos = transform.position.z;
            if (zPos >= tunnel.ReloadDistance)
            {
                tunnel.GenerateNewSegment();
                distanceTravelled -= tunnel.LastRemovedDistance;
            }
            if (zPos >= tunnel.TransformDistance)
            {
                tunnel.TransformTunnel();
            }

            transform.position = tunnel.GetPointAtDistance(distanceTravelled);
            transform.rotation = tunnel.GetRotationAtDistance(distanceTravelled);

            distanceTravelled += speed * Time.deltaTime;
        }
    }

    public bool IsMove
    {
        get
        {
            return isMove;
        }
        set
        {
            isMove = value;
        }
    }
}
