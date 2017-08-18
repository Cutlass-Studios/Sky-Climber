using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Highscores : MonoBehaviour
{

    //http://dreamlo.com/lb/Gb56EkuTUkCJ8zZTcu1AMQ9fh4a7Ddd0GKDXLeQl5_fA

    const string privateCode = "Gb56EkuTUkCJ8zZTcu1AMQ9fh4a7Ddd0GKDXLeQl5_fA";
    const string publicCode = "599734bb6b2b65188c173762";
    const string webURL = "http://dreamlo.com/lb/";
    public Highscore[] highscoresList;


    public Text[] highscoreName;
    public Text[] highscoreScore;

    public void AddNewHighscore(string username, int score)
    {
        StartCoroutine(UploadNewHighScore(username, score));
    }


    IEnumerator UploadNewHighScore(string username, int score)
    {
        WWW www = new WWW(webURL + privateCode + "/add/" + WWW.EscapeURL(username) + "/" + score);
        yield return www;

        if (string.IsNullOrEmpty(www.error))
        {
            DownloadHighscores();
            print("upload successful");
        }
        else
        {
            print("error uploading: " + www.error);
        }
    }

    public void DownloadHighscores()
    {
        StartCoroutine("DownloadHighScoresFromDataBase");
    }

    IEnumerator DownloadHighScoresFromDataBase()
    {
        WWW www = new WWW(webURL + publicCode + "/pipe/");
        yield return www;

        if (string.IsNullOrEmpty(www.error))
        {
            FormatHighscores(www.text);
        }
        else
        {
            print("error pulling: " + www.error);
        }
    }

    void FormatHighscores(string textStream)
    {
        //split entries based on new line
        string[] entries = textStream.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        highscoresList = new Highscore[entries.Length];

        int num = 10;
        if (entries.Length < 10)
        {
            num = entries.Length;
        }

        

        for (int i = 0; i < num; i++)
        {
            string[] entryInfo = entries[i].Split(new char[] { '|' });
            string username = entryInfo[0];
            int score = int.Parse(entryInfo[1]);
            highscoresList[i] = new Highscore(username, score);

            highscoreName[i].text = "";
            highscoreScore[i].text = "";

            highscoreName[i].text =  highscoresList[i].username;
            highscoreScore[i].text = highscoresList[i].score.ToString();
            
            
            //print(highscoresList[i].username + ": " + highscoresList[i].score);
        }
    }
}

public struct Highscore
{
    public string username;
    public int score;

    public Highscore(string _username, int _score)
    {
        username = _username;
        score = _score;
    }
}