using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;



public enum PickupType
{
    Gold,
    Health
}
public class Pickup : MonoBehaviourPun
{
    public PickupType type;
    public int value;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!PhotonNetwork.IsMasterClient)
        {//Only director handles pickups
            return;
        }
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.gameObject.GetComponent<PlayerController>();

            if (type == PickupType.Gold)
            {
                player.photonView.RPC("GiveGold", player.photonPlayer, value);
            }
            else if (type == PickupType.Health)
            {
                player.photonView.RPC("Heal", player.photonPlayer, value);
            }
            PhotonNetwork.Destroy(gameObject);

        }
    }


}