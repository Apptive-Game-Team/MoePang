using System.Collections.Generic;
using UnityEngine;

namespace _01.Scripts._06.Effect
{
    [RequireComponent(typeof(ParticleSystem))]
    public class OnParticleCollisionColliderOff : MonoBehaviour
    {
        private void OnParticleCollision(GameObject other)
        {
            Collider2D col = other.GetComponent<Collider2D>();
            if (col != null)
            {
                col.enabled = false;
            }
        }
    }
}