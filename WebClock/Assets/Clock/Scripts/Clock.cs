using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;
using TMPro;

public class Clock : MonoBehaviour {

    // Класс для парсинга ответа с TimeZoneDB
    [Serializable]
    public class TimeZoneDBResponse
    {
        public string status;      
        public string message;     
        public string countryName; 
        public string zoneName;    
        public double timestamp;     
    }

    public int seconds = 0;
    public int minutes = 0;
    public int hour = 0;

    public bool isServerConnected = false;

    public GameObject pointerSeconds;
    public GameObject pointerMinutes;
    public GameObject pointerHours;

    public TMP_Text TMP_Text;

    DateTime exactTime;

    float msecs = 0;

    // Ссылка на таймсервер и ключ API
    string url = "https://api.timezonedb.com/v2.1/get-time-zone?key={0}&format=json&by=zone&zone=UTC";
    string apiKey = "X00KE51ZO8SC";

    void Start()
    {
        Application.runInBackground = true;
        StartCoroutine(CheckTimeEveryHour());
    }

    void Update()
    {
        if (isServerConnected)
        {
            msecs += Time.deltaTime;

            CalculateTime(msecs);

            SetPointers();

            SetTextClock();
        }
    }

    // Внутренний таймер на движение стрелок
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

    // Устанавливаем время на текстовых часах
    private void SetTextClock()
    {
        TMP_Text.text = hour.ToString("D2") + ":" + minutes.ToString("D2") + ":" + seconds.ToString("D2");
    }

    // Просчитываем поворот и вращаем стрелки
    private void SetPointers()
    {
        float rotationSeconds = (360.0f / 60.0f) * seconds;
        float rotationMinutes = (360.0f / 60.0f) * minutes;
        float rotationHours = ((360.0f / 12.0f) * hour) + ((360.0f / (60.0f * 12.0f)) * minutes);

        pointerSeconds.transform.localEulerAngles = new Vector3(0.0f, 0.0f, rotationSeconds);
        pointerMinutes.transform.localEulerAngles = new Vector3(0.0f, 0.0f, rotationMinutes);
        pointerHours.transform.localEulerAngles = new Vector3(0.0f, 0.0f, rotationHours);
    }

    // Проверка каждый час
    private IEnumerator CheckTimeEveryHour()
    {
        while (true)
        {
            yield return ServerConnection();
            yield return new WaitForSecondsRealtime(3600f);
        }
    }

    // Запрашиваем данные с таймсервера
    private IEnumerator ServerConnection()
    {
        string formattedUrl = string.Format(url, apiKey);

        using (UnityWebRequest webRequest = UnityWebRequest.Get(formattedUrl))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                string jsonResponse = webRequest.downloadHandler.text;

                ParseResponse(jsonResponse);
            }
        }
    }

    // Парсим ответ от таймсервера
    private void ParseResponse(string jsonResponse)
    {
        try
        {
            var json = JsonUtility.FromJson<TimeZoneDBResponse>(jsonResponse);

            exactTime = UnixTimeStampToDateTime(json.timestamp);

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

    // Unix преобразование
    DateTime UnixTimeStampToDateTime(double unixTimeStamp)
    {
        DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return epoch.AddSeconds(unixTimeStamp).ToLocalTime();
    }

}
