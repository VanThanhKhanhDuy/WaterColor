using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottleController : MonoBehaviour
{
    public Color[] bottleColors;
    public SpriteRenderer bottleMaskSR;
    public AnimationCurve scaleAndRotationMultiplierCurve;
    public AnimationCurve fillAmountCurve;
    public AnimationCurve rotationSpeedMultiplier;
    void Start()
    {
        UpdateColorsOnShader();
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space)){
            StartCoroutine(RotateBottle());
        }
    }
    public float timeToRotate = 1.0f;
    void UpdateColorsOnShader(){
        bottleMaskSR.material.SetColor("_C1", bottleColors[0]);
        bottleMaskSR.material.SetColor("_C2", bottleColors[1]);
        bottleMaskSR.material.SetColor("_C3", bottleColors[2]);
        bottleMaskSR.material.SetColor("_C4", bottleColors[3]);
    }
    IEnumerator RotateBottle(){
        float t = 0;
        float lerpValue;
        float angleValue;
        
        while(t< timeToRotate){
            lerpValue = t/timeToRotate;
            angleValue = Mathf.Lerp(0.0f, 90.0f, lerpValue);

            transform.eulerAngles = new Vector3(0, 0, angleValue);
            bottleMaskSR.material.SetFloat("_SARM", scaleAndRotationMultiplierCurve.Evaluate(angleValue));
            bottleMaskSR.material.SetFloat("_FillAmount", fillAmountCurve.Evaluate(angleValue));
            t += Time.deltaTime * rotationSpeedMultiplier.Evaluate(angleValue);
            yield return new WaitForEndOfFrame();
        }
        angleValue = 90.0f;
        transform.eulerAngles = new Vector3(0, 0, angleValue);
        bottleMaskSR.material.SetFloat("_SARM", scaleAndRotationMultiplierCurve.Evaluate(angleValue));
        bottleMaskSR.material.SetFloat("_FillAmount", fillAmountCurve.Evaluate(angleValue));
        StartCoroutine(RotateBottleBack());
    }
    IEnumerator RotateBottleBack(){
        float t = 0;
        float lerpValue;
        float angleValue;
        
        while(t< timeToRotate){
            lerpValue = t/timeToRotate;
            angleValue = Mathf.Lerp(90.0f, 0.0f, lerpValue);

            transform.eulerAngles = new Vector3(0, 0, angleValue);
            bottleMaskSR.material.SetFloat("_SARM", scaleAndRotationMultiplierCurve.Evaluate(angleValue));
            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        angleValue = 0;
        transform.eulerAngles = new Vector3(0, 0, angleValue);
        bottleMaskSR.material.SetFloat("_SARM", scaleAndRotationMultiplierCurve.Evaluate(angleValue));
    }
}
