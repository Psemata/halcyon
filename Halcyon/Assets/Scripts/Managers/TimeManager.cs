using System.Collections;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    [Header("Skyboxes")]
    [SerializeField] private Texture2D skyboxNight;
    [SerializeField] private Texture2D skyboxSunrise;
    [SerializeField] private Texture2D skyboxDay;
    [SerializeField] private Texture2D skyboxSunset;

    [Header("Light Gradients")]
    [SerializeField] private Gradient gradientNightToSunrise;
    [SerializeField] private Gradient gradientSunriseToDay;
    [SerializeField] private Gradient gradientDayToSunset;
    [SerializeField] private Gradient gradientSunsetToNight;

    [Header("References")]
    [SerializeField] private Light globalLight;

    public int Minutes { get; private set; }
    public int Hours { get; private set; } = 5;

    private float tempSecond;

    private void Start()
    {
        if (Hours >= 22 || Hours < 6)
        {
            RenderSettings.skybox.SetTexture("_Texture1", skyboxNight);
            RenderSettings.skybox.SetTexture("_Texture2", skyboxNight);
            RenderSettings.skybox.SetFloat("_Blend", 0f);
            globalLight.color = gradientSunsetToNight.Evaluate(1f);
            RenderSettings.fogColor = globalLight.color;
        }
        else if (Hours >= 6 && Hours < 8)
        {
            RenderSettings.skybox.SetTexture("_Texture1", skyboxNight);
            RenderSettings.skybox.SetTexture("_Texture2", skyboxSunrise);
            float blend = (Hours - 6 + Minutes / 60f) / 2f;
            RenderSettings.skybox.SetFloat("_Blend", blend);
            globalLight.color = gradientNightToSunrise.Evaluate(blend);
            RenderSettings.fogColor = globalLight.color;
        }
        else if (Hours >= 8 && Hours < 18)
        {
            RenderSettings.skybox.SetTexture("_Texture1", skyboxDay);
            RenderSettings.skybox.SetTexture("_Texture2", skyboxDay);
            RenderSettings.skybox.SetFloat("_Blend", 0f);
            globalLight.color = gradientSunriseToDay.Evaluate(1f);
            RenderSettings.fogColor = globalLight.color;
        }
        else if (Hours >= 18 && Hours < 22)
        {
            RenderSettings.skybox.SetTexture("_Texture1", skyboxDay);
            RenderSettings.skybox.SetTexture("_Texture2", skyboxSunset);
            float blend = (Hours - 18 + Minutes / 60f) / 4f;
            RenderSettings.skybox.SetFloat("_Blend", blend);
            globalLight.color = gradientDayToSunset.Evaluate(blend);
            RenderSettings.fogColor = globalLight.color;
        }

        float sunAngle = (Hours + Minutes / 60f) / 24f * 360f - 90f;
        globalLight.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);
    }

    private void Update()
    {
        tempSecond += Time.deltaTime;
        if (tempSecond >= 1f)
        {
            tempSecond = 0f;
            AddMinute();
        }
    }

    private void AddMinute()
    {
        Minutes++;
        globalLight.transform.Rotate(Vector3.up, (1f / (1440f / 4f)) * 360f, Space.World);

        if (Minutes >= 60)
        {
            Minutes = 0;
            AddHour();
        }
    }

    private void AddHour()
    {
        Hours++;
        if (Hours >= 24)
        {
            Hours = 0;
        }
        HandleHourTransition(Hours);
    }

    private void HandleHourTransition(int hour)
    {
        switch (hour)
        {
            case 6:
                StartTransition(skyboxNight, skyboxSunrise, gradientNightToSunrise);
                break;
            case 8:
                StartTransition(skyboxSunrise, skyboxDay, gradientSunriseToDay);
                break;
            case 18:
                StartTransition(skyboxDay, skyboxSunset, gradientDayToSunset);
                break;
            case 22:
                StartTransition(skyboxSunset, skyboxNight, gradientSunsetToNight);
                break;
        }
    }

    private void StartTransition(Texture2D from, Texture2D to, Gradient gradient)
    {
        StartCoroutine(LerpSkybox(from, to, 10f));
        StartCoroutine(LerpLight(gradient, 10f));
    }

    private IEnumerator LerpSkybox(Texture2D a, Texture2D b, float duration)
    {
        RenderSettings.skybox.SetTexture("_Texture1", a);
        RenderSettings.skybox.SetTexture("_Texture2", b);
        RenderSettings.skybox.SetFloat("_Blend", 0f);
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            RenderSettings.skybox.SetFloat("_Blend", t / duration);
            yield return null;
        }
        RenderSettings.skybox.SetTexture("_Texture1", b);
        RenderSettings.skybox.SetFloat("_Blend", 0f);
    }

    private IEnumerator LerpLight(Gradient gradient, float duration)
    {
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            var color = gradient.Evaluate(t / duration);
            globalLight.color = color;
            RenderSettings.fogColor = color;
            yield return null;
        }
    }
}
