using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Pathfinding))]
public class GameController : MonoBehaviour
{
    public static GameController instance { get; private set; } //There only one GameController
    public static Pathfinding pathfinding { get; private set; } //Is Required a Pathfinding to the game Work
    public Grid grid; //Set the grid

    void Awake()
    {
        //Set instance
        instance = this;
        //Get pathfinding
        pathfinding = GetComponent<Pathfinding>();
    }

    void Update()
    {
        
    }
}
