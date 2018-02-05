using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SpaceInvadersRecord : Round_Record
{
    /**
     *  Error rate, accuracy, completion time (stored in base round_record)
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
    public int player_missed_shots;
    public List<float> time_of_invader_hit = new List<float>();   // Time since beginning of round
    public List<float> x_pos_of_invader_at_hit = new List<float>();     // Position of invader ENEMY at time they are hit
    public List<float> x_pos_of_bullet_at_invader_hit = new List<float>();  // X position of the bullet that killed the invader on hit
    public int num_finished_shots;      // How many bullets of the player hit an invader or a wall?
    public int num_player_shots;        // How many bullets did the player fire?

    // Getting hit by invaders (Error rate)
    public int num_errors;              // Number of times player was hit by a bullet while not invulnerable
    public List<float> time_of_error = new List<float>();   // The times (since start of round, in seconds) that the player got hit
    public List<float> pos_of_player_at_error = new List<float>();      // Position of the player when hit
    public List<float> pos_of_bullet_at_error = new List<float>();      // Position of bullet when the player is hit

    // Number of bullets spawned at the top of the screen
    public float time_to_react_from_enemy_bullets;
    public float bullet_speed;
    public int num_enemy_bullets_fired;
    public float enemy_bullets_fired_per_second_rate;   // Number of bullets fired 




    public override string ToString()
    {
        return base.ToString() 
            + "," + time_to_react_from_enemy_bullets + "," + num_invaders_hit + "," + Round_Record.ListToString<float>(time_of_invader_hit) + "," + player_missed_shots
            + "," + Round_Record.ListToString<float>(x_pos_of_invader_at_hit) + "," + Round_Record.ListToString<float>(x_pos_of_bullet_at_invader_hit)
            + "," + num_player_shots + "," + num_finished_shots
            + "," + num_errors + "," + Round_Record.ListToString<float>(time_of_error) + "," + Round_Record.ListToString<float>(pos_of_player_at_error) + "," + Round_Record.ListToString<float>(pos_of_bullet_at_error)
            + "," + num_enemy_bullets_fired + "," + enemy_bullets_fired_per_second_rate + "," + bullet_speed;
    }
    public override string FieldNames()
    {
        return base.FieldNames() +
            ",time_to_react_from_enemy_bullet,num_invaders_hit,time_of_invader_hit,player_missed_shots," +
            "x_pos_of_invader_at_hit,x_pos_of_bullet_at_invader_hit," +
            "num_player_shots,num_finished_shots," +
            "num_errors,time_of_error,pos_of_player_at_error,pos_of_bullet_at_error," +
            "num_enemy_bullets_fired,enemy_bullets_fired_per_second_rate,bullet_speed";
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
    public List<float> enemy_bullet_travel_times = new List<float>();
    public float cur_bullet_speed = 5f;
    float bullet_cooldown = 0.4f;
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


        enemy_bullet_travel_times.Clear();

        string[] splits = { "\n" };
        string[] str_vals = settings.text.Split(splits, StringSplitOptions.RemoveEmptyEntries);

        foreach (string s in str_vals)
        {
            enemy_bullet_travel_times.Add(float.Parse(s));
        }
        
        Debug.Log("Done loading settings", this.gameObject);
    }


    public override void StartRound()
    {
        // Create enemies
        current_enemy_parent = Instantiate(enemies_to_spawn, position_to_spawn_enemies.transform.position, Quaternion.identity);

        // Reset player position
        PlayerSpaceInvaders.player_invader.TurnOffInvulnerability();

        // Reset scores
        ScoreManager.score_manager.ResetScore();

        base.StartRound();

        // Create new round record
        round_results.Add(current_round_record);
        current_round_record.participant_id = "" + GlobalSettings.GetParticipantId(0);
        current_round_record.ms_input_lag_of_round = input_delay_per_round[current_round];
        current_round_record.enemy_bullets_fired_per_second_rate = bullet_cooldown;

        // Set current round features
        // Set speed of bullet to setting found in file
        // movement_speed = Distance_to_travel / time_taken;
        current_round_record.time_to_react_from_enemy_bullets = enemy_bullet_travel_times[current_round];
        cur_bullet_speed = (CameraRect.camera_settings.topright.transform.position.y - PlayerSpaceInvaders.player_invader.transform.position.y) / enemy_bullet_travel_times[current_round];
        current_round_record.bullet_speed = cur_bullet_speed;
        /*
        time_taken = Distance_to_travel / movement_speed;
        time_taken * movement_speed  = Distance_to_travel;
        
        */

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
        CreateTextFile();
        
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

        cur_bullet_cooldown = bullet_cooldown;
        current_round_record.num_enemy_bullets_fired++;
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
