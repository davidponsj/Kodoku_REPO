using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;

    [Header("Virtual Cameras (Cinemachine 3)")]
    [SerializeField] private CinemachineCamera[] _allVirtualCameras;
    private CinemachineCamera _currentCamera;

    [Header("Controls for lerping the Y damping during player jump/fall")]
    [SerializeField] private float _fallPanAmount = 0.25f;
    [SerializeField] private float _fallPanTime = 0.35f;
    public float _fallSpeedDampingChangeThreshold = -15f;

    public bool IsLerpingYDamping { get; private set; }
    public bool LerpedFromPlayerFalling { get; set; }

    private Coroutine _lerpYPanCoroutine;

    // En CM3 usamos PositionComposer
    private CinemachinePositionComposer _composer;

    private float _normYPanAmount;

    private void Awake()
    {
        if (instance == null)
            instance = this;

        // Encontrar la cámara activa y su PositionComposer
        for (int i = 0; i < _allVirtualCameras.Length; i++)
        {
            if (_allVirtualCameras[i].enabled)
            {
                _currentCamera = _allVirtualCameras[i];
                _composer = _currentCamera.GetComponent<CinemachinePositionComposer>();

                // Guardar el damping original
                _normYPanAmount = _composer.Damping.y;
            }
        }
    }

    #region Lerp Y Damping

    public void LerpYDamping(bool isPlayerFalling)
    {
        if (_lerpYPanCoroutine != null)
            StopCoroutine(_lerpYPanCoroutine);

        _lerpYPanCoroutine = StartCoroutine(LerpYAction(isPlayerFalling));
    }

    private IEnumerator LerpYAction(bool isPlayerFalling)
    {
        IsLerpingYDamping = true;

        float startDampAmount = _composer.Damping.y;
        float endDampAmount = isPlayerFalling ? _fallPanAmount : _normYPanAmount;

        if (isPlayerFalling)
            LerpedFromPlayerFalling = true;

        float elapsedTime = 0f;

        while (elapsedTime < _fallPanTime)
        {
            elapsedTime += Time.deltaTime;

            float lerped = Mathf.Lerp(startDampAmount, endDampAmount, elapsedTime / _fallPanTime);

            Vector3 damping = _composer.Damping;
            damping.y = lerped;
            _composer.Damping = damping;

            yield return null;
        }

        IsLerpingYDamping = false;
    }

    #endregion
}
