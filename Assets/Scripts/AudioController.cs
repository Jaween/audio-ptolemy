using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioController : MonoBehaviour {

    public PlanetController[] planets;
    public float spectraCutOffRatio;
    public bool showSpectrum;
    public Camera tempCamera;

    private AudioSource visualiserAudioSource;
    private int sampleCount = 8192;

    void Start() {
        visualiserAudioSource = GetComponent<AudioSource>();
        visualiserAudioSource.enabled = false;
    }

    // Update is called once per frame
    void Update () {
        float[] channelASpectrum = new float[sampleCount];
        float[] channelBSpectrum = new float[sampleCount];
        int channelA = 0;
		int channelB = 1;
        FFTWindow window = FFTWindow.BlackmanHarris;

        int usefulSampleCount = (int) (spectraCutOffRatio * sampleCount);

        bool useVisualiserAudioSource = false;
        foreach (PlanetController planet in planets) {
            if (!planet.isActiveAndEnabled) {
                continue;
            }

            // Planets can each have their own AudioSource or just use our own AudioSource
            AudioSource audioSource = planet.GetComponent<AudioSource>();
            if (audioSource == null) {
                audioSource = visualiserAudioSource;
                useVisualiserAudioSource = true;
                Debug.Log("Use happy days toy town");
            }

            audioSource.GetSpectrumData(channelASpectrum, channelA, window);
            audioSource.GetSpectrumData(channelBSpectrum, channelB, window);

            float audioForce = 0;
            int frequencyCount = 0;

            List<float> spectrumValues = new List<float>();
            for (int i = 0; i < usefulSampleCount; i++) {
                float monoSpectrumValue = channelASpectrum[i] + channelBSpectrum[i];

                float indexRatio = i / (float) usefulSampleCount;
                if (indexRatio >= planet.frequencyThresholdLowerBound &&
                    indexRatio <= planet.frequencyThresholdUpperBound) {
                    frequencyCount++;
                    audioForce += monoSpectrumValue;
                    spectrumValues.Add(monoSpectrumValue);
                }
            }
            audioForce /= (float) frequencyCount;
            planet.setAudioForce(audioForce);
            planet.setSpectrumValues(spectrumValues);
        }

        if (useVisualiserAudioSource) {
            visualiserAudioSource.enabled = true;
        }

        if (showSpectrum) {
            Vector3[] spectrumPoints = new Vector3[usefulSampleCount];
            float width = 10;
            for (int i = 0; i < usefulSampleCount; i++) {
                float monoSpectrumValue = channelASpectrum [i] + channelBSpectrum [i];
                Vector3 point = spectrumPoints [i];
                point.x = tempCamera.transform.position.x - width / 2 + i * width / (float)usefulSampleCount;
                point.y = monoSpectrumValue * 20;
                point.z = tempCamera.transform.position.z + 5;
                spectrumPoints [i] = point;
            }
            GetComponent<LineRenderer>().SetVertexCount(usefulSampleCount);
            GetComponent<LineRenderer>().SetPositions(spectrumPoints);
        } else {
            GetComponent<LineRenderer>().SetVertexCount(0);
        }
    }
}
