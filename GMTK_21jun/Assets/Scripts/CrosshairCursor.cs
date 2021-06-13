using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CrosshairCursor : MonoBehaviour
{
    public Sprite onReleaseSprite, onHoldDownSprite;
    new SpriteRenderer renderer;
    new Camera camera;

    private void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
        camera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Mouse0))
            renderer.sprite = onHoldDownSprite;
        else
            renderer.sprite = onReleaseSprite;

        transform.position = (Vector2)camera.ScreenToWorldPoint(Input.mousePosition);
        
    }
}
