using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public int numRoundsToWin = 5;
    public float startDelay = 3f;
    public float endDelay = 3f;
    public CameraControl cameraControl;
    public Text uiMessage;
    public GameObject tankPrefab;
    //Array de los tanques de las escuelas
    public TankManager[] tanks;
    //public TankManager[] selectedTanks;


    private int roundNumber;
    private WaitForSeconds startWait;
    private WaitForSeconds endWait;
    private TankManager roundWinner;
    private TankManager gameWinner;

    //Fixes a change to the physics engine
    const float maxDepenetrationVelocity = float.PositiveInfinity;

    private void Start()
    {
        //Applying the fix to the physics engine
        Physics.defaultMaxDepenetrationVelocity = maxDepenetrationVelocity;
        
        startWait = new WaitForSeconds(startDelay);
        endWait = new WaitForSeconds(endDelay);

        SpawnAllTanks();
        SetCameraTargets();

        //Siempre que se llame una corrutina se pone dentro de un StartCoroutine porque dentro tiene uno o varios "yield return" aunque en estos tambien tengan StartCoroutine's con sus respectivas corrutinas, como es el caso de este GameLoop
        StartCoroutine(GameLoop());
    }


    private void SpawnAllTanks()
    {
        /*for (int i = 0; i < 2; i++)
        {
            selectedTanks[i] = tanks[Random.Range(0, tanks.Length)];
        }

        for (int i = 0; i < selectedTanks.Length; i++)
        {
            selectedTanks[i].instance =
                Instantiate(tankPrefab, selectedTanks[i].spawnPoint.position, selectedTanks[i].spawnPoint.rotation) as GameObject;
            selectedTanks[i].playerNumber = i + 1;
            selectedTanks[i].Setup();
        }*/

        for (int i = 0; i < tanks.Length; i++)
        {
            tanks[i].instance = Instantiate(tankPrefab, tanks[i].spawnPoint.position, tanks[i].spawnPoint.rotation);
            tanks[i].playerNumber = i + 1;
            tanks[i].Setup();
        }
    }


    private void SetCameraTargets()
    {
        Transform[] targets = new Transform[tanks.Length];
        //Transform[] targets = new Transform[selectedTanks.Length];

        for (int i = 0; i < targets.Length; i++)
        {
            targets[i] = tanks[i].instance.transform;
            //targets[i] = selectedTanks[i].instance.transform;
        }

        cameraControl.targetsToFollow = targets;        
    }


    private IEnumerator GameLoop()
    {
        yield return StartCoroutine(RoundStarting());
        yield return StartCoroutine(RoundPlaying());
        yield return StartCoroutine(RoundEnding());

        if (gameWinner != null)
        {
            SceneManager.LoadScene(0);
            
            //Escena de creditos
            //SceneManager.LoadScene(1);
        }
        else
        {
            StartCoroutine(GameLoop());
        }
    }


    private IEnumerator RoundStarting()
    {
        ResetAllTanks();
        DisableTankControl();

        cameraControl.SetStartPositionAndSize();

        roundNumber++;
        uiMessage.text = "ROUND " + roundNumber;

        yield return startWait;
    }


    private IEnumerator RoundPlaying()
    {
        EnableTankControl();

        uiMessage.text = string.Empty;

        while (!OneTankLeft())
        {
            yield return null;
        }
    }


    private IEnumerator RoundEnding()
    {
        DisableTankControl();

        roundWinner = null;

        roundWinner = GetRoundWinner();

        if (roundWinner != null)
        {
            roundWinner.wins++;
        }

        gameWinner = GetGameWinner();

        string message = EndMessage();
        uiMessage.text = message;

        yield return endWait;
    }


    private bool OneTankLeft()
    {
        int numTanksLeft = 0;

        for (int i = 0; i < tanks.Length; i++)
        {
            if (tanks[i].instance.activeSelf)
                numTanksLeft++;
        }

        return numTanksLeft <= 1;
    }


    //Es tipo TankManager porque regresa un tanque con el return del if en el for, pero si no se cumple el if entonces la funcion regresara null
    private TankManager GetRoundWinner()
    {
        for (int i = 0; i < tanks.Length; i++)
        {
            if (tanks[i].instance.activeSelf)
            {
                return tanks[i];
            }
        }

        return null;
    }


    private TankManager GetGameWinner()
    {
        for (int i = 0; i < tanks.Length; i++)
        {
            if (tanks[i].wins == numRoundsToWin)
                return tanks[i];
        }

        return null;
    }


    private string EndMessage()
    {
        string message = "";

        if (roundWinner != null)
        {
            message = roundWinner.coloredPlayerText + " WINS THE ROUND!";
        }

        message += "\n\n\n\n";

        for (int i = 0; i < tanks.Length; i++)
        {
            message += tanks[i].coloredPlayerText + ": " + tanks[i].wins + " WINS\n";
        }

        //Aunque pareciera que esta linea va a quedar debajo de las que se generan arriba, no es asi, porque lo que hace es reasignar el valor de uiMessage del que ya se le habia dado arriba, por eso dice =, no +=
        if (gameWinner != null)
        {
            message = gameWinner.coloredPlayerText + " WINS THE GAME!";
        }
        
        return message;
    }


    private void ResetAllTanks()
    {
        for (int i = 0; i < tanks.Length; i++)
        {
            tanks[i].Reset();
        }
    }


    private void EnableTankControl()
    {
        for (int i = 0; i < tanks.Length; i++)
        {
            tanks[i].EnableControl();
        }
    }


    private void DisableTankControl()
    {
        for (int i = 0; i < tanks.Length; i++)
        {
            tanks[i].DisableControl();
        }
    }
}