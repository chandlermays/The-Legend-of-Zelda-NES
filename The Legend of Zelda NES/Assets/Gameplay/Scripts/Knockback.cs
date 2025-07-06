using System.Collections;
using UnityEngine;

public class Knockback : MonoBehaviour
{
    private float m_thrust = 20f;
    private float m_knockTime = 0.3f;
    private float m_gracePeriod = 1.5f; // Grace period after knockback

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && !player.IsKnockedBack && !player.IsInvulnerable)
            {
                Rigidbody2D hit = other.GetComponent<Rigidbody2D>();
                if (hit != null)
                {
                    Vector2 difference = hit.transform.position - transform.position;
                    difference = difference.normalized * m_thrust;
                    hit.AddForce(difference, ForceMode2D.Impulse);
                    StartCoroutine(KnockbackEvent(hit, player));
                }
            }
        }
        else if (other.gameObject.CompareTag("Enemy"))
        {
            Rigidbody2D hit = other.GetComponent<Rigidbody2D>();
            if (hit != null)
            {
                if (other.TryGetComponent(out SkeletonEnemy skeletonEnemy))
                {
                    if (skeletonEnemy.GetCurrentHealth() <= 0)
                    {
                        return; // Exit if the skeleton is dead
                    }
                }
                else if (other.TryGetComponent(out GoriyaEnemy goriyaEnemy))
                {
                    if (goriyaEnemy.GetCurrentHealth() <= 0)
                    {
                        return; // Exit if the goriya is dead
                    }
                }

                IProjectile projectile = GetComponent<IProjectile>();
                if (projectile != null)
                {
                    Vector2 difference = projectile.Direction.normalized * m_thrust;
                    Debug.Log("Enemy Knockback difference: " + difference);
                    hit.AddForce(difference, ForceMode2D.Impulse);
                    StartCoroutine(KnockbackEvent(hit, other.gameObject));
                }
            }
        }
    }

    private IEnumerator KnockbackEvent(Rigidbody2D rb2D, PlayerController player)
    {
        if (rb2D != null && player != null)
        {
            player.TakeDamage(1);

            if (player.GetCurrentHealth() <= 0)
            {
                yield break; // Exit the coroutine if the player is dead
            }

            player.IsKnockedBack = true; // Set knockback flag
            player.IsInvulnerable = true; // Set invulnerability flag
            player.PauseEntity(true); // Disable player movement

            Debug.Log("Knockback");
            yield return new WaitForSeconds(m_knockTime);

            rb2D.linearVelocity = Vector2.zero; // Stop the player's movement
            player.PauseEntity(false); // Enable player movement
            player.IsKnockedBack = false; // Reset knockback flag

            yield return new WaitForSeconds(m_gracePeriod); // Wait for the grace period
            player.IsInvulnerable = false; // Reset invulnerability flag
        }
    }

    private IEnumerator KnockbackEvent(Rigidbody2D rb2D, GameObject enemy)
    {
        if (rb2D != null && enemy != null)
        {
            if (enemy.TryGetComponent(out SkeletonEnemy skeletonEnemy))
            {
                Debug.Log("Skeleton Enemy Knockback");
                skeletonEnemy.IsKnockedBack = true;
                skeletonEnemy.IsInvulnerable = true;
                skeletonEnemy.PauseEntity(true);
                yield return new WaitForSeconds(m_knockTime);

                rb2D.linearVelocity = Vector2.zero;
                skeletonEnemy.PauseEntity(false);
                skeletonEnemy.IsKnockedBack = false;

                yield return new WaitForSeconds(m_gracePeriod);
                skeletonEnemy.IsInvulnerable = false;
            }
            else if (enemy.TryGetComponent(out GoriyaEnemy goriyaEnemy))
            {
                Debug.Log("Goriya Enemy Knockback");
                goriyaEnemy.IsKnockedBack = true;
                goriyaEnemy.IsInvulnerable = true;
                goriyaEnemy.PauseEntity(true);
                yield return new WaitForSeconds(m_knockTime);

                rb2D.linearVelocity = Vector2.zero;
                goriyaEnemy.PauseEntity(false);
                goriyaEnemy.IsKnockedBack = false;

                yield return new WaitForSeconds(m_gracePeriod);
                goriyaEnemy.IsInvulnerable = false;
            }
        }
    }
}