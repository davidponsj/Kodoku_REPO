using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;

public class CameraControlTrigger : MonoBehaviour
{
    public CustomInspectorObjects customInspectorObjects;
}

[System.Serializable]
public class CustomInspectorObjects
{
    public bool swapCameras = false;
    public bool panCameraOnContact = false;

    // Swap cameras
    [HideInInspector] public CinemachineCamera cameraOnLeft;
    [HideInInspector] public CinemachineCamera cameraOnRight;

    // Pan camera
    [HideInInspector] public PanDirection panDirection;
    [HideInInspector] public float panDistance = 3f;
    [HideInInspector] public float panTime = 0.35f;
}

public enum PanDirection
{
    Up,
    Down,
    Left,
    Right
}

#if UNITY_EDITOR
[CustomEditor(typeof(CameraControlTrigger))]
public class MyScriptEditor : Editor
{
    private CameraControlTrigger cameraControlTrigger;

    private void OnEnable()
    {
        cameraControlTrigger = (CameraControlTrigger)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var data = cameraControlTrigger.customInspectorObjects;

        // Swap Cameras
        if (data.swapCameras)
        {
            data.cameraOnLeft = EditorGUILayout.ObjectField(
                "Camera On Left",
                data.cameraOnLeft,
                typeof(CinemachineCamera),
                true
            ) as CinemachineCamera;

            data.cameraOnRight = EditorGUILayout.ObjectField(
                "Camera On Right",
                data.cameraOnRight,
                typeof(CinemachineCamera),
                true
            ) as CinemachineCamera;
        }

        // Pan Camera
        if (data.panCameraOnContact)
        {
            data.panDirection = (PanDirection)EditorGUILayout.EnumPopup(
                "Pan Direction",
                data.panDirection
            );

            data.panDistance = EditorGUILayout.FloatField(
                "Pan Distance",
                data.panDistance
            );

            data.panTime = EditorGUILayout.FloatField(
                "Pan Time",
                data.panTime
            );
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(cameraControlTrigger);
        }
    }
}
#endif
