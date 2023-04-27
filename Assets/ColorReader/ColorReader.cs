using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorReader : MonoBehaviour{
    [SerializeField] private float saturationMultiplier = 1;
    [SerializeField] private float valueMultiplier = 1;
    [SerializeField] private float saturationAdded = 0;
    [SerializeField] private float valueAdded = 0;
    [SerializeField] private float lerpTest = 0.5f;
    public static ColorReader i;

    public Color color;
    [SerializeField] private RenderTexture renderTexture;

    private void Awake() {
        i = this;
    }

    public void Move(Vector3 position, Vector3 forward) {
        transform.position = position - forward * 0.2f;
        transform.forward = forward;
    }

    public Color ReadColor() {
        Texture2D texture = new Texture2D(16, 16, TextureFormat.RGBAFloat, false);
        Rect rectReadPicture = new Rect(0, 0, 5, 5);
        RenderTexture.active = renderTexture;

        // Read pixels
        texture.ReadPixels(rectReadPicture, 0, 0);
        texture.Apply();

        RenderTexture.active = null; // added to avoid errors 

        color = texture.GetPixel(3, 3, 0);
        Color[] colors = texture.GetPixels(0, 0, 5, 5, 0);
        foreach (Color c in colors) {
            color += c;
        }
        color /= colors.Length;

        //color = Color.Lerp(color, Color.white, valueMultiplier);
        color *= valueMultiplier;
        color = Color.Lerp(color, Color.white, lerpTest);

        //float H, S, V;
        //Color.RGBToHSV(new Color(color.r, color.g, color.b, 1.0F), out H, out S, out V);
        //V *= valueMultiplier;
        //S *= saturationMultiplier;
        //V += valueAdded;
        //S += saturationAdded;
        //color = Color.HSVToRGB(H, S, V);

        return color;
    }
}
