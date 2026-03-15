using UnityEngine;

public class Explosion : MonoBehaviour
{
    public void Init(Color fruitColor)
    {
        var ps = GetComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = fruitColor;
    }
}