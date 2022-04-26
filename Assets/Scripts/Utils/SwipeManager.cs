using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum Swipe { Left, Up, Right, Down, UpRight, DownRight, DownLeft, UpLeft, Tap, None };

class CardinalDirection
{
    public static readonly Vector2 Up = new Vector2(0, 1);
    public static readonly Vector2 Down = new Vector2(0, -1);
    public static readonly Vector2 Right = new Vector2(1, 0);
    public static readonly Vector2 Left = new Vector2(-1, 0);
    public static readonly Vector2 UpRight = new Vector2(1, 1);
    public static readonly Vector2 UpLeft = new Vector2(-1, 1);
    public static readonly Vector2 DownRight = new Vector2(1, -1);
    public static readonly Vector2 DownLeft = new Vector2(-1, -1);
}

public class SwipeManager : MonoBehaviour
{
    #region Inspector Variables & Constatnts

    [Tooltip("Min swipe distance (inches) to register as swipe")]
    [SerializeField] float minSwipeLength = 0.5f;

    #endregion

    //const float fourDirAngle = 0.5f;
    const float eightDirAngle = 0.906f;
    const float defaultDPI = 72f;
    const float dpcmFactor = 2.54f;

    static Dictionary<Swipe, Vector2> cardinalDirections = new Dictionary<Swipe, Vector2>()
    {
        { Swipe.Up,         CardinalDirection.Up        },
        { Swipe.Down,       CardinalDirection.Down      },
        { Swipe.Right,      CardinalDirection.Right     },
        { Swipe.Left,       CardinalDirection.Left      },
        { Swipe.UpRight,    CardinalDirection.UpRight   },
        { Swipe.UpLeft,     CardinalDirection.UpLeft    },
        { Swipe.DownRight,  CardinalDirection.DownRight },
        { Swipe.DownLeft,   CardinalDirection.DownLeft  }
    };

    public static Vector2 swipeVelocity;

    static float dpcm;
    static bool swipeEnded;
    static Swipe swipeDirection;
    static Vector2 firstPressPos;
    static Vector2 secondPressPos;
    static SwipeManager instance;

    void Awake()
    {
        instance = this;
        float dpi = (Screen.dpi == 0) ? defaultDPI : Screen.dpi;
        dpcm = dpi / dpcmFactor;
    }

    void Update()
    {
        DetectSwipe();
    }

    static void DetectSwipe()
    {
        if (GetTouchInput() || GetMouseInput())
        {
            // Swipe already ended, don't detect until a new swipe has begun
            if (swipeEnded)
            {
                return;
            }

            Vector2 currentSwipe = secondPressPos - firstPressPos;
            float swipeCm = currentSwipe.magnitude / dpcm;

            // Check the swipe is long enough to count as a swipe (not a touch, etc)
            if (swipeCm < instance.minSwipeLength)
            {
                swipeDirection = Swipe.Tap;
                Messenger<Swipe, Vector2>.Broadcast(GameEvent.ON_SWIPE, swipeDirection, secondPressPos);
                return;
            }
                        
            swipeDirection = GetSwipeDirByTouch(currentSwipe);
            swipeEnded = true;

            Messenger<Swipe, Vector2>.Broadcast(GameEvent.ON_SWIPE, swipeDirection, secondPressPos);
        }
        else
        {
            swipeDirection = Swipe.None;
        }
    }

    #region Helper Functions

    static bool GetTouchInput()
    {
        if (Input.touches.Length > 0)
        {
            Touch t = Input.GetTouch(0);

            // Swipe/Touch started
            if (t.phase == TouchPhase.Began)
            {
                firstPressPos = t.position;
                swipeEnded = false;
                // Swipe/Touch ended
            }
            else if (t.phase == TouchPhase.Ended)
            {
                secondPressPos = t.position;
                return true;
                // Still swiping/touching
            }
        }

        return false;
    }

    static bool GetMouseInput()
    {
        // Swipe/Click started
        if (Input.GetMouseButtonDown(0))
        {
            firstPressPos = Input.mousePosition;
            swipeEnded = false;
            // Swipe/Click ended
        }
        else if (Input.GetMouseButtonUp(0))
        {
            secondPressPos = Input.mousePosition;
            return true;
            // Still swiping/clicking
        }

        return false;
    }

    static bool IsDirection(Vector2 direction, Vector2 cardinalDirection)
    {
        return Vector2.Dot(direction, cardinalDirection) > eightDirAngle;
    }

    static Swipe GetSwipeDirByTouch(Vector2 currentSwipe)
    {
        currentSwipe.Normalize();
        var swipeDir = cardinalDirections.FirstOrDefault(dir => IsDirection(currentSwipe, dir.Value));
        return swipeDir.Key;
    }

    #endregion
}