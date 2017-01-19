using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class PlanetController : MonoBehaviour {

    public float deferentRadius;
    public float epicycleRadius;
    public float deferentSpeed;
    public float epicycleSpeed;
    public LineRenderer epicycleRenderer;
    public LineRenderer deferentRenderer;
    public Camera camera;

    public float frequencyThresholdLowerBound;
    public float frequencyThresholdUpperBound;
    public float audioForcePreExponentMultiplier;
    public float audioForceExponent;
    public float audioForcePostExponentMultipiler;
    public float visualPreExponentMultiplier;
    public float visualExponent;
    public Color colour;
    public Text text;

    private Vector3 cameraOffset;
    private float totalDeferentTravel = 0;
    private float totalEpicycleTravel = 0;
    private LineRenderer pathRenderer;
    private float audioForce = 0;
    private List<float> spectrumValues = new List<float>();
    private int spectrumSamplesCount;
    private List<Vector3> pathPointsList = new List<Vector3>();

    public void setAudioForce(float audioForce) {
        this.audioForce = Mathf.Clamp(Mathf.Pow(audioForce * audioForcePreExponentMultiplier, 
            audioForceExponent) * audioForcePostExponentMultipiler, 0, 0.95f);
        text.text = "Force: " + audioForce;
    }

    public void setSpectrumValues(List<float> spectrumValues) {
        this.spectrumValues = spectrumValues;
    }

    void Start() {
        if (camera != null) {
            cameraOffset = camera.transform.position - transform.position;
        }

        int maxPathPointCount = 2850;
        for (int i = 0; i < maxPathPointCount; i++) {
            pathPointsList.Add(new Vector3(deferentRadius + epicycleRadius, 0, 0));
        }

        pathRenderer = GetComponent<LineRenderer>();
    }

    float totalDotDeferentTravel = 0;
    float totalDotEpicycleTravel = 0;
    float remainingDotTravelThisFrame = 0;
    void Update() {
        colour.a = 0;
        Color endColor = colour;
        endColor.a = 0.94f;
        pathRenderer.SetColors(colour, endColor);

        totalDeferentTravel += deferentSpeed * audioForce;
        totalEpicycleTravel += epicycleSpeed * audioForce;
        float deferentCircumference = 2 * Mathf.PI * deferentRadius;
        float deferentPercent = totalDeferentTravel / deferentCircumference;
        float epicycleCircumference = 2 * Mathf.PI * epicycleRadius;
        float epicyclePercent = totalEpicycleTravel / epicycleCircumference;
        float deferentAngle = deferentPercent * Mathf.PI * 2;
        float epicycleAngle = epicyclePercent * Mathf.PI * 2;
        deferentRenderer.SetColors(Color.magenta, Color.cyan);
        float dotSeparation = Mathf.PI / 20.0f;

        remainingDotTravelThisFrame += audioForce;
        while (remainingDotTravelThisFrame >= dotSeparation) {
            float discreteDeferentAngle = (totalDotDeferentTravel/deferentCircumference) * Mathf.PI * 2;
            float discreteEpicycleAngle = (totalDotEpicycleTravel/epicycleCircumference) * Mathf.PI * 2;

            remainingDotTravelThisFrame -= dotSeparation;
            totalDotDeferentTravel += deferentSpeed * dotSeparation;
            totalDotEpicycleTravel += epicycleSpeed * dotSeparation;

            Vector3 point = pathPointsList[0];
            pathPointsList.RemoveAt(0);
            point.x = deferentRadius * Mathf.Cos(discreteDeferentAngle) + epicycleRadius * Mathf.Cos(discreteEpicycleAngle);
            point.z = deferentRadius * Mathf.Sin(discreteDeferentAngle) + epicycleRadius * Mathf.Sin(discreteEpicycleAngle);
            pathPointsList.Add(point);
        }

        Vector3 deferentPosition = new Vector3(
               deferentRadius * Mathf.Cos(deferentAngle),
               0,
               deferentRadius * Mathf.Sin(deferentAngle)
        );

        Vector3 epicyclePosition = new Vector3(
            epicycleRadius * Mathf.Cos(epicycleAngle),
            0,
            epicycleRadius * Mathf.Sin(epicycleAngle)
        );

        Vector3 newPosition = deferentPosition + epicyclePosition;
        transform.position = newPosition;

        for (int reverseIndex = pathPointsList.Count - 1; reverseIndex >= 0; reverseIndex--) {
            int forwardIndex = pathPointsList.Count - reverseIndex - 1;
            Vector3 pathPoint = pathPointsList[reverseIndex];
            if (forwardIndex < spectrumValues.Count) {
                float spectrumValue = spectrumValues[forwardIndex];
                float adjustedValue = Mathf.Pow(spectrumValue * visualPreExponentMultiplier, visualExponent);
                pathPoint.y = adjustedValue;
            } else {
                pathPoint.y = 0;
            }
            pathPointsList[reverseIndex] = pathPoint;
        }

        pathRenderer.SetVertexCount(pathPointsList.Count);
        pathRenderer.SetPositions((Vector3[]) pathPointsList.ToArray());

        if (camera != null) {
            camera.transform.position = deferentPosition + cameraOffset;
        }

        //drawDeferentAndEpicyclePaths(deferentPosition);
    }

    private void drawDeferentAndEpicyclePaths(Vector3 deferentPosition) {
        int max = 180;
        Vector3[] deferentPoints = new Vector3[max + 1];
        Vector3[] epicyclePoints = new Vector3[max + 1];
        for (int i = 0; i < max; i++) {
            float angle = 2 * Mathf.PI * (i / (float) max);

            Vector3 deferentPoint = new Vector3(
                deferentRadius * Mathf.Cos(angle),
                0,
                deferentRadius * Mathf.Sin(angle)
            );

            Vector3 epicyclePoint = new Vector3(
                epicycleRadius * Mathf.Cos(angle),
                0,
                epicycleRadius * Mathf.Sin(angle)
            );
            epicyclePoint += deferentPosition;

            deferentPoints[i] = deferentPoint;
            epicyclePoints[i] = epicyclePoint;
        }

        // Closes the polygons
        epicyclePoints[max] = epicyclePoints[0];
        deferentPoints[max] = deferentPoints[0];

        epicycleRenderer.SetVertexCount(epicyclePoints.Length);
        epicycleRenderer.SetPositions(epicyclePoints);

        deferentRenderer.SetVertexCount(deferentPoints.Length);
        deferentRenderer.SetPositions(deferentPoints);
    }
}
