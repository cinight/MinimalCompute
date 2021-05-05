using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleSceneSwitch : MonoBehaviour
{
    public float scale = 1f;
    public void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public void Start()
    {
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
        GUI.skin.label.fontSize = Mathf.RoundToInt ( 16 * scale );
        //GUI.backgroundColor = new Color(0, 0, 0, .80f);
        GUI.color = new Color(1, 1, 1, 1);
        float w = 410 * scale;
        float h = 90 * scale;
        GUILayout.BeginArea(new Rect(Screen.width - w -5, Screen.height - h -5, w, h), GUI.skin.box);

        GUILayout.BeginHorizontal();
        //GUI.backgroundColor = new Color(1, 1, 1, .80f);
        GUIStyle customButton = new GUIStyle("button");
        customButton.fontSize = GUI.skin.label.fontSize;
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
