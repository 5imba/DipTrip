using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PathGenerator), typeof(MeshGenerator))]
public class MainMenuTunnelController : MonoBehaviour
{
    PathGenerator pathGenerator;
    MeshGenerator meshGenerator;

    private void Awake()
    {
        pathGenerator = GetComponent<PathGenerator>();
        meshGenerator = GetComponent<MeshGenerator>();

        pathGenerator.CreatePath(new Vector3[] { new Vector3(0f, 0f, -100f), new Vector3(0f, 0f, 500f), new Vector3(0f, 0f, 501f) });
        meshGenerator.CreateMesh();
    }
}
