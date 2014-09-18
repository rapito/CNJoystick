using UnityEngine;
using System.Collections;

public class CameraFollowExample : MonoBehaviour
{
    public CNJoystick RotateJoystick;
    public float RotationSpeed = 10f;

    private Transform _transformCache;
    private Transform _parentTransformCache;

    // Use this for initialization
    void Start()
    {
        _transformCache = GetComponent<Transform>();
        _parentTransformCache = _transformCache.parent;
    }

    // Update is called once per frame
    void Update()
    {
        if (RotateJoystick != null)
        {
            float rotation = RotateJoystick.GetAxis("Horizontal");
            _transformCache.RotateAround(_parentTransformCache.position, Vector3.up, rotation * RotationSpeed * Time.deltaTime);
        }

        /*
        if (Target != null)
        {
            if (RotateJoystick != null)
            {
                float rotation = RotateJoystick.GetAxis("Horizontal");
                _transformCache.RotateAround(Target.position, Vector3.up, rotation * RotationSpeed * Time.deltaTime);
            }
            _transformDifference = _transformCache.position - Target.position;

            _transformCache.position = Target.position + _transformDifference.normalized * _transformDistance;
        }
        */
    }
}
