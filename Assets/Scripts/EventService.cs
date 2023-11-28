using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class EventService : MonoBehaviour
{
    private const float COUNTDOWN_BEFORE_SEND = 3f;
    private const string SERVER_URL = "SERVER_URL";

    private List<EventData> pendingEvents = new();
    private Coroutine countdownCoroutine;
    private bool isCountdownEnded = false;

    private void Start()
    {
        if (pendingEvents.Count > 0)
        {
            countdownCoroutine = StartCoroutine(CountdownCoroutine());
        }
    }

    public void TrackEvent(string type, string data)
    {
        EventData eventData = new(type, data);
        pendingEvents.Add(eventData);

        if (countdownCoroutine == null)
        {
            countdownCoroutine = StartCoroutine(CountdownCoroutine());
        }
        else if (isCountdownEnded)
        {
            StartCoroutine(WaitForCountdown());
        }
    }

    private IEnumerator WaitForCountdown()
    {
        while (countdownCoroutine != null)
        {
            yield return null;
        }

        countdownCoroutine = StartCoroutine(CountdownCoroutine());
    }

    private IEnumerator CountdownCoroutine()
    {
        isCountdownEnded = false;
        while (pendingEvents.Count > 0)
        {
            yield return new WaitForSecondsRealtime(COUNTDOWN_BEFORE_SEND);
            isCountdownEnded = true;
            yield return SendEventsCoroutine();
        }

        countdownCoroutine = null;
    }

    private IEnumerator SendEventsCoroutine()
    {
        string jsonData = JsonUtility.ToJson(new EventBatch(pendingEvents));

        using (UnityWebRequest unityWebRequest = UnityWebRequest.Post(SERVER_URL, jsonData))
        {
            unityWebRequest.SetRequestHeader("Content-Type", "application/json");

            yield return unityWebRequest.SendWebRequest();

            if (unityWebRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Events sent successfully.");
                pendingEvents.Clear();
            }
            else
            {
                Debug.LogWarning("Failed to send events. They will be retried later.");
            }
        }
    }

    [Serializable]
    private class EventData
    {
        public string Type;
        public string Data;

        public EventData(string type, string data)
        {
            this.Type = type;
            this.Data = data;
        }
    }

    [Serializable]
    private class EventBatch
    {
        public List<EventData> Events;

        public EventBatch(List<EventData> events)
        {
            this.Events = events;
        }
    }
}
