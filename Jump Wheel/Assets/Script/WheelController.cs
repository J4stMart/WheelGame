using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class WheelController : MonoBehaviour
{
    public float speed = 7;
    public float jumpTakeOffSpeed = 9;
    public float chargeSpeed = 1.5f;
    public float maxCharge = 13;
    public float momentumReduction = 5.5f;
    public BoxCollider2D groundCheck;
    public BoxCollider2D wallCheckL, wallCheckR;
    public LayerMask collisionMask;

    public GameObject sprite;
    public SpriteRenderer arrowSprite; //temporary arrow to easily see which way the player is facing;

    private Rigidbody2D rb2d;
    private Vector2 velocity; //Current velocity of the wheel
    private Vector2 addVelocity; //Velocity that needs to be added once

    private bool grounded;
    private bool facingLeft; //This one I made to check the orientation the player is facing. True = facing left, False = facing right

    private float charge = 0;
    private float distanceFullRotation;
    private float wheelVelocity; //for visual rotation

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();

        var circleCollider = GetComponent<CircleCollider2D>();
        distanceFullRotation = 2 * circleCollider.bounds.extents[0] * Mathf.PI;
    }

    void Update()
    {
        CheckGrounded();
        CheckWalls();

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

        if (velocity.x == 0)
        {
            if (Input.GetButton("Left") || Input.GetButton("Right"))
            {
                charge += Time.deltaTime * chargeSpeed;
                charge = Mathf.Clamp(charge, 0, maxCharge);
                wheelVelocity = charge * speed;
            }
            else if (Input.GetButtonUp("Left") || Input.GetButtonUp("Right"))
            {
                if (facingLeft)
                {
                    addVelocity.x = -charge * speed;
                }
                else
                {
                    addVelocity.x = charge * speed;
                }
                charge = 0;
            }
        }
        else
        {
            if (Input.GetButton("Down"))
            {
                velocity.x -= Mathf.Sign(velocity.x) * Time.deltaTime * 30;
            }
        }

        if (Input.GetButtonDown("Left") && (grounded || velocity.x == 0))
        {
            facingLeft = true;

            velocity.x = -Mathf.Abs(velocity.x);
        }
        else if (Input.GetButtonDown("Right") && (grounded || velocity.x == 0))
        {
            facingLeft = false;

            velocity.x = Mathf.Abs(velocity.x);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        if (grounded)
        {
            if (velocity.x != 0)
            {
                velocity.x -= Mathf.Sign(velocity.x) * momentumReduction * Time.deltaTime;
                wheelVelocity = velocity.x;
            }

            if (Input.GetButtonDown("Jump"))
            {
                addVelocity.y = jumpTakeOffSpeed;
            }
        }
        else
        {
            velocity.x -= Mathf.Sign(velocity.x) * momentumReduction / 5 * Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        rb2d.velocity = new Vector2(velocity.x + addVelocity.x, rb2d.velocity.y + addVelocity.y);
        velocity = rb2d.velocity;
        addVelocity = Vector2.zero;
    }

    private void CheckGrounded()
    {
        RaycastHit2D hit = Physics2D.BoxCast(groundCheck.bounds.center, groundCheck.bounds.size, 0f, Vector2.down, 0f, collisionMask);
        grounded = hit.collider != null;
    }

    private void CheckWalls()
    {
        RaycastHit2D hit = Physics2D.BoxCast(wallCheckL.bounds.center, wallCheckL.bounds.size, 0f, Vector2.left, 0f, collisionMask);

        if (hit.collider == null)
            hit = Physics2D.BoxCast(wallCheckR.bounds.center, wallCheckR.bounds.size, 0f, Vector2.right, 0f, collisionMask);

        if (hit.collider != null)
        {
            velocity.x *= -1;
            facingLeft = !facingLeft;

            addVelocity.y += jumpTakeOffSpeed * (Mathf.Abs(velocity.x) / (maxCharge * speed));
            addVelocity.x -= Mathf.Sign(velocity.x) * 2 * jumpTakeOffSpeed * (Mathf.Abs(velocity.x) / (maxCharge * speed));
        }
    }
}