using DG.Tweening;
using UnityEngine;

public class Missile : MonoBehaviour
{
    public Transform projectile = null;
    public SpriteRenderer warningCircleRenderer = null;
    public Color warningCircleColor;
    public float arrivalTime;
    public float groupingRadius;

    public AudioClip warningClip;
    public AudioClip explosionClip;
    public GameObject explosionPrefab;

    public void SetTarget(Vector3 startPosition, Vector3 targetPosition)
    {
        AudioSource.PlayClipAtPoint(warningClip, targetPosition);
        targetPosition.x += Random.Range(-groupingRadius, groupingRadius);
        targetPosition.y += Random.Range(-groupingRadius, groupingRadius);
        transform.position = targetPosition;
        projectile.position = startPosition;
        warningCircleColor.a = 0;
        warningCircleRenderer.color = warningCircleColor;

        Vector2 diff = targetPosition - startPosition;
        var isPositiveY = diff.y < 0;
        var ease = isPositiveY ? Ease.InBack : Ease.OutBack;

        var yTween = projectile.DOMoveY(targetPosition.y, arrivalTime);
        yTween.SetEase(ease);
        yTween.onComplete = () =>
        {
            ObjectPool.instance.TryGet(explosionPrefab, out var fx);
            fx.transform.position = targetPosition;
            //fx.transform.localScale = new Vector3(2, 2f, 2f);

            DamageCheck(targetPosition);
            AudioSource.PlayClipAtPoint(explosionClip, targetPosition);
        };
        
        var xTween = projectile.DOMoveX(targetPosition.x, arrivalTime);
        xTween.SetEase(Ease.InQuad);

        warningCircleRenderer.DOFade(1f, arrivalTime);
    }

    void DamageCheck(Vector3 pos)
    {
        var hits = Physics2D.OverlapCircleAll(pos, 0.5f);
        foreach (var hit in hits)
        {
            if (hit.gameObject.layer != LayerMask.NameToLayer("Enemy"))
                hit.GetComponent<ICrasher>()?.GetHit(3);
        }
        gameObject.SetActive(false);
    }
}
