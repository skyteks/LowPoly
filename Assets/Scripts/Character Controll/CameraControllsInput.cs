using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControllsInput : MonoBehaviour
{
    public struct ClipPlanePoints
    {
        public Vector3 UpperLeft;
        public Vector3 UpperRight;
        public Vector3 LowerLeft;
        public Vector3 LowerRight;
    }

    public Transform target;
    [Tooltip("Lerp speed between Camera States")]
    public float smoothCameraRotation = 12f;
    [Tooltip("What layer will be culled")]
    public LayerMask cullingLayer = 1 << 0;
    [Tooltip("Debug purposes, lock the camera behind the character for better align the states")]
    public bool lockCamera;

    public float rightOffset = 0f;
    public float defaultDistance = 2.5f;
    public float height = 1.4f;
    public float smoothFollow = 10f;
    public float xMouseSensitivity = 3f;
    public float yMouseSensitivity = 3f;
    public float yMinLimit = -40f;
    public float yMaxLimit = 80f;

    [HideInInspector]
    public int indexList, indexLookPoint;
    [HideInInspector]
    public float offSetPlayerPivot;
    [HideInInspector]
    public string currentStateName;
    [HideInInspector]
    public Transform currentTarget;
    [HideInInspector]
    public Vector2 movementSpeed;

    private Transform targetLookAt;
    private Vector3 currentTargetPos;
    private Vector3 current_cPos;
    private Vector3 desired_cPos;
    private Camera cam;
    private float distance = 5f;
    private float mouseY = 0f;
    private float mouseX = 0f;
    private float currentHeight;
    private float cullingDistance;
    private float checkHeightRadius = 0.4f;
    private float clipPlaneMargin = 0f;
    private float forward = -1f;
    private float xMinLimit = -360f;
    private float xMaxLimit = 360f;
    private float cullingHeight = 0.2f;
    private float cullingMinDist = 0.1f;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void Start()
    {
        Init();
    }

    public void Init()
    {
        if (target == null)
        {
            return;
        }

        currentTarget = target;
        currentTargetPos = new Vector3(currentTarget.position.x, currentTarget.position.y + offSetPlayerPivot, currentTarget.position.z);

        targetLookAt = new GameObject("targetLookAt").transform;
        targetLookAt.position = currentTarget.position;
        targetLookAt.hideFlags = HideFlags.HideInHierarchy;
        targetLookAt.rotation = currentTarget.rotation;

        mouseY = currentTarget.eulerAngles.x;
        mouseX = currentTarget.eulerAngles.y;

        distance = defaultDistance;
        currentHeight = height;
    }

    void FixedUpdate()
    {
        if (target == null || targetLookAt == null)
        {
            return;
        }
        CameraMovement();
    }

    /// <summary>
    /// Set the target for the camera
    /// </summary>
    /// <param name="New cursorObject"></param>
    public void SetTarget(Transform newTarget)
    {
        currentTarget = newTarget ? newTarget : target;
    }

    public void SetMainTarget(Transform newTarget)
    {
        target = newTarget;
        currentTarget = newTarget;
        mouseY = currentTarget.rotation.eulerAngles.x;
        mouseX = currentTarget.rotation.eulerAngles.y;
        Init();
    }

    /// <summary>    
    /// Convert a point in the screen in a Ray for the world
    /// </summary>
    /// <param name="Point"></param>
    /// <returns></returns>
    public Ray ScreenPointToRay(Vector3 Point)
    {
        return cam.ScreenPointToRay(Point);
    }

    /// <summary>
    /// Camera Rotation behaviour
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void RotateCamera(float x, float y)
    {
        // free rotation 
        mouseX += x * xMouseSensitivity;
        mouseY -= y * yMouseSensitivity;

        movementSpeed.x = x;
        movementSpeed.y = -y;
        if (!lockCamera)
        {
            mouseY = Mathf.Clamp(mouseY % 360f, yMinLimit, yMaxLimit);
            mouseX = Mathf.Clamp(mouseX % 360f, xMinLimit, xMaxLimit);
        }
        else
        {
            mouseY = currentTarget.root.localEulerAngles.x;
            mouseX = currentTarget.root.localEulerAngles.y;
        }
    }

    /// <summary>
    /// Camera behaviour
    /// </summary>    
    private void CameraMovement()
    {
        if (currentTarget == null)
        {
            return;
        }

        distance = Mathf.Lerp(distance, defaultDistance, smoothFollow * Time.deltaTime);
        cullingDistance = Mathf.Lerp(cullingDistance, distance, Time.deltaTime);
        Vector3 camDir = (forward * targetLookAt.forward) + (rightOffset * targetLookAt.right);

        camDir.Normalize();

        Vector3 targetPos = new Vector3(currentTarget.position.x, currentTarget.position.y + offSetPlayerPivot, currentTarget.position.z);
        currentTargetPos = targetPos;
        desired_cPos = targetPos + new Vector3(0f, height, 0f);
        current_cPos = currentTargetPos + new Vector3(0f, currentHeight, 0f);
        RaycastHit hitInfo;

        ClipPlanePoints planePoints = NearClipPlanePoints(cam, current_cPos + (camDir * (distance)), clipPlaneMargin);
        ClipPlanePoints oldPoints = NearClipPlanePoints(cam, desired_cPos + (camDir * distance), clipPlaneMargin);

        //Check if Height is not blocked 
        if (Physics.SphereCast(targetPos, checkHeightRadius, Vector3.up, out hitInfo, cullingHeight + 0.2f, cullingLayer))
        {
            float t = hitInfo.distance - 0.2f;
            t -= height;
            t /= cullingHeight - height;
            cullingHeight = Mathf.Lerp(height, cullingHeight, Mathf.Clamp(t, 0f, 1f));
        }

        //Check if desired target position is not blocked       
        if (CullingRayCast(desired_cPos, oldPoints, out hitInfo, distance + 0.2f, cullingLayer, Color.blue))
        {
            distance = hitInfo.distance - 0.2f;
            if (distance < defaultDistance)
            {
                float t = hitInfo.distance;
                t -= cullingMinDist;
                t /= cullingMinDist;
                currentHeight = Mathf.Lerp(cullingHeight, height, Mathf.Clamp(t, 0f, 1f));
                current_cPos = currentTargetPos + new Vector3(0f, currentHeight, 0f);
            }
        }
        else
        {
            currentHeight = height;
        }
        //Check if target position with culling height applied is not blocked
        if (CullingRayCast(current_cPos, planePoints, out hitInfo, distance, cullingLayer, Color.cyan)) distance = Mathf.Clamp(cullingDistance, 0f, defaultDistance);
        Vector3 lookPoint = current_cPos + targetLookAt.forward * 2f;
        lookPoint += (targetLookAt.right * Vector3.Dot(camDir * distance, targetLookAt.right));
        targetLookAt.position = current_cPos;

        Quaternion newRot = Quaternion.Euler(mouseY, mouseX, 0f);
        targetLookAt.rotation = Quaternion.Slerp(targetLookAt.rotation, newRot, smoothCameraRotation * Time.deltaTime);
        transform.position = current_cPos + (camDir * distance);
        Quaternion rotation = Quaternion.LookRotation(lookPoint - transform.position);

        transform.rotation = rotation;
        movementSpeed = Vector2.zero;
    }

    /// <summary>
    /// Custom Raycast using NearClipPlanesPoints
    /// </summary>
    /// <param name="to"></param>
    /// <param name="from"></param>
    /// <param name="hit"></param>
    /// <param name="distance"></param>
    /// <param name="cullingLayer"></param>
    /// <returns></returns>
    private bool CullingRayCast(Vector3 from, ClipPlanePoints to, out RaycastHit hit, float distance, LayerMask cullingLayer, Color color)
    {
        bool value = false;

        if (Physics.Raycast(from, to.LowerLeft - from, out hit, distance, cullingLayer))
        {
            value = true;
            cullingDistance = hit.distance;
        }

        if (Physics.Raycast(from, to.LowerRight - from, out hit, distance, cullingLayer))
        {
            value = true;
            if (cullingDistance > hit.distance)
            {
                cullingDistance = hit.distance;
            }
        }

        if (Physics.Raycast(from, to.UpperLeft - from, out hit, distance, cullingLayer))
        {
            value = true;
            if (cullingDistance > hit.distance)
            {
                cullingDistance = hit.distance;
            }
        }

        if (Physics.Raycast(from, to.UpperRight - from, out hit, distance, cullingLayer))
        {
            value = true;
            if (cullingDistance > hit.distance)
            {
                cullingDistance = hit.distance;
            }
        }

        return hit.collider && value;
    }

    public static ClipPlanePoints NearClipPlanePoints(Camera camera, Vector3 pos, float clipPlaneMargin)
    {
        ClipPlanePoints points = new ClipPlanePoints();

        Transform transform = camera.transform;
        float halfFOV = camera.fieldOfView * 0.5f * Mathf.Deg2Rad;
        float aspect = camera.aspect;
        float distance = camera.nearClipPlane;
        float height = distance * Mathf.Tan(halfFOV);
        float width = height * aspect;
        height *= 1f + clipPlaneMargin;
        width *= 1f + clipPlaneMargin;
        points.LowerRight = pos + transform.right * width;
        points.LowerRight -= transform.up * height;
        points.LowerRight += transform.forward * distance;

        points.LowerLeft = pos - transform.right * width;
        points.LowerLeft -= transform.up * height;
        points.LowerLeft += transform.forward * distance;

        points.UpperRight = pos + transform.right * width;
        points.UpperRight += transform.up * height;
        points.UpperRight += transform.forward * distance;

        points.UpperLeft = pos - transform.right * width;
        points.UpperLeft += transform.up * height;
        points.UpperLeft += transform.forward * distance;

        return points;
    }
}
