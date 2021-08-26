using System.Collections;
using UnityEngine;

public abstract class MovingObject : MonoBehaviour
{
    //The abstract keyword enables you to create classes and class members that are incomplete and must be implemented in a derived class.
    public float moveTime = 0.1f;
    public LayerMask blockingLayer;

    private BoxCollider2D boxCollider;
    private Rigidbody2D rd2d;
    private float inverseMoveTime;
    private bool isMoving;

    protected virtual void Start()
    {//Protected, virtual functions can be overridden by inheriting classes.
        boxCollider = GetComponent<BoxCollider2D>();
        rd2d = GetComponent<Rigidbody2D>();
        inverseMoveTime = 1f / moveTime;
        //By storing the reciprocal of the move time we can use it by multiplying instead of dividing, this is more efficient.
    }

    protected bool Move(int xDir, int yDir, out RaycastHit2D hit)
    {
        
        Vector2 start = transform.position;
        Vector2 end = start + new Vector2(xDir, yDir);

        boxCollider.enabled = false;
        hit = Physics2D.Linecast(start, end, blockingLayer);

        boxCollider.enabled = true;

        if (hit.transform == null && !isMoving)
        {
            StartCoroutine(SmoothMovement(end));
            return true;
        }
        return false;
    }

    protected IEnumerator SmoothMovement(Vector3 end)
    {
        isMoving = true;
        float sqrRemainingDistance = (transform.position - end).sqrMagnitude;
        while (sqrRemainingDistance > float.Epsilon)
        {
            Vector3 newPosition = Vector3.MoveTowards(rd2d.position, end, inverseMoveTime * Time.deltaTime);
            rd2d.MovePosition(newPosition);
            //Calculate the remaining distance to move based on the square magnitude of the difference between current position and end parameter. 
            //Square magnitude is used instead of magnitude because it's computationally cheaper.
            sqrRemainingDistance = (transform.position - end).sqrMagnitude;
            yield return null;
        }
        rd2d.MovePosition(end);
        isMoving = false;
    }

    protected virtual void AttemptMove<T>(int xDir, int yDir)
        where T : Component
    {
        RaycastHit2D hit;
        bool canMove = Move(xDir, yDir, out hit);
        if (hit.transform == null)
        {
            return;
        }
        T hitComponent = hit.transform.GetComponent<T>();
        if (!canMove && hitComponent != null)
        {
            OnCantMove(hitComponent);
        }
    }
    protected abstract void OnCantMove<T>(T component)
        where T : Component;
}
