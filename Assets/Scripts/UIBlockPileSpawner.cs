using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIBlockPileSpawner : MonoBehaviour
{
    [Header("References")]
    public RectTransform canvasRoot;
    public RectTransform pileRoot;
    public GameObject blockPrefab;

    [Header("Grid")]
    public int columns = 8;
    public float blockSize = 64f;

    [Header("Spawn")]
    public Sprite[] blockSprites;   // 19 PNGs
    public float spawnInterval = 0.12f;
    public float fallSpeed = 900f;

    private int[] columnHeights;
    private float spawnY;
    private float startX;

    void Start()
    {
        columnHeights = new int[columns];

        spawnY = canvasRoot.rect.height / 2 + blockSize;
        float totalWidth = columns * blockSize;
        startX = -totalWidth / 2 + blockSize / 2;

        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            SpawnBlock();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnBlock()
    {
        int column = Random.Range(0, columns);
        int row = columnHeights[column];

        float x = startX + column * blockSize;
        float targetY = row * blockSize;

        GameObject block = Instantiate(blockPrefab, pileRoot);
        RectTransform rect = block.GetComponent<RectTransform>();
        Image img = block.GetComponent<Image>();

        // SAFETY CHECK (fixes missing sprite issue)
        if (img != null && blockSprites.Length > 0)
        {
            img.sprite = blockSprites[Random.Range(0, blockSprites.Length)];
            img.enabled = true;
        }

        rect.anchoredPosition = new Vector2(x, spawnY);

        UIFallingBlock fb = block.GetComponent<UIFallingBlock>();
        fb.fallSpeed = fallSpeed;
        fb.Init(targetY);

        columnHeights[column]++;
    }
}
