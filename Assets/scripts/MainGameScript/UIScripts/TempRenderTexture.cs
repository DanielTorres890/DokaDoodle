using UnityEngine;

public class TempRenderTexture : MonoBehaviour
{
    public RenderTexture tempRenderTexture;
    public Camera myCam;

    public int textureWidth = 256;
    public int textureHeight = 256;

    //idk what this one is
    public int depthBufferBits = 24;
    void Start()
    {
        myCam = transform.GetComponentInChildren<Camera>();
        if(myCam == null ) { return; }

        tempRenderTexture = new RenderTexture(textureWidth, textureHeight, depthBufferBits);
        tempRenderTexture.Create();
        myCam.targetTexture = tempRenderTexture;
    }

    // Update is called once per frame
    private void OnDestroy()
    {
        if(tempRenderTexture != null) 
        { 
            tempRenderTexture.Release();
            Destroy(tempRenderTexture);
        }
        
    }
}
