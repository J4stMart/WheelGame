using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PhysicsObject : MonoBehaviour
{

    public float minGroundNormal = 48f;
    public float gravityModifier = 1f;
    public bool physicsActive = true;

    protected Vector2 targetVelocity;
    protected bool grounded;
    protected Vector2 groundNormal;
    protected Rigidbody2D rb2d;
    protected Vector2 velocity;
    protected ContactFilter2D contactFilter;
    protected RaycastHit2D[] hitBuffer = new RaycastHit2D[16];
    protected List<RaycastHit2D> hitBufferList = new List<RaycastHit2D>(16);
    protected Vector2 gravity;
    protected Vector2 gravityVelocity;

    protected bool parentHackHitWall = false;


    protected const float minMoveDistance = 0.001f;
    protected const float shellRadius = 0.01f;

    void OnEnable()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    protected virtual void Start()
    {
        contactFilter.useTriggers = false;
        contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        contactFilter.useLayerMask = true;

        gravity = Physics2D.gravity;
    }

    void Update()
    {
        targetVelocity = Vector2.zero;

        if (physicsActive)
            ComputeVelocity();
    }

    protected virtual void ComputeVelocity()
    {

    }

    protected virtual void FixedUpdate()
    {
        gravityVelocity += gravityModifier * gravity * Time.deltaTime;
        velocity = targetVelocity + gravityVelocity;

        grounded = false;

        Vector2 deltaPosition = velocity * Time.deltaTime;

        Vector2 moveAlongGround = new Vector2(groundNormal.y, -groundNormal.x);

        Vector2 move = moveAlongGround * Vector2.Dot(moveAlongGround, deltaPosition);

        Movement(move, false);

        move = transform.up * Vector2.Dot(transform.up, deltaPosition);

        Movement(move, true);
    }

    void Movement(Vector2 move, bool yMovement)
    {
        float distance = move.magnitude;

        if (distance > minMoveDistance)
        {
            int count = rb2d.Cast(move, contactFilter, hitBuffer, distance + shellRadius);
            hitBufferList.Clear();
            for (int i = 0; i < count; i++)
            {
                hitBufferList.Add(hitBuffer[i]);
            }

            for (int i = 0; i < hitBufferList.Count; i++)
            {
                Vector2 currentNormal = hitBufferList[i].normal;
                if (Vector2.Angle(currentNormal, transform.up) < minGroundNormal)
                {
                    grounded = true;
                    if (yMovement)
                    {
                        groundNormal = currentNormal;
                        currentNormal = transform.up * Vector2.Dot(transform.up, currentNormal);
                    }
                }

                if (yMovement)
                {
                    float projection = Vector2.Dot(gravityVelocity, currentNormal);
                    if (projection < 0)
                    {
                        gravityVelocity = Vector2.zero;
                    }
                }

                float modifiedDistance = hitBufferList[i].distance - shellRadius;

                if (modifiedDistance < distance)
                {
                    if(!yMovement)
                    {
                        parentHackHitWall = true;
                    }

                    distance = modifiedDistance;
                }
            }


        }

        rb2d.position = rb2d.position + move.normalized * distance;
    }
}