using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleSceneSwitch : MonoBehaviour
{
    public float scale = 1f;

    private int fontSize = 16;
    private GUIStyle customButton;
    private float w = 0;
    private float h = 0;

    public void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public void Start()
    {
        //Setup styles
        fontSize = Mathf.RoundToInt ( 16 * scale );
        customButton = new GUIStyle("button");
        customButton.fontSize = fontSize;
        w = 410 * scale;
        h = 90 * scale;

        NextScene();
    }

    public void Update()
    {
        if (Input.GetKeyUp(KeyCode.PageUp))
        {
            NextScene();
        }
        else if (Input.GetKeyUp(KeyCode.PageDown))
        {
            PrevScene();
        }
    }

    void OnGUI()
    {
        GUI.skin.label.fontSize = fontSize;
        GUI.color = new Color(1, 1, 1, 1);
        GUILayout.BeginArea(new Rect(Screen.width - w -5, Screen.height - h -5, w, h), GUI.skin.box);

        GUILayout.BeginHorizontal();
        if(GUILayout.Button("\n Prev \n",customButton,GUILayout.Width(200 * scale), GUILayout.Height(50 * scale))) PrevScene();
        if(GUILayout.Button("\n Next \n",customButton,GUILayout.Width(200 * scale), GUILayout.Height(50 * scale))) NextScene();
        GUILayout.EndHorizontal();

        int currentpage = SceneManager.GetActiveScene().buildIndex;
        int totalpages = SceneManager.sceneCountInBuildSettings-1;
        GUILayout.Label( currentpage + " / " + totalpages + " " + SceneManager.GetActiveScene().name );

        GUILayout.EndArea();
    }

    public void NextScene()
    {
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;

        if (sceneIndex < SceneManager.sceneCountInBuildSettings - 1)
            SceneManager.LoadScene(sceneIndex + 1);
        else
            SceneManager.LoadScene(1);
    }

    public void PrevScene()
    {
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;

        if (sceneIndex > 1)
            SceneManager.LoadScene(sceneIndex - 1);
        else
            SceneManager.LoadScene(SceneManager.sceneCountInBuildSettings - 1);
    }
}
