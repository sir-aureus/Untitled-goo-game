using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class Polyomino : MonoBehaviour
{
    public Tilemap myGrid;
    public RuleTile myRuleTile;

    public GooyoController controller;

    public GameObject eyePrefab;

    public bool falling = false;

    public int colorIndex = 0;

    public int getWidth()
    {
        return this.maxX - this.minX + 1;
    }

    public int getHeight()
    {
        return this.maxY - this.minY + 1;
    }

    public int getMinX()
    {
        return this.minX;
    }
    
    public int getMaxX()
    {
        return this.maxX;
    }
    public int getMinY()
    {
        return this.minY;
    }
    
    public int getMaxY()
    {
        return this.maxY;
    }

    public int getGridX()
    {
        return this.gridX;
    }

    public int getGridY()
    {
        return this.gridY;
    }

    public int getNumTiles()
    {
        return this.numTiles;
    }

    protected bool[,] shape;
    protected int gridX, gridY;
    protected int minX = 0, maxX = 0, minY = 0, maxY = 0;
    protected int numTiles = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.init();
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    public void init()
    {
        if (this.shape == null)
        {
            this.shape = new bool[1,1];
            this.shape[0,0] = true;
        }
    }

    public void setVisualPosition(int gridX, int gridY)
    {
        this.gridX = gridX;
        this.gridY = gridY;
        this.applyGridPosition();
    }

    public bool canSetGridPosition(int gridX, int gridY)
    {
        int centerX = 0;//this.minX;
        int centerY = this.minY;

        var gameGrid = this.controller.getGameGrid();

        for (int i = this.minX; i <= this.maxX; i++)
        {
            for (int j = this.minY; j <= this.maxY; j++)
            {
                if (this.shape[i,j])
                {
                    int x = gridX - centerX + i;
                    int y = gridY - centerY + j;
                    if (x < 0 || x >= gameGrid.GetLength(0) || y < 0 || y >= gameGrid.GetLength(1))
                    {
                        return false;
                    }
                    if (gameGrid[x, y] != null && gameGrid[x, y] != this && !gameGrid[x, y].falling)
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    public void applyGridPosition()
    {
        int centerX = 0;//this.minX;
        int centerY = this.minY;
        this.transform.position = this.controller.getGridOffset() + this.controller.getGridSpacingX() * (this.gridX - centerX) + this.controller.getGridSpacingY() * (this.gridY - centerY);
    }

    // updates the polyomino's position on the grid; if the new position is invalid, returns false and reverts
    public bool setGridPosition(int gridX, int gridY, bool force = false)
    {
        int centerX = 0;//this.minX;
        int centerY = this.minY;
        int lastGridX = this.gridX, lastGridY = this.gridY;
        this.gridX = gridX;
        this.gridY = gridY;

        bool valid = true;

        var gameGrid = this.controller.getGameGrid();

        for (int i = this.minX; i <= this.maxX; i++)
        {
            for (int j = this.minY; j <= this.maxY; j++)
            {
                if (this.shape[i,j])
                {
                    int x = this.gridX - centerX + i;
                    int y = this.gridY - centerY + j;
                    if (x < 0 || x >= gameGrid.GetLength(0) || y < 0 || y >= gameGrid.GetLength(1))
                    {
                        valid = false;
                        break;
                    }
                    if (gameGrid[x, y] != null && gameGrid[x, y] != this && !gameGrid[x, y].falling)
                    {
                        valid = false;
                        break;
                    }
                }
            }
        }

        if (!valid && !force)
        {
            this.gridX = lastGridX;
            this.gridY = lastGridY;

            this.applyGridPosition();
            return false;
        }
        else
        {
            // remove the polyomino from the grid
            this.removeFromGrid();

            // add the polyomino to the grid in the new position
            for (int i = minX; i <= maxX; i++)
            {
                for (int j = minY; j <= maxY; j++)
                {
                    if (this.shape[i,j])
                    {
                        int x = this.gridX - centerX + i;
                        int y = this.gridY - centerY + j;
                        gameGrid[x, y] = this;
                    }
                }
            }

            // update position on screen

            this.applyGridPosition();
            return true;
        }        
    }

    public void removeFromGrid()
    {
        var gameGrid = this.controller.getGameGrid();
        for (int i = 0; i < gameGrid.GetLength(0); i++)
        {
            for (int j = 0; j < gameGrid.GetLength(1); j++)
            {
                if (gameGrid[i,j] == this)
                {
                    gameGrid[i,j] = null;
                }
            }
        }
    }
    public void setRandomShape()
    {
        int numTiles = Random.Range(2, 5);
        this.numTiles = numTiles;
        this.shape = new bool[numTiles, numTiles];

        int x = numTiles / 2, y = numTiles / 2;

        this.minX = this.maxX = x;
        this.minY = this.maxY = y;

        this.shape[x, y] = true;

        for (int count = 0; count < numTiles;)
        {
            x = Random.Range(0, numTiles);
            y = Random.Range(0, numTiles);

            if (this.shape[x, y] == false)
            {
                bool valid = false;

                if (x > 1 && this.shape[x-1, y] == true)
                {
                    valid = true;
                }
                else if (x < numTiles - 1 && this.shape[x+1, y] == true)
                {
                    valid = true;
                }
                else if (y > 1 && this.shape[x, y-1] == true)
                {
                    valid = true;
                }
                else if (y < numTiles - 1 && this.shape[x, y+1] == true)
                {
                    valid = true;
                }

                if (valid)
                {
                    this.shape[x, y] = true;
                    count++;
                    if (x < this.minX)
                    {
                        this.minX = x;
                    }
                    if (x > this.maxX)
                    {
                        this.maxX = x;
                    }
                    if (y < this.minY)
                    {
                        this.minY = y;
                    }
                    if (y > this.maxY)
                    {
                        this.maxY = y;
                    }
                }
            }
        }

        this.applyShape();
    }

    protected void applyShape()
    {
        int centerX = 0;
        int centerY = 0;
        for (int i = this.minX; i <= this.maxX; i++)
        {
            for (int j = this.minY; j <= this.maxY; j++)
            {
                if (this.shape[i,j])
                {
                    if (this.shape[i,j])
                    {
                        for (int subX = -1; subX <= 1; subX++)
                        {
                            for (int subY = -1; subY <= 1; subY++)
                            {
                                int x = (i - centerX) * 3 + subX;
                                int y = (j - centerY) * 3 + subY;
                                this.myGrid.SetTile(new Vector3Int(x, y, 0), this.myRuleTile);

                                if (subX == 0 && subY == 0)
                                {
                                    bool addEye = (Random.Range(0, 2) == 0);

                                    if (addEye)
                                    {
                                        GameObject eye = Instantiate(this.eyePrefab, this.myGrid.transform);
                                        eye.transform.position = this.myGrid.GetCellCenterWorld(new Vector3Int(x, y, 0));
                                        eye.transform.Translate(new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), -1));
                                    }
                                }
                            }
                        }
                    } 
                }
            }
        }
    }

    public bool rotate(bool clockwise)
    {

        return true;
    }

    public void moveHorizontal(int direction)
    {

    }

    public bool canFallOneTile()
    {
        return this.canSetGridPosition(this.gridX, this.gridY - 1);
    }

    public bool fallOneTile()
    {
        bool valid = this.setGridPosition(this.gridX, this.gridY - 1);
        
        return valid;
    }

    public HashSet<Polyomino> getAdjacentMatches()
    {
        HashSet<Polyomino> adjacent = new HashSet<Polyomino>();

        int centerX = 0;//this.minX;
        int centerY = this.minY;

        var gameGrid = this.controller.getGameGrid();

        for (int i = this.minX; i <= this.maxX; i++)
        {
            for (int j = this.minY; j <= this.maxY; j++)
            {
                if (this.shape[i,j])
                {
                    int x = this.gridX - centerX + i - 1;
                    int y = this.gridY - centerY + j;
                    if (x >= 0 && x < gameGrid.GetLength(0) && y >= 0 && y < gameGrid.GetLength(1) && gameGrid[x, y] != null && gameGrid[x, y] != this && gameGrid[x,y].colorIndex == this.colorIndex)
                    {
                        adjacent.Add(gameGrid[x, y]);
                    }

                    x = this.gridX - centerX + i + 1;
                    if (x >= 0 && x < gameGrid.GetLength(0) && y >= 0 && y < gameGrid.GetLength(1) && gameGrid[x, y] != null && gameGrid[x, y] != this && gameGrid[x,y].colorIndex == this.colorIndex)
                    {
                        adjacent.Add(gameGrid[x, y]);
                    }
                    
                    x = this.gridX - centerX + i;
                    y = this.gridY - centerY + j - 1;
                    if (x >= 0 && x < gameGrid.GetLength(0) && y >= 0 && y < gameGrid.GetLength(1) && gameGrid[x, y] != null && gameGrid[x, y] != this && gameGrid[x,y].colorIndex == this.colorIndex)
                    {
                        adjacent.Add(gameGrid[x, y]);
                    }

                    y = this.gridY - centerY + j + 1;
                    if (x >= 0 && x < gameGrid.GetLength(0) && y >= 0 && y < gameGrid.GetLength(1) && gameGrid[x, y] != null && gameGrid[x, y] != this && gameGrid[x,y].colorIndex == this.colorIndex)
                    {
                        adjacent.Add(gameGrid[x, y]);
                    }
                }
            }
        }
        return adjacent;
    }
}
