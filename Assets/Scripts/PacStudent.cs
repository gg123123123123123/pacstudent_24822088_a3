using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PacStudent : MonoBehaviour
{
    [Header("Refs")]
    public Animator anim;

    [Header("Grid Move")]
    public float tileSize = 1f;            // world units per tile
    public float tilesPerSecond = 6f;      // movement speed (tiles per second)
    public LayerMask wallMask;             // unused in auto demo
    public float castSkin = 0.02f;

    [Header("A3 Demo")]
    public bool a3AutoDemo = true;         // set TRUE for A3, FALSE later for WASD
    public Vector3 p1 = new Vector3(1f, -1f, 0f);
    public Vector3 p2 = new Vector3(6f, -1f, 0f);
    public Vector3 p3 = new Vector3(6f, -5f, 0f);
    public Vector3 p4 = new Vector3(1f, -5f, 0f);

    enum Dir { Down, Left, Right, Up }
    Dir facing = Dir.Right;

    bool isMoving = false;
    Rigidbody2D rb;
    BoxCollider2D col;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        if (!anim) anim = GetComponent<Animator>();

        // Physics: kinematic is cleanest for scripted motion
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void Start()
    {
        if (a3AutoDemo)
        {
            // snap to start corner and begin loop
            rb.position = new Vector2(p1.x, p1.y);
            StartCoroutine(AutoLoop());
        }
    }

    void Update()
    {
        if (a3AutoDemo) return; // ignore input in A3 demo

        if (isMoving) return;

        // --- WASD (kept for A4) ---
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))  TryStep(Vector2.right, Dir.Right);
        else if (Input.GetKeyDown(KeyCode.LeftArrow)  || Input.GetKeyDown(KeyCode.A)) TryStep(Vector2.left,  Dir.Left);
        else if (Input.GetKeyDown(KeyCode.UpArrow)    || Input.GetKeyDown(KeyCode.W)) TryStep(Vector2.up,    Dir.Up);
        else if (Input.GetKeyDown(KeyCode.DownArrow)  || Input.GetKeyDown(KeyCode.S)) TryStep(Vector2.down,  Dir.Down);
    }

    // --------- A3 Auto Path ---------
    IEnumerator AutoLoop()
    {
        // loop forever around the rectangle
        while (true)
        {
            yield return MoveTo(p2);
            yield return MoveTo(p3);
            yield return MoveTo(p4);
            yield return MoveTo(p1);
        }
    }

    IEnumerator MoveTo(Vector3 target)
    {
        isMoving = true;

        Vector2 from = rb.position;
        Vector2 to   = new Vector2(target.x, target.y);

        // choose facing for animation
        Vector2 delta = (to - from);
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            facing = (delta.x >= 0f) ? Dir.Right : Dir.Left;
        else
            facing = (delta.y >= 0f) ? Dir.Up : Dir.Down;

        PlayWalk(facing);

        // optional: a short move sfx
        if (AudioManager.Instance != null) AudioManager.Instance.PlayMoveSFX();

        float dist = Vector2.Distance(from, to);
        float unitsPerSecond = tilesPerSecond * tileSize;
        float duration = (unitsPerSecond <= 0f) ? 0f : dist / unitsPerSecond;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / duration);
            rb.MovePosition(Vector2.Lerp(from, to, u));
            yield return null;
        }
        rb.MovePosition(to);

        PlayIdle(facing);
        isMoving = false;
    }

    // --------- Original grid-step (kept for later) ---------
    void TryStep(Vector2 dir, Dir dirEnum)
    {
        Vector2 start = rb.position;
        Vector2 end = start + dir * tileSize;

        Vector2 boxSize = col.bounds.size - Vector3.one * castSkin;
        RaycastHit2D hit = Physics2D.BoxCast(start, boxSize, 0f, dir, tileSize, wallMask);
        if (hit.collider != null) { PlayIdle(dirEnum); return; }

        facing = dirEnum;
        StartCoroutine(MoveStep(start, end));
    }

    IEnumerator MoveStep(Vector2 from, Vector2 to)
    {
        isMoving = true;
        PlayWalk(facing);

        float t = 0f;
        float duration = 1f / tilesPerSecond;
        while (t < duration)
        {
            t += Time.deltaTime;
            rb.MovePosition(Vector2.Lerp(from, to, t / duration));
            yield return null;
        }
        rb.MovePosition(to);

        isMoving = false;
        PlayIdle(facing);
    }

    // ---- Animation helpers ----
    void PlayWalk(Dir d)
    {
        switch (d)
        {
            case Dir.Right: anim.Play("playerRight_walk"); break;
            case Dir.Left:  anim.Play("playerLeft_Walk");  break;
            case Dir.Up:    anim.Play("playerUp_Walk");    break;
            case Dir.Down:  anim.Play("playerDown_Walk");  break;
        }
    }

    void PlayIdle(Dir d)
    {
        switch (d)
        {
            case Dir.Right: anim.Play("playerRight"); break;
            case Dir.Left:  anim.Play("playerLeft");  break;
            case Dir.Up:    anim.Play("playerUp");    break;
            case Dir.Down:  anim.Play("playerDown");  break;
        }
    }

    public void Die()
    {
        switch (facing)
        {
            case Dir.Right: anim.Play("playerRight_dead"); break;
            case Dir.Left:  anim.Play("playerLeft_dead");  break;
            case Dir.Up:    anim.Play("playerUp_dead");    break;
            case Dir.Down:  anim.Play("playerDown_dead");  break;
        }
        enabled = false;
    }
}
