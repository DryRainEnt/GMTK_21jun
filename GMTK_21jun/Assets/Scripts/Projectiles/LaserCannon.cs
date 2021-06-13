using System.Collections;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using System;

public class LaserCannon : MonoBehaviour
{
    public float startDelay;

    public bool disableShortLaser;
    public float shortLaserCooldown;
    public float shortLaserDelay;
    public float shortLaserDuration;
    public float shortLaserWidth;
    public int shortLaserDamage;

    public bool disableLongLaser;
    public float longLaserCooldown;
    public float longLaserDelay;
    public float longLaserLifeTime;
    public float longLaserDuration;
    public float longLaserWidth;
    public int longLaserDamage;

    public float laserLength = 100;

    public Transform target;

    public GameObject laserPrefab;
    public AudioSource source;

    private void Awake()
    {
        if (!disableShortLaser)
        {
            StartCoroutine(CoLaunchShortLaser());
        }
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
            renderer.color = Color.red;

            var collider = laserObject.GetComponent<BoxCollider2D>();
            var colliderSizeX = collider.size.x;
            collider.size = new Vector2(colliderSizeX, laserLength);
            collider.enabled = false;

            laserObject.transform.localScale = new Vector3(0f, 1f, 0f);
            laserObject.transform.DOScaleX(shortLaserWidth, shortLaserDelay);
            yield return new WaitForSeconds(shortLaserDelay);

            var size = new Vector2(laserLength, shortLaserWidth * (width / ppu));
            var blockObject = DealDamage(
                laserObject.transform.position,
                size,
                direction);
            if (blockObject)
            {
                var collector = blockObject.GetComponent<VirtualCollectorBehaviour>();
                var isAlive = collector.GetDamage(shortLaserDamage);

                if (isAlive)
                {
                    var length = Vector2.Distance(blockObject.position, transform.position);
                    laserObject.transform.position = transform.position + (direction * length * 0.5f);
                    laserObject.transform.localScale = new Vector3(shortLaserWidth, 1f, 1f);
                    renderer.size = new Vector2(1f, length);
                    collider.size = new Vector2(colliderSizeX, length);
                }
            }

            source.PlayOneShot(source.clip, 0.5f);
            collider.enabled = true;
            renderer.color = Color.white;
            laserObject.transform.DOScaleX(0f, shortLaserDuration);
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
            renderer.color = Color.red;

            var collider = laserObject.GetComponent<BoxCollider2D>();
            var colliderSizeX = collider.size.x;
            collider.size = new Vector2(colliderSizeX, laserLength);
            collider.enabled = false;

            laserObject.transform.localScale = new Vector3(0f, 1f, 0f);
            laserObject.transform.DOScaleX(longLaserWidth, longLaserDelay);
            yield return new WaitForSeconds(longLaserDelay);

            var size = new Vector2(laserLength, longLaserWidth * (width / ppu));
            var blockObject = DealDamage(
                laserObject.transform.position,
                size,
                direction);
            if (blockObject)
            {
                var collector = blockObject.GetComponent<VirtualCollectorBehaviour>();
                var isAlive = collector.GetDamage(longLaserDamage);

                if (isAlive)
                {
                    var length = Vector2.Distance(blockObject.position, transform.position);
                    laserObject.transform.position = transform.position + (direction * length * 0.5f);
                    laserObject.transform.localScale = new Vector3(longLaserWidth, 1f, 1f);
                    renderer.size = new Vector2(1f, length);
                    collider.size = new Vector2(colliderSizeX, length);
                }
            }

            collider.enabled = true;
            renderer.color = Color.white;
            yield return new WaitForSeconds(longLaserLifeTime);

            laserObject.transform.DOScaleX(0f, longLaserDuration);
            yield return new WaitForSeconds(longLaserDuration);
            laserObject.SetActive(false);
        }
    }

    private Transform DealDamage(Vector2 pos, Vector2 size, Vector2 direction)
    {
        var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        size = new Vector3(size.x, size.y, 1f);

        var blockObject = Physics2D.OverlapBoxAll(pos, size, angle)
            .FirstOrDefault(x => x.CompareTag("BlockLaser"));

        return blockObject is null ? null : blockObject.transform;
    }

    private Vector3 SetLaserTransform(GameObject laserObject, float laserLength, float ppu)
    {
        var direction = (target.position - transform.position).normalized;
        laserObject.transform.position = transform.position + (direction * laserLength * 0.5f);
        laserObject.transform.up = -direction;
        return direction;
    }
}
