using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Health : NetworkBehaviour
{
    private bool isInstantKill = false;
    SceneChanger changer;
    public bool isdead = false;
    [SerializeField] Movement movement;
    [SerializeField] IgnoreCollisions ignorecollision;
    [SerializeField] Balance[] balances;
    [SerializeField] Rigidbody2D[] rigidbodies;
    [SerializeField] CapsuleCollider2D[] capsuleCollider;
    [SerializeField] CircleCollider2D headColider;
    [SerializeField] Arms arm;

    // Ensure that only the server can modify this variable
    public NetworkVariable<int> health = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField] private TextMeshProUGUI healthtext;

    private void Awake()
    {
        changer = GameObject.FindGameObjectWithTag("SceneManager").GetComponent<SceneChanger>();
        
    }

    private void Start()
    {
        health.OnValueChanged += OnHealthChanged; // Update UI for all clients
    }

    private void Update()
    {
        if (IsOwner)
        {
            if (health.Value <= 0)
            {
                healthtext.text = "0";
                /*foreach (Rigidbody2D rb in rigidbodies)
                {
                    rb.velocity = Vector3.zero;
                }*/

                dead();
            }
            else
            {
                healthtext.text = health.Value.ToString();
            }
        }
    }

    private void OnHealthChanged(int oldValue, int newValue)
    {
        healthtext.text = newValue.ToString();
    }

    [ServerRpc]
    public void TakeDamageServerRpc(int body_part)
    {
        switch (body_part)
        {
            case 0:
                health.Value -= Random.Range(5, 25);
                break;
            case 1:
                health.Value -= Random.Range(5, 10);
                break;
            case 2:
                health.Value -= Random.Range(5, 10);
                break;
            case 3:
                health.Value -= Random.Range(5, 15);
                break;
            default:
                health.Value -= Random.Range(5, 20);
                break;
        }
    }

    [ServerRpc]
    public void InstantKillServerRpc()
    {
        isInstantKill = true;
        health.Value = 0;
    }


    private void dead()
    {
        if (IsOwner)
        {
            if (!isdead)
            {
                reduce_alivecountServerRpc();
                movement.enabled = false;
                ignorecollision.enabled = false;
                arm.enabled = false;
                foreach (Balance balance in balances)
                {
                    balance.enabled = false;
                }

                if (isInstantKill)
                {
                    foreach (Rigidbody2D rb in rigidbodies)
                    {
                        rb.gravityScale = 0;
                        rb.velocity = Vector2.zero;
                        rb.angularVelocity = 0f;
                        rb.constraints = RigidbodyConstraints2D.FreezeAll;
                    }
                    Debug.Log("Movement disabled");
                }
                

                isdead = true;
            }
        }
    }



    [ClientRpc]
    private void BringAliveClientRpc()
    {
        StartCoroutine(bringAlivecount());
    }

    IEnumerator bringAlivecount()
    {
        yield return new  WaitForSeconds(3f);
        if (isdead)
        {
            movement.enabled = true;
            ignorecollision.enabled = true;
            arm.enabled = true;

            foreach (Balance balance in balances)
            {
                balance.enabled = true;
            }

            foreach (Rigidbody2D rb in rigidbodies)
            {
                rb.gravityScale = 1;
                rb.constraints = RigidbodyConstraints2D.None;
            }
            isdead = false;
            isInstantKill = false;
        }
    }

    [ServerRpc]
    private void reduce_alivecountServerRpc()
    {
        changer.UpdateAliveCountServerRpc();
    }

    // Health reset is done on the server and synced to all clients
    [ServerRpc(RequireOwnership = false)]
    public void ResetHealthServerRpc()
    {
        health.Value = 100; // Reset health to default value on server
        BringAliveClientRpc();
    }
}
