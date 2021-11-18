using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    //Get the instance of GameController
    GameController gameController
    {
        get
        {
            return GameController.instance;
        }
    }

    //Get the grid setting in Game Controller
    Grid grid
    {
        get
        {
            return gameController.grid;
        }
    }

    //Get the pathfinding setting in Game Controller
    Pathfinding pathfinding
    {
        get
        {
            return GameController.pathfinding;
        }
    }

    [Header("Properties")]
    string characterName; //Name of this Character

    [Range(0,100)]
    [SerializeField]
    [Tooltip("100% health may be from the strongest character in the game, so it's easy to balance this way.")]
    int health, baseDamage; //Health and baseDamage by percentage
    [SerializeField]
    int damageMultiplier; //Multiply the BaseDamage percentage
    [HideInInspector]
    public int currentBox, index; //Where this player stay in the grid world
    public Transform target; //Who him will attack

    void Start()
    {
        GridSlot gridSlot = grid.GetGridSlot(transform.position);
        transform.position = gridSlot.position;

        pathfinding.FindPath(transform.position, target.position);
    }

    void Update()
    {
        
    }
}
