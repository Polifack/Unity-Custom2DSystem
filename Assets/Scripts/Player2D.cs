using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class Player2D : MonoBehaviour
{
    Controller2D m_Controller;
    Vector3 m_Velocity;
    float m_JumpVelocity;
    float m_CurrentVelocity;

    public float Gravity;
    public float MoveSpeed;
    public float JumpHeight;
    public float JumpTime;
    public KeyCode JumpKey;
    public float AccelTimeAir;
    public float AccelTimeGround;

    void Start()
    {
        m_Controller = GetComponent<Controller2D>();

        //Resolvemos la gravedad y velocidad del salto deseadas con la altura y el tiempo
        //dx = vi * t + (a*t^2)/2
        //variacion movimiento = velocidad inicial * tiempo + (aceleracion * tiempo^2)/2
        //
        //v(t) = vi+a*t 
        //La velocidad en un punto de tiempo concreto es la velocidad inicial + aceleracion * tiempo

        Gravity = -(2 * JumpHeight) / Mathf.Pow(JumpTime, 2);
        m_JumpVelocity = Mathf.Abs(Gravity * JumpTime);
    }

    void Update()
    {
        //Si no estamos en terreno resetea la gravedad
        if (m_Controller.getCollisions().above || m_Controller.getCollisions().below)
        {
            m_Velocity.y = 0;
        }

        m_Velocity.y += Gravity * Time.deltaTime;
        GetInput();
        m_Controller.Move(m_Velocity*Time.deltaTime);
    }

    void GetInput()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        
        if (Input.GetKeyDown(JumpKey) && m_Controller.getCollisions().below)
        {
            m_Velocity.y = m_JumpVelocity;
        }
        float targetVelocityX = input.x * MoveSpeed;

        //Cambiamos la aceleracion si estamos en suelo o aire
        m_Velocity.x = Mathf.SmoothDamp(m_Velocity.x, targetVelocityX, ref m_CurrentVelocity, 
            m_Controller.getCollisions().below?AccelTimeGround:AccelTimeAir);
    }
}
