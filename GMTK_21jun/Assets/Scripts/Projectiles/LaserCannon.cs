using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LaserCannon : MonoBehaviour
{
    public float startDelay;

    public float shortLaserCooldown;
    public float shortLaserDelay;
    public float shortLaserDuration;
    public float shortlaserWidth = 30f;

    public float longLaserCooldown;
    public float longLaserDelay;
    public float longLaserDuration;
    public float longLaserWidth = 80f;

    public float laserLength = 5000f;

    public Transform target;

    public GameObject laserPrefab;

    private void Awake()
    {
        StartCoroutine(CoLaunchShortLaser());
        StartCoroutine(CoLaunchLongLaser());
    }

    private IEnumerator CoLaunchShortLaser()
    {
        yield return new WaitForSeconds(startDelay);
        var waitSec = new WaitForSeconds(shortLaserCooldown);
        while (gameObject.activeSelf)
        {
            yield return waitSec;
            if (!ObjectPool.instance.TryGet(laserPrefab, out var laserObject))
            {
                continue;
            }

            var direction = SetLaserTransform(laserObject, laserLength);
            var renderer = laserObject.GetComponent<SpriteRenderer>();
            var ppu = renderer.sprite.pixelsPerUnit;

            renderer.color = Color.white;
            laserObject.transform.localScale = new Vector3(laserLength, 0f, 0f);
            laserObject.transform.DOScaleY(shortlaserWidth, shortLaserDelay);
            yield return new WaitForSeconds(shortLaserDelay);

            var interruptedCount = GetInterruptedLaserCount(
                laserObject.transform.position,
                laserObject.transform.localScale,
                direction,
                ppu);
            Debug.Log(interruptedCount);
            if (interruptedCount > 0)
            {
                var length = Vector2.Distance(target.position, transform.position);
                laserObject.transform.position = transform.position + (direction * length * 0.5f);
                // Multiply PPU
                laserObject.transform.localScale = new Vector3(length * ppu, shortlaserWidth, 1f);
            }
            renderer.color = Color.red;
            renderer.DOFade(0f, shortLaserDuration);
            yield return new WaitForSeconds(shortLaserDuration);
            laserObject.SetActive(false);
        }
    }

    private IEnumerator CoLaunchLongLaser()
    {
        yield return new WaitForSeconds(startDelay);
        var waitSec = new WaitForSeconds(longLaserCooldown);
        while (gameObject.activeSelf)
        {
            yield return waitSec;
            if (!ObjectPool.instance.TryGet(laserPrefab, out var laserObject))
            {
                continue;
            }

            var direction = SetLaserTransform(laserObject, laserLength);
            var renderer = laserObject.GetComponent<SpriteRenderer>();
            var ppu = renderer.sprite.pixelsPerUnit;

            renderer.color = Color.white;
            laserObject.transform.localScale = new Vector3(laserLength, 0f, 0f);
            laserObject.transform.DOScaleY(longLaserWidth, longLaserDelay);
            yield return new WaitForSeconds(longLaserDelay);

            var interruptedCount = GetInterruptedLaserCount(
                laserObject.transform.position,
                laserObject.transform.localScale,
                direction,
                ppu);
            Debug.Log(interruptedCount);
            if (interruptedCount > 0)
            {
                var length = Vector2.Distance(target.position, transform.position);
                laserObject.transform.position = transform.position + (direction * length * 0.5f);
                // Multiply PPU
                laserObject.transform.localScale = new Vector3(length * ppu, longLaserWidth, 1f);
            }
            renderer.color = Color.red;
            renderer.DOFade(0f, longLaserDuration);
            yield return new WaitForSeconds(longLaserDuration);
            laserObject.SetActive(false);
        }
    }

    private int GetInterruptedLaserCount(Vector2 pos, Vector2 size, Vector2 direction, float pixelPerUnit)
    {
        var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        size = new Vector3(size.x / pixelPerUnit, size.y / pixelPerUnit, 1f);
        return Physics2D.OverlapBoxAll(pos, size, angle).Length;
    }

    private Vector3 SetLaserTransform(GameObject laserObject, float laserLength)
    {
        var direction = (target.position - transform.position).normalized;
        // 레이저 길이 절반 * (1 / 100) * (100 / 32) <- PPU
        laserObject.transform.position = transform.position + (direction * laserLength * 0.5f * 0.03125f);
        laserObject.transform.right = direction;
        return direction;
    }
}
