using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float movementSpeed = 10f;

    public void SetDirection(float angle)
    {
        var rot = Quaternion.Euler(0f, 0f, -angle);
        transform.rotation = rot;

        angle *= Mathf.Deg2Rad;
        var x = Mathf.Cos(angle);
        var y = Mathf.Sin(angle);
        var direction = new Vector2(x, y);
        StartCoroutine(CoFly(direction.normalized));
    }

    private IEnumerator CoFly(Vector3 direction)
    {
        var waitForFixedUpdate = new WaitForFixedUpdate();
        var delta = movementSpeed * Time.fixedDeltaTime;
        while (gameObject.activeSelf)
        {
            transform.position += direction * delta;
            yield return waitForFixedUpdate;
        }
    }

    public void Dispose()
    {
        gameObject.SetActive(false);
    }
}
