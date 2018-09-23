﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    #region public
    public void SetId(int id)
    {
        this.id = id;
    }
    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }
    public void SetOrientation(float orientation)
    {
        GetMovementModule().SetRequestRotation(orientation);
        transform.eulerAngles = new Vector3(0, orientation, 0);
    }
    public void SetPostStartAction(Action postStartAction)
    {
        this.postStartAction = postStartAction;
    }
    public void DisableMovement(bool disable)
    {
        disableMovement = disable;
    }

    public EntityAnimation GetAnimationModule()
    {
        return animationModule;
    }
    public EntityInventory GetInventoryModule()
    {
        return inventoryModule;
    }
    public EntityMovement GetMovementModule()
    {
        return movementModule;
    }
    public EntityVisual GetVisualModule()
    {
        return visualModule;
    }
    public EntityInfo GetInfoModule()
    {
        return infoModule;
    }
    public CharacterController GetCharacterController()
    {
        return characterController;
    }
    public int GetId()
    {
        return id;
    }
    #endregion


    #region protected
    protected virtual void Awake()
    {
        
    }
    protected virtual void Start ()
    {
        infoModule = GetComponent<EntityInfo>();

        animationModule = new EntityAnimation(this);
        inventoryModule = new EntityInventory(this);
        movementModule = new EntityMovement(this);
        visualModule = new EntityVisual(this);

        characterController = GetComponent<CharacterController>();

        Game.OnCreatedEntity(this);

        if (postStartAction != null)
            postStartAction();
    }
	protected virtual void Update ()
    {
        if (!disableMovement)
        {
            GetMovementModule().CalculateMovement();
            transform.eulerAngles = movementModule.GetRotation();
            characterController.Move(movementModule.GetMovementDirection() * Time.deltaTime);
        }
    }
    protected virtual void LateUpdate()
    {

    }
    protected virtual void FixedUpdate()
    {
        
    }
    #endregion

    #region private
    private EntityAnimation animationModule;
    private EntityInventory inventoryModule;
    private EntityMovement movementModule;
    private EntityVisual visualModule;
    private EntityInfo infoModule;
    private CharacterController characterController;
    private int id;
    private Action postStartAction = null;
    private bool disableMovement;
    #endregion
}
