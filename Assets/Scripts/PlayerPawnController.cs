using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SwipeManager))]
public class PlayerPawnController : MonoBehaviour
{
    public event System.Action OnDeath;

    [SerializeField] private TunnelControler tunnel;
    [SerializeField] private PlayerMovement playerMovement;
    [Header("Input Settings")]
    [SerializeField] private float swipeAngleError = 0.5f;

    [Header("Moving Settings")]
    [SerializeField] private float changeSideDuration = 0.2f;
    [SerializeField] private float holeEnteringDuration = 0.3f;
    [SerializeField] private bool isCollide = true;

    private Vector3[] sides;
    Vector2[][] screenTriangles;

    private bool changingSide = false;
    private bool getInput = true;

    private int currentSide = 0;

    // Start is called before the first frame update
    void Start()
    {
        Bounds bounds = new Bounds();
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();

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

        Vector3 up = new Vector3(0f, tunnel.TunnelHeight - bounds.extents.y, 0f);
        Vector3 right = new Vector3(tunnel.TunnelWidth - bounds.extents.y, 0f, 0f);

        sides = new Vector3[4];
        sides[0] = -right;
        sides[1] = up;
        sides[2] = right;
        sides[3] = -up;

        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Vector2 topLeft = new Vector2(0f, Screen.height);
        Vector2 topRight = new Vector2(Screen.width, Screen.height);
        Vector2 bottomRight = new Vector2(Screen.width, 0f);
        Vector2 bottomLeft = Vector2.zero;

        screenTriangles = new Vector2[][]
        {
            new Vector2[] { bottomLeft, screenCenter, topLeft },
            new Vector2[] { topLeft, screenCenter, topRight },
            new Vector2[] { topRight, screenCenter, bottomRight },
            new Vector2[] { bottomRight, screenCenter, bottomLeft }
        };

        Messenger<bool>.AddListener(GameEvent.ON_PAUSE, OnPause);
        Messenger<Swipe, Vector2>.AddListener(GameEvent.ON_SWIPE, OnSwipe);

        transform.localPosition = -up;
    }

    private void OnDestroy()
    {
        Messenger<bool>.RemoveListener(GameEvent.ON_PAUSE, OnPause);
        Messenger<Swipe, Vector2>.RemoveListener(GameEvent.ON_SWIPE, OnSwipe);
    }

    void OnSwipe(Swipe swipe, Vector2 secondPressPos)
    {
        if (!changingSide && getInput)
        {
            switch (swipe)
            {
                case Swipe.Left: StartCoroutine(ChangeSide(0, changeSideDuration)); break;
                case Swipe.Up: StartCoroutine(ChangeSide(1, changeSideDuration)); break;
                case Swipe.Right: StartCoroutine(ChangeSide(2, changeSideDuration)); break;
                case Swipe.Down: StartCoroutine(ChangeSide(3, changeSideDuration)); break;
                case Swipe.UpRight:
                    if (currentSide == 0) StartCoroutine(ChangeSide(1, changeSideDuration));
                    if (currentSide == 3) StartCoroutine(ChangeSide(2, changeSideDuration));
                    break;
                case Swipe.DownRight:
                    if (currentSide == 1) StartCoroutine(ChangeSide(2, changeSideDuration));
                    if (currentSide == 0) StartCoroutine(ChangeSide(3, changeSideDuration));
                    break;
                case Swipe.DownLeft:
                    if (currentSide == 1) StartCoroutine(ChangeSide(0, changeSideDuration));
                    if (currentSide == 2) StartCoroutine(ChangeSide(3, changeSideDuration));
                    break;
                case Swipe.UpLeft:
                    if (currentSide == 2) StartCoroutine(ChangeSide(1, changeSideDuration));
                    if (currentSide == 3) StartCoroutine(ChangeSide(0, changeSideDuration));
                    break;

                case Swipe.Tap:
                    for (int i = 0; i < screenTriangles.Length; i++)
                    {
                        if (Utils.IsInsidePolygon(screenTriangles[i], secondPressPos))
                            StartCoroutine(ChangeSide(i, changeSideDuration));
                    }
                    break;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCollide && PlayerPrefs.GetInt("Collision", 1) > 0)
        {
            TagSystem ts = other.GetComponent<TagSystem>();
            if (ts != null)
            {
                if (ts.tags.Contains(Tags.Obstacle))
                {
                    if (OnDeath != null)
                    {
                        StopAllCoroutines();
                        Messenger<Sound>.Broadcast(GameEvent.ON_SOUND_PLAY, Sound.Boom);
                        getInput = false;
                        playerMovement.IsMove = false;
                        StartCoroutine(HoleEntering(other.transform, holeEnteringDuration));                        
                    }
                }
                if (ts.tags.Contains(Tags.Coin))
                {
                    PlayerData.Coins += 1;
                    Messenger.Broadcast(GameEvent.ON_COIN_VALUE);
                    Destroy(other.gameObject);
                }
            }
        }
    }

    IEnumerator HoleEntering(Transform target, float duration)
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = target.position;

        Quaternion startRot = transform.rotation;
        Quaternion targetRot = Quaternion.LookRotation(-target.forward, transform.forward);

        //switch (currentSide)
        //{
        //    case 0: targetRot = Quaternion.Euler(0f, -90f, 270f); break;
        //    case 1: targetRot = Quaternion.Euler(-90f, 0f, 180f); break;
        //    case 2: targetRot = Quaternion.Euler(0f, 90f, 90f); break;
        //}

        Debug.Log(string.Format("pO:{0} pT:{1} fT{2}", startPos, targetPos, target.forward));

        float time = 0.0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            //Vector3 p1 = Vector3.Lerp(startPos, new Vector3(startPos.x, startPos.)

            transform.position = Vector3.Lerp(startPos, targetPos, t);
            transform.rotation = Quaternion.Lerp(startRot, targetRot, t);

            yield return null;
        }

        startPos = transform.position;
        targetPos = startPos + transform.forward * 5f;

        time = 0.0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            transform.position = Vector3.Lerp(startPos, targetPos, t);

            yield return null;
        }

        OnDeath();
    }

    IEnumerator ChangeSide(int targetSide, float duration)
    {
        changingSide = true;
        currentSide = targetSide;

        Messenger<Sound>.Broadcast(GameEvent.ON_SOUND_PLAY, Sound.Swipe);

        Vector3 startPos = transform.localPosition;
        Vector3 targetPos = sides[targetSide];

        Quaternion startRot = transform.localRotation;
        Quaternion targetRot = Quaternion.Euler(0f, 0f, 0f);

        switch (targetSide)
        {
            case 0: targetRot = Quaternion.Euler(0f, 0f, 270f); break;
            case 1: targetRot = Quaternion.Euler(0f, 0f, 180f); break;
            case 2: targetRot = Quaternion.Euler(0f, 0f, 90f); break;
        }

        float time = 0.0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            transform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            transform.localRotation = Quaternion.Lerp(startRot, targetRot, t);

            yield return null;
        }

        changingSide = false;
    }

    private void OnPause(bool pause)
    {
        getInput = !pause;
    }
}
