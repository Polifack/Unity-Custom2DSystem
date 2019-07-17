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

    //Define the width of the bounds where we raycast
    const float skinWidth = .015f;

    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;
    public bool debug = false;

    float horizontalRaySpacing;
    float verticalRaySpacing;

    BoxCollider2D m_BoxCollider;
    RaycastOrigins m_RaycastOrigins;

    void Start()
    {
        m_BoxCollider = GetComponent<BoxCollider2D>();
    }

    void GetRaycastOrigins()
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

    private void Update()
    {
        GetRaycastOrigins();
        CalculateRaySpacing();

        if (debug) ShowDebug();
    }

    void ShowDebug()
    {
        for (int i = 0; i < verticalRayCount; i++)
        {
            Debug.DrawRay(m_RaycastOrigins.bottomLeft + Vector2.right * verticalRaySpacing * i, Vector2.up * -2, Color.red);
        }

        for (int i = 0; i < horizontalRayCount; i++)
        {
            Debug.DrawRay(m_RaycastOrigins.bottomRight + Vector2.up * horizontalRaySpacing * i, Vector2.right * 2, Color.red);
        }
    }


}
