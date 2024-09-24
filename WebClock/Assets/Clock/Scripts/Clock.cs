using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;

public class Clock : MonoBehaviour {

    [Serializable]
    public class YandexTime
    {
        public long time;  
        public Clocks clocks;  
    }

    [Serializable]
    public class Clocks
    {
        // for future needs
    }

    public int seconds = 0;
    public int minutes = 0;
    public int hour = 0;

    public bool isServerConnected = false;

    public GameObject pointerSeconds;
    public GameObject pointerMinutes;
    public GameObject pointerHours;

    float msecs = 0;

    string url = "https://yandex.com/time/sync.json";

    void Start()
    {
        StartCoroutine(ServerConnection());
    }

    void Update()
    {
        if (isServerConnected)
        {
            msecs += Time.deltaTime;

            CalculateTime(msecs);

            SetPointers();
        }
    }

    private void CalculateTime(float currentTime)
    {
        if (currentTime >= 1.0f)
        {
            msecs -= 1.0f;
            seconds++;
            if (seconds >= 60)
            {
                seconds = 0;
                minutes++;
                if (minutes > 60)
                {
                    minutes = 0;
                    hour++;
                    if (hour >= 24)
                        hour = 0;
                }
            }
        }
    }

    private void SetPointers()
    {
        float rotationSeconds = (360.0f / 60.0f) * seconds;
        float rotationMinutes = (360.0f / 60.0f) * minutes;
        float rotationHours = ((360.0f / 12.0f) * hour) + ((360.0f / (60.0f * 12.0f)) * minutes);

        pointerSeconds.transform.localEulerAngles = new Vector3(0.0f, 0.0f, rotationSeconds);
        pointerMinutes.transform.localEulerAngles = new Vector3(0.0f, 0.0f, rotationMinutes);
        pointerHours.transform.localEulerAngles = new Vector3(0.0f, 0.0f, rotationHours);
    }

    private IEnumerator ServerConnection()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                string jsonResponse = webRequest.downloadHandler.text;
                Debug.Log("Response: " + jsonResponse);

                ParseResponse(jsonResponse);
            }
        }
    }

    private void ParseResponse(string jsonResponse)
    {
        try
        {
            var json = JsonUtility.FromJson<YandexTime>(jsonResponse);

            DateTime exactTime = UnixTimeStampToDateTime(json.time);
            Debug.Log("Exact Time from Server: " + exactTime.ToString());
            seconds = exactTime.Second;
            minutes = exactTime.Minute;
            hour = exactTime.Hour;
            isServerConnected = true;
        }
        catch (Exception e)
        {
            Debug.LogError("Error parsing time: " + e.Message);
        }
    }

    DateTime UnixTimeStampToDateTime(long unixTimeStamp)
    {
        DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return epoch.AddMilliseconds(unixTimeStamp).ToLocalTime();
    }

}
