using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class DifficultySetter : MonoBehaviour
{
    public static DifficultySetter Instance { get; private set; }
    [SerializeField] int difficulty;
    public GameObject loadButton;
    public bool goingToLoad;
    private bool goingToHost;
    private bool goingToJoin;
    private string username;
    private string lobbyCode;
    [SerializeField] private GameObject warningPage;
    [SerializeField] private GameObject lobbyPage;
    private void Awake()
    {
        Instance = this;

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

    public void ResetAllFields()
    {
        lobbyCode = "";
        username = "";
        goingToHost = false;
        goingToJoin = false;
        goingToLoad = false;
    }

    public void SetUsername(string val)
    {
        username = val;
    }

    public void SetLobbyCode(string val)
    {
        lobbyCode = val;
    }

    public void SetAsHost()
    {
        goingToHost = true;
    }

    public void SetAsJoin()
    {
        goingToJoin = true;
    }

    public void EnterGame()
    {
        StartCoroutine(WaitToBeginGame());
    }

    public void JoinGame()
    {
        if (lobbyCode != null && lobbyCode != "")
        {
            Debug.Log(lobbyCode);
            StartCoroutine(WaitToJoinGame());
        }
    }

    private IEnumerator WaitToJoinGame()
    {
        SceneManager.LoadScene(1);
        yield return new WaitForSeconds(.1f);
        GameManager.Instance.JoinServer(lobbyCode);
    }

    public void ContinueButton()
    {
        if (goingToHost)
        {
            if (goingToLoad)
            {
                StartCoroutine(BeginGame());
            }
            else
            {
                warningPage.SetActive(true);
            }
            return;
        }

        if (!goingToLoad && !goingToJoin)
        {
            warningPage.SetActive(true);
        }
        if (goingToJoin)
        {
            lobbyPage.SetActive(true);
            return;
        }
        warningPage.SetActive(true);

    }

    public void SoloGameButton(GameObject obj)
    {
        if (!goingToLoad)
        {
            warningPage.SetActive(true);
            obj.SetActive(false);
        }
        else
        {
            StartCoroutine(WaitToBeginGame());
        }
    }

    private IEnumerator WaitToBeginGame()
    {
        SceneManager.LoadScene(1);
        yield return new WaitForSeconds(.1f);

        if (goingToLoad)
        {
            if (goingToHost)
            {
                GameManager.Instance.HostServer();
            }
            else
            {
                GameManager.Instance.StartSinglePlayer();
            }
        }
        else
        {
            if (goingToHost)
            {
                GameManager.Instance.HostServer();
            }
            else
            {
                GameManager.Instance.StartSinglePlayer();
            }
        }
    }

    public void SetAsLoad()
    {
        goingToLoad = true;
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
        goingToLoad = true;
    }

    private IEnumerator BeginGame()
    {
        SceneManager.LoadScene(1);
        yield return new WaitForSeconds(.01f);

        if (goingToHost)
        {
            GameManager.Instance.HostServer();
        }
        else
        {
            GameManager.Instance.StartSinglePlayer();
        }
    }
}
