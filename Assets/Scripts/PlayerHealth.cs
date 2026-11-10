using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maximumHealth = 100;
    public int Current { get; private set; }
    float immuneUntil;
    void Awake() { Current = maximumHealth; }
    public void TakeDamage(int damage)
    {
        if (damage <= 0 || Current <= 0 || Time.time < immuneUntil || (MatchGame.Instance && MatchGame.Instance.Finished)) return;
        Current = Mathf.Max(0, Current - damage);
        immuneUntil = Time.time + 0.65f;
        MatchGame.Instance?.ShowDamage();
        if (Current == 0) MatchGame.Instance?.Finish(false);
    }
}

