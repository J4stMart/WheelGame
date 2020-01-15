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
    public GameObject chargeBar;

    public bool jumpEnabled = true;
    public bool brakeEnabled = true;

    private Rigidbody2D rb2d;
    private Vector2 velocity; //Current velocity of the wheel
    private float addYVelocity;

    private bool grounded;
    private bool facingLeft; //This one I made to check the orientation the player is facing. True = facing left, False = facing right

    private float charge = 0;
    private float distanceFullRotation;
    private float wheelVelocity; //for visual rotation

    private Vector3 spawnLocation;

    enum ChargingState
    {
        CantCharge,
        CanCharge,
        IsCharging
    }

    private ChargingState chargingState = ChargingState.CantCharge;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();

        var circleCollider = GetComponent<CircleCollider2D>();
        distanceFullRotation = 2 * circleCollider.bounds.extents[0] * Mathf.PI;

        var box = GameObject.FindWithTag("TutorialStart").GetComponent<BoxCollider2D>();

        if (box)
        {
            if (box.bounds.Contains(transform.position))
            {
                jumpEnabled = false;
                brakeEnabled = false;
            }
        }
    }

    void Update()
    {
        CheckGrounded();
        CheckWalls();

        ComputeVelocity();

        if (wheelVelocity != 0)
            sprite.transform.Rotate(Vector3.forward, -wheelVelocity * Time.deltaTime / distanceFullRotation * 360);

        arrowSprite.flipY = facingLeft;

        Debug.Log(chargingState);
    }

    private void ComputeVelocity()
    {
        if (Mathf.Abs(velocity.x) < 0.1f)
        {
            velocity.x = 0;
            wheelVelocity = 0;
        }

        if (velocity.x == 0)
        {
            if ((Input.GetButtonDown("Left") || Input.GetButtonDown("Right")) && chargingState == ChargingState.CanCharge)
            {
                chargingState = ChargingState.IsCharging;
            }

            if ((Input.GetButton("Left") || Input.GetButton("Right")) && chargingState == ChargingState.IsCharging)
            {
                charge += Time.deltaTime * (chargeSpeed + Mathf.Sqrt(charge));
                charge = Mathf.Clamp(charge, 0, maxCharge);

                if (facingLeft)
                    wheelVelocity = -charge * speed;
                else
                    wheelVelocity = charge * speed;

                chargeBar.transform.localScale = new Vector2(charge / maxCharge, 1);
            }
            else if (Input.GetButtonUp("Left") || Input.GetButtonUp("Right"))
            {
                if (facingLeft)
                {
                    velocity.x = -charge * speed;
                }
                else
                {
                    velocity.x = charge * speed;
                }

                charge = 0;
                chargingState = ChargingState.CantCharge;
            }
        }
        else
        {
            if (Input.GetButton("Down") && brakeEnabled)
            {
                Vector2 brakeVelocity = velocity.normalized * Time.deltaTime * 30;
                velocity.x -= brakeVelocity.x;
                if (brakeVelocity.y > 0)
                    addYVelocity -= brakeVelocity.y;
            }
        }

        if (Input.GetButtonDown("Left") && velocity.x == 0)
        {
            facingLeft = true;

            velocity.x = -Mathf.Abs(velocity.x);
        }
        else if (Input.GetButtonDown("Right") && velocity.x == 0)
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
            else
            {
                if (chargingState != ChargingState.IsCharging)
                    chargingState = ChargingState.CanCharge;
            }

            if (Input.GetButtonDown("Jump") && jumpEnabled)
            {
                addYVelocity = jumpTakeOffSpeed;
            }
        }
        else
        {
            velocity.x -= Mathf.Sign(velocity.x) * momentumReduction / 5 * Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        rb2d.velocity = new Vector2(velocity.x, rb2d.velocity.y + addYVelocity);
        velocity = rb2d.velocity;
        addYVelocity = 0;
    }

    private void Respawn()
    {

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

            addYVelocity += jumpTakeOffSpeed * (Mathf.Abs(velocity.x) / (maxCharge * speed));
            velocity.x -= Mathf.Sign(velocity.x) * 2 * jumpTakeOffSpeed * (Mathf.Abs(velocity.x) / (maxCharge * speed));
        }
    }
}