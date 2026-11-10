using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class ReciveDamage : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public TextMeshProUGUI HealthText;
    public Image HealthBar;
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
            Health -= 25;
            HealthText.text = Health.ToString();
            HealthBar.fillAmount = Health / 100F;
            Debug.Log("Current health" + Health);
				
				
            if (Health<=0)
            {
                Debug.Log("Player is dead");
                Destroy(gameObject);
            }    


             }

    }
}
