using UnityEngine;
using System.Collections.Generic;
using System;
using TMPro;

public enum GameState
{
    Spawn,
    Holding,
    Dropping,
    Clearing,
    Falling,
    GameOver
}

public class GooyoController : MonoBehaviour
{
    // settings
    public GameObject[] polyominoPrefabs;
    public GameObject background;
    public GameObject nextPreview;
    public GameObject pauseMenu;

    public TextMeshProUGUI scoreText;

    public Camera sceneCamera;

    public AudioSource[] landSounds;
    public AudioSource[] comboSounds;
    public AudioLowPassFilter lowPassFilter;

    public int tilesHoriz = 16;
    public int tilesVert = 28;

    public float tileScale = 0.75f;

    public float dropSpeed = 2.0f;
    public float clearSpeed = 2.0f;

    public float holdTimeLimit = 1.5f;

    public int maxColors = 5;
    
    public int shuffleColorCount = 3;

    public bool disableClicks = false;

    // runtime state
    protected bool paused = false;

    protected GameState gameState = GameState.Spawn;

    protected List<Polyomino> gamePieces = new List<Polyomino>();
    protected Polyomino[,] gameGrid;
    protected Polyomino currentPolyomino;

    protected Vector3 gridOffset;
    protected Vector3 gridSpacingX, gridSpacingY;

    protected float gameTick = 0.0f;

    protected List<Polyomino> polyQueue;

    protected int combo = 0;
    protected int score = 0;

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
    void Awake()
    {
        this.gameGrid = new Polyomino[this.tilesHoriz, this.tilesVert];
        for (int x = 0; x < this.tilesHoriz; x++)
        {
            for (int y = 0; y < this.tilesVert; y++)
            {
                this.gameGrid[x, y] = null;
            }
        }
        this.gridOffset = new Vector3(-this.tilesHoriz * tileScale * 0.5f + 0.5f, -this.tilesVert * tileScale * 0.5f + 0.5f, 0);
        this.gridSpacingX = new Vector3(tileScale, 0, 0);
        this.gridSpacingY = new Vector3(0, tileScale, 0);

        this.maxColors = 3 + (int)DifficultyTracker.CurrentDifficulty;
        
        this.polyQueue = new List<Polyomino>();
        this.generateNewQueue();
        this.disableClicks = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (this.paused)
        {
            return;
        }

        if (this.gameState == GameState.Spawn)
        {
            this.currentPolyomino = this.polyQueue[0];
            this.currentPolyomino.gameObject.SetActive(true);
            this.currentPolyomino.transform.SetParent(this.background.transform);
            this.currentPolyomino.falling = true;
            this.gamePieces.Add(this.currentPolyomino);
            int x = this.getGridXFromMouse();
            int y = this.tilesVert - this.currentPolyomino.getHeight();

            this.polyQueue.RemoveAt(0);

            if (this.polyQueue.Count == 0)
            {
                this.generateNewQueue();
            }
            Polyomino next = this.polyQueue[0];
            next.transform.SetParent(this.nextPreview.transform);
            next.gameObject.SetActive(true);
            next.transform.SetLocalPositionAndRotation(new Vector3((-next.getMinX() - next.getWidth() * 0.5f) * this.gridSpacingX.x + 0.5f,
                (-next.getMinY() - next.getHeight() * 0.5f) * this.gridSpacingY.y + 0.5f,
                0f), Quaternion.identity);
          
            this.currentPolyomino.setVisualPosition(x, y);
            this.gameState = GameState.Holding;
            this.gameTick = 0;
        }
        else if (this.gameState == GameState.Holding)
        {
            this.gameTick += Math.Min(Time.deltaTime, 1f/30f);
            bool click = (this.gameTick >= this.holdTimeLimit && this.holdTimeLimit > 0)
                || (!this.disableClicks && Input.GetMouseButtonDown(0));

            int x = this.getGridXFromMouse();
            int y = this.tilesVert - this.currentPolyomino.getHeight();
            if (this.currentPolyomino.canSetGridPosition(x, y) || !this.currentPolyomino.canSetGridPosition(this.currentPolyomino.getGridX(), this.currentPolyomino.getGridY()))
            {
                this.currentPolyomino.setVisualPosition(x, y);
            }

            if (click)
            {
                this.gameTick = 0;
                bool hasSpace = this.currentPolyomino.setGridPosition(this.currentPolyomino.getGridX(), this.currentPolyomino.getGridY());
                if (!hasSpace)
                {
                    this.gameState = GameState.GameOver;
                    this.currentPolyomino.setGridPosition(x, y, true);
                    this.currentPolyomino.falling = false;
                    Debug.Log("game over");
                }
                else
                {
                    this.gameState = GameState.Dropping;
                }
            }
        }
        else if (this.gameState == GameState.Dropping)
        {
            this.gameTick += Math.Min(Time.deltaTime, 1f/30f);

            if (this.gameTick >= 1/this.dropSpeed)
            {
                this.gameTick -= 1/this.dropSpeed;
                this.currentPolyomino.fallOneTile();
                bool landed = !this.currentPolyomino.canFallOneTile();
                if (landed)
                {
                    this.playRandomSound(this.landSounds);
                    this.currentPolyomino.falling = false;
                    this.combo = 0;
                    bool didClear = this.checkForClears();
                    if (didClear)
                    {
                        this.gameState = GameState.Clearing;
                    }
                    else
                    {
                        this.gameState = GameState.Spawn;
                    }
                }
            }

            foreach (Polyomino piece in this.gamePieces)
            {
                if (piece.falling)
                {
                    float lerp = this.gameTick * this.dropSpeed;
                    piece.applyGridPosition();
                    piece.transform.position -= this.gridSpacingY *lerp;
                }
            }
        }
        else if (this.gameState == GameState.Clearing)
        {
            this.gameTick += Math.Min(Time.deltaTime, 1f/30f);
            if (this.gameTick >= 1/this.clearSpeed)
            {
                this.gameTick -= 1/this.clearSpeed;
                bool shouldFall = this.checkForUnsupported();
                if (shouldFall)
                {
                    this.gameState = GameState.Falling;
                }
                else
                {
                    this.gameState = GameState.Spawn;
                }
            }
        }
        else if (this.gameState == GameState.Falling)
        {
            this.gameTick += Math.Min(Time.deltaTime, 1f/30f);
            if (this.gameTick >= 1/this.dropSpeed)
            {
                this.gameTick -= 1/this.dropSpeed;
            
                bool stillFalling = false;
                bool anyLanded = true;
                bool firstIteration = true;

                while (anyLanded)
                {
                    anyLanded = false;
                    foreach (Polyomino piece in this.gamePieces)
                    {
                        if (piece.falling)
                        {
                            if (piece.canFallOneTile() && firstIteration)
                            {
                                piece.fallOneTile();
                            }

                            if (!piece.canFallOneTile())
                            {
                                piece.falling = false;
                                anyLanded = true;
                                this.playRandomSound(this.landSounds);
                            }
                            else
                            {
                                stillFalling = true;
                            }
                        }
                    }
                    firstIteration = false;
                }

                if (!stillFalling)
                {
                    bool didClear = this.checkForClears();
                    if (didClear)
                    {
                        this.gameState = GameState.Clearing;
                    }
                    else
                    {
                        this.gameState = GameState.Spawn;
                    }
                }
            }
            // this.currentPolyomino.fallOneTile(this.gameGrid);
            // this.gameState = GameState.Dropping;

            foreach (Polyomino piece in this.gamePieces)
            {
                if (piece.falling)
                {
                    float lerp = this.gameTick * this.dropSpeed;
                    piece.applyGridPosition();
                    piece.transform.position -= this.gridSpacingY *lerp;
                }
            }
        }
    }

    protected void generateNewQueue()
    {
        this.polyQueue.Clear();
        int numColors = Math.Min(this.maxColors, this.polyominoPrefabs.GetLength(0));
        Debug.Log("colors: " + numColors);

        for (int i = 0; i < numColors; i++)
        {
            for (int j = 0; j < this.shuffleColorCount; j++)
            {
                Polyomino poly = Instantiate(this.polyominoPrefabs[i], null).GetComponent<Polyomino>();
                this.polyQueue.Add(poly);

                poly.controller = this;
                poly.init();
                poly.setRandomShape();
                poly.gameObject.SetActive(false);
            }
        }
        
        int count = numColors * this.shuffleColorCount;

        for (int i = 0; i < count - 1; i++)
        {
            int newIndex = UnityEngine.Random.Range(0, count);
            Polyomino temp = this.polyQueue[i];
            this.polyQueue[i] = this.polyQueue[newIndex];
            this.polyQueue[newIndex] = temp;
        }
    }

    protected int getGridXFromMouse(bool adjustToFit = true)
    {
        Vector3 pos = sceneCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, sceneCamera.pixelHeight - Input.mousePosition.y, 0));

        pos = this.background.transform.InverseTransformPoint(pos);

        float width = background.GetComponent<SpriteRenderer>().size.x;

        int posX = (int)Math.Clamp(Math.Round((pos.x + width * 0.5f) * this.tilesHoriz / width - (this.currentPolyomino.getMinX() + this.currentPolyomino.getMaxX()) * 0.5f - 0.5f),
            -this.currentPolyomino.getMinX(),
            this.tilesHoriz - this.currentPolyomino.getMaxX()-1);

        return posX;
    }

    public bool checkForClears()
    {
        HashSet<Polyomino> toRemove = new HashSet<Polyomino>();
        foreach (Polyomino piece in this.gamePieces)
        {
            HashSet<Polyomino> adjacent = piece.getAdjacentMatches();
            if (adjacent.Count > 1)
            {
                toRemove.Add(piece);
                toRemove.UnionWith(adjacent);
            }
        }

        this.combo += toRemove.Count - 2;

        foreach (Polyomino piece in toRemove)
        {
            this.gamePieces.Remove(piece);
            piece.removeFromGrid();
            Destroy(piece.gameObject);
            this.score += this.combo * piece.getNumTiles();
        }

        this.updateScore();
        if (toRemove.Count > 0)
        {
            int soundIndex = Math.Clamp(this.combo - 1, 0, this.comboSounds.GetLength(0)-1);
            this.comboSounds[soundIndex].Play();
        }

        return toRemove.Count > 0;
    }

    public bool checkForUnsupported()
    {
        bool shouldFall = false;
        foreach (Polyomino piece in this.gamePieces)
        {
            piece.falling = true;
        }

        bool anyLanded = true;

        while (anyLanded)
        {
            // iterate multiple times, because pieces that become supported may in turn support other pieces
            anyLanded = false;
            for (int x = 0; x < this.tilesHoriz; x++)
            {
                for (int y = 0; y < this.tilesVert; y++)
                {
                    Polyomino piece = this.gameGrid[x, y];
                    if (piece != null && piece.falling)
                    {
                        if (y == 0 || (this.gameGrid[x, y-1] != null && !this.gameGrid[x, y-1].falling))
                        {
                            piece.falling = false;
                            anyLanded = true;
                        }
                        else
                        {
                            shouldFall = true;
                        }
                    }
                }
            }
        }
        
        return shouldFall;
    }

    protected void updateScore()
    {
        this.scoreText.text = string.Format("{0:000000}", score);
    }

    public void togglePause()
    {
        this.paused = !this.paused;
        if (this.paused)
        {
            this.lowPassFilter.enabled = true;
            this.pauseMenu.SetActive(true);
        }
        else
        {
            this.lowPassFilter.enabled = false;
            this.pauseMenu.SetActive(false);
        }
    }

    public void restart()
    {
        this.score = 0;
        this.gameTick = 0f;
        this.updateScore();
        this.gameState = GameState.Spawn;
        foreach (Polyomino polyomino in this.gamePieces)
        {
            Destroy(polyomino.gameObject);
        }
        this.gamePieces.Clear();
        foreach (Polyomino polyomino in this.polyQueue)
        {
            Destroy(polyomino.gameObject);
        }
        this.generateNewQueue();
    }

    protected void playRandomSound(AudioSource[] sounds)
    {
        if (sounds.GetLength(0) > 0)
        {
            int soundIndex = UnityEngine.Random.Range(0, sounds.GetLength(0));
            sounds[soundIndex].Play();
        }
    }
}
