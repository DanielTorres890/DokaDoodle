using UnityEngine;

public class AllyButtonCondition : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ClientChecks.Instance.onRoundStart.AddListener(ShouldShow);
    }

    

    private void ShouldShow()
    {
        gameObject.SetActive(NetworkData.Instance.GetCurrentPlayer().partyMembers.Count > 0);
    }
}
