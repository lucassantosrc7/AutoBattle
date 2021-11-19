using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SpriteRenderer))]
public class Character : MonoBehaviour
{
    //Receive character sprite
    public SpriteRenderer sprite { get; private set; }
    //To save the color
    Color spriteColor;

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

    //Get the camera setting in Game Controller
    Camera cam
    {
        get
        {
            return gameController.cam;
        }
    }

    //Save which team is this character
    [HideInInspector]
    public GameController.Teams team;

    [Header("Properties")]
    string characterName; //Name of this Character

    //private health
    int _health;
    //Get the health and in the moment that set, update the healthbar and verify if his dead
    int health
    {
        get
        {
            return _health;
        }
        set
        {
            _health = value;
            //Update healthBar
            if (healthBar != null) healthBar.value = _health;

            //Verify if his dead
            if (_health <= 0)
            {
                Dead();
            }
        }
    }

    [Range(1,100)]
    [SerializeField]
    [Tooltip("100% health may be from the strongest character in the game, so it's easy to balance this way.")]
    int maxHealth, baseDamage; //Health and baseDamage by percentage
    [SerializeField]
    int damageMultiplier = 1; //Multiply the BaseDamage percentage
    [SerializeField]
    int tileMove = 1; //How many tile his move in one round
    [SerializeField]
    int rangeAttack = 1; //attack range
    [HideInInspector]
    public int currentBox, index; //Where this player stay in the grid world

    //Final damage
    public int damage
    {
        get
        {
            return baseDamage * damageMultiplier;
        }
    }

    //private grid
    GridSlot _grid;
    //Get the grid and in the moment that set, leave the old slot and go to the new
    GridSlot currentGrid
    {
        get
        {
            return _grid;
        }
        set
        {
            //Check if have a old slot
            if(_grid != null && _grid.character.Contains(this))
            {
                _grid.character.Remove(this);
            }

            _grid = value;

            //Check if receive a new
            if (_grid != null && !_grid.character.Contains(this))
            {
                _grid.character.Add(this);
            }
        }
    }

    //healthBar to feedback life
    Slider healthBar;

    #region Move
    // Transforms to act as start and end markers for the journey.
    Vector2 startMove, endMove;
    List<GridSlot> gridsToMove = new List<GridSlot>();

    // Movement speed in units per second.
    public float speed = 1.0F;

    // Time when the movement started.
    private float startTime;

    // Total distance between the markers.
    private float journeyLength;
    #endregion

    void Awake()
    {
        //Get sprite
        sprite = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        //Put player on Grid
        RepositionCharacter();

        //Check if can create a healthBar
        if (gameController.mainCanvas != null
        && gameController.healthBar != null)
        {
            //Create a healthBar
            healthBar = Instantiate(gameController.healthBar, gameController.healthBarsParent.transform);
            healthBar.maxValue = maxHealth;
            healthBar.minValue = 0;
        }
        //Initialize character with maxHealth
        health = maxHealth;

        //Get the inital color
        spriteColor = sprite.color;
        //Put healthBar in character head
        RepositionHealthBar();
    }

    void Update()
    {
        //Check if have moves
        if (gridsToMove.Count == 0) return;

        // Distance moved equals elapsed time times speed..
        float distCovered = (Time.time - startTime) * speed;

        // Fraction of journey completed equals current distance divided by total distance.
        float fractionOfJourney = distCovered / journeyLength;

        // Set our position as a fraction of the distance between the markers.
        transform.position = Vector2.Lerp(startMove, endMove, fractionOfJourney);

        //Update the healthBar
        RepositionHealthBar();

        //Verify if character is arrived at destination
        if (Vector2.Distance(transform.position, endMove) < 0.05f)
        {
            //Update player pos
            RepositionCharacter();
            //Check if have a Enemy
            Character target = FindATarget(rangeAttack); //Who him will attack

            if(target != null)
            {
                //Reset grid move and attack the target
                gridsToMove = new List<GridSlot>();
                target.ReceiveDamage(damage);
            }
            else
            {
                //Remove the point that character moved on
                gridsToMove.RemoveAt(0);
                //Verify if have a next
                if (gridsToMove.Count > 0)
                {
                    //Start a new movement
                    StartGridMove(gridsToMove[0].position);
                }
                else
                {
                    //End the move
                    gameController.EndOfMove();
                }
            }
        }
    }
    
    public void StartTheMove()
    {
        //Check if relay is your turn
        if (team != gameController.currentTeamTurn)
        {
            return;
        }
        //Update Character
        RepositionCharacter();

        Character target = FindATarget(rangeAttack); //Who him will attack

        //If don't have a target
        if (target == null)
        {
            //Find the nearest by GameController list
            target = gameController.FindNearestEnemy(this);
            if(target != null)
            {
                //Find the path to the target
                List<GridSlot> path = pathfinding.FindPath(transform.position, target.transform.position);

                //if you're not far enough away, end the move
                if (path.Count < 2)
                {
                    gameController.EndOfMove();
                }
                else
                {
                    //Control to don't move more than must be
                    int toMove = Mathf.Clamp(tileMove, 0, path.Count - 1);

                    //Save the points
                    for (int i = 0; i < toMove; i++)
                    {
                        gridsToMove.Add(path[i]);
                    }

                    //Start to move
                    StartGridMove(gridsToMove[0].position);
                }
            }
        }
        else
        {
            //Attack the target
            target.ReceiveDamage(damage);
        }
    }

    public void ReceiveDamage(int damage)
    {
        //If you attacked
        StartCoroutine(DamageFeedback(0.3f, damage));
    }

    //Check if have a target in your range
    Character FindATarget(int range)
    {
        //Initialize
        Character target = null;
        List<GridSlot> grids = new List<GridSlot>();

        //Starts the grid list if the current character grid
        grids.Add(currentGrid);

        //While don't verify in all range or don't have a target
        while (range > 0 && target == null)
        {
            //Save grids.Count before for
            int numGrids = grids.Count;
            for (int i = 0; i < numGrids; i++)
            {
                //Check in all 4 sides
                for (int j = 0; j < 4; j++)
                {
                    //Get the neighbor by index
                    GridSlot neighbor = grids[i].GetNeighborsByIndex(j);

                    //If don't have a neighbor or has already been checked
                    if (neighbor == null || grids.Contains(neighbor))
                    {
                        continue;
                    }

                    //Verify if have a character
                    if(neighbor.character != null 
                    && neighbor.character.Count > 0)
                    {
                        //Check if one of this characters is a Enemy
                        for(int c = 0; c < neighbor.character.Count; c++)
                        {
                            if (neighbor.character[c].team != team)
                            {
                                target = neighbor.character[c];
                                break;
                            }
                        }
                        break;
                    }

                    grids.Add(neighbor);
                }
            }

            //Subtract from range
            range--;
        }

        return target;
    }

    //Put character on gridSlot
    void RepositionCharacter()
    {
        currentGrid = grid.GetGridSlot(transform.position);
        transform.position = currentGrid.position;
    }

    //Put healthbar in character head
    void RepositionHealthBar()
    {
        Vector2 pos = (Vector2)transform.position + (Vector2.up * gameController.healthBarDis);
        healthBar.transform.position = cam.WorldToScreenPoint(pos);
    }

    //Start move with Lerp
    void StartGridMove(Vector2 endMove)
    {
        //Set the initial and final position
        startMove = transform.position;
        this.endMove = endMove;

        // Keep a note of the time the movement started.
        startTime = Time.time;

        // Calculate the journey length.
        journeyLength = Vector2.Distance(startMove, endMove);

        //Update HealthBar
        RepositionHealthBar();
    }

    void Dead()
    {
        //Remove from gridSlot
        currentGrid = null;

        //Remove from team
        gameController.RemoveFromTeam(this);

        //Destroy objects
        Destroy(healthBar.gameObject);
        Destroy(gameObject);
    }

    //Change color to demonstrate that he took damage
    IEnumerator DamageFeedback(float time, int damage)
    {
        sprite.color = Color.red;
        yield return new WaitForSeconds(time);
        sprite.color = spriteColor;
        yield return new WaitForSeconds(time);
        sprite.color = Color.red;
        yield return new WaitForSeconds(time);
        sprite.color = spriteColor;
        //After that end move and update life
        gameController.EndOfMove();
        health -= damage;
    }
}
