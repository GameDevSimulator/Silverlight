using UnityEngine;

public class SceneFadeInOut : MonoBehaviour
{
	public float FadeSpeed = 1.5f;
    private bool SceneStarting = true;

    void Awake()
    {
        GetComponent<GUITexture>().pixelInset = new Rect(0f, 0f, Screen.width, Screen.height);
    }
    void Update()
    {
        if(SceneStarting)
        {
            StartScene();
        }
    }
    void FadeToClear()
    {
        GetComponent<GUITexture>().color = Color.Lerp(GetComponent<GUITexture>().color, Color.clear, FadeSpeed * Time.deltaTime);
    }
    void FadeToBlack()
    {
        GetComponent<GUITexture>().color = Color.Lerp(GetComponent<GUITexture>().color, Color.black, FadeSpeed * Time.deltaTime);
    }
    public void StartScene()
    {
        FadeToClear();

        if(GetComponent<GUITexture>().color.a <= 0.05f)
        {
            GetComponent<GUITexture>().color = Color.clear;
            GetComponent<GUITexture>().enabled = false;
            SceneStarting = false;
        }
    }
    public void EndScene()
    {
        GetComponent<GUITexture>().enabled = true;
        FadeToBlack();
    }
}
