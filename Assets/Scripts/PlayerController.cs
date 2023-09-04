using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using Unity.Netcode;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine.UIElements;
using JetBrains.Annotations;
using Unity.VisualScripting;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float rotationSpeed;
    private Camera _mainCamera;
    private Vector3 _mouseInput ;
    private PlayerLength thisPlayerLength;

    [CanBeNull] public static event Action GameOverEvent;
    private readonly ulong[] _targetClientArray = new ulong[1];
    private void Initialize()
    {
        _mainCamera = Camera.main;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Initialize();
    }
    private void Start()
    {
        if(_mainCamera==null)
        _mainCamera = Camera.main;
        thisPlayerLength = GetComponent<PlayerLength>();
    }

    private void Update()
    {
        if (!IsOwner ||  !Application.isFocused) return;
        _mouseInput.x = Input.mousePosition.x;
        _mouseInput.y = Input.mousePosition.y;
        _mouseInput.z = _mainCamera.nearClipPlane;
        Vector3 mouseWorldCoordinates = _mainCamera.ScreenToWorldPoint(_mouseInput);
        mouseWorldCoordinates.z = 0f;
        transform.position =
            Vector3.MoveTowards(transform.position, mouseWorldCoordinates, Time.deltaTime * speed);

        //Rotation
        if (mouseWorldCoordinates != transform.position)
        {
            Vector3 targetDirection = mouseWorldCoordinates - transform.position;
            targetDirection.z = 0f;
            Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, targetDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void DetermineCollisionWinnerServerRpc(PlayerData player1, PlayerData player2)
    {
        if (player1.Length > player2.Length)
        {
            WinInformationServerRpc(player1.Id, player2.Id);
        }
        else
        {
            WinInformationServerRpc(player2.Id, player1.Id);
        }
    }

    [ServerRpc]
    private void WinInformationServerRpc(ulong winner, ulong loser)
    {
        _targetClientArray[0] = winner;
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = _targetClientArray
            }
        };
        AtePlayerClientRpc(clientRpcParams);

        _targetClientArray[0] = loser;
        clientRpcParams.Send.TargetClientIds = _targetClientArray;
        
        GameOverClientRpc(clientRpcParams);

    }


    [ClientRpc]
    private void AtePlayerClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if (!IsOwner) return;
        Debug.Log("You ate a palyer");
    }

    [ClientRpc]
    private void GameOverClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if (!IsOwner) return;
        Debug.Log("You Lose!");
        GameOverEvent?.Invoke(); 
        NetworkManager.Singleton.Shutdown();
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Player collision");
        if (!collision.gameObject.CompareTag("Player")) return;
        if (!IsOwner) return;

        //head-on collision
        if (collision.gameObject.TryGetComponent(out PlayerLength playerLength))
        {
            var player1 = new PlayerData()
            {
                Id = OwnerClientId,
                Length = thisPlayerLength.length.Value
            };
            var player2 = new PlayerData()
            {
                Id = playerLength.OwnerClientId,
                Length = playerLength.length.Value
            };

            DetermineCollisionWinnerServerRpc(player1, player2);
        }
        else if(collision.gameObject.TryGetComponent(out Tail tail))
        {
            Debug.Log("Tail collision");
            WinInformationServerRpc(tail.networkedOwner.GetComponent<PlayerController>().OwnerClientId, OwnerClientId);
        }
    }

    struct PlayerData : INetworkSerializable
    {
        public ulong Id;
        public ushort Length;

        void INetworkSerializable.NetworkSerialize<T>(BufferSerializer<T> serializer)
        {
            serializer.SerializeValue(ref Id);
            serializer.SerializeValue(ref Length);
        }
    }

}
