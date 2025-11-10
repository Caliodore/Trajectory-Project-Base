using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaliLaser : MonoBehaviour
{
    [SerializeField] LayerMask targetLayer;
    [SerializeField] GameObject crosshair;
    [SerializeField] float baseTurnSpeed = 3;
    [SerializeField] GameObject gun;
    [SerializeField] Transform turretBase;
    [SerializeField] Transform barrelEnd;
    [SerializeField] LineRenderer line;

    public List<Vector3> laserPoints = new List<Vector3>();
    
    [SerializeField] GameObject hitMarker;

    private Vector3 crosshairNormal;
    public bool testingMarker = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        TrackMouse();
        TurnBase();

        laserPoints.Clear();
        laserPoints.Add(barrelEnd.position);

        if(Physics.Raycast(barrelEnd.position, barrelEnd.forward, out RaycastHit hit, 1000.0f, targetLayer))
        {
            laserPoints.Add(hit.point);
            CalculateReflection(barrelEnd.position, hit.point, hit.normal);
        }

        line.positionCount = laserPoints.Count;
        for(int i = 0; i < line.positionCount; i++)
        {
            line.SetPosition(i, laserPoints[i]);
        }
        
        if(testingMarker)
        {
            foreach(Vector3 hitOut in laserPoints)
            { 
                GameObject raycastHitMarker = Instantiate(hitMarker, hitOut, Quaternion.identity);
                Destroy(raycastHitMarker, 0.2f);
            } 
        }
    }

    void TrackMouse()
    {
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if(Physics.Raycast(cameraRay, out hit, 1000, targetLayer ))
        {
            crosshairNormal = hit.normal;
            crosshair.transform.forward = crosshairNormal;
            crosshair.transform.position = hit.point + hit.normal * 0.1f;
        }
    }

    void TurnBase()
    {
        Vector3 directionToTarget = (crosshair.transform.position - turretBase.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToTarget.x, directionToTarget.y, directionToTarget.z));
        turretBase.transform.rotation = Quaternion.Slerp(turretBase.transform.rotation, lookRotation, Time.deltaTime * baseTurnSpeed);
    }

    private void CalculateReflection(Vector3 previousOrigin, Vector3 hitPoint, Vector3 hitSurfaceNormal)
    { 
        Vector3 directionalVec = (hitPoint - previousOrigin).normalized;
        Vector3 reflectedVec = directionalVec - (2 * Vector3.Dot(directionalVec, hitSurfaceNormal) * hitSurfaceNormal);
        //Vector3 dotVec = Vector3.Dot(directionalVec, hitSurfaceNormal) * hitSurfaceNormal;

        Debug.DrawRay(hitPoint, directionalVec, Color.green, 0.25f);
        Debug.DrawRay(hitPoint, reflectedVec, Color.cyan, 0.25f);
        //Debug.DrawRay(hitPoint, dotVec, Color.yellow, 0.25f);

        if(Physics.Raycast(hitPoint, reflectedVec, out RaycastHit hitOut, 1000.0f, targetLayer))
        {
            laserPoints.Add(hitOut.point);
            CalculateReflection(hitPoint, hitOut.point, hitOut.normal);
        }
        else
        { 
            Vector3 infiniteBounce = reflectedVec * 200;
            laserPoints.Add(infiniteBounce);
        }
    }
}
