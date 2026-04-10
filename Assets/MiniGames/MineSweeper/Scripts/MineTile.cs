using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class MineTile : MonoBehaviour, IPointerClickHandler
{
    [Header("Tile State")]
    public bool isBomb;
    public bool isRevealed;
    public bool isFlagged;
    public int adjacentBombs;

    [Header("Visuals")]
    public GameObject unviewedVisual;
    public GameObject viewedVisual;
    public GameObject bombVisual;
    public GameObject flagVisual;
    public TextMeshPro numberText;

    [Header("Checkerboard Settings")]
    [Range(0.5f, 1f)]
    public float darkenMultiplier = 0.85f;

    private BoardManager board;
    private int boardX;
    private int boardY;

    public void Init(BoardManager manager, int x, int y)
    {
        board = manager;
        boardX = x;
        boardY = y;

        isBomb = false;
        isRevealed = false;
        isFlagged = false;
        adjacentBombs = 0;

        unviewedVisual.SetActive(true);
        viewedVisual.SetActive(false);
        bombVisual.SetActive(false);
        flagVisual.SetActive(false);
        numberText.text = "";

        ApplyCheckerboard(x, y);
    }

    void ApplyCheckerboard(int x, int y)
    {
        // Only darken alternate tiles
        if ((x + y) % 2 != 0)
            return;

        ApplyDarken(unviewedVisual);
        ApplyDarken(viewedVisual);
    }

    void ApplyDarken(GameObject visual)
    {
        SpriteRenderer sr = visual.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        Color baseColor = sr.color;
        sr.color = new Color(
            baseColor.r * darkenMultiplier,
            baseColor.g * darkenMultiplier,
            baseColor.b * darkenMultiplier,
            baseColor.a
        );
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (board.gameOver) return;

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            ToggleFlag();
            return;
        }

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (isRevealed || isFlagged) return;
            Reveal();
        }
    }

    void ToggleFlag()
    {
        if (isRevealed) return;

        isFlagged = !isFlagged;
        flagVisual.SetActive(isFlagged);
        board.UpdateFlagCount(isFlagged ? 1 : -1);
    }

    public void SetBomb()
    {
        isBomb = true;
    }

    public void SetAdjacentBombs(int count)
    {
        adjacentBombs = count;
    }

    public void Reveal()
    {
        isRevealed = true;
        unviewedVisual.SetActive(false);
        flagVisual.SetActive(false);

        if (isBomb)
        {
            bombVisual.SetActive(true);
            board.OnBombClicked();
        }
        else
        {
            viewedVisual.SetActive(true);

            if (adjacentBombs > 0)
            {
                numberText.text = adjacentBombs.ToString();
                ApplyNumberColor();
            }
            else
            {
                board.FloodFill(boardX, boardY);
            }
        }
    }

    public void RevealBombOnly()
    {
        if (isBomb)
        {
            unviewedVisual.SetActive(false);
            bombVisual.SetActive(true);
        }
    }

    void ApplyNumberColor()
    {
        switch (adjacentBombs)
        {
            case 1: numberText.color = Color.blue; break;
            case 2: numberText.color = Color.green; break;
            case 3: numberText.color = Color.red; break;
            case 4: numberText.color = new Color(0f, 0f, 0.5f); break;
            case 5: numberText.color = new Color(0.5f, 0f, 0f); break;
            case 6: numberText.color = Color.cyan; break;
            case 7: numberText.color = Color.black; break;
            case 8: numberText.color = Color.gray; break;
        }
    }
}
