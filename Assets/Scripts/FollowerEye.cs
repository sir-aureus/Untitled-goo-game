using UnityEngine;

public class FollowerEye : MonoBehaviour
{
    private enum TargetType { Cursor, Transform }

    [SerializeField] private TargetType _targetType;
    [SerializeField] private Transform _iris;

    [Range(0.1f, 1.0f)]
    [SerializeField] private float _maxDistance = 0.25f;

    [SerializeField] private Transform targetTransform = null;

    private void Start()
    {
        if(_targetType == TargetType.Transform && targetTransform == null)
        {
            Debug.LogWarning("The eye's target type is a Transform but no target transform has been defined.");
        }    
    }

    private void Update()
    {
        Vector3 direction = GetTargetPosition() - transform.position;

        if(direction.magnitude > _maxDistance)
        {
            direction = direction.normalized * _maxDistance;
        }

        _iris.position = transform.position + direction;
    }

    private Vector3 GetTargetPosition()
    {
        switch(_targetType)
        {
            case TargetType.Cursor:
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePosition.z = 0f;
                return mousePosition;

            case TargetType.Transform:
                return targetTransform != null ? targetTransform.position : Vector3.zero;

            default:
                return Vector3.zero;
        }
    }
}
