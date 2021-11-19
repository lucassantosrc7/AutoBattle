using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Pathfinding))]
public class GameController : MonoBehaviour
{
    //All teams that we have
    public enum Teams
    {
        red, blue
    }

    public static GameController instance { get; private set; } //There only one GameController
    public static Pathfinding pathfinding { get; private set; } //Is Required a Pathfinding to the game Work
    
    //Private teamTurn
    Teams teamTurn;
    //Get the current team turn and in the moment that set, make a feedback in canvas
    public Teams currentTeamTurn
    {
        get
        {
            return teamTurn;
        }
        private set
        {
            teamTurn = value;
            string txt = "Is " + teamTurn.ToString() + " Turn";
            StartCoroutine(InfoText(1, txt, "ResetCurrentMove"));
        }
    }

    //Private how many character have to move
    int characterMove;
    //Get the current character move and in the moment that set, check if the team turn is finish
    public int currentCharacterMove
    {
        get
        {
            return characterMove;
        }
        private set
        {
            characterMove = value;

            List<Character> team = currentTeamTurn == Teams.blue ? blueTeam : redTeam;
            Teams next = currentTeamTurn == Teams.blue? Teams.red: Teams.blue;

            if (currentCharacterMove >= team.Count)
            {
                currentTeamTurn = next;
            }
            else
            {
                team[currentCharacterMove].StartTheMove();
            }
        }
    }

    public Camera cam; //Camera of the game

    [Header("Gameplay")]
    public Grid grid; //Set the grid
    [SerializeField]
    int numCharPlayer = 3; //Number of character that player can put on
    [SerializeField]
    Character characterPrefab; //Character Prefab
    [SerializeField]
    Vector2[] redTeamPositions; //Where the red team we will instantiate

    //Color of the teams
    [SerializeField]
    Color blueTeamColor = Color.cyan,
          redTeamColor = Color.red;

    //List of the characters that which team have
    List<Character> blueTeam = new List<Character>();
    List<Character> redTeam = new List<Character>();

    //GameObject that we put the instances to maintain the organization
    Transform blueTeamParent, redTeamParent;

    [Header("UI")]
    public Canvas mainCanvas; //The canvas in game
    public TMP_Text infoText; //Text where we talk with player
    public Slider healthBar; //Healthebar to alls player use
    public Transform healthBarsParent; //GameObject that we put the instances to maintain the organization
    public float healthBarDis = 0.5f; //distance from the head of the characters

    void Awake()
    {
        //Set instance
        instance = this;
        //Get pathfinding
        pathfinding = GetComponent<Pathfinding>();

        //Create the gameObjects
        blueTeamParent = new GameObject("BlueTeam").transform;
        redTeamParent = new GameObject("RedTeam").transform;

        //Disable the text
        infoText.gameObject.SetActive(false);

        //Get the camera
        if (cam == null)
        {
            cam = Camera.main;
        }

        //Set the firts text
        string txt = "You have " + numCharPlayer.ToString() + " to put in battle, click to add";
        StartCoroutine(InfoText(1, txt, "CheckNumPlayer"));

        //Create the red team
        for (int i = 0; i < redTeamPositions.Length; i++)
        {
            Character character = Instantiate(this.characterPrefab, redTeamPositions[i], Quaternion.identity, redTeamParent);
            character.sprite.color = redTeamColor;
            character.team = Teams.red;
            redTeam.Add(character);
        }
    }

    //Get the nearest enemy from a character
    public Character FindNearestEnemy(Character character)
    {
        //Which team we will search
        List<Character> enemies = character.team == Teams.blue? redTeam: blueTeam;
        Character toReturn = null; //Charatcter to return
        float minDis = float.MaxValue; //Get the min distance
        for (int i = 0; i < enemies.Count; i++)
        {
            //Get distance
            float dis = Vector2.Distance(character.transform.position, enemies[i].transform.position);

            //If is less than the last distance
            if (dis < minDis)
            {
                //Set a new minDis
                minDis = dis;
                //Save to return
                toReturn = enemies[i];

                //Since we won't have anything smaller than zero, then we return
                if (dis == 0) break;
            }
        }

        return toReturn;
    }

    //When a charactar end his move
    public void EndOfMove()
    {
        //Get the next in team
        currentCharacterMove++;
    }

    //Remove the character from his team
    public void RemoveFromTeam(Character character)
    {
        //Check which team and remove
        if (character.team == Teams.red)
        {
            redTeam.Remove(character);
        }
        else
        {
            blueTeam.Remove(character);
        }

        //Verify if we have a champion
        if (blueTeam.Count == 0)
        {
            string txt = "The victorius is Red team";
            StartCoroutine(InfoText(3, txt, "ResetScene"));
            foreach (Character teamCharacter in redTeam)
            {
                teamCharacter.enabled = false;
            }
        }

        if (redTeam.Count == 0)
        {
            string txt = "The victorius is Blue team";
            StartCoroutine(InfoText(3, txt, "ResetScene"));
            foreach (Character teamCharacter in blueTeam)
            {
                teamCharacter.enabled = false;
            }
        }
    }

    void ResetCurrentMove()
    {
        currentCharacterMove = 0;
    }

    void ResetScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    //Add a character to player team
    public void AddPlayer(GridSlot gridSlot)
    {
        //Verify if they can
        if (numCharPlayer <= 0) return;
        //Subtract one
        numCharPlayer--;
        //Instantiete and put in the position
        Character character = Instantiate(this.characterPrefab, gridSlot.position, Quaternion.identity, blueTeamParent);
        //Set color
        character.sprite.color = blueTeamColor;
        //Set team
        character.team = Teams.blue;
        //Add to team
        blueTeam.Add(character);

        //Warns the player how many are left
        string txt = "You have " + numCharPlayer.ToString() + " to put in battle, click to add";
        //If nothing, notify they
        if(numCharPlayer == 0)
        {
            txt = "Get ready!";
        }
        StartCoroutine(InfoText(1, txt, "CheckNumPlayer"));
    }

    //Verify if player put all characters
    void CheckNumPlayer()
    {
        if (numCharPlayer > 0) return;

        //Create a random
        int random = Random.Range(1,100);
        if(random > 50)
        {
            currentTeamTurn = Teams.blue;
        }
        else
        {
            currentTeamTurn = Teams.red;
        }
    }

    //Show the infos
    IEnumerator InfoText(float time, string txt, string nameFunction)
    {
        //Enable the text
        infoText.gameObject.SetActive(true);
        infoText.text = txt;
        yield return new WaitForSeconds(time);
        //Disable and call a function
        infoText.gameObject.SetActive(false);
        gameObject.SendMessage(nameFunction, SendMessageOptions.RequireReceiver);
    }
}