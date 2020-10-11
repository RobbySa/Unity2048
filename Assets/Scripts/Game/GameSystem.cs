using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;

public class GameSystem : MonoBehaviour
{
    [SerializeField] GameObject placeTilesHere;
    [SerializeField] List<GameObject> tilesTemplates;
    [SerializeField] List<GameObject> tileSpacesShit;

    List<List<GameObject>> tileSpaces = new List<List<GameObject>>();
    List<List<GameObject>> objectsOnScreen = new List<List<GameObject>>();

    float cellStep = 1.6f;
    float durationStep = 0.2f;

    public void Init()
    {
        // Is it a new game? or did it just restart?
        if (objectsOnScreen.Count == 0) {
            // initialise both arrays
            for (int i = 0; i < 4; i++) {
                tileSpaces.Add(new List<GameObject>());
                objectsOnScreen.Add(new List<GameObject>());
            }
            // populate objectOnScreen with null as placeholder for future tiles
            for (int i = 0; i < 4; i++) {
                for (int j = 0; j < 4; j++) {
                    objectsOnScreen[i].Add(null);
                }
            }
            // create a better system to hold placeholder tiles
            int x = 0;
            int y = 0;
            foreach (var tile in tileSpacesShit) {
                tileSpaces[x].Add(tile);

                y++;
                if (y >= 4) {
                    y = 0;

                    x++;
                    if (x >= 4) {
                        break;
                    }
                }
            }
        } else {
            // Destroy all the items in the list
            for (int i = 0; i < 4; i++) {
                for (int j = 0; i < 4; j++) {
                    if (objectsOnScreen[i][j] != null) {
                        Destroy(objectsOnScreen[i][j]);
                        objectsOnScreen[i][j] = null;
                    }
                }
            }
        }
    }

    public void AddTile(int objectIndex, int[] locationIdex)
    {
        GameObject newObject = Instantiate(tilesTemplates[objectIndex]);
        newObject.transform.SetParent(placeTilesHere.transform);
        newObject.transform.localScale = new Vector3(1, 1, 1);
        newObject.transform.position = tileSpaces[locationIdex[0]][locationIdex[1]].transform.position;

        objectsOnScreen[locationIdex[0]][locationIdex[1]] = newObject;
    }

    public int[] GetRandomEmptyLocation()
    {
        List<int[]> availableSpaces = new List<int[]>();

        for (int i = 0; i < 4; i++) {
            for (int j = 0; j < 4; j++) {
                if (objectsOnScreen[i][j] == null) {
                    availableSpaces.Add(new int[] { i, j });
                }
            }
        }

        return availableSpaces[new System.Random().Next(0, availableSpaces.Count)];
    }

    // Look for empty space, if found the game isn't over
    public bool IsGameOver()
    {
        for (int i = 0; i < 4; i++) {
            for (int j = 0; j < 4; j++) {
                if (objectsOnScreen[i][j] == null) {
                    return false;
                }
            }
        }
        return true;
    }

    // Handles movemovent of the tiles
    public IEnumerator Move(int startX, int endX, int incrementX, int startY, int endY, int incrementY, int changeInX, int changeInY)
    {
        List<GameObject> objectsToMove = new List<GameObject>();
        List<int> moveSpaces = new List<int>();

        List<int[]> objectsToUpgrade = new List<int[]>();
        List<GameObject> objectsToDestroy = new List<GameObject>();

        bool didAnythingChange = false;

        for (int i = startX; (i - endX) != 0; i += incrementX) {
            for (int j = startY; (j - endY) != 0; j += incrementY) {
                if (objectsOnScreen[i][j] != null) {
                    int placesToMove = CanMove(i, j, changeInX, changeInY);
                    objectsToMove.Add(objectsOnScreen[i][j]);

                    if (placesToMove > 0) {
                        objectsOnScreen[i + (placesToMove * changeInX)][j + (placesToMove * changeInY)] = objectsOnScreen[i][j];
                        objectsOnScreen[i][j] = null;
                        didAnythingChange = true;
                    }
                    // If the tile is not at the wall, check if it can merge with the tile next to it
                    if (!objectsToUpgrade.Any(p => p.SequenceEqual(new int[] { i + ((placesToMove + 1) * changeInX), j + ((placesToMove + 1) * changeInY) }))) {
                        if (CanMerge(i + (placesToMove * changeInX), j + (placesToMove * changeInY), changeInX, changeInY)) {
                            objectsToDestroy.Add(objectsOnScreen[i + (placesToMove * changeInX)][j + (placesToMove * changeInY)]);
                            objectsOnScreen[i + (placesToMove * changeInX)][j + (placesToMove * changeInY)] = null;
                            didAnythingChange = true;

                            placesToMove++;
                            objectsToUpgrade.Add(new int[] { i + (placesToMove * changeInX), j + (placesToMove * changeInY) });
                        }
                    }
                    moveSpaces.Add(placesToMove);
                }
            }
        }
        // Display the changes
        for (int i = 0; i < objectsToMove.Count; i++) {
            Vector3 currentPosition = objectsToMove[i].transform.position;
            if (changeInY == 0) {
                if (changeInX == -1) {
                    yield return objectsToMove[i].transform.DOMoveY(currentPosition.y + (cellStep * moveSpaces[i]), durationStep);
                } else {
                    yield return objectsToMove[i].transform.DOMoveY(currentPosition.y - (cellStep * moveSpaces[i]), durationStep);
                }
            } else {
                if (changeInY == 1) {
                    yield return objectsToMove[i].transform.DOMoveX(currentPosition.x + (cellStep * moveSpaces[i]), durationStep);
                } else {
                    yield return objectsToMove[i].transform.DOMoveX(currentPosition.x - (cellStep * moveSpaces[i]), durationStep);
                }
            }
        }
        // Destroy one tile to upgrade another
        for (int i = 0; i < objectsToDestroy.Count; i++) {
            Destroy(objectsToDestroy[i]);
            UpgradeTile(objectsToUpgrade[i][0], objectsToUpgrade[i][1]);
        }
        // At the end place a new tile if changes were made
        if (didAnythingChange) {
            yield return new WaitForSeconds(0.1f);
            AddTile(new System.Random().Next(0, 2), GetRandomEmptyLocation());
        }
    }

    // handles the merging of two identical tiles
    public void UpgradeTile(int locationX, int locationY)
    {
        GameObject objectOfInterest = objectsOnScreen[locationX][locationY];
        string objectText = objectOfInterest.GetComponentInChildren<Text>().text;
        int numberFromText = Int32.Parse(objectText);
        int index = (int)Math.Log((double)numberFromText, 2.0);

        // Destroy and create
        Destroy(objectOfInterest);
        AddTile(index, new int[] { locationX, locationY });
    }

    // Checks if two adjecient tiles could actually merge
    public bool CanMerge(int positionX, int positionY, int changeInX, int changeInY)
    {
        if (changeInX != 0 && (positionX + changeInX > 3 || positionX + changeInX < 0)) {
            return false;
        } else if (changeInY != 0 && (positionY + changeInY > 3 || positionY + changeInY < 0)) {
            return false;
        }
        // We compare the text of the current loction and the blocked location
        if (objectsOnScreen[positionX][positionY].GetComponentInChildren<Text>().text == objectsOnScreen[positionX + changeInX][positionY + changeInY].GetComponentInChildren<Text>().text) {
            return true;
        }
        return false;
    }

    // Returns if a tile can move
    public int CanMove(int positionX, int positionY, int changeInX, int changeInY)
    {
        // Check that the target is not trying to move outside
        if (positionX + changeInX > 3 || positionX + changeInX < 0) {
            return 0;
        }
        if (positionY + changeInY > 3 || positionY + changeInY < 0) {
            return 0;
        }
        // Check if the next space is free
        if (objectsOnScreen[positionX + changeInX][positionY + changeInY] != null) {
            return 0;
        }

        return 1 + CanMove(positionX + changeInX, positionY + changeInY, changeInX, changeInY);
    }

    // Get the current score for this board
    public int SumUpAllTiles()
    {
        int total = 0;
        for (int i = 0; i < 4; i++) {
            for (int j = 0; j< 4; j++) {
                if (objectsOnScreen[i][j] != null) {
                    total += Int32.Parse(objectsOnScreen[i][j].GetComponentInChildren<Text>().text);
                }
            }
        }
        return total;
    }
}

// A tile can either move towards a wall or tile, can merge with another tile, 
public enum TypeState
{
    move,
    merge,
    wall
}