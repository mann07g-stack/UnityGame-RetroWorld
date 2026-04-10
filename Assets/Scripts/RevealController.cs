using UnityEngine;
using UnityEngine.UI;

// [ExecuteAlways] lets you see changes in the Editor without hitting Play
[ExecuteAlways] 
public class RevealController : MonoBehaviour
{
    public RawImage targetImage;
    
    [Header("Reveal Settings (Edit Here)")]
    [Range(0f, 0.5f)] public float radius = 0.2f;   // Drag this slider to change Size
    [Range(0f, 0.5f)] public float softness = 0.05f; // Drag this slider to change Blur

    [Header("Aspect Correction")]
    [Tooltip("Offsets the aspect ratio to fix the oval shape.")]
    public float aspectRatioOffset = 0.5f; 

    private Material mat;
    private RectTransform rectTrans;

    void Start()
    {
        if (targetImage == null) return;
        rectTrans = targetImage.rectTransform;
        
        // Use a clone material at runtime so we don't break the original asset
        if (Application.isPlaying)
        {
            mat = new Material(targetImage.material);
            targetImage.material = mat;
        }
        else
        {
            // In Editor mode, use the shared material so we can see previews
            mat = targetImage.material;
        }
    }

    void Update()
    {
        if (mat == null || targetImage == null) return;
        
        // Continuously apply the Slider values to the Shader
        UpdateMaterialProperties();
        
        // Only track mouse when playing
        if (Application.isPlaying) 
        {
            UpdateMousePosition();
        }
    }

    void UpdateMaterialProperties()
    {
        // 1. Send Slider Values to Shader
        mat.SetFloat("_Radius", radius);
        mat.SetFloat("_Softness", softness);

        // 2. Fix Aspect Ratio (Width / Height + Offset)
        float width = rectTrans.rect.width;
        float height = rectTrans.rect.height;

        if (height > 0)
        {
            float baseAspect = width / height;
            mat.SetFloat("_Aspect", baseAspect + aspectRatioOffset);
        }
    }

    void UpdateMousePosition()
    {
        Vector2 mousePos = Input.mousePosition;
        Vector2 localPoint;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTrans,
            mousePos,
            null, 
            out localPoint
        );

        // Normalize pixels to 0..1 UV space
        float u = (localPoint.x / rectTrans.rect.width) + 0.5f;
        float v = (localPoint.y / rectTrans.rect.height) + 0.5f;

        mat.SetVector("_MousePos", new Vector4(u, v, 0, 0));
    }
}