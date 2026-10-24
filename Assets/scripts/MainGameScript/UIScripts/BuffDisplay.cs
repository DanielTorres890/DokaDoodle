
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuffDisplay : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject buffIconPrefab;

    public List<GameObject> spawnedIcons;
    void Start()
    {
        CreateDisplay();
        ClientChecks.Instance.onClassAbilityUse.AddListener(CreateDisplay);
        ClientChecks.Instance.onItemUse.AddListener(CreateDisplay);
    }

    public void CreateDisplay()
    {
        foreach(var icon in spawnedIcons)
        {
            Destroy(icon);
        }

        spawnedIcons.Clear();

        foreach(var status in NetworkData.Instance.GetCurrentPlayer().statuses)
        {
            var statusInfo = NetworkData.Instance.buffDataBase.GetItem[status.buffId];
            if(statusInfo.displayIcon)
            {
                playerData applier = null;
                if (status.whoAppliedId != -1)
                {
                    applier = NetworkData.Instance.players[status.whoAppliedId];
                }

                GameObject spawned = Instantiate(buffIconPrefab, transform);
                spawned.GetComponent<Image>().sprite = statusInfo.displayIcon;
                spawned.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = statusInfo.GetDescription(NetworkData.Instance.GetCurrentPlayer(), applier);
                spawnedIcons.Add(spawned);
            }
        }
    }
}
