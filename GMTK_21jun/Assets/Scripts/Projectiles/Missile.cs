using DG.Tweening;
using UnityEngine;

public class Missile : MonoBehaviour
{
    public Transform projectile = null;
    public float arrivalTime;
    public float groupingRadius;

    public void SetTarget(Vector3 startPosition, Vector3 targetPosition)
    {
        targetPosition.x += Random.Range(-groupingRadius, groupingRadius);
        targetPosition.y += Random.Range(-groupingRadius, groupingRadius);
        transform.position = targetPosition;
        projectile.position = startPosition;

        Vector2 diff = targetPosition - startPosition;
        var isPositiveY = diff.y < 0;
        var ease = isPositiveY ? Ease.InBack : Ease.OutBack;

        var yTween = projectile.DOMoveY(targetPosition.y, arrivalTime);
        yTween.SetEase(ease);
        yTween.onComplete = () => gameObject.SetActive(false);

        var xTween = projectile.DOMoveX(targetPosition.x, arrivalTime);
        xTween.SetEase(Ease.InQuad);
    }
}
