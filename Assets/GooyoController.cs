using UnityEngine;
using System.Collections.Generic;

public enum GameState
{
    Spawn,
    Dropping,
    Clearing,
    Falling,
    GameOver
}

public class GooyoController : MonoBehaviour
{
    // settings
    public GameObject polyominoPrefab;
    public GameObject background;

    public int tilesHoriz = 16;
    public int tilesVert = 28;

    public float tileScale = 0.75f;

    public float dropSpeed = 2.0f;

    // runtime state
    protected bool paused = false;

    protected GameState gameState = GameState.Spawn;

    protected List<Polyomino> gamePieces = new List<Polyomino>();
    protected Polyomino[,] gameGrid;
    protected Polyomino currentPolyomino;

    protected Vector3 gridOffset;
    protected Vector3 gridSpacingX, gridSpacingY;

    protected float gameTick = 0.0f;

    public Polyomino[,] getGameGrid()
    {
        return this.gameGrid;
    }

    public Vector3 getGridOffset()
    {
        return this.gridOffset;
    }

    public Vector3 getGridSpacingX()
    {
        return this.gridSpacingX;
    }

    public Vector3 getGridSpacingY()
    {
        return this.gridSpacingY;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.gameGrid = new Polyomino[this.tilesHoriz, this.tilesVert];
        for (int x = 0; x < this.tilesHoriz; x++)
        {
            for (int y = 0; y < this.tilesVert; y++)
            {
                this.gameGrid[x, y] = null;
            }
        }
        this.gridOffset = new Vector3(-this.tilesHoriz * tileScale * 0.5f + 0.25f, -this.tilesVert * tileScale * 0.5f + 0.25f, 0);
        this.gridSpacingX = new Vector3(tileScale, 0, 0);
        this.gridSpacingY = new Vector3(0, tileScale, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (this.gameState == GameState.Spawn)
        {
            this.currentPolyomino = Instantiate(this.polyominoPrefab, background.transform).GetComponent<Polyomino>();
            this.currentPolyomino.controller = this;
            this.currentPolyomino.init();
            this.currentPolyomino.falling = true;
            this.currentPolyomino.setRandomShape();
            this.gamePieces.Add(this.currentPolyomino);
            int x = this.tilesHoriz/2 - this.currentPolyomino.getWidth()/2;
            int y = this.tilesVert - this.currentPolyomino.getHeight();
            Debug.Log("width: " + this.currentPolyomino.getWidth() + ", height: " + this.currentPolyomino.getHeight());
            Debug.Log("x: " + x + ", y: " + y);
            bool hasSpace = this.currentPolyomino.setGridPosition(x, y);
            if (!hasSpace)
            {
                this.gameState = GameState.GameOver;
                this.currentPolyomino.setGridPosition(x, y, true);
                Debug.Log("game over");
            }
            else
            {
                this.gameState = GameState.Dropping;
            }
        }
        else if (this.gameState == GameState.Dropping)
        {
            this.gameTick += Time.deltaTime;

            if (this.gameTick >= 1/this.dropSpeed)
            {
                this.gameTick -= 1/this.dropSpeed;
                bool landed = !this.currentPolyomino.fallOneTile();
                if (landed)
                {
                    this.gameState = GameState.Spawn;
                }
                Debug.Log("tick");
            }
            // this.currentPolyomino.transform.position += new Vector3(0, -this.dropSpeed * Time.deltaTime, 0);
            // if (this.currentPolyomino.transform.position.y < 0)
            // {
            //     this.gameState = GameState.Falling;
            // }
        }
        // else if (this.gameState == GameState.Falling)
        // {
        //     this.currentPolyomino.fallOneTile(this.gameGrid);
        //     this.gameState = GameState.Dropping;
        // }
    }
}
