using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public static PlayerCamera instance;
    public float maxDistance = 10f;        //maximální vzdálenost kamery od hráče
    public float minDistance = 5f;         //minimální vzdálenost kamery od hráče
    public LayerMask raycastMask;


    private Vector2 sensitivity = new Vector2(200f, 200f);
    private Vector2 rotationLimit = new Vector2(-80f, 80f);
    private Vector2 actualCameraRotation = new Vector2(0, 30f);

    private float targetHeight = 1.3f;      //výška NPC
    private float initDistance = 5.0f;      //počáteční vzdálenost kamery od hráče
    
    private float zoomRate = 40f;           //rychlost přiblížení
    private float currentDistance;
    private float desiredDistance = 8;
    private float correctedDistance;
    private Vector3 vTargetOffset;
    private List<string> inputFreeze = new List<string>();

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        vTargetOffset = new Vector3(0, targetHeight, 0);
    }

    void LateUpdate ()
    {
        if (Game.GetPlayer() == null)
            return;

        Entity controllableEntity = Game.GetPlayer().ControllableEntity;
        if (controllableEntity == null)
            return;

        if (Input.GetMouseButton(1))
        {
            actualCameraRotation.x += Input.GetAxis("Mouse X") * sensitivity.x * 0.02f;
            actualCameraRotation.y -= Input.GetAxis("Mouse Y") * sensitivity.y * 0.02f;
            desiredDistance -= Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * zoomRate * Mathf.Abs(desiredDistance);
        }

        actualCameraRotation.y = MathX.ClampAngle(actualCameraRotation.y, rotationLimit.x, rotationLimit.y);
        desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
        correctedDistance = desiredDistance;
        Quaternion rotation = Quaternion.Euler(actualCameraRotation.y, actualCameraRotation.x, 0);
        RaycastHit hit;

        Vector3 position = controllableEntity.transform.position - (rotation * Vector3.forward * desiredDistance - vTargetOffset);
        Vector3 targetPos = controllableEntity.transform.position + vTargetOffset;

        if (Physics.Linecast(targetPos, position, out hit, raycastMask))
        {
            correctedDistance = Vector3.Distance(targetPos, hit.point);
        }

        position = controllableEntity.transform.position - (rotation * Vector3.forward * correctedDistance - vTargetOffset);

        transform.position = Vector3.Lerp(transform.position, position, 0.5f);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, 0.5f);
    }
}
