using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class WheelController : MonoBehaviour
{

    public float minGroundNormal = 48f;
    public float gravityMultiplier = 1f;
    public bool physicsActive = true;

    protected Vector2 targetVelocity;
    protected bool grounded;
    protected Vector2 groundNormal = Vector2.up;
    protected Rigidbody2D rb2d;
    protected Vector2 velocity;
    protected ContactFilter2D contactFilter;
    protected RaycastHit2D[] hitBuffer = new RaycastHit2D[16];
    protected List<RaycastHit2D> hitBufferList = new List<RaycastHit2D>(16);

    protected const float minMoveDistance = 0.001f;
    protected const float shellRadius = 0.01f;

    protected bool parentHackHitWall = false;

    public float maxSpeed = 7;
    public float jumpTakeOffSpeed = 7;
    public bool flipped;
    float charge = 0;
    public float wallmomentumLoss = .1f;
    public float chargeForce = 1.5f;
    public float momentumReduction = 1;

    void OnEnable()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    protected virtual void Start()
    {
        contactFilter.useTriggers = false;
        contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        contactFilter.useLayerMask = true;
    }

    void Update()
    {
        targetVelocity = Vector2.zero;

        if (physicsActive)
            ComputeVelocity();
    }

    private void ComputeVelocity()
    {
        if (Mathf.Abs(velocity.x) < 1f)
        {
            velocity.x = 0;
        }

        if (parentHackHitWall)
        {
            velocity.x *= -1;
            parentHackHitWall = false;
            velocity.y = jumpTakeOffSpeed / 2;

            if(velocity.x > 0)
            {
                velocity.x -= wallmomentumLoss;
            }
            else
            {
                velocity.x += wallmomentumLoss;
            }
        }

        if (Input.GetKey(KeyCode.Space) && velocity.x == 0)
        {
            charge += Time.deltaTime * chargeForce;
        }
        else if (Input.GetKeyUp(KeyCode.Space) && velocity.x == 0)
        {
            velocity.x = charge * maxSpeed;
        }

        if (Input.GetButtonDown("Left"))
        {
            velocity.x = -Mathf.Abs(velocity.x);
        }
        if (Input.GetButtonDown("Right"))
        {
            velocity.x = Mathf.Abs(velocity.x);
        }

        if (Input.GetButtonDown("Jump") && grounded && velocity.x != 0)
        {
            velocity.y = jumpTakeOffSpeed;
        }

        if(grounded)
        {
            velocity.x = Mathf.Lerp(velocity.x, 0, momentumReduction * Time.deltaTime);
        }
        else
        {
            velocity.x = Mathf.Lerp(velocity.x, 0, momentumReduction * Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        velocity += gravityMultiplier * Physics2D.gravity * Time.deltaTime;

        grounded = false;

        Vector2 deltaPosition = velocity * Time.deltaTime;

        Vector2 moveAlongGround = new Vector2(groundNormal.y, -groundNormal.x);

        Vector2 move = moveAlongGround * deltaPosition.x;

        Movement(move, false);

        move = Vector2.up * deltaPosition.y;

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
                        currentNormal.x = 0;
                    }
                }

                if (yMovement)
                {
                    float projection = Vector2.Dot(velocity, currentNormal);
                    if (projection < 0)
                    {
                        velocity.y = 0;
                    }
                }

                float modifiedDistance = hitBufferList[i].distance - shellRadius;

                if (modifiedDistance < distance)
                {
                    if (!yMovement)
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