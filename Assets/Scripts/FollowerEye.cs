using UnityEngine;

public class FollowerEye : MonoBehaviour
{
    private enum TargetType { Cursor, Transform }

    [SerializeField] private TargetType _targetType;
    [SerializeField] private RectTransform _iris;

    [Range(1.0f, 10.0f)]
    [SerializeField] private float _maxDistance = 4f;

    [SerializeField] private RectTransform _targetTransform = null;

    private RectTransform _eyeRect;
    private Canvas _canvas;

    private void Start()
    {
        _eyeRect = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();

        if(_targetType == TargetType.Transform && _targetTransform == null)
        {
            Debug.LogWarning("The eye's target type is a Transform but no target transform has been defined.");
        }    
    }

    private void Update()
    {
        Vector2 targetPos = GetTargetPosition();

        Vector2 direction = targetPos - (Vector2)_eyeRect.position;

        if(direction.magnitude > _maxDistance)
        {
            direction = direction.normalized * _maxDistance;
        }

        _iris.anchoredPosition = direction;
    }

    private Vector3 GetTargetPosition()
    {
        switch(_targetType)
        {
            case TargetType.Cursor:
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_eyeRect, Input.mousePosition, _canvas.worldCamera, out localPoint);
                return _eyeRect.TransformPoint(localPoint);

            case TargetType.Transform:
                return _targetTransform != null ? _targetTransform.position : Vector3.zero;

            default:
                return Vector3.zero;
        }
    }
}
