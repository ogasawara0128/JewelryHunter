using System;
using UnityEngine;

public class ScoreItem : MonoBehaviour
{
    public ItemData itemdata;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer.sprite = itemdata.itemSprite;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            player.AddScore(itemdata);

            Destroy(gameObject); //自分自身を消す
        }
    }
}
