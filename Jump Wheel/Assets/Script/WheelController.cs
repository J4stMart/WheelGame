using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class WheelController : MonoBehaviour
{

    public float minGroundNormal = 48f;
    public float minWallNormal = 45f;
    public float gravityMultiplier = 1f;
    public bool physicsActive = true;

    protected bool grounded;
    private bool airTurn; //I made this bool to check if the player has already turned orientation in air, so they can only turn once in the air.
    private bool facingLeft; //This one I made to check the orientation the player is facing. True = facing left, False = facing right

    public bool FacingLeft
    {
        get { return facingLeft; }
        set
        {
            facingLeft = value;
            if (!grounded)
            {
                airTurn = true;
            }
        }
    }

    protected Vector2 groundNormal = Vector2.up;
    protected Rigidbody2D rb2d;
    protected Vector2 velocity;
    protected ContactFilter2D contactFilter;
    protected RaycastHit2D[] hitBuffer = new RaycastHit2D[16];
    protected List<RaycastHit2D> hitBufferList = new List<RaycastHit2D>(16);

    protected const float minMoveDistance = 0.001f;
    protected const float shellRadius = 0.05f;

    public float maxSpeed = 7;
    public float jumpTakeOffSpeed = 7;
    public bool flipped;
    float charge = 0;
    public float wallmomentumLoss = .1f;
    public float chargeForce = 1.5f;
    public float momentumReduction = 1;
    public GameObject sprite;
    private float wheelVelocity; //for rotation
    public SpriteRenderer arrowSprite; //temporay arrow speed to easily see which way the player is facing;

    private float distanceFullRotation;

    void OnEnable()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        contactFilter.useTriggers = false;
        contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        contactFilter.useLayerMask = true;

        var circleCollider = GetComponent<CircleCollider2D>();
        distanceFullRotation = 2 * circleCollider.bounds.extents[0] * Mathf.PI;
    }

    void Update()
    {
        if (physicsActive)
            ComputeVelocity();

        if (wheelVelocity != 0)
            sprite.transform.Rotate(Vector3.forward, -wheelVelocity * Time.deltaTime / distanceFullRotation * 360);

        arrowSprite.flipY = facingLeft;
    }

    private void ComputeVelocity()
    {
        if (Mathf.Abs(velocity.x) < 1f)
        {
            velocity.x = 0;
            wheelVelocity = 0;
        }

        if ((Input.GetKey(KeyCode.Space) || Input.GetButton("Up")) && velocity.x == 0 && grounded)
        {
            charge += Time.deltaTime * chargeForce;
            wheelVelocity = charge * maxSpeed;
        }
        else if ((Input.GetKeyUp(KeyCode.Space) || Input.GetButtonUp("Up")) && velocity.x == 0)
        {
            if (facingLeft)
            {
                velocity.x = -charge * maxSpeed;
            }
            else
            {
                velocity.x = charge * maxSpeed;
            }
            charge = 0;
        }

        if (Input.GetButtonDown("Left") && !airTurn)
        {
            if (velocity.x >= 0)
                FacingLeft = true;

            velocity.x = -Mathf.Abs(velocity.x);
        }
        if (Input.GetButtonDown("Right") && !airTurn)
        {
            if (velocity.x <= 0)
                FacingLeft = false;

            velocity.x = Mathf.Abs(velocity.x);
        }

        if (Input.GetButton("Down") && velocity.x != 0 && grounded)
        {
            if (velocity.x > 0)
            {
                velocity.x -= Time.deltaTime * 30;
            }
            else
            {
                velocity.x += Time.deltaTime * 30;
            }
        }

        if (Input.GetButtonDown("Jump") && grounded && velocity.x != 0)
        {
            velocity.y = jumpTakeOffSpeed;
        }

        if (grounded)
        {
            if (velocity.x != 0)
            {
                velocity.x -= Mathf.Sign(velocity.x) * momentumReduction * Time.deltaTime;
                wheelVelocity = velocity.x;
            }

            airTurn = false;
        }
        else
        {
            velocity.x -= Mathf.Sign(velocity.x) * momentumReduction / 5 * Time.deltaTime;
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
                if (Vector2.Angle(currentNormal, Vector2.up) < minGroundNormal)
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
                else
                {
                    //hit wall
                    currentNormal.x = Mathf.Abs(currentNormal.x);
                    if (Vector2.Angle(currentNormal, Vector2.right) < minWallNormal)
                    {
                        velocity.x *= -1;
                        facingLeft = !facingLeft;

                        velocity.y += jumpTakeOffSpeed * (Mathf.Abs(velocity.x) / 100);
                        velocity.x -= Mathf.Sign(velocity.x) * 2 * jumpTakeOffSpeed * (Mathf.Abs(velocity.x) / 100);
                    }
                }

                float modifiedDistance = hitBufferList[i].distance - shellRadius;

                if (modifiedDistance < distance)
                    distance = modifiedDistance;
            }
        }

        rb2d.position = rb2d.position + move.normalized * distance;
    }
}