using UnityEngine;

public class SimpleRotate : MonoBehaviour
{
    enum Axis { X, Y, Z };

    [SerializeField, Range(0f, 100f)] private float rotationSpeed = 5f;
    [SerializeField] Axis axis;
    void Update()
    {
        float x = 0f;
        float y = 0f;
        float z = 0f;

        switch(axis)
        {
            case Axis.X:
                x = rotationSpeed * Time.deltaTime;
                break;
            case Axis.Y:
                y = rotationSpeed * Time.deltaTime;
                break;
            case Axis.Z:
                z = rotationSpeed * Time.deltaTime;
                break;
        }

        transform.Rotate(x, y, z, Space.Self);
    }
}
