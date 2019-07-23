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
        public bool climbSlope, descendSlope;
        public float slopeAngle, slopeAngleOld;

        public void Reset()
        {
            above = below = false;
            left = right = false;
            climbSlope = false;
            descendSlope = false;

            slopeAngleOld = slopeAngle;
            slopeAngle = 0;
        }
    }

    //Define the width of the bounds where we raycast
    const float skinWidth = .015f;

    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;
    public bool debug = false;
    public float maxClimbAngle = 80;
    public float maxDescendAngle = 75;
    public LayerMask collisionMask;

    float horizontalRaySpacing;
    float verticalRaySpacing;

    BoxCollider2D m_BoxCollider;
    RaycastOrigins m_RaycastOrigins;
    CollisionInfo m_Collisions;
    Vector3 m_OldVelocity;

    public CollisionInfo getCollisions()
    {
        return m_Collisions;
    }

    void Start()
    {
        m_BoxCollider = GetComponent<BoxCollider2D>();
        CalculateRaySpacing();
    }
    public void Move(Vector3 velocity)
    {
        UpdateRaycastOrigin();
        m_Collisions.Reset();
        m_OldVelocity = velocity;

        if (velocity.y < 0)
        {
            DescendSlope(ref velocity);
        }

        if (velocity.x!=0)
            HorizontalCollisions(ref velocity);

        if (velocity.y != 0)
            VerticalCollisions(ref velocity);

        transform.Translate(velocity);
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
                velocity.y = (hit.distance - skinWidth) * directionY;
                //Cambiamos la rayLegth en cuanto chocamos con algo para que no choquemos con algo más "lejos"
                rayLength = hit.distance;

                //Si estamos en una pendiente y chocamos contra un obstaculo desde abajo estamos cambiando solo la vel y
                //Es necesario cambiar la vel x para no glichearse
                if (m_Collisions.climbSlope)
                {
                    velocity.x = velocity.y / Mathf.Tan(m_Collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
                }

                //Actualizamos la matriz de colisiones con los bool de direccion
                m_Collisions.below = directionY == -1;
                m_Collisions.above = directionY == 1;
            }
        }
        
        //Comprobar si despues de una pendiente encontramos otra
        if (m_Collisions.climbSlope)
        {
            //Hacemos un raycast en horizontal
            float directionX = Mathf.Sign(velocity.x);
            rayLength = Mathf.Abs(velocity.x) + skinWidth;
            Vector2 rayOrigin = ((directionX == -1) ? m_RaycastOrigins.bottomLeft : m_RaycastOrigins.bottomRight) +
                Vector2.up * velocity.y;

            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            //Si nos encontramos con una pendiente...
            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

                //Y si la pendiente es distinta a la que ya estamos...
                if (slopeAngle != m_Collisions.slopeAngle)
                {
                    velocity.x = (hit.distance - skinWidth) * directionX;
                    m_Collisions.slopeAngle = slopeAngle;
                }
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
                //Para obtener el angulo usamos el vector normal
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

                if (i == 0 && slopeAngle <= maxClimbAngle)
                {
                    //Si empezamos a subir una pendiente anulamos la parte en la que la estamos bajando
                    if (m_Collisions.descendSlope)
                    {
                        m_Collisions.descendSlope = false;
                        velocity = m_OldVelocity;
                    }

                    float distanceToSlopeStart = 0;
                    
                    //Si estamos en una nueva pendiente
                    if (slopeAngle != m_Collisions.slopeAngleOld)
                    {
                        //Con esto evitamos que el objeto acelere demasiado y se levante de la pendiente
                        distanceToSlopeStart = hit.distance - skinWidth;
                        velocity.x -= distanceToSlopeStart * directionX;
                    }
                    ClimbSlope(ref velocity, slopeAngle);
                    velocity.x += distanceToSlopeStart * directionX;
                }

                //Si no estamos en una pendiente o el angulo de la pendiente es mayor de lo 'permitido'
                if (!m_Collisions.climbSlope || slopeAngle > maxClimbAngle)
                {
                    velocity.x = (hit.distance - skinWidth) * directionX;
                    
                    //Cambiamos la rayLegth en cuanto chocamos con algo para que no choquemos con algo más "lejos"
                    rayLength = hit.distance;

                    //Si estamos en una pendiente y chocamos contra un obstaculo estamos cambiando solo la vel x
                    //Es necesario cambiar la vel y tambien
                    if (m_Collisions.climbSlope)
                    {
                        //Si chocamos contra algun obstaculo en una pendiente anulamos tambien la velocidad y
                        velocity.y = Mathf.Tan(m_Collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
                    }

                    //Actualizamos la matriz de colisiones con los bool de direccion
                    m_Collisions.left = directionX == -1;
                    m_Collisions.right = directionX == 1;
                }
            }
        }
    }
    void ClimbSlope(ref Vector3 velocity, float slopeAngle)
    {
        //Para mover el personaje sobre la pendiente tenemos que incrementar la velocidad en el eje Y así como la X
        float moveDistance = Mathf.Abs(velocity.x);

        float climbVelY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

        //Si no estamos saltando en la pendiente
        if (velocity.y <= climbVelY)
        {
            velocity.y = climbVelY;
            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);

            //Como estamos cambiando la velocidad y debemos reafirmar que estamos en el suelo.
            m_Collisions.below = true;
            m_Collisions.climbSlope = true;
            m_Collisions.slopeAngle = slopeAngle;
        }
    }
    void DescendSlope(ref Vector3 velocity)
    {
        float directionX = Mathf.Sign(velocity.x);

        //Decidimos desde donde estamos raycasteando la pendiente
        Vector2 rayOrigin = (directionX == -1) ? m_RaycastOrigins.bottomRight : m_RaycastOrigins.bottomLeft;

        //Raycasteamos hasta el infinito porque no conocemos la distancia a la que bajamos
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, Mathf.Infinity, collisionMask);

        if (hit)
        {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

            //Si el suelo no es plano y la pendiente tiene un angulo valido
            if (slopeAngle != 0 && slopeAngle <= maxDescendAngle)
            {
                //Si estamos descendiendo la pendiente
                if (Mathf.Sign(hit.normal.x) == directionX)
                {
                    //Si la distancia respecto a la pendiente es menor que lo que nos tenemos que mover 
                    //estamos lo suficiente cerca de la pendiente como para movernos a traves de ella
                    if (hit.distance-skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x))
                    {
                        float moveDistance = Mathf.Abs(velocity.x);

                        //Igual que para subir pendientes descomponemos la velocidad
                        float descVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                        velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
                        velocity.y -= descVelocityY;

                        //Actualizamos las colisiones
                        m_Collisions.slopeAngle = slopeAngle;
                        m_Collisions.descendSlope = true;
                        m_Collisions.below = true;
                    }
                }
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
