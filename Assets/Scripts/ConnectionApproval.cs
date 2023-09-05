using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor.Experimental.Rendering;
using UnityEngine;

public class ConnectionApproval : MonoBehaviour
{
    private const int MaxPlayers=2;

    private void Start()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        Debug.Log("Connection Approval Check");
        response.Approved = true;
        response.CreatePlayerObject = true;
        response.PlayerPrefabHash = null;
        if(NetworkManager.Singleton.ConnectedClientsList.Count>=MaxPlayers)
        {
            response.Approved = false;
            response.Reason = "Server is Full";
            Debug.Log("Connection failed : Server is full");
        }
        response.Pending = false;
    }

}
