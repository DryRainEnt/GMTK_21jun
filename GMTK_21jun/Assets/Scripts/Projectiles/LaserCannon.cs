using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;

public class LaserCannon : MonoBehaviour
{
    public float startDelay;

    public float shortLaserCooldown;
    public float shortLaserDelay;
    public float shortLaserDuration;
    public float shortlaserWidth;

    public float longLaserCooldown;
    public float longLaserDelay;
    public float longLaserDuration;
    public float longLaserWidth;

    public float laserLength = 100;

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

            var renderer = laserObject.GetComponent<SpriteRenderer>();
            var ppu = renderer.sprite.pixelsPerUnit;
            var spriteBorder = renderer.sprite.border;
            var width = renderer.sprite.rect.width - (spriteBorder.x + spriteBorder.z);
            var direction = SetLaserTransform(laserObject, laserLength, ppu);
            renderer.size = new Vector2(1f, laserLength);

            renderer.color = Color.white;

            laserObject.transform.localScale = new Vector3(0f, 1f, 0f);
            laserObject.transform.DOScaleX(shortlaserWidth, shortLaserDelay);
            yield return new WaitForSeconds(shortLaserDelay);

            var size = new Vector2(laserLength, shortlaserWidth * (width / ppu));
            var isInterrupted = CheckLaserInterrupted(
                laserObject.transform.position,
                size,
                direction);
            if (isInterrupted)
            {
                Debug.Log("interrupt");
                var length = Vector2.Distance(target.position, transform.position);
                laserObject.transform.position = transform.position + (direction * length * 0.5f);
                laserObject.transform.localScale = new Vector3(shortlaserWidth, 1f, 1f);
                renderer.size = new Vector2(1f, length);
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


            var renderer = laserObject.GetComponent<SpriteRenderer>();
            var ppu = renderer.sprite.pixelsPerUnit;
            var spriteBorder = renderer.sprite.border;
            var width = renderer.sprite.rect.width - (spriteBorder.x + spriteBorder.z);
            var direction = SetLaserTransform(laserObject, laserLength, ppu);
            renderer.size = new Vector2(1f, laserLength);

            renderer.color = Color.white;
            laserObject.transform.localScale = new Vector3(0f, 1f, 0f);
            laserObject.transform.DOScaleX(longLaserWidth, longLaserDelay);
            yield return new WaitForSeconds(longLaserDelay);

            var size = new Vector2(laserLength, longLaserWidth * (width / ppu));
            var isInterrupted = CheckLaserInterrupted(
                laserObject.transform.position,
                size,
                direction);
            if (isInterrupted)
            {
                var length = Vector2.Distance(target.position, transform.position);
                laserObject.transform.position = transform.position + (direction * length * 0.5f);
                laserObject.transform.localScale = new Vector3(longLaserWidth, 1f, 1f);
                renderer.size = new Vector2(1f, length);
            }
            renderer.color = Color.red;
            renderer.DOFade(0f, longLaserDuration);
            yield return new WaitForSeconds(longLaserDuration);
            laserObject.SetActive(false);
        }
    }

    private bool CheckLaserInterrupted(Vector2 pos, Vector2 size, Vector2 direction)
    {
        var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        size = new Vector3(size.x, size.y, 1f);
        return Physics2D.OverlapBoxAll(pos, size, angle)
            .Where(x => x.CompareTag("BlockLaser"))
            .Any();
    }

    private Vector3 SetLaserTransform(GameObject laserObject, float laserLength, float ppu)
    {
        var direction = (target.position - transform.position).normalized;
        laserObject.transform.position = transform.position + (direction * laserLength * 0.5f);
        laserObject.transform.up = -direction;
        return direction;
    }
}
