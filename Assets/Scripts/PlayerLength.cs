using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro.EditorUtilities;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine.XR;
using UnityEditor.Timeline.Actions;

public class PlayerLength : NetworkBehaviour
{

    [SerializeField] private GameObject tailPrefab;
    public NetworkVariable<ushort> length = new(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private Transform _lastTail;
    private Collider2D _collider2D;
    private List<GameObject> tails;

    [CanBeNull] public static event System.Action<ushort> changedLengthEvent;


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _lastTail = transform;
        _collider2D = GetComponent<Collider2D>();
        tails = new List<GameObject>();

        if (!IsServer) length.OnValueChanged += LengthChangedEvent;


    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        DestroyTails();
    }
    void DestroyTails()
    {
        while(tails.Count>0)
        {
            GameObject tail = tails[0];
            tails.RemoveAt(0);
            Destroy(tail);
        }
    }
    private void LengthChangedEvent(ushort previous, ushort current)
    {
        Debug.Log("Length Changed Callback");
        LengthChanged();
    }

    private void LengthChanged()
    {
        InstantiateTail();

        if (!IsOwner) return;
        changedLengthEvent?.Invoke(length.Value);
        ClientMusicPlayer.Instance.PlayNomAudioClip();
    }

    //Called by the server only
    [ContextMenu("Add Length")] 
    public void AddLength()
    {
        length.Value += 1;
        LengthChanged();
    }
    private void InstantiateTail()
    {
        GameObject tail = Instantiate(tailPrefab, transform.position, Quaternion.identity);
        //tail.GetComponent<SpriteRenderer>().sortingLayerID = -length.Value;
        if(tail.TryGetComponent(out Tail _tail))
        {
            _tail.networkedOwner = transform;
            _tail.followTransform = _lastTail;
            _lastTail = tail.transform;
            Physics2D.IgnoreCollision(tail.GetComponent <Collider2D>(), _collider2D);
            
        }
        tails.Add(tail);
    }

}
