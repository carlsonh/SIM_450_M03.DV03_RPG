using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;
using System;

public class GameManager : MonoBehaviourPun
{
    [Header("Players")]
    public PlayerController[] players;
    public string playerPrefabPath;
    public Transform[] spawnPoints;
    public float respawnTime;

    private int playersInGame;

    public static GameManager instance;
    void Awake()
    {
        if (instance != null && instance != this)
            gameObject.SetActive(false);
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }


    #region Player Creation
    [PunRPC]
    void ImInGame()
    {
        playersInGame++;
        Debug.Log("Player In Game");

        if (playersInGame == PhotonNetwork.PlayerList.Length)
        {
            Debug.Log("Player Spawned");
            SpawnPlayer();
        }
    }


    private void Start()
    {
        Debug.Log("GM Started");
        players = new PlayerController[PhotonNetwork.PlayerList.Length];
        photonView.RPC("ImInGame", RpcTarget.AllBuffered);
    }

    void SpawnPlayer()
    {
        Debug.Log("Player Spawned");
        GameObject playerGO = PhotonNetwork.Instantiate(playerPrefabPath, spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)].position, Quaternion.identity);

        //Init player
        playerGO.GetComponent<PhotonView>().RPC("Initialize", RpcTarget.All, PhotonNetwork.LocalPlayer);
    }

    #endregion Player Creation

}
