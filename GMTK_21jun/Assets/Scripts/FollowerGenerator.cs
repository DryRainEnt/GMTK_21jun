using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowerGenerator : MonoBehaviour
{
    public GameObject followerPrefab;

    [Header("Spawn Intaval")]
    [MinMaxSlider(0, 64, DataFields = true, FlexibleFields = false, Round = true)]
    public Vector2 spawnIntervalSeconds;

    [Header("Map Size")]
    [Min(1)]
    public int width;
    [Min(1)]
    public int height;

    [Header("Map Grid")]
    [Min(1)]
    public int mapGridSideX;
    [Min(1)]
    public int mapGridSideY;

    [Header("Generate Count Per Interval")]
    [Min(1)]
    public int cellCountPerInterval;
    public int minFollowerCount, maxFollowerCount;

    private Vector2 CellSize => new Vector2((float)width / mapGridSideX, (float)height / mapGridSideY);
    private Vector2 GridLeftTop => transform.position - new Vector3(width / 2.0f, height / 2.0f);

    private float currentIntervalSeconds = 0;

    Dictionary<Vector2, List<GameObject>> followerByCoord = new Dictionary<Vector2, List<GameObject>>();

    // Start is called before the first frame update
    void Start()
    {
        currentIntervalSeconds = Random.Range(spawnIntervalSeconds.x, spawnIntervalSeconds.y);
    }

    // Update is called once per frame
    void Update()
    {
        if (currentIntervalSeconds <= 0)
        {
            GenerateOnCells(cellCountPerInterval);

            currentIntervalSeconds = Random.Range(spawnIntervalSeconds.x, spawnIntervalSeconds.y);
        }

        currentIntervalSeconds -= Time.deltaTime;
    }

    private void GenerateOnCells(int count)
    {
        int maxLoopCount = mapGridSideX * mapGridSideY;
        int loopCount = 0;

        for (int i = 0; i < count; i++, loopCount++) 
        {
            bool isOkayToGenerate = false;

            if (loopCount >= maxLoopCount)
                break;

            Vector2 key = new Vector2(Random.Range(0, mapGridSideX), Random.Range(0, mapGridSideY));

            if (followerByCoord.ContainsKey(key))
                if (followerByCoord[key].Count != 0)
                    continue;
                else
                    isOkayToGenerate = true;
            else
                isOkayToGenerate = true;

            if(isOkayToGenerate)
                GenerateFollowers(Random.Range(minFollowerCount, maxFollowerCount), key);
        }
    }

    private void GenerateFollowers(int count, Vector2 coord)
    {
        List<GameObject> followers = new List<GameObject>();

        for (int i = 0; i < count; i++)
            if (ObjectPool.instance.TryGet(followerPrefab, out GameObject follower))
            {
                Rect rect = GetCellRectAtCoord((int)coord.x, (int)coord.y);

                follower.GetComponent<EntityBehaviour>().AddCollectedEventListener(this, coord);
                follower.transform.position = new Vector3(Random.Range(0, CellSize.x/2) + rect.x, Random.Range(0, CellSize.y/2) + rect.y);
                followers.Add(follower);
            }
    }

    public void CollectedFollowerOn(Vector2 coord, GameObject follower)
    {
        followerByCoord[coord].Remove(follower);
    }

    Rect GetCellRectAtCoord(int xCoord, int yCoord)
    {
        Vector2 cellPos = GridLeftTop + CellSize / 2;

                cellPos.x += CellSize.x * xCoord;
                cellPos.y += CellSize.y * yCoord;

        return new Rect(cellPos, CellSize);
    }

    private void OnDrawGizmos()
    {
        DrawGridLineX();
        DrawGridLineY();

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(width, height, 0));
    }

    private void DrawGridLineX()
    {
        Vector2 gridDrawPos1 = GridLeftTop;

        Gizmos.color = Color.magenta;
        for (int x = 0; x < mapGridSideX; x++)
        {
            Gizmos.DrawLine(gridDrawPos1, new Vector3(gridDrawPos1.x, gridDrawPos1.y + height));
            gridDrawPos1.x += CellSize.x;
        }
    }

    private void DrawGridLineY()
    {
        Vector2 gridDrawPos2 = GridLeftTop;

        Gizmos.color = Color.magenta;
        for (int y = 0; y < mapGridSideY; y++)
        {
            Gizmos.DrawLine(gridDrawPos2, new Vector3(gridDrawPos2.x + width, gridDrawPos2.y));
            gridDrawPos2.y += CellSize.y;
        }
    }
}
