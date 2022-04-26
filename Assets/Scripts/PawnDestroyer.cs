using UnityEngine;

[RequireComponent(typeof(BoxCollider), typeof(Rigidbody))]
public class PawnDestroyer : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        TagSystem ts = other.GetComponent<TagSystem>();
        if (ts != null && !ts.tags.Contains(Tags.Tunnel))
        {
            Destroy(other.gameObject);
        }
    }
}
