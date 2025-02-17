using UnityEngine;
using UnityEngine.Rendering.Universal;

public class EyeAnimation : MonoBehaviour
{
    [SerializeField] private Sprite[] _defaultSprite;
    
    private float _nextCallTime;

    private Animator _animator;
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        int eyeType = Random.Range(0, 5);
        _spriteRenderer.sprite = _defaultSprite[eyeType];
        _animator.SetInteger("Variation", eyeType);
    }

    public void Update()
    {
        if(Time.time >= _nextCallTime)
        {
            SetTrigger("Animation");
            _nextCallTime = Time.time + Random.Range(5f, 10f);
        }
    }

    private void SetTrigger(string paramName)
    {
        _animator.SetTrigger(paramName);
    }
}
