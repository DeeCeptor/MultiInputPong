using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SpaceInvadersRecord : Round_Record
{
    /**
     * 	Accuracy: Number of invaders
	Num invaders killed
	Time (in seconds from start) of each kill
	Number of player bullets fired
	Num enemy bullets fired
	Completion time
	Error rate: number of times player is hit (player cannot fire for 1 second, but can still move and is invulnerable)
     */
    // Hitting invaders (Accuracy)
    public int num_invaders_hit;
    public List<float> time_of_invader_hit;   // Time since beginning of round
    public List<float> x_pos_of_invader_at_hit;
    public int num_shots;               // How many bullets did rhe player fire?
    public int num_finished_shots;      // How many bullets of the player hit an invader or a wall?

    // Getting hit by invaders (Error rate)
    public int num_errors;
    public List<float> time_of_error;   // Time since beginning of round
    public List<float> pos_of_error;

    public override string ToString()
    {
        return base.ToString() + ",";
    }
    public override string FieldNames()
    {
        return base.FieldNames() + ",";
    }
}


public class SpaceInvaders : Trial
{
    public static SpaceInvaders space_invaders;

    public TextAsset settings;
    public SpaceInvadersRecord current_round_record;

    public CameraRect camera_rect;
    public Transform position_to_spawn_enemies;
    public GameObject enemies_to_spawn;
    public GameObject current_enemy_parent;

    // Bullet parameters
    public Transform bullet_parent;
    public GameObject enemy_bullet_prefab;
    float leftmost_x;
    float rightmost_x;
    float top_y;
    float bullet_cooldown = 0.2f;
    float cur_bullet_cooldown;


    public override void StartTrial()
    {
        PopulateSettings();
        base.StartTrial();
    }
    public void PopulateSettings()
    {
        if (settings == null)
            return;

        /*
        ball_speeds.Clear();
        string[] splits = { "\n" };
        string[] str_vals = settings.text.Split(splits, StringSplitOptions.RemoveEmptyEntries);

        foreach (string s in str_vals)
        {
            // Ball speed, distance between players
            string[] items = s.Split(',');
            ball_speeds.Add(float.Parse(items[0]));
            distances_between_players.Add(float.Parse(items[1]));
        }
        */
    Debug.Log("Done loading settings", this.gameObject);
    }


    public override void StartRound()
    {
        // Create enemies
        current_enemy_parent = Instantiate(enemies_to_spawn, position_to_spawn_enemies.transform.position, Quaternion.identity);

        // Reset player position

        // Reset scores

        base.StartRound();

        // Create new round record
        round_results.Add(current_round_record);
        current_round_record.participant_id = "" + GlobalSettings.GetParticipantId(0);
        current_round_record.ms_input_lag_of_round = input_delay_per_round[current_round];

        // Set current round features
    }


    public override void ResetBetweenRounds()
    {
        base.ResetBetweenRounds();

        // Destroy existing space invaders
        if (current_enemy_parent != null)
            Destroy(current_enemy_parent);

        // Reset all bullets
        cur_bullet_cooldown = 0;
        BulletPooler.DespawnAllBullets();

        current_round_record = new SpaceInvadersRecord();
    }


    public override void FinishRound()
    {
        // Record any more necessary round data

        base.FinishRound();
    }


    public override void FinishTrial()
    {
        base.FinishTrial();
    }


    public override void Awake()
    {
        base.Awake();

        space_invaders = this;
    }
    public override void Start()
    {
        camera_rect = Camera.main.GetComponent<CameraRect>();
        leftmost_x = camera_rect.bottomleft.transform.position.x;
        rightmost_x = camera_rect.topright.transform.position.x;
        top_y = camera_rect.topright.transform.position.y;

        BulletPooler.PopulateBulletPool();
    }


    public void CreateEnemyBullet()
    {
        // Spawn bullets at top of screen in random positions
        Vector2 new_bullet_pos = new Vector2(UnityEngine.Random.Range(leftmost_x, rightmost_x), top_y - 0.5f);
        GameObject new_bullet = BulletPooler.GetEnemyBullet(new_bullet_pos); ;

        // Give downwards direction
        new_bullet.GetComponent<Rigidbody2D>().velocity = new Vector2(0, -4f);

        cur_bullet_cooldown = bullet_cooldown;
    }


    public override void Update()
    {
        base.Update();

        if (this.round_running)
        {
            cur_bullet_cooldown -= Time.deltaTime;
            if (cur_bullet_cooldown <= 0)
                CreateEnemyBullet();
        }
    }
}
