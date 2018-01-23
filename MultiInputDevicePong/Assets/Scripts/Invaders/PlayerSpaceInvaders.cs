using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles shooting bullets
public class PlayerSpaceInvaders : MonoBehaviour
{
    public GameObject bullets_to_use;

    float cur_cooldown;
    float shot_cooldown = 0.5f;

	void Start () {
		
	}


    void Update ()
    {
        cur_cooldown -= Time.deltaTime;

        //if (Input.GetKeyDown(KeyCode.Space))
        if (Input.GetMouseButton(0) && cur_cooldown <= 0)
            ShootBullet();
    }


    public void GotHit(Bullet bull)
    {
        Debug.Log("Hit player", this.gameObject);

        // Turn on invulnerability

        // Change animation

        // Make sound
    }


    public void ShootBullet()
    {
        cur_cooldown = shot_cooldown;

        // Record a bullet was shot
        GameObject new_bullet = BulletPooler.GetPlayerBullet(this.transform.position);
        new_bullet.GetComponent<Rigidbody2D>().velocity = new Vector3(0, 5, 0);

        SpaceInvaders.space_invaders.current_round_record.num_player_shots++;
    }




}


public static class BulletPooler
{
    public static List<GameObject> all_bullets = new List<GameObject>();


    static int num_pooled_bullets = 100;

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

        return inactive_bullet;
    }
    public static GameObject GetPlayerBullet(Vector2 position)
    {
        GameObject bullet = GetPooledBullet();
        bullet.transform.position = position;
        bullet.SetActive(true);
        bullet.GetComponent<Rigidbody2D>().velocity = new Vector3(0, 5, 0);
        bullet.GetComponent<SpriteRenderer>().color = Color.white;
        bullet.layer = LayerMask.NameToLayer("Player Bullet");
        return bullet;
    }
    public static GameObject GetEnemyBullet(Vector2 position)
    {
        GameObject bullet = GetPooledBullet();
        bullet.transform.position = position;
        bullet.SetActive(true);
        bullet.GetComponent<Rigidbody2D>().velocity = new Vector3(0, 5, 0);
        bullet.GetComponent<SpriteRenderer>().color = Color.red;
        bullet.layer = LayerMask.NameToLayer("Enemy Bullet");
        return bullet;
    }

    public static void DespawnThisBullet(GameObject bullet)
    {
        bullet.SetActive(false);
    }
    public static void DespawnAllBullets()
    {
        foreach (GameObject bull in all_bullets)
        {
            bull.SetActive(false);
        }
    }
}
