using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class CreatePlayer : MonoBehaviour
{
    Rigidbody rb;//Get Rigibody to identify MouseClick

    //Receive GameController
    GameController gameController
    {
        get
        {
            return GameController.instance;
        }
    }

    //Receive the gridSlot
    public GridSlot gridSlot;

    void Awake()
    {
        //Get rigibody
        rb = GetComponent<Rigidbody>();
        //Set kinematic to it not move
        rb.isKinematic = true;
    }

    //Event to get mouse Click
    void OnMouseDown()
    {
        //Verify if alredy have Character
        if (gridSlot.character.Count > 0) return;
        //If not add
        gameController.AddPlayer(gridSlot);
    }
}
