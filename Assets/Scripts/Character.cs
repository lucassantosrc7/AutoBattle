using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SpriteRenderer))]
public class Character : MonoBehaviour
{
    public SpriteRenderer sprite { get; private set; }

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

    [HideInInspector]
    public GameController.Teams team;

    [Header("Properties")]
    string characterName; //Name of this Character

    int _health;
    int health
    {
        get
        {
            return _health;
        }
        set
        {
            _health = value;
            if (healthBar != null) healthBar.value = _health;

            if(_health <= 0)
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
    int tileMove = 1; //Multiply the BaseDamage percentage
    [SerializeField]
    int rangeAttack = 1; //Multiply the BaseDamage percentage
    [HideInInspector]
    public int currentBox, index; //Where this player stay in the grid world

    public int damage
    {
        get
        {
            return baseDamage * damageMultiplier;
        }
    }

    GridSlot _grid;
    GridSlot currentGrid
    {
        get
        {
            return _grid;
        }
        set
        {
            if(_grid != null && _grid.character.Contains(this))
            {
                _grid.character.Remove(this);
            }

            _grid = value;

            if (_grid != null && !_grid.character.Contains(this))
            {
                _grid.character.Add(this);
            }
        }
    }

    //UI
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
        sprite = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        RepositionCharacter();

        if (gameController.mainCanvas != null
        && gameController.healthBar != null)
        {
            healthBar = Instantiate(gameController.healthBar, gameController.mainCanvas.transform);
            healthBar.maxValue = maxHealth;
            healthBar.minValue = 0;
        }
        health = maxHealth;

        RepositionHealthBar();
    }

    void Update()
    {
        if (gridsToMove.Count == 0) return;

        // Distance moved equals elapsed time times speed..
        float distCovered = (Time.time - startTime) * speed;

        // Fraction of journey completed equals current distance divided by total distance.
        float fractionOfJourney = distCovered / journeyLength;

        // Set our position as a fraction of the distance between the markers.
        transform.position = Vector2.Lerp(startMove, endMove, fractionOfJourney);

        Vector2 pos = (Vector2)transform.position + (Vector2.up * gameController.healthBarDis);
        healthBar.transform.position = cam.WorldToScreenPoint(pos);

        if (Vector2.Distance(transform.position, endMove) < 0.05f)
        {
            RepositionCharacter();
            Character target = FindATarget(rangeAttack); //Who him will attack

            if(target != null)
            {
                gridsToMove = new List<GridSlot>();
                target.ReceiveDamage(damage);
            }
            else
            {
                gridsToMove.RemoveAt(0);
                if (gridsToMove.Count > 0)
                {
                    StartGridMove(gridsToMove[0].position);
                }
                else
                {
                    gameController.EndOfMove();
                }
            }
        }
    }
    
    public void StartTheMove()
    {
        if(team != gameController.currentTeamTurn)
        {
            return;
        }
        RepositionHealthBar();

        Character target = FindATarget(rangeAttack); //Who him will attack

        if(target == null)
        {
            target = gameController.FindNearestEnemy(this);
            List<GridSlot> path = pathfinding.FindPath(transform.position, target.transform.position);

            if (path.Count < 2)
            {
                gameController.EndOfMove();
            }
            else
            {
                int toMove = Mathf.Clamp(tileMove, 0, path.Count - 1);

                for (int i = 0; i < toMove; i++)
                {
                    gridsToMove.Add(path[i]);
                }

                StartGridMove(gridsToMove[0].position);
            }
        }
        else
        {
            target.ReceiveDamage(damage);
        }
    }

    public void ReceiveDamage(int damage)
    {
        health -= damage;

        StartCoroutine(DamageFeedback(0.3f));
    }

    Character FindATarget(int range)
    {
        Character target = null;
        List<GridSlot> grids = new List<GridSlot>();
        grids.Add(currentGrid);
        while (range > 0 && target == null)
        {
            int numGrids = grids.Count;
            for (int i = 0; i < numGrids; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    GridSlot neighbor = grids[i].GetNeighborsByIndex(j);

                    if (neighbor == null || grids.Contains(neighbor))
                    {
                        continue;
                    }

                    if(neighbor.character != null 
                    && neighbor.character.Count > 0)
                    {
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
                    neighbor.sprite.color = Color.green;
                }
            }

            range--;
        }

        return target;
    }

    void RepositionCharacter()
    {
        currentGrid = grid.GetGridSlot(transform.position);
        transform.position = currentGrid.position;
    }

    void RepositionHealthBar()
    {
        Vector2 pos = (Vector2)transform.position + (Vector2.up * gameController.healthBarDis);
        healthBar.transform.position = cam.WorldToScreenPoint(pos);
    }

    void StartGridMove(Vector2 endMove)
    {
        startMove = transform.position;
        this.endMove = endMove;

        // Keep a note of the time the movement started.
        startTime = Time.time;

        // Calculate the journey length.
        journeyLength = Vector2.Distance(startMove, endMove);

        RepositionHealthBar();
    }

    void Dead()
    {
        currentGrid = null;
        gameController.RemoveFromTeam(this);
        Destroy(healthBar.gameObject);
        Destroy(gameObject);
    }

    IEnumerator DamageFeedback(float time)
    {
        sprite.color = Color.red;
        yield return new WaitForSeconds(time);
        sprite.color = Color.white;
        yield return new WaitForSeconds(time);
        sprite.color = Color.red;
        yield return new WaitForSeconds(time);
        sprite.color = Color.white;
        gameController.EndOfMove();
    }
}
