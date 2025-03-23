using UnityEngine;
using System.Collections;

public class GammaChanger : MonoBehaviour
{
    public float minIntensity = 0.5f; // Минимальное значение Intensity Multiplier
    public float maxIntensity = 2.0f; // Максимальное значение Intensity Multiplier
    public float minTime = 60f;      // Минимальное время в секундах (1 минута)
    public float maxTime = 300f;     // Максимальное время в секундах (5 минут)

    private float targetIntensity;
    private float currentIntensity;
    private float transitionDuration;
    private float transitionTimer;

    void Start()
    {
        // Инициализация начальных значений
        currentIntensity = RenderSettings.ambientIntensity;
        StartCoroutine(ChangeIntensity());
    }

    void Update()
    {
        if (transitionTimer < transitionDuration)
        {
            // Плавное изменение Intensity Multiplier
            transitionTimer += Time.deltaTime;
            float t = transitionTimer / transitionDuration;
            float newIntensity = Mathf.Lerp(currentIntensity, targetIntensity, t);
            SetIntensity(newIntensity);
        }

        Debug.Log("Current Intencity: " + currentIntensity);
    }

    IEnumerator ChangeIntensity()
    {
        while (true)
        {
            // Ожидание случайного времени
            float waitTime = Random.Range(minTime, maxTime);
            yield return new WaitForSeconds(waitTime);

            // Установка нового целевого значения Intensity Multiplier
            currentIntensity = GetIntensity();
            targetIntensity = Random.Range(minIntensity, maxIntensity);
            transitionDuration = Random.Range(1f, 5f); // Длительность перехода от 1 до 5 секунд
            transitionTimer = 0f;
        }
    }

    float GetIntensity()
    {
        return RenderSettings.ambientIntensity;
    }

    void SetIntensity(float intensity)
    {
        RenderSettings.ambientIntensity = intensity;
    }
}
