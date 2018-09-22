using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityMovement
{
    #region public
    public enum MovementType
    {
        walk,
        run,
    }

    public EntityMovement(Entity baseEntity)
    {
        this.baseEntity = baseEntity;
        calculatedRotation = baseEntity.transform.eulerAngles;
    }
    public void CalculateMovement()
    {
        jumpRequested = false;
        fallRequested = false;

        if (IsControllerOnGround())
        {
            inJump = false;
            isFalling = false;
            fallTimer = 0;

            calculatedMovement = baseEntity.transform.TransformDirection(new Vector3(0, 0, requestInputs.z)).normalized;
            calculatedMovement *= baseEntity.GetInfoModule().movementSpeed[(int)currentMovementType];

            if (requestInputs.y > 0)
            {
                calculatedMovement.y = 5;
                jumpRequested = true;
                inJump = true;
                fallTimer = dfn_fallTimerNotOnGround;
            }
        }
        else
        {
            fallTimer += Time.deltaTime;
            if (!IsOnGround() && !isFalling && !inJump)
            {
                isFalling = true;
                fallRequested = true;
            }
        }

        //align by ground 
        RaycastHit hit;
        Vector3 pointNearGround = baseEntity.transform.position;
        if (IsControllerOnGround() && Physics.Linecast(pointNearGround, pointNearGround + Vector3.down * 3, out hit))
        {
            Transform model = baseEntity.transform.Find("_model");
            Quaternion rotation = Quaternion.FromToRotation(model.transform.up, hit.normal) * model.rotation;
            rotation.eulerAngles = new Vector3(rotation.eulerAngles.x, 0, 0);
            model.localRotation = Quaternion.Lerp(model.localRotation, rotation, Time.deltaTime * 10);
        }

        if ((forceImpulsIgnoreFrame || IsControllerOnGround()) && forceImpuls.magnitude >= 0.15f)
            calculatedMovement += forceImpuls;

        calculatedMovement.y -= dfn_gravity * Time.deltaTime;
        calculatedRotation.y = requestRotation;

        forceImpuls = Vector3.Lerp(forceImpuls, Vector3.zero, 5 * Time.deltaTime);
        forceImpulsIgnoreFrame = false;
    }
    public void AddForce(Vector3 dir, float power)
    {
        if (dir.y < 0)
            dir.y = -dir.y;

        dir *= power;

        forceImpuls += dir;
        forceImpulsIgnoreFrame = true;
    }
    public void SetRequestInputs(Vector3 requestInputs)
    {
        this.requestInputs = requestInputs;
    }
    public void SetRequestRotation(float requestRotation)
    {
        this.requestRotation = requestRotation;
    }
    public Vector3 GetMovementDirection()
    {
        return calculatedMovement;
    }
    public Vector3 GetRotation()
    {
        return calculatedRotation;
    }
    public bool IsMoving()
    {
        if (calculatedMovement.x != 0)
            return true;

        if (calculatedMovement.z != 0)
            return true;

        return false;
    }
    public bool IsJumpRequested()
    {
        return jumpRequested;
    }
    public bool IsFallRequested()
    {
        return fallRequested;
    }
    public bool IsOnGround()
    {
        return fallTimer < dfn_fallTimerNotOnGround;
    }
    public bool IsControllerOnGround()
    {
        return baseEntity.GetCharacterController().isGrounded;
    }
    public MovementType CurrentMovementType
    {
        get { return currentMovementType; }
        set { currentMovementType = value; }
    }
    #endregion

    #region private
    protected float dfn_gravity = 9.8f * 1.4f;
    protected float dfn_fallTimerNotOnGround = 0.5f;

    private Entity baseEntity;
    private Vector3 requestInputs;
    private float requestRotation;
    private Vector3 calculatedMovement;
    private Vector3 calculatedRotation;
    private Vector3 forceImpuls;
    private bool forceImpulsIgnoreFrame;

    protected MovementType currentMovementType = MovementType.run;
    protected bool jumpRequested;
    protected bool fallRequested;
    protected bool inJump;
    protected bool isFalling;
    protected float fallTimer;
    #endregion
}
