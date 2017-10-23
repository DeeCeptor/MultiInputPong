using RavingBots.MultiInput;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Player : MonoBehaviour
{
    public Team team;
    public Color team_colour;
    public IDevice input;
    public int player_id;

    public bool show_player_label = true;
    Text player_label;


	void Awake ()
    {
        if (ScoreManager.score_manager != null)
            ScoreManager.score_manager.players.Add(this);
    }
    private void Start()
    {
        ScoreManager.score_manager.AssignTeam(this.gameObject);
        ScoreManager.score_manager.SetPlayerColours(team, this.gameObject);

        if (show_player_label)
        {
            player_label = Instantiate((Resources.Load("Player Label") as GameObject)).GetComponent<Text>();
            player_label.text = this.transform.name + "\n\n\n ";
            player_label.transform.name = "Player " + this.transform.name + " label";
            player_label.transform.SetParent(GameObject.FindObjectOfType<Canvas>().transform);
            player_label.transform.localScale = Vector3.one;
        }
    }


    private void Update()
    {
        // Update position of UI label above player
        if (show_player_label)
            player_label.transform.position = this.transform.position;
    }
}
