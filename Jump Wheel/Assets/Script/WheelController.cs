using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelController : PhysicsObject
{

    public float maxSpeed = 7;
    public float jumpTakeOffSpeed = 7;
    public bool flipped;
    Vector2 move = Vector2.zero;
    float charge = 0;

    //public SpriteRenderer spriteRenderer;
    //private Animator animator;

    // Use this for initialization
    void Awake()
    {
        //spriteRenderer = GetComponent<SpriteRenderer>();
        //animator = GetComponent<Animator>();
    }

    protected override void ComputeVelocity()
    {
        move.y = 0;

        if(parentHackHitWall)
        {
            move.x *= -1;
            parentHackHitWall = false;
        }

        if(Input.GetKey(KeyCode.Space) && move.x == 0)
        {
            charge += Time.deltaTime;
        }
        else if (Input.GetKeyUp(KeyCode.Space) && move.x == 0)
        {
            move.x = charge;
        }

        if (Input.GetButtonDown("Left"))
        {
            move.x = -Mathf.Abs(move.x);
        }
        if (Input.GetButtonDown("Right"))
        {
            move.x = Mathf.Abs(move.x);
        }

        if (Input.GetButtonDown("Jump") && grounded && move.x != 0)
        {
            gravityVelocity.y = jumpTakeOffSpeed;
        }
        else if (Input.GetButtonUp("Jump"))
        {
            if (gravityVelocity.y > 0)
            {
                gravityVelocity.y *= 0.5f;
            }
        }

        //bool flipSprite = (!flipped ? (move.x > 0f) : (move.x < 0f));
        //if (flipSprite)
        //{
        //    transform.localScale = new Vector3(transform.localScale.x * -1f, transform.localScale.y);
        //    flipped = !flipped;
        //}

        //animator.SetBool("grounded", grounded);
        //animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);

        targetVelocity = move * maxSpeed;
    }
}