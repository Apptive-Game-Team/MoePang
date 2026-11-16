using UnityEngine;

namespace _01.Scripts._04.UI.TitleScene
{
    public class CloudMovement : MonoBehaviour
    {
        private enum MoveDirection
        {
            Left,
            Right
        }
        
        [SerializeField] private MoveDirection moveDirection = MoveDirection.Left;
        [SerializeField] private float moveSpeed = 50f;
        [SerializeField] private float moveDistance = 2000f;

        private RectTransform _rectTransform;
        private Vector2 _startPosition;
        private float _movedDistance;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _startPosition = _rectTransform.anchoredPosition;
        }

        private void Update()
        {
            float direction = moveDirection == MoveDirection.Right ? 1f : -1f;

            float movement = moveSpeed * direction * Time.deltaTime;

            _rectTransform.anchoredPosition += new Vector2(movement, 0f);

            _movedDistance += Mathf.Abs(movement);

            if (_movedDistance >= moveDistance)
            {
                _rectTransform.anchoredPosition = _startPosition;
                _movedDistance = 0f;
            }
        }
    }
}