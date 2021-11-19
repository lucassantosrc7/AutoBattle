using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class CreatePlayer : MonoBehaviour
{
    Rigidbody rb;

    GameController gameController
    {
        get
        {
            return GameController.instance;
        }
    }

    public GridSlot gridSlot;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    void OnMouseDown()
    {
        if (gridSlot.character.Count > 0) return;
        gameController.AddPlayer(gridSlot);
    }
}
