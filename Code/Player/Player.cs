using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    #region public
    public void SetPlayerId(int id)
    {

    }
    public Entity ControllableEntity
    {
        get { return controllableEntity; }
        set
        {
            controllableEntity = value;
            controllableEntity.GetMovementModule().SetRequestInputs(Vector3.zero);
        }
    }
    public int GetPlayerId()
    {
        return playerId;
    }
    #endregion

    #region private
    private Entity controllableEntity;
    private int playerId = -1;

    private void Start ()
    {
		
	}
    private void Update ()
    {
        if (controllableEntity == null)
            return;

		if(Input.GetKeyDown(KeyCode.O))
        {
            controllableEntity.GetMovementModule().AddForce(Vector3.one, 3);
        }

        Vector3 inputs = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));     
        float vertical = Input.GetKeyDown(KeyCode.Space) ? 1 : Input.GetKeyDown(KeyCode.LeftControl) ? -1 : 0;
        Vector3 requestInput = new Vector3(0, vertical, 0);

        if (inputs.x != 0 || inputs.z != 0)
        {
            requestInput.z = 1;

            float angleToRotate = Vector3.Angle(controllableEntity.transform.forward, controllableEntity.transform.TransformVector(inputs));
            if (inputs.x < 0)
                angleToRotate = -angleToRotate;
            angleToRotate += PlayerCamera.instance.transform.eulerAngles.y;
            controllableEntity.GetMovementModule().SetRequestRotation(angleToRotate);
        }
        controllableEntity.GetMovementModule().SetRequestInputs(requestInput);
    }
    #endregion
}
