using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class Player2D : MonoBehaviour
{
    Controller2D m_Controller;
    Vector3 m_Velocity;

    public float Gravity;
    public float MoveSpeed;

    void Start()
    {
        m_Controller = GetComponent<Controller2D>();
    }

    void Update()
    {
        //Si no estamos en terreno resetea la gravedad
        if (m_Controller.getCollisions().above || m_Controller.getCollisions().below)
            m_Velocity.y = 0;

        m_Velocity.y += Gravity * Time.deltaTime;
        GetInput();
        m_Controller.Move(m_Velocity*Time.deltaTime);
    }

    void GetInput()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        m_Velocity.x = input.x * MoveSpeed;
    }
}
