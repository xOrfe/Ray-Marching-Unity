using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class Test : MonoBehaviour
{
    public ComputeShader rayMarchingShader;
    
    public RenderTexture outTexture;
    
    public Camera camera;
    public Light light;
    List<ComputeBuffer> bufferDisposeBuffer;
    public float iTime;
    void OnRenderImage (RenderTexture inTexture, RenderTexture destination)
    {
        bufferDisposeBuffer = new List<ComputeBuffer> ();
        
        PrepareRenderTexture();
        PopulateWorld();
        
        

        rayMarchingShader.SetTexture(0,"inTexture",inTexture);
        rayMarchingShader.SetTexture (0, "outTexture", outTexture);

        
        rayMarchingShader.Dispatch(0,outTexture.width / 8 ,outTexture.height / 8,1);

        Graphics.Blit(outTexture,destination);
        
        foreach (var buffer in bufferDisposeBuffer) {
            buffer.Dispose ();
        }
    }

    private void PopulateWorld()
    {
        List<RenderableContainer> renderableContainers = new List<RenderableContainer> (FindObjectsOfType<RenderableContainer> ());
        List<RenderableData> renderables = new List<RenderableData>();

        foreach (var container in renderableContainers)
        {
            RenderableData baseRenderable = GetRenderableDataFromRenderable(container.renderables[0]);
            baseRenderable.childCount = container.renderables.Count-1;
            renderables.Add(baseRenderable);
            for (int i = 1; i < container.renderables.Count; i++)
            {
                renderables.Add(GetRenderableDataFromRenderable(container.renderables[i]));
            }
        }

        RenderableData[] renderableDatas = new RenderableData[renderables.Count];
        renderableDatas = renderables.ToArray();
        
        ComputeBuffer renderableBuffer = new ComputeBuffer (renderableDatas.Length, RenderableData.GetSize ());
        renderableBuffer.SetData (renderableDatas);
        rayMarchingShader.SetBuffer (0, "renderables", renderableBuffer);
        rayMarchingShader.SetInt ("renderableCount", renderableDatas.Length);

        bufferDisposeBuffer.Add (renderableBuffer);
        
        
        //---
        
        rayMarchingShader.SetMatrix ("cameraToWorldMatrix", camera.cameraToWorldMatrix);
        rayMarchingShader.SetMatrix ("cameraInverseProjection", camera.projectionMatrix.inverse);
        
        rayMarchingShader.SetVector ("lightDir", light.transform.forward);
        rayMarchingShader.SetFloat ("lightStrength", light.intensity);
        rayMarchingShader.SetFloat ("iTime", iTime);

    }
    
    private void PrepareRenderTexture () {
        camera = Camera.current;
        light = FindObjectOfType<Light>();
        if (outTexture == null || outTexture.width != camera.pixelWidth || outTexture.height != camera.pixelHeight) {
            if (outTexture != null) {
                outTexture.enableRandomWrite = true;
                outTexture.Release ();
            }
            outTexture = new RenderTexture(camera.pixelWidth, camera.pixelHeight, 0) {enableRandomWrite = true};
            outTexture.Create ();

        }
        
    }

    public RenderableData GetRenderableDataFromRenderable(Renderable renderable)
    {
        RenderableData renderableData = new RenderableData();
        var renderableTransform = renderable.transform;
        renderableData.localPosition = renderableTransform.localPosition;
        renderableData.localScale = renderableTransform.localScale;
        renderableData.localColor = new Vector3 (renderable.color.r, renderable.color.g, renderable.color.b);;

        renderableData.shape = (int)renderable.shape;
        renderableData.operation = (int)renderable.operation;

        renderableData.blendStrength = renderable.blendStrength;
        return renderableData;
    }
    
    public struct RenderableData
    {
        public Vector3 localPosition;
        public Vector3 localScale;
        public Vector3 localColor;
        public int shape;
        public int operation;
        
        public float blendStrength;
        
        public int childCount;
        
        public static int GetSize () {
            return sizeof (float) * 10 + sizeof (int) * 3;
        }
    }
}
