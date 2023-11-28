using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class EventService : MonoBehaviour
{
    [SerializeField]
    private float cooldownBeforeSend = 3f; // ������� � ��������
    private readonly string serverUrl = "SERVER_URL"; // URL �������

    [SerializeField]
    private List<EventData> pendingEvents = new();
    private Coroutine cooldownCoroutine;
    [SerializeField]
    private bool isRequestIsCreatedButCoroutineDontEnd = false;

    private void Start()
    {
        // �������� ������� ������ ���� ���� ���� �� ���� �������
        if (pendingEvents.Count > 0)
        {
            cooldownCoroutine = StartCoroutine(CooldownCoroutine());
        }
    }

    public void TrackEvent(string type, string data)
    {
        EventData eventData = new(type, data);
        pendingEvents.Add(eventData);


        if (cooldownCoroutine == null)  // �������� ��������, ���� ��� �� ��������
        {
            cooldownCoroutine = StartCoroutine(CooldownCoroutine());
        }
        else if (isRequestIsCreatedButCoroutineDontEnd) // ���� �������� ��� ������� request, �� ���� ���� ��� ���������� � ������� ����� ��������
        {
            StartCoroutine(WaitForCooldown());
        }
    }

    private IEnumerator WaitForCooldown()
    {
        while (cooldownCoroutine != null)
        {
            yield return null;
        }

        cooldownCoroutine = StartCoroutine(CooldownCoroutine());
    }

    private IEnumerator CooldownCoroutine()
    {
        isRequestIsCreatedButCoroutineDontEnd = false;
        while (pendingEvents.Count > 0)
        {
            yield return new WaitForSeconds(cooldownBeforeSend);
            yield return SendEventsCoroutine();
        }

        // ������� �����������, ��� ��� ������ ������� ����
        cooldownCoroutine = null;
    }

    private IEnumerator SendEventsCoroutine()
    {
        EventData[] eventsArray = pendingEvents.ToArray();
        pendingEvents.Clear();
        isRequestIsCreatedButCoroutineDontEnd = true;

        string jsonData = JsonUtility.ToJson(new EventBatch(eventsArray));
        Debug.Log(jsonData);
        using (UnityWebRequest unityWebRequest = UnityWebRequest.Post(serverUrl, jsonData))
        {
            unityWebRequest.SetRequestHeader("Content-Type", "application/json");

            yield return unityWebRequest.SendWebRequest();

            if (unityWebRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Events sent successfully.");
            }
            else
            {
                Debug.LogWarning("Failed to send events. They will be retried later.");
                pendingEvents.AddRange(eventsArray); // ��������� ������ � pending ����� �� �� ��������
            }
        }
    }

    [System.Serializable]
    private class EventData
    {
        public string type;
        public string data;

        public EventData(string type, string data)
        {
            this.type = type;
            this.data = data;
        }
    }

    [System.Serializable]
    private class EventBatch
    {
        public EventData[] events;

        public EventBatch(EventData[] events)
        {
            this.events = events;
        }
    }
}
