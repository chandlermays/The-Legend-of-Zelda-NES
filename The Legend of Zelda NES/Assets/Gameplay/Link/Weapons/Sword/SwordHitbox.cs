#define DEBUG_LOG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordHitbox : MonoBehaviour
{
    private int m_swordDamage = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
#if DEBUG_LOG
            Debug.Log("Sword projectile hit an enemy.");
#endif
            collision.gameObject.SendMessage("OnHit", m_swordDamage, SendMessageOptions.DontRequireReceiver); // Broadcast OnHit message
        }
    }
}
