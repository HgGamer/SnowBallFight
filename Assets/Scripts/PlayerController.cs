using System.Collections.Generic;
using System.Linq;
using SpacetimeDB;
using SpacetimeDB.Types;
using Unity.Cinemachine;
using UnityEngine;
using PimDeWitte.UnityMainThreadDispatcher;
using System.Collections;
public class PlayerController : MonoBehaviour
{

    const int SEND_UPDATES_PER_SEC = 20;
    const float SEND_UPDATES_FREQUENCY = 1f / SEND_UPDATES_PER_SEC;

    public static PlayerController Local { get; private set; }

    private uint PlayerId;
    private float LastMovementSendTimestamp;

    private Vector2 LockInputPosition;
    private List<PuppetController> OwnedPuppets = new List<PuppetController>();
    public int NumberOfOwnedPuppet => OwnedPuppets.Count;
    public bool IsLocalPlayer => this == Local;


    public void Initialize(Player player)
    {
        PlayerId = player.PlayerId;
        if (player.Identity == GameManager.LocalIdentity)
        {
            Local = this;
        }
        GameManager.GetObstacles();
    }
    private void OnDestroy()
    {
        // If we have any puppets, destroy them
        foreach (var puppet in OwnedPuppets)
        {
            if (puppet != null)
            {
                Destroy(puppet.gameObject);
            }
        }
        OwnedPuppets.Clear();
    }

    public void OnPuppetSpawned(PuppetController puppet)
    {
        OwnedPuppets.Add(puppet);
         if ( IsLocalPlayer )
        {
            GameObject.Find("CinemachineCamera").GetComponent<CinemachineCamera>().Target.TrackingTarget = OwnedPuppets[0].gameObject.transform;
        }
      

    }
    public void OnPuppetDeleted(PuppetController deletedPuppet)
    {
        // This means we got eaten
        if (OwnedPuppets.Remove(deletedPuppet) && IsLocalPlayer && OwnedPuppets.Count == 0)
        {
            // DeathScreen.Instance.SetVisible(true);
        }
    }

    private void OnGUI()
    {
        if (!IsLocalPlayer || !GameManager.IsConnected())
        {
            return;
        }

        //GUI.Label(new Rect(0, 0, 100, 50), $"Total Mass: {TotalMass()}");
    }
    // Update is called once per frame
    
    void Update()
    {
        if (!IsLocalPlayer || NumberOfOwnedPuppet == 0)
        {
            return;
        }
      
        if (Input.GetMouseButton(1)) // Right mouse button
        {
            float mouseX = Input.GetAxis("Mouse X");
            Rotate(mouseX);
        }
        if (Input.GetMouseButtonDown(0)) // Left mouse button
        {
            if(!OwnedPuppets[0].Puppet.HasSnowball){
                Debug.Log($"PlayerController: OnMouseDown: {PlayerId} , Crafting snowball");
                GameManager.CraftSnowBall(PlayerId);
            }else{
                var from =  OwnedPuppets[0].transform.position;
                Debug.Log($"PlayerController: OnMouseDown: {PlayerId} , Spawning snowball from {from}");
                GameManager.SpawnSnowBall(PlayerId, new Vector2(from.x, from.z));
            }
           
        }
        LockInputPosition = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // Throttled input requests
        if (Time.time - LastMovementSendTimestamp >= SEND_UPDATES_FREQUENCY)
        {
            LastMovementSendTimestamp = Time.time;
            var mousePosition = LockInputPosition;
            var direction = GetMoveDirection(mousePosition);
            GameManager.Conn.Reducers.UpdatePlayerInput(direction, transform.rotation.eulerAngles.y);
            LockInputPosition = new Vector2(0, 0);
        }
    }

    Vector2 GetMoveDirection(Vector2 direction)
    {

        Vector3 move = new Vector3(direction.x, 0, direction.y).normalized;
        move = transform.TransformDirection(move);
        // Return the XZ components as a Vector2 (worldspace movement)
        return new Vector2(move.x, move.z);
    }

    void Rotate(float mouseX)
    {
        float rotationSpeed = 3f;
        // Rotate player (and camera if camera is child)
        transform.Rotate(0, mouseX * rotationSpeed, 0);
    }
}
