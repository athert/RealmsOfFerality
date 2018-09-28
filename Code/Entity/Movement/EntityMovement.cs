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
        fly,
        swim,
    }

    public struct MovementSnapshot
    {
        public int time;
        public int id;
        public Vector3 position;
        public float rotation;
        public Vector3 inputs;

        public MovementSnapshot(int time, int id, Vector3 position, float rotation, Vector3 inputs)
        {
            this.time = time;
            this.id = id;
            this.position = position;
            this.rotation = rotation;
            this.inputs = inputs;
        }
    }

    public EntityMovement(Entity baseEntity)
    {
        this.baseEntity = baseEntity;
        calculatedRotation = baseEntity.transform.eulerAngles;
    }

    public void OnUpdate()
    {
        if (Game.GetMap() != null && Game.GetMap().GetSuimonoModule() != null)
        {

            float waterHeight = Game.GetMap().GetSuimonoModule().SuimonoGetHeight(baseEntity.transform.position + new Vector3(0, baseEntity.GetInfoModule().waterSwimHeight, 0), "object depth");
            if (waterHeight - waterHeightOffset > 0 && CurrentMovementType != MovementType.swim)
            {
                OnSwimStarted();
            }
            else if (waterHeight + waterHeightOffset < 0 && CurrentMovementType == MovementType.swim)
            {
                OnSwimEnded();
            }
        }
    }

    public void CalculateMovement()
    {
        jumpRequested = false;
        fallRequested = false;

        if (IsInWalkableMovementType())
        {
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
                if (requestInputs.y > 0 && CanFly() && (inJump || fallTimer > 0.25f))
                {
                    SetFlying(true);
                }

                fallTimer += Time.deltaTime;
                if (!IsOnGround() && !isFalling && !inJump)
                {
                    isFalling = true;
                    fallRequested = true;
                }
            }
        }
        else
        {
            inJump = false;
            isFalling = false;
            fallTimer = 0;

            calculatedMovement = baseEntity.transform.TransformDirection(new Vector3(0, requestInputs.y, requestInputs.z)).normalized;
            calculatedMovement *= baseEntity.GetInfoModule().movementSpeed[(int)currentMovementType];

            if (IsFlying())
            {
                float angle = requestInputs.y > 0 ? -90 : PlayerCamera.instance.transform.eulerAngles.x;
                calculatedMovement *= CalculateFlySpeedMultiplierByPitch(angle);
            }

            if (IsFlying() && IsControllerOnGround())
            {
                SetFlying(false);
            }
        }

        #region allignWithGround
        RaycastHit hit;
        Vector3 pointNearGround = baseEntity.transform.position;
        Transform model = baseEntity.transform.Find("_model");

        if (IsControllerOnGround() && IsInWalkableMovementType() && Physics.Linecast(pointNearGround, pointNearGround + Vector3.down * 3, out hit))
        {
            Quaternion rotation = Quaternion.FromToRotation(model.transform.up, hit.normal) * model.rotation;
            rotation.eulerAngles = new Vector3(rotation.eulerAngles.x, 0, 0);
            model.localRotation = Quaternion.Lerp(model.localRotation, rotation, Time.deltaTime * 10);
        }
        else
        {
            model.localRotation = Quaternion.Lerp(model.localRotation, Quaternion.identity, Time.deltaTime * 10);
        }
        #endregion

        if ((forceImpulsIgnoreFrame || IsControllerOnGround()) && forceImpuls.magnitude >= 0.15f)
            calculatedMovement += forceImpuls;

        if(IsInWalkableMovementType())
            calculatedMovement.y -= dfn_gravity * Time.deltaTime;

        if (!IsInWalkableMovementType() && Game.GetPlayer().GetInputs().z > 0 && Game.GetPlayer().GetInputs().x == 0)
        {
            calculatedRotation.x = PlayerCamera.instance.transform.eulerAngles.x;
        }
        else
            calculatedRotation.x = 0;

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
    public void AddSnapshot(MovementSnapshot snapshot)
    {
        //todo: logic plz

        baseEntity.transform.position = snapshot.position;
        requestRotation = snapshot.rotation;
        //requestInputs = snapshot.inputs;
    }
    public void SetCurrentMovementType(MovementType type)
    {
        currentMovementType = type;
    }
    public void SetFlying(bool set)
    {
        if (set)
            OnFlyStarted();
        else
            OnFlyEnded();
    }
    public void SetFlyingPossibility(bool set)
    {
        canFly = set;

        if(!canFly && IsFlying())
        {
            SetFlying(false);
        }
    }
    public Vector3 GetMovementDirection()
    {
        return calculatedMovement;
    }
    public Vector3 GetRotation()
    {
        return calculatedRotation;
    }
    public Vector3 GetInputs()
    {
        return requestInputs;
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
    public bool IsInWalkableMovementType()
    {
        return CurrentMovementType != MovementType.swim && CurrentMovementType != MovementType.fly;
    }
    public bool CanFly()
    {
        return canFly;
    }
    public bool IsFlying()
    {
        return CurrentMovementType == MovementType.fly;
    }
    public MovementType CurrentMovementType
    {
        get { return currentMovementType; }
        set { currentMovementType = value; }
    }
    #endregion

    #region private
    private float dfn_gravity = 9.8f * 1.4f;
    private float dfn_fallTimerNotOnGround = 0.5f;
    private float waterHeightOffset = 0.15f;

    private Entity baseEntity;
    private Vector3 requestInputs;
    private float requestRotation;
    private Vector3 calculatedMovement;
    private Vector3 calculatedRotation;
    private Vector3 forceImpuls;
    private bool forceImpulsIgnoreFrame;
    private bool canFly;

    protected MovementType currentMovementType = MovementType.run;
    protected bool jumpRequested;
    protected bool fallRequested;
    protected bool inJump;
    protected bool isFalling;
    protected float fallTimer;

    private void OnSwimStarted()
    {
        currentMovementType = MovementType.swim;
    }
    private void OnSwimEnded()
    {
        currentMovementType = MovementType.run;
    }
    private void OnFlyStarted()
    {
        currentMovementType = MovementType.fly;
    }
    private void OnFlyEnded()
    {
        currentMovementType = MovementType.run;
    }
    private float CalculateFlySpeedMultiplierByPitch(float pitch)
    {
        float upMult = 0.5f;
        float downMult = 2;

        pitch = (pitch > 180) ? pitch - 360 : pitch;

        float value = Mathf.InverseLerp(0, 90, Mathf.Abs(pitch));

        if (pitch <= 0)
            return Mathf.Lerp(1, upMult, value);
        else
            return Mathf.Lerp(1, downMult, value);
    }
    #endregion
}
