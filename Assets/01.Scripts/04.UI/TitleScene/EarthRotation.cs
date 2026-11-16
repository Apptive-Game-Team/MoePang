using UnityEngine;

namespace _01.Scripts._04.UI.TitleScene
{
    public class EarthRotation : MonoBehaviour
    {
        [SerializeField, Min(0f)]
        private float rotationSpeed = 4f;

        private void Update()
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }
    }
}
