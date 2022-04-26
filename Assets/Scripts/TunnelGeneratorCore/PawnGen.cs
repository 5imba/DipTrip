using UnityEngine;

/*
public class PawnGen : MonoBehaviour
{
    [Header("Pawn Objects")]
    [SerializeField] private Transform pawnHolder;
    [SerializeField] private Pattern patternSettings;

    [Header("Settings")]
    [SerializeField] private float distanceBetweenPawns = 20f;

    private TunnelControler tunnelCTRL;
    private float currentDistance = 100f;
    private float tunnelWidth, tunnelHeigth;

    private bool initialize = false;

    void AssignComponents()
    {
        tunnelCTRL = GetComponent<TunnelControler>();

        tunnelWidth = tunnelCTRL.TunnelWidth;
        tunnelHeigth = tunnelCTRL.TunnelHeight;

        initialize = true;
    }

    public void SpawnPawns()
    {
        if (!initialize)
        {
            AssignComponents();
        }

    }

    void Spawn(Pawn pawn, Vector3 pos, Vector3 up, Vector3 right, int side)
    {
        Vector3 sidePos = new Vector3();
        switch (side)
        {
            case 0: sidePos = -right * (tunnelWidth - pawn.height); break;
            case 1: sidePos = up * (tunnelHeigth    - pawn.height); break;
            case 2: sidePos = right * (tunnelWidth  - pawn.height); break;
            case 3: sidePos = -up * (tunnelHeigth   - pawn.height); break;
        }

        Transform _pawn = Instantiate(pawn.transform);
        _pawn.position = pos + sidePos;
        _pawn.rotation = Quaternion.LookRotation(-sidePos);
        _pawn.parent = pawnHolder;
    }

    public void OverrideDistance()
    {
        currentDistance -= tunnelCTRL.LastRemovedDistance;
    }

    public void TransformPawns(float distance)
    {
        TagSystem[] pawnsTS = pawnHolder.GetComponentsInChildren<TagSystem>();
        for (int i = 1; i < pawnsTS.Length; i++)
        {
            if (pawnsTS[i].tags.Contains(Tags.Pawn))
            {
                Vector3 pos = pawnsTS[i].transform.position;
                pawnsTS[i].transform.position = new Vector3(pos.x, pos.y, pos.z - distance);
            }
        }
    }

    public float PawnHeight(Transform trans)
    {
        Bounds bounds = new Bounds();
        if (transform != null)
        {
            Renderer[] renderers = trans.GetComponentsInChildren<Renderer>();

            if (renderers.Length > 0)
            {
                //Find first enabled renderer to start encapsulate from it
                foreach (Renderer renderer in renderers)
                {
                    if (renderer.enabled)
                    {
                        bounds = renderer.bounds;
                        break;
                    }
                }

                //Encapsulate for all renderers
                foreach (Renderer renderer in renderers)
                {
                    if (renderer.enabled)
                    {
                        bounds.Encapsulate(renderer.bounds);
                    }
                }
            }
        }

        return bounds.extents.y;
    }

    [System.Serializable]
    public struct Pawn
    {
        public Transform transform;
        public float height;

        public Pawn(Transform transform)
        {
            this.transform = transform;
            height = 0f;
        }
    }   

}
*/