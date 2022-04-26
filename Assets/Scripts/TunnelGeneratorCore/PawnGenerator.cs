using UnityEngine;
using UnityEngine.Serialization;

public class PawnGenerator : MonoBehaviour
{
    [Header("Pawn Objects")]
    [SerializeField] private Transform pawnHolder;
    [SerializeField] private Pawn[] pawns;

    [Header("Settings")]
    [SerializeField] private float distanceBetweenPawns = 20f;

    private Pattern pattern;

    private TunnelControler tunnelCTRL;
    private float currentDistance = 100f;
    private float tunnelWidth, tunnelHeigth;

    private bool initialize = false;

    void AssignComponents()
    {
        pattern = new Pattern();

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

        float maxLength = tunnelCTRL.PathLength;

        while(currentDistance < maxLength)
        {
            Vector3 pos = tunnelCTRL.GetPointAtDistance(currentDistance);
            Vector3 up = tunnelCTRL.GetUpAtDistance(currentDistance);
            Vector3 right = tunnelCTRL.GetRightAtDistance(currentDistance);

            int[] patternPoint = pattern.GetPatternPoint();
            for (int i = 0; i < patternPoint.Length; i++)
            {
                if (patternPoint[i] > 0)
                {
                    Spawn(pawns[patternPoint[i] - 1], pos, up, right, i);
                }
            }

            currentDistance += distanceBetweenPawns;
        }
    }

    void Spawn(Pawn pawn, Vector3 pos, Vector3 up, Vector3 right, int side)
    {
        Vector3 sidePos = new Vector3();
        switch (side)
        {
            case 0: sidePos = -right * (tunnelWidth /* - pawn.height*/); break;
            case 1: sidePos = up * (tunnelHeigth    /* - pawn.height*/); break;
            case 2: sidePos = right * (tunnelWidth  /* - pawn.height*/); break;
            case 3: sidePos = -up * (tunnelHeigth   /* - pawn.height*/); break;
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

    class Pattern
    {
        int[][] pattern = new int[][]
        {
            // Easy level
            new int[] { 0, 1, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 1, 0, 1 },
            new int[] { 0, 0, 0, 1, 2, 2, 2, 2, 0, 1, 2, 2, 2, 2, 0 },
            new int[] { 0, 1, 0, 1, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0 },
            new int[] { 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, 0 },
                                                                    
            // Medium level                                         
            new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                                                    
            // Hard level                                           
            new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
        };

        int currentLevel = 0;
        int patternCounter = 0;

        public int[] GetPatternPoint()
        {
            if (patternCounter >= pattern[0].Length)
            {
                patternCounter = 0;
                ShufflePattern();
            }

            int[] patternPoint = new int[]
            {
                pattern[currentLevel * 4 + 0][patternCounter],
                pattern[currentLevel * 4 + 1][patternCounter],
                pattern[currentLevel * 4 + 2][patternCounter],
                pattern[currentLevel * 4 + 3][patternCounter]
            };

            patternCounter++;

            return patternPoint;
        }

        public void ShufflePattern()
        {
            int startIndex = currentLevel * 4;
            int endIndex = startIndex + 4;

            if (endIndex > pattern.Length)
                endIndex = pattern.Length;

            for (int i = startIndex; i < endIndex; i++)
            {
                int[] tmp = pattern[i];
                int r = Random.Range(startIndex, endIndex);
                pattern[i] = pattern[r];
                pattern[r] = tmp;
            }
        }
    }

    [System.Serializable]
    public struct Pawn
    {
        public Transform transform;
        public float height
        {
            get
            {
                Bounds bounds = new Bounds();
                if (transform != null)
                {
                    Renderer[] renderers = transform.GetComponentsInChildren<Renderer>();

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
        }

        public Pawn(Transform transform)
        {
            this.transform = transform;
        }
    }
}
