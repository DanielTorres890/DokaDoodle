using TMPro;
using UnityEngine;

public class TileInfoDisplay : MonoBehaviour
{

    public TextMeshProUGUI tileDescription;

    public void UpdateText(TileScript lookedAtTile)
    {
        tileDescription.text = lookedAtTile.tileDescription;
    }
}
