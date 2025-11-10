//using NUnit.Framework;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProjectileLine : MonoBehaviour
{
    [SerializeField] Transform instantiationPoint;
    [SerializeField] ProjectileTurret attachedTurretScript;

    LayerMask collisionLayer;

    public List<Vector3> linePoints = new List<Vector3>();
    public LineRenderer lineRenderer;

    public Vector3 startPos;
    public Vector3 shotDir;
    public Vector3 projVelocity;
    private Vector3 initialProjVel;

    public float projectileSpeed;
    public float projectileTrailLifetime = 10f;
    public float relativeYMaxHeight;

    private float initialYVel;

    private void Start()
    {
        //attachedTurretScript = gameObject.GetComponent<ProjectileTurret>();
        collisionLayer = LayerMask.GetMask("Ground");
        startPos = instantiationPoint.transform.position;

        //Cache intended initial projectile velocity
        initialProjVel = instantiationPoint.forward * projectileSpeed;  //The whole vector for both lateral and horizontal velocity.
        initialYVel = initialProjVel.y;
    }

    private void Update()
    {
        linePoints.Clear();
        linePoints.Add(startPos);
        GeneratePredictedPath();
        DrawTrajectory();
    }

    public void GeneratePredictedPath()
    {
        shotDir = instantiationPoint.forward;

        //Cache intended initial projectile velocity
        initialProjVel = shotDir * projectileSpeed;  //The whole vector for both lateral and horizontal velocity.
        initialYVel = initialProjVel.y;

        Physics.Raycast(startPos, shotDir, out RaycastHit hitOut, 500f, collisionLayer);

        //Calculate lateral distance to collision
        Vector3 collisionLoc = hitOut.point;                            //Gives world position of the hit point.
        collisionLoc = new Vector3(collisionLoc.x, 0, collisionLoc.z);  //Distance along the lateral axis to the collision spot
        Vector3 startLateral = new Vector3(shotDir.x, 0, shotDir.z);    //Using the forward to generate a relative axis of motion
        Vector3 lateralDistVec = collisionLoc - startLateral;           //Subtract destination on the corresponding axis by the axis start.
        Vector3 lateralAxis = lateralDistVec.normalized;                //Normalize for a better reference

        //Some key points for reference
        float yMax = ((Mathf.Pow(initialYVel,2))/(19.62f) + startPos.y);    //The highest the projectile will go.
        float deltaTForYMax = Mathf.Abs(initialYVel/Physics.gravity.y);     //Time it takes to reach the highest point.
        float deltaTForArc = deltaTForYMax*2;                               //Time it takes to reach the starting height.

        GenerateLinePoints();
    }

    /*
     * Yf = Yi + (Vi * deltaT) + (0.5 * Ay * (deltaT ^ 2))
     * Vf^2 = Vi^2 + 2 * Ay * (Yf - Yi)
     * 
     * Ymax = ((Vi ^ 2)/(2 * Ay)) + Yi
     * 
     * Ay = gravity
     * For the peak, Vf = 0
     * Yi = startPos.y
     * 
     * Ymax = ((Vi^2)/(19.62)) + startPos.y
     * 
     */

    private void GenerateLinePoints()
    {
        //Determine the velocity vector at each step based on time
        for(float elapsedVirtualTime = 0f; elapsedVirtualTime < projectileTrailLifetime; elapsedVirtualTime += Time.deltaTime)
        { 
            float yVelOutput = initialYVel + (Physics.gravity.y * elapsedVirtualTime);                 //Vf = Vi + a*t
            Vector3 predictedVelVec = new Vector3(initialProjVel.x, yVelOutput, initialProjVel.z);     //Since accel is only on y we can take the inital x and z vel.
            
            Vector3 lastPointStored = linePoints[linePoints.Count-1];

            if(CheckForCollision(lastPointStored, predictedVelVec))
            {
                return;    
            }

            //To determine the position along the arc, use Yf = Yi + (Vi * deltaT) + (0.5 * Ay * (deltaT ^ 2))
            float yPosOutput = startPos.y + (initialYVel * elapsedVirtualTime) + (0.5f * Physics.gravity.y * Mathf.Pow(elapsedVirtualTime,2));

            //Since x and z have no acceleration, the equation is just Xf = Xi + (Vi * deltaT)
            float xPosOutput = startPos.x + (initialProjVel.x * elapsedVirtualTime);
            float zPosOutput = startPos.z + (initialProjVel.z * elapsedVirtualTime);

            Vector3 predictedPosVec = new Vector3(xPosOutput, yPosOutput, zPosOutput);
            linePoints.Add(predictedPosVec);
        }
    }

    private bool CheckForCollision(Vector3 rayStart, Vector3 rayDir)
    { 
        if(Physics.Raycast(rayStart, rayDir, out RaycastHit hitCheck, 1f, collisionLayer))
        { 
            linePoints.Add(hitCheck.point);
            return true;    
        }
        else
            return false;
    }

    private void DrawTrajectory()
    { 
        lineRenderer.positionCount = linePoints.Count;

        for(int i = 0; i < lineRenderer.positionCount; i++)
        {
            lineRenderer.SetPosition(i, linePoints[i]);
        }    
    }
}
