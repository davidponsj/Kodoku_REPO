using System.Collections;
using UnityEngine;

public class CameraFollowObject : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _playerTransform;

    [Header("Flip Rotation Stats")]
    [SerializeField] private float _flipRotationTime = 0.5f;

    private Coroutine _turnCoroutine;
    private PlayerMovement _player;
    private bool _isFacingRight;

    private void Awake()
    {
        _player = _playerTransform.GetComponent<PlayerMovement>();
        _isFacingRight = _player.isFacingRight;
    }

    private void Update()
    {
        // Follow the player's position (but not rotation)
        transform.position = _playerTransform.position;
    }

    public void CallTurn()
    {
        if (_turnCoroutine != null)
            StopCoroutine(_turnCoroutine);

        _turnCoroutine = StartCoroutine(FlipLerp());
    }

    private IEnumerator FlipLerp()
    {
        float startRotation = transform.localEulerAngles.y;
        float endRotationAmount = DetermineEndRotation();
        float elapsedTime = 0f;

        while (elapsedTime < _flipRotationTime)
        {
            elapsedTime += Time.deltaTime;

            float t = elapsedTime / _flipRotationTime;
            float yRotation = Mathf.Lerp(startRotation, endRotationAmount, t);

            transform.rotation = Quaternion.Euler(0f, yRotation, 0f);

            yield return null;
        }

        // Ensure exact final rotation
        transform.rotation = Quaternion.Euler(0f, endRotationAmount, 0f);
    }

    private float DetermineEndRotation()
    {
        _isFacingRight = _player.isFacingRight;
        return _isFacingRight ? 0f : 180f;
    }
}
