using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class DifficultySetter : MonoBehaviour
{
    [SerializeField] int difficulty;
    public GameObject loadButton;
    private void Awake()
    {
        DontDestroyOnLoad(this);

        loadButton.SetActive(false);
        if (Application.isEditor)
        {
            if (File.Exists(Application.persistentDataPath + "/SaveFiles/EDITORSAVES/PlayerInfo.json"))
            {
                loadButton.SetActive(true);
            }
        }
        else
        {
            if (File.Exists(Application.persistentDataPath + "/SaveFiles/PlayerInfo.json"))
            {
                loadButton.SetActive(true);
            }
        }

    }

    public void SetDifficulty(int val)
    {
        difficulty = val;
        StartCoroutine(BeginGame());
    }

    public void BeginLoadGame()
    {
        StartCoroutine(LoadGame());
    }

    private IEnumerator LoadGame()
    {
        SceneManager.LoadScene(1);
        yield return new WaitForSeconds(.01f);
        GameManager.Instance.Load();
    }

    private IEnumerator BeginGame()
    {
        SceneManager.LoadScene(1);
        yield return new WaitForSeconds(.01f);
        GameManager.Instance.difficulty = (GameManager.DifficultyOptions)difficulty;
    }
}
