using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Every 2D Element will have a BoxCollder
[RequireComponent(typeof (BoxCollider2D))]
public class Controller2D : MonoBehaviour
{

    //Auxiliar structure to define where we raycast
    struct RaycastOrigins
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }

    public struct CollisionInfo
    {
        public bool above, below;
        public bool left, right;

        public void Reset()
        {
            above = below = false;
            left = right = false;
        }
    }

    //Define the width of the bounds where we raycast
    const float skinWidth = .015f;

    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;
    public bool debug = false;
    public LayerMask collisionMask;

    float horizontalRaySpacing;
    float verticalRaySpacing;

    BoxCollider2D m_BoxCollider;
    RaycastOrigins m_RaycastOrigins;
    CollisionInfo m_Collisions;

    void Start()
    {
        m_BoxCollider = GetComponent<BoxCollider2D>();
        CalculateRaySpacing();
    }

    public void Move(Vector3 velocity)
    {
        m_Collisions.Reset();
        UpdateRaycastOrigin();

        if (velocity.x!=0)
            HorizontalCollisions(ref velocity);

        if (velocity.y != 0)
            VerticalCollisions(ref velocity);

        transform.Translate(velocity);
    }
    public CollisionInfo getCollisions()
    {
        return m_Collisions;
    }


    void VerticalCollisions(ref Vector3 velocity)
    {
        //Pasamos el velocity por referencia para poder modificarla dentro del metodo

        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + skinWidth;

        for (int i = 0; i < verticalRayCount; i++)
        {
            //Si nos movemos hacia arriba detectamos las colisiones desde arriba (topleft)
            //Si nos movemos hacia abajo detectamos las colisiones desde abajo (bottomleft)
            Vector2 rayOrigin = (directionY == -1) ? m_RaycastOrigins.bottomLeft : m_RaycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);

            //Hacemos el raycast como tal
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

            //Dibujamos las lineas si tenemos que hacerlo
            if (debug) Debug.DrawRay(rayOrigin, Vector2.up*directionY, Color.red);

            //Si colideamos con algo
            if (hit)
            {
                velocity.y = (hit.distance-skinWidth) * directionY;
                //Cambiamos la rayLegth en cuanto chocamos con algo para que no choquemos con algo más "lejos"
                rayLength = hit.distance;

                //Actualizamos la matriz de colisiones con los bool de direccion
                m_Collisions.below = directionY == -1;
                m_Collisions.above = directionY == 1;
            }
        }
    }
    void HorizontalCollisions(ref Vector3 velocity)
    {
        //Pasamos el velocity por referencia para poder modificarla dentro del metodo
        float directionX = Mathf.Sign(velocity.x);
        float rayLength = Mathf.Abs(velocity.x) + skinWidth;

        for (int i = 0; i < horizontalRayCount; i++)
        {
            //Si nos movemos hacia arriba detectamos las colisiones desde arriba (topleft)
            //Si nos movemos hacia abajo detectamos las colisiones desde abajo (bottomleft)
            Vector2 rayOrigin = (directionX == -1) ? m_RaycastOrigins.bottomLeft : m_RaycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);

            //Hacemos el raycast como tal
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            //Dibujamos las lineas si tenemos que hacerlo
            if (debug) Debug.DrawRay(rayOrigin, Vector2.right*directionX, Color.red);

            //Si colideamos con algo
            if (hit)
            {
                velocity.x = (hit.distance - skinWidth) * directionX;
                //Cambiamos la rayLegth en cuanto chocamos con algo para que no choquemos con algo más "lejos"
                rayLength = hit.distance;

                //Actualizamos la matriz de colisiones con los bool de direccion
                m_Collisions.left = directionX == -1;
                m_Collisions.right = directionX == 1;
            }
        }
    }
    void UpdateRaycastOrigin()
    {
        Bounds bounds = m_BoxCollider.bounds;
        //Multiply by -2 to shrink.
        bounds.Expand(skinWidth * -2);

        //Define the points
        m_RaycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        m_RaycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        m_RaycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        m_RaycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }
    void CalculateRaySpacing()
    {
        Bounds bounds = m_BoxCollider.bounds;
        bounds.Expand(skinWidth * -2);
        
        //We make that at least we have 2 raycasts horizontally and vertically
        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
        verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

        //The space will be the 'spaces' between each raycast
        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }

}
