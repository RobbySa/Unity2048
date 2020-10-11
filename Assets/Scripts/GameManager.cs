using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] TopBarHandler topBar;
    [SerializeField] GameSystem gameSystem;

    bool isGameOver;

    // Start is called before the first frame update
    private void Start()
    {
        Restart();
    }
    void Restart()
    {
        topBar.Init();
        gameSystem.Init();
        //gameSystem.AddTile(new System.Random().Next(0, 2), gameSystem.GetRandomEmptyLocation());
        //gameSystem.AddTile(new System.Random().Next(0, 2), gameSystem.GetRandomEmptyLocation());
        gameSystem.AddTile(0, new int[] { 0, 0 });
        gameSystem.AddTile(0, new int[] { 0, 1 });
        gameSystem.AddTile(0, new int[] { 0, 2 });

        isGameOver = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isGameOver) {
            topBar.UpdateCurrentPoints(gameSystem.SumUpAllTiles());
            HandleUserInput();
            isGameOver = gameSystem.IsGameOver();
        } else {
            topBar.UpdateCurrentPoints(gameSystem.SumUpAllTiles());
            HandleUserInput();
            isGameOver = gameSystem.IsGameOver();
            topBar.WriteToDataFile();
        }
    }

    void HandleUserInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            StartCoroutine(gameSystem.Move(0, 4, 1, 0, 4, 1, -1, 0));
            //gameSystem.AddTile(new System.Random().Next(0, 2), gameSystem.GetRandomEmptyLocation());
        }
        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            StartCoroutine(gameSystem.Move(3, -1, -1, 0, 4, 1, 1, 0));
            //gameSystem.AddTile(new System.Random().Next(0, 2), gameSystem.GetRandomEmptyLocation());
        }
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            StartCoroutine(gameSystem.Move(0, 4, 1, 3, -1, -1, 0, 1));
            //gameSystem.AddTile(new System.Random().Next(0, 2), gameSystem.GetRandomEmptyLocation());
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            StartCoroutine(gameSystem.Move(0, 4, 1, 0, 4, 1, 0, -1));
            //gameSystem.AddTile(new System.Random().Next(0, 2), gameSystem.GetRandomEmptyLocation());
        }
    }
}