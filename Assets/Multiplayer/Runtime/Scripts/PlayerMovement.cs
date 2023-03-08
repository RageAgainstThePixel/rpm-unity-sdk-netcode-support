using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    private static readonly int IsWalking = Animator.StringToHash(nameof(IsWalking));
    private readonly NetworkVariable<bool> isWalking =
        new NetworkVariable<bool>(writePerm: NetworkVariableWritePermission.Owner);

    [SerializeField] private float speed = 6f;
    [SerializeField] private PlayerAvatarLoader playerAvatarLoader;
    [SerializeField] private CharacterController controller;
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private Transform fireballSpawnTransform;
    
    private bool waitForPunchToFinish;

    private void Update()
    {
        if (playerAvatarLoader.Animator != null)
        {
            playerAvatarLoader.Animator.SetBool(IsWalking, isWalking.Value);
        }

        if (!IsOwner)
        {
            return;
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            SpawnFireballServerRpc();
        }

        var horizontalAxis = Input.GetAxis("Horizontal");
        var direction = new Vector3(horizontalAxis, 0, 0);

        if (direction.magnitude > 0f)
        {
            isWalking.Value = true;
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            var move = direction * (speed * Time.deltaTime);
            controller.Move(new Vector3(move.x, 0, 0));
        }
        else
        {
            isWalking.Value = false;
        }
    }

    [ServerRpc]
    private void SpawnFireballServerRpc()
    {
        var fireball = Instantiate(fireballPrefab);
        fireball.transform.position = fireballSpawnTransform.position;
        var fireballComponent = fireball.GetComponent<Fireball>();
        fireballComponent.player = gameObject;
        fireballComponent.SetDirection(transform.forward);

        fireball.GetComponent<NetworkObject>().Spawn();
    }
}
