using UnityEngine;
using System.Collections;

public class GammaChanger : MonoBehaviour
{
    public float minIntensity = 0.5f; // ����������� �������� Intensity Multiplier
    public float maxIntensity = 2.0f; // ������������ �������� Intensity Multiplier
    public float minTime = 60f;      // ����������� ����� � �������� (1 ������)
    public float maxTime = 300f;     // ������������ ����� � �������� (5 �����)

    private float targetIntensity;
    private float currentIntensity;
    private float transitionDuration;
    private float transitionTimer;

    void Start()
    {
        // ������������� ��������� ��������
        currentIntensity = RenderSettings.ambientIntensity;
        StartCoroutine(ChangeIntensity());
    }

    void Update()
    {
        if (transitionTimer < transitionDuration)
        {
            // ������� ��������� Intensity Multiplier
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
            // �������� ���������� �������
            float waitTime = Random.Range(minTime, maxTime);
            yield return new WaitForSeconds(waitTime);

            // ��������� ������ �������� �������� Intensity Multiplier
            currentIntensity = GetIntensity();
            targetIntensity = Random.Range(minIntensity, maxIntensity);
            transitionDuration = Random.Range(1f, 5f); // ������������ �������� �� 1 �� 5 ������
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
