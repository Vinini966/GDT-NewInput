using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    InputManger _inputManger;

    Vector2 _velocity;

    public float Acceleration;
    public float MaxAcceleration;
    public float Friction;
    public float JumpPower;
    public float Gravity;

    // Start is called before the first frame update
    void Start()
    {
        _inputManger = InputManger.Instance;

        _inputManger.ActionMap.Player.Fire.performed += _ => OnFire();
        _inputManger.ActionMap.Player.Jump.performed += _ => OnJump();

        _velocity = new Vector2(0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 moveDirection = _inputManger.ActionMap.Player.Move.ReadValue<Vector2>();

        _velocity.x = moveDirection.x * Acceleration * Time.deltaTime;
        _velocity.x = Mathf.Clamp(_velocity.x, -MaxAcceleration, MaxAcceleration);

        transform.position += new Vector3(_velocity.x,
                                          _velocity.y,
                                          0);

    }

    private void OnEnable()
    {
        _inputManger.EnablePlayerControls();
    }

    private void OnDisable()
    {
        _inputManger.DisablePlayerControls();
    }

    public void OnFire()
    {

    }

    public void OnJump()
    {

    }

    public void OnFixedUpdate()
    {

    }

}
