using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviourPun
{
    [HideInInspector]
    public int id;


    [Header("Info")]
    public float moveSpeed;
    public int gold;
    public int curHp;
    public int maxHp;
    public bool dead;


    [Header("Attack")]
    public int damage;
    public float attackRange;
    public float attackRate;
    private float lastAttackTime;


    [Header("Components")]
    public Rigidbody2D rb;
    public Player photonPlayer;
    public SpriteRenderer sr;
    public Animator weaponAnim;
    public HeaderInfo headerInfo;

    //Local player
    public static PlayerController me;




    [PunRPC]
    public void Initialize(Player player)
    {
        id = player.ActorNumber;
        photonPlayer = player;

        headerInfo.Initialize(player.NickName, maxHp);

        if (player.IsLocal)
        {
            me = this;
        }
        else
        {
            rb.isKinematic = true;
        }

        GameManager.instance.players[id - 1] = this;
    }


    private void Update()
    {
        if (!photonView.IsMine)
        {
            return;
        }


        Move();

        if (Input.GetMouseButtonDown(0) && Time.time - lastAttackTime >= attackRate)
        {
            Attack();
        }



        //Flip the player by facing direction
        float mouseX = (Screen.width / 2) - Input.mousePosition.x;

        if (mouseX < 0)
        {
            weaponAnim.transform.parent.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            weaponAnim.transform.parent.localScale = new Vector3(1, 1, 1);
        }
    }


    void Move()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        //Apply to rb vel
        rb.velocity = new Vector2(x, y) * moveSpeed;
    }







    #region Health & Damage


    void Attack()
    {
        lastAttackTime = Time.time;

        //Calc direction
        Vector3 dir = (Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position)).normalized / 5f;

        //Raycast that dir
        RaycastHit2D hit = Physics2D.Raycast(transform.position + dir, dir, attackRange);
        Debug.DrawRay(transform.position + dir, dir * attackRange, Color.green, 1f);

        //Hit an enemy?
        if (hit.collider != null && hit.collider.gameObject.CompareTag("Enemy"))
        {
            //Deal the dmg
            Enemy enemy = hit.collider.GetComponent<Enemy>();
            enemy.photonView.RPC("TakeDamage", RpcTarget.MasterClient, damage);
        }

        //Play anim
        weaponAnim.SetTrigger("Attack");
    }


    [PunRPC]
    void Heal(int amountToHeal)
    {
        curHp = Mathf.Clamp(curHp + amountToHeal, 0, maxHp);

        //Update health bar
        headerInfo.photonView.RPC("UpdateHealthBar", RpcTarget.All, curHp);
    }



    [PunRPC]
    public void TakeDamage(int damage)
    {
        curHp -= damage;

        //Update health bar
        headerInfo.photonView.RPC("UpdateHealthBar", RpcTarget.All, curHp);


        if (curHp <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(DamageFlash());

            IEnumerator DamageFlash()
            {
                sr.color = Color.red;
                yield return new WaitForSeconds(0.05f);
                sr.color = Color.white;
            }
        }
    }

    void Die()
    {
        dead = true;
        rb.isKinematic = true;

        transform.position = new Vector3(0, 99, 0);

        Vector3 spawnPos = GameManager.instance.spawnPoints[Random.Range(0, GameManager.instance.spawnPoints.Length)].position;

        StartCoroutine(Spawn(spawnPos, GameManager.instance.respawnTime));
    }

    IEnumerator Spawn(Vector3 spawnPos, float timeToSpawn)
    {
        yield return new WaitForSeconds(timeToSpawn);

        dead = false;
        transform.position = spawnPos;
        curHp = maxHp;
        rb.isKinematic = false;

        //Update health bar
        headerInfo.photonView.RPC("UpdateHealthBar", RpcTarget.All, curHp);
    }

    #endregion Health & Damage


    #region Gold

    [PunRPC]
    void GiveGold(int goldToGive)
    {
        gold += goldToGive;

        //Update UI
        GameUI.instance.UpdateGoldText(gold);
    }

    #endregion Gold
}
