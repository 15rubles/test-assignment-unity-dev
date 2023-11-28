using UnityEngine;

public class AddTestEventButton : MonoBehaviour
{
    private int eventNumber = 0;

    [SerializeField]
    private EventService eventService;

    public void AddTestEvent()
    {
        eventService.TrackEvent("testType" + eventNumber, "testData" + eventNumber);
        eventNumber++;
    }
}
