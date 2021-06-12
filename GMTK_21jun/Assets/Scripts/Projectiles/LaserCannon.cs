using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserCannon : MonoBehaviour
{
    public float startDelay;
    public float shortLaserDelay;
    public float longLaserDelay;

    public Transform target;

    private void Awake()
    {
        StartCoroutine(CoLaunchShortLaser());
        StartCoroutine(CoLaunchLongLaser());
    }

    private IEnumerator CoLaunchShortLaser()
    {
        yield return new WaitForSeconds(startDelay);
        var waitSec = new WaitForSeconds(shortLaserDelay);
        while (gameObject.activeSelf)
        {
            yield return waitSec;


        }
    }

    private IEnumerator CoLaunchLongLaser()
    {
        yield return new WaitForSeconds(startDelay);
        var waitSec = new WaitForSeconds(longLaserDelay);
        while (gameObject.activeSelf)
        {
            yield return waitSec;


        }
    }
}
