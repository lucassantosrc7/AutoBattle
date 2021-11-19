using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Pathfinding))]
public class GameController : MonoBehaviour
{
    public enum Teams
    {
        red, blue
    }

    public static GameController instance { get; private set; } //There only one GameController
    public static Pathfinding pathfinding { get; private set; } //Is Required a Pathfinding to the game Work
    Teams teamTurn;
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
    int characterMove;
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

    public Camera cam;

    [Header("Gameplay")]
    public Grid grid; //Set the grid
    [SerializeField]
    int numCharPlayer = 3;
    [SerializeField]
    Character characterPrefab;
    [SerializeField]
    Vector2[] redTeamPositions;
    List<Character> blueTeam = new List<Character>();
    List<Character> redTeam = new List<Character>();

    [Header("UI")]
    public Canvas mainCanvas;
    public TMP_Text infoText;
    public Slider healthBar;
    public float healthBarDis = 0.5f;

    void Awake()
    {
        //Set instance
        instance = this;
        //Get pathfinding
        pathfinding = GetComponent<Pathfinding>();

        infoText.gameObject.SetActive(false);

        if (cam == null)
        {
            cam = Camera.main;
        }

        string txt = "You have " + numCharPlayer.ToString() + " to put in battle, click to add";
        StartCoroutine(InfoText(1, txt, "CheckNumPlayer"));
    }

    public Character FindNearestEnemy(Character character)
    {
        List<Character> enemies = character.team == Teams.blue? redTeam: blueTeam;
        Character toReturn = null;
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

    public void EndOfMove()
    {
        currentCharacterMove++;
    }

    public void RemoveFromTeam(Character character)
    {
        if (character.team == Teams.red)
        {
            redTeam.Remove(character);
        }
        else
        {
            blueTeam.Remove(character);
        }

        if(blueTeam.Count == 0)
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

    public void AddPlayer(GridSlot gridSlot)
    {
        if (numCharPlayer <= 0) return;
        numCharPlayer--;
        Character character = Instantiate(this.characterPrefab, gridSlot.position, Quaternion.identity);
        character.sprite.color = Color.cyan;
        blueTeam.Add(character);

        string txt = "You have " + numCharPlayer.ToString() + " to put in battle, click to add";
        StartCoroutine(InfoText(1, txt, "CheckNumPlayer"));
    }

    void CheckNumPlayer()
    {
        if (numCharPlayer > 0) return;
        for(int i = 0; i < redTeamPositions.Length; i++)
        {
            Character character = Instantiate(this.characterPrefab, redTeamPositions[i], Quaternion.identity);
            character.sprite.color = Color.red;
            redTeam.Add(character);
        }

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

    IEnumerator InfoText(float time, string txt, string nameFunction)
    {
        infoText.gameObject.SetActive(true);
        infoText.text = txt;
        yield return new WaitForSeconds(time);
        infoText.gameObject.SetActive(false);
        gameObject.SendMessage(nameFunction, SendMessageOptions.RequireReceiver);
    }
}