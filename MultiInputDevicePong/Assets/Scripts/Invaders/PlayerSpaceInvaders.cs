using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles shooting bullets
public class PlayerSpaceInvaders : MonoBehaviour
{
    public static PlayerSpaceInvaders player_invader;
    public GameObject bullets_to_use;
    public Bullet previous_bullet = null;
    public QueueMouseClicks mouse_click_input;

    float cur_cooldown;
    float shot_cooldown = 0.5f;

    float amount_of_invuln_time = 0.6f;
    public float invuln_time_left = 0;
    float prev_invuln_time_left = 0;
    SpriteRenderer sprite;

    AudioSource hit_noise;

    public float player_bullet_speed = 7f;



    void Awake ()
    {
        player_invader = this;
        mouse_click_input = this.GetComponentInChildren<QueueMouseClicks>();
        sprite = this.GetComponentInChildren<SpriteRenderer>();
        hit_noise = this.GetComponent<AudioSource>();
    }


    void Update ()
    {
        cur_cooldown -= Time.deltaTime;

        // Check if we should turn off invulnerability
        prev_invuln_time_left = invuln_time_left;
        invuln_time_left -= Time.deltaTime;
        if (prev_invuln_time_left > 0 && invuln_time_left <= 0)
            TurnOffInvulnerability();

        // Player can only have 1 bullet on-screen at a time. Can fire again when that bullet is inactive
        if (invuln_time_left <= 0f && mouse_click_input.cur_left_mouse_held_down && (previous_bullet == null || previous_bullet.shot_by_player == false) || (Input.GetMouseButton(1) && Application.isEditor))
            ShootBullet();
    }


    public void TurnOffInvulnerability()
    {
        invuln_time_left = 0;
        prev_invuln_time_left = 0;
        sprite.color = Color.white;
    }
    public void TurnOnInvulnerability()
    {
        invuln_time_left = amount_of_invuln_time;
        prev_invuln_time_left = amount_of_invuln_time;
        sprite.color = Color.yellow;
    }


    public void GotHit(Bullet bull)
    {
        Debug.Log("Hit player", this.gameObject);
        hit_noise.Play();
        ScoreManager.score_manager.BlueScored(-1);
        SpaceInvaders.space_invaders.current_round_record.num_errors++;
        SpaceInvaders.space_invaders.current_round_record.time_of_error.Add(SpaceInvaders.space_invaders.time_for_current_round);
        SpaceInvaders.space_invaders.current_round_record.pos_of_player_at_error.Add(this.transform.position.x);
        SpaceInvaders.space_invaders.current_round_record.pos_of_bullet_at_error.Add(bull.transform.position.x);
        TurnOnInvulnerability();
    }


    public void ShootBullet()
    {
        cur_cooldown = shot_cooldown;

        // Record a bullet was shot
        GameObject new_bullet = BulletPooler.GetPlayerBullet(this.transform.position);
        new_bullet.GetComponent<Rigidbody2D>().velocity = new Vector2(0, player_bullet_speed);
        previous_bullet = new_bullet.GetComponent<Bullet>();
        SpaceInvaders.space_invaders.current_round_record.num_player_shots++;
    }




}


public static class BulletPooler
{
    public static List<GameObject> all_bullets = new List<GameObject>();

    static int num_pooled_bullets = 20;
    public static int num_active_enemy_bullets = 0;


    public static void PopulateBulletPool()
    {
        all_bullets.Clear();

        // Spawn 100 bullets
        for (int x = 0; x < num_pooled_bullets; x++)
        {
            GameObject go = GameObject.Instantiate(SpaceInvaders.space_invaders.enemy_bullet_prefab, SpaceInvaders.space_invaders.bullet_parent);
            go.SetActive(false);
            all_bullets.Add(go);
        }
    }

    public static GameObject GetPooledBullet()
    {
        // Find an inactive bullet
        GameObject inactive_bullet = all_bullets[0];

        foreach(GameObject bullet in all_bullets)
        {
            if (!bullet.activeSelf)
            {
                inactive_bullet = bullet;
                break;
            }
        }

        inactive_bullet.GetComponent<Bullet>().time_of_creation = Time.time;
        return inactive_bullet;
    }
    public static GameObject GetPlayerBullet(Vector2 position)
    {
        GameObject bullet = GetPooledBullet();
        bullet.transform.position = position;
        bullet.SetActive(true);
        // Use constant player bullet speed
        //bullet.GetComponent<Rigidbody2D>().velocity = new Vector3(0, SpaceInvaders.space_invaders.cur_bullet_speed, 0);
        bullet.GetComponent<SpriteRenderer>().color = Color.white;
        bullet.layer = LayerMask.NameToLayer("Player Bullet");
        bullet.GetComponent<Bullet>().shot_by_player = true;
        return bullet;
    }
    public static GameObject GetEnemyBullet(Vector2 position)
    {
        GameObject bullet = GetPooledBullet();
        bullet.transform.position = position;
        bullet.SetActive(true);
        bullet.GetComponent<Rigidbody2D>().velocity = new Vector3(0, -SpaceInvaders.space_invaders.cur_bullet_speed, 0);
        bullet.GetComponent<SpriteRenderer>().color = Color.red;
        bullet.layer = LayerMask.NameToLayer("Enemy Bullet");
        num_active_enemy_bullets++;
        return bullet;
    }

    public static void DespawnThisBullet(GameObject bullet)
    {
        if (bullet.layer == LayerMask.NameToLayer("Enemy Bullet"))
            num_active_enemy_bullets--;

        bullet.SetActive(false);
    }
    public static void DespawnAllBullets()
    {
        foreach (GameObject bull in all_bullets)
        {
            bull.SetActive(false);
        }
        PlayerSpaceInvaders.player_invader.previous_bullet = null;
        num_active_enemy_bullets = 0;
    }
}
