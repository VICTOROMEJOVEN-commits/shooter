using UnityEngine;

public class ReciveDamage : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public int Health = 100;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Projectile"))
        {
            Debug.Log("Received damage from projectile");
            Destroy(collision.gameObject);
            Health -= 10;
            Debug.Log("Current health" + Health);
				
				
            if (Health<=0)
            {
                Destroy(gameObject);
            }    


             }

    }
}
