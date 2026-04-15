using UnityEngine;
using PrimeTween;
using UnityEngine.Events;
public class ScaleWithParent : MonoBehaviour
{
    [SerializeField] private LineRenderer line;
    [SerializeField] private Vector3[] additionalPoints;
    private int index = 0;
    public UnityEvent visualFinish;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(line == null) return;

        line.startWidth = (transform.parent.localScale.x + transform.parent.localScale.z) / 2;
    }
    public void beginPoints()
    {
        Tween.Delay(0.05f, AddAdditionalPoint);
    }
    private void AddAdditionalPoint()
    {

        if (line == null) { return; }

        if (additionalPoints.Length <= index) { visualFinish.Invoke(); return; }

        Vector3[] positions = new Vector3[line.positionCount + 1];

        positions[0] = additionalPoints[index];
        
        for (int i = 0; i < line.positionCount; i++)
        {
            positions[i + 1] = line.GetPosition(i);
        }
        line.positionCount = positions.Length;
        line.SetPositions(positions);
        
        index++;
        Tween.Delay(0.05f, AddAdditionalPoint);
    }

}
