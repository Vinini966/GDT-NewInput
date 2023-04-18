using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class Player_Controler : MonoBehaviour
{
    
    public enum facing { RIGHT, LEFT };
    [Header("External")]
    public Transform rayTrans;
    private CapsuleCollider2D capCol;
    public GameObject atkPoint;
    private BoxCollider2D sword;
    private Animator anim;
    public AudioSource sound;
    public GameObject speech;
    public Image tail;

    [Header("Movement")]
    public bool lockInputs;
    [Range(0.0f, 10.0f)]
    public float jumpForce;
    [Range(0.0f, 10.0f)]
    public float speed;
    [Range (0.0f, 2.0f)]
    public float maxAcell;
    [Range(0.0f, 10.0f)]
    public float Deceleration;
    [Range(0.1f, 10.0f)]
    public float maxSpeed;
    public Vector2 velocity;
    [SerializeField]
    public facing dir = facing.RIGHT;

    [Header("Collision")]
    [Range(0.0f, 1f)]
    public float rayLength;
    private float curveMod = 0.01f;
    public bool grounded;

    private float input;
    private bool jump2;
    private bool wall;
    private bool right;
    private bool dirChange = false;

    [Header("Health")]
    public int Health;
    public float invulTimer;
    private float invulCtnDwn;

    [Header("Sound")]
    public AudioClip Atack;
    public AudioClip Jump;
    public AudioClip Hurt;

    //public Text healthBar;

    InputManger _inputManger;

    Vector3 _baseScale;

    // Start is called before the first frame update
    void Start()
    {
        //anim = GetComponent<Animator>();
        capCol = GetComponent<CapsuleCollider2D>();
        //sword = atkPoint.GetComponent<BoxCollider2D>();
        //sound = GetComponent<AudioSource>();

        //healthBar.text = "Health: " + Health;

        _inputManger = InputManger.Instance;

        _inputManger?.EnablePlayerControls();

        _inputManger.ActionMap.Player.Fire.performed += _ => OnFire();

        _baseScale = transform.localScale;
    }

    private void OnFire()
    {
        Debug.Log("Pew!");
    }

    private void OnEnable()
    {
        _inputManger?.EnablePlayerControls();
    }

    private void OnDisable()
    {
        _inputManger?.DisablePlayerControls();
    }

    // Update is called once per frame
    void Update()
    {
        input = Input.GetAxisRaw("Horizontal");

        if (input > 0)
        {
            transform.localScale = _baseScale;
            if (dir == facing.LEFT)
                dirChange = true;
            else
                dirChange = false;
            dir = facing.RIGHT;
        }
        else if (input < 0)
        {
            Vector3 scale = _baseScale;
            scale.x *= -1;
            transform.localScale = scale;
            if (dir == facing.RIGHT)
                dirChange = true;
            else
                dirChange = false;
            dir = facing.LEFT;
        }


        if (input != 0 && !lockInputs)
        {
            velocity.x = Mathf.MoveTowards(velocity.x, speed * input, maxAcell * Time.deltaTime);
            if (dirChange)
                velocity.x /= 2.5f;
        }
        else
        {
            velocity.x = Mathf.MoveTowards(velocity.x, 0, Deceleration * Time.deltaTime);
        }
           

        if (Mathf.Abs(velocity.x) > maxSpeed)//limits speed
            velocity.x = maxSpeed * Mathf.Sign(velocity.x);

        velocity.y += Physics2D.gravity.y * Time.deltaTime;
        //gM.anim.SetFloat("Walking",input);
  

        //deals with checking for floors and slopes
        LayerMask lM = 1 << 8;
        RaycastHit2D hit2D = Physics2D.Raycast(rayTrans.position, -Vector2.up, rayLength, lM);
        RaycastHit2D hit2DL = Physics2D.Raycast(rayTrans.position, RotatePoint(-Vector2.up, 1, 35f), rayLength - curveMod, lM);
        RaycastHit2D hit2DR = Physics2D.Raycast(rayTrans.position, RotatePoint(-Vector2.up, 1, -35f), rayLength - curveMod, lM);
        Debug.DrawRay(rayTrans.position, RotatePoint(-Vector2.up, 1, 35f) * (rayLength - curveMod), Color.green);
        Debug.DrawRay(rayTrans.position, RotatePoint(-Vector2.up, 1, -35f) * (rayLength - curveMod), Color.blue);
        Debug.DrawRay(rayTrans.position, -Vector2.up * rayLength, Color.red);

        //Debug.Log((Vector3)(-Vector2.up * rayLength) + rayTrans.position);
        if (hit2D.collider != null)
        {
            //landing Anim
            if (hit2D.collider.tag == "Ground")
            {
                //if (!grounded)
                //    anim.SetBool("Jump", false);
                //anim.SetTrigger("Ground");
                velocity.y = 0;
                grounded = true;
                jump2 = false;
            }
        }
        else if (hit2DL.collider != null)
        {
            //landing Anim
            if (hit2DL.collider.tag == "Ground")
            {
                //if (!grounded)
                //    anim.SetBool("Jump", false);
                velocity.y -= Physics2D.gravity.y * Time.deltaTime;
                grounded = true;
                jump2 = false;
            }
        }
        else if (hit2DR.collider != null)
        {
            //landing Anim
            if (hit2DR.collider.tag == "Ground")
            {
                //if(!grounded)
                //    anim.SetBool("Jump", false);
                velocity.y -= Physics2D.gravity.y * Time.deltaTime;
                grounded = true;
                jump2 = false;
            }
        }
        else
            grounded = false;

        

        //anti-wall clipping
        if (wall)
            velocity.x = 0;

        //jumping
        if (grounded || !jump2)
        {
            if (_inputManger.ActionMap.Player.Jump.WasPerformedThisFrame())
            {
                //sound.PlayOneShot(Jump);
                ////anim.SetBool("Jump", true);
                ////anim.SetTrigger("Jump");
                velocity.y = Mathf.Sqrt(2 * jumpForce * Mathf.Abs(Physics2D.gravity.y));
                if (!grounded)
                {
                    jump2 = true;
                }
             }
            
        }


        transform.Translate(velocity * Time.deltaTime);

        wall = false;

        Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position , capCol.size, 0);
        foreach (Collider2D hit in hits)
        {
            if (hit == capCol || hit == sword || hit.isTrigger)
                continue;

            ColliderDistance2D colliderDistance = hit.Distance(capCol);

            if (colliderDistance.isOverlapped)
            {
                transform.Translate(colliderDistance.pointA - colliderDistance.pointB);
                if (Vector2.Angle(colliderDistance.normal, Vector2.down) < 90 && velocity.y > 0)
                {
                    velocity.y *= -0.5f;


                }
                if (hit.tag == "Baddie")
                {
                    TakeDamage(1);
                }
                if (Vector2.Angle(colliderDistance.normal, Vector2.right) == 180 && velocity.x > 0)
                {
                    Debug.Log("Triggered");
                    wall = true;
                }
                if (Vector2.Angle(colliderDistance.normal, Vector2.left) == 180 && velocity.x < 0)
                {
                    Debug.Log("Triggered");
                    wall = true;
                }
            }

            if(invulCtnDwn > 0)
            {
                invulCtnDwn -= Time.deltaTime;
            }
            
        }


    }


    public Vector2 RotatePoint(Vector2 inputPoint, float radius, float degrees)
    {
        float rads = degrees * Mathf.Deg2Rad;
        float x = Mathf.Cos(rads + Mathf.Acos(inputPoint.x / radius));
        //Debug.Log("X: " + Mathf.Acos(inputPoint.x / radius));
        float y = Mathf.Sin(rads + Mathf.Asin(inputPoint.y / radius));
        //Debug.Log("Y: " + y);
        Vector2 output = new Vector2(x, y);
        return output;
    }

    public void TakeDamage(int amt)
    {
        //if (invulCtnDwn <= 0)
        //{
        //    sound.PlayOneShot(Hurt);
        //    Health--;
        //    gM.updateHealth(Health);
        //    invulCtnDwn = invulTimer;
        //}
        //if (Health <= 0)
        //{

        //    lockInputs = true;
        //    SceneChange.Instance().sceneToChange = "TitleTest";
        //    SceneChange.Instance().GO = true;
        //    SceneChange.Instance().startChange();
        //}
    }
}
