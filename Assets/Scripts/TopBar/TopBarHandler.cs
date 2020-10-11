using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;

public class TopBarHandler : MonoBehaviour
{
    [SerializeField] Text currentPointsText;
    [SerializeField] Text maxPointsArchived;

    [SerializeField] GameObject newGameButton;
    [SerializeField] GameObject resetPointsButton;

    // Start is called before the first frame update
    public void Init()
    {
        currentPointsText.text = "0";
        maxPointsArchived.text = OpenDataFile();
    }

    // Update is called to update the points
    public void UpdateCurrentPoints(int points)
    {
        currentPointsText.text = points.ToString();
    }

    private String OpenDataFile()
    {
        String pointsRead = "";
        try {
            //Pass the file path and file name to the StreamReader constructor
            StreamReader sr = new StreamReader(Path.Combine(Directory.GetCurrentDirectory(), "Assets/Scripts/TopBar/PointsFile.txt"));
            //Read the first line of text
            pointsRead =  sr.ReadLine();
            //close the file
            sr.Close();
        }
        catch (Exception e) {
            Debug.Log(e);
        }

        return pointsRead;
    }

    public void WriteToDataFile()
    {
        if (Int32.Parse(currentPointsText.text) > Int32.Parse(maxPointsArchived.text))
        {
            System.IO.File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "Assets/Scripts/TopBar/PointsFile.txt"), currentPointsText.text);
        }
    }
}
