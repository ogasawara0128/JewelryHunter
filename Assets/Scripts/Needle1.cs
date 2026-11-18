using UnityEngine;

public class Needle1 : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D collision) //PlayerControllerのOver()がOnTrigger始まりの為、OnCollisionでは殺せない
    {
        Destroy(gameObject);
    }
}
