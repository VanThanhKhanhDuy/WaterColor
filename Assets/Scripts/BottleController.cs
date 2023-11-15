using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class BottleController : MonoBehaviour
{
    public Color[] bottleColors;
    public SpriteRenderer bottleMaskSR;
    public AnimationCurve scaleAndRotationMultiplierCurve;
    public AnimationCurve fillAmountCurve;
    public AnimationCurve rotationSpeedMultiplier;

    public float[] fillAmounts;
    public float[] rotationValues;
    private int rotationIndex = 0;
    [Range(0,4)]
    public int numberOfColorsInBottle = 4;
    public Color topColor;
    public int numberOfTopColorLayers;
    public BottleController bottleControllerRef;
    //public bool justThisBottle = false;
    private int numberOfColorsToTransfer = 0;
    public Transform leftRotationPoint;
    public Transform rightRotationPoint;
    private Transform chosenRotationPoint;
    private float directionMultiplier = 1.0f;
    Vector3 originalPosition;
    Vector3 startPosition;
    Vector3 endPosition;
    public LineRenderer lineRenderer;
    public AudioSource pourSound;
    public AudioSource filledSound;
    private bool hasPlayedSound = false;
    public bool win = false;
    private static List<BottleController> allBottlesInScene = new List<BottleController>();

    private void OnEnable()
    {
        allBottlesInScene.Add(this);
    }

    private void OnDisable()
    {
        allBottlesInScene.Remove(this);
    }
    void Start()
    {
        bottleMaskSR.material.SetFloat("_FillAmount", fillAmounts[numberOfColorsInBottle]);
        originalPosition = transform.position;
        UpdateColorsOnShader();
        UpdateTopColorValues();
    }
    void Update()
    {
        CheckAllColorsSame();
        // if(Input.GetKeyDown(KeyCode.Space) && justThisBottle == true){
        //     UpdateTopColorValues();
        //     if(bottleControllerRef.FillBottleCheck(topColor)){
        //         ChoseRotationPointAndDirection();
        //         numberOfColorsToTransfer = Mathf.Min(numberOfTopColorLayers, 4 - bottleControllerRef.numberOfColorsInBottle);
        //         for(int i = 0; i< numberOfColorsToTransfer; i++){
        //             bottleControllerRef.bottleColors[bottleControllerRef.numberOfColorsInBottle + i] = topColor;
        //         }
        //         bottleControllerRef.UpdateColorsOnShader();
        //     }
        //     CaculateRotationIndex(4 - bottleControllerRef.numberOfColorsInBottle);
        //     StartCoroutine(RotateBottle());
        // }
        CheckVictory();
        if(CheckVictory()){
            StartCoroutine(FewSecsBeforeTurnNextScene());
        }
    }
    public bool CheckAllColorsSame(){
    if (numberOfColorsInBottle == 4)
    {
        if (bottleColors[0] == bottleColors[1] && bottleColors[1] == bottleColors[2] && bottleColors[2] == bottleColors[3]){
            if (!hasPlayedSound){
                filledSound.Play();
                hasPlayedSound = true;
            }
            return true;
        }
        else{
            hasPlayedSound = false;
        }
    }

    return false;
    }

    public void StartColorTransfer(){
        if (!CheckAllColorsSame())
        {
        ChoseRotationPointAndDirection();
        numberOfColorsToTransfer = Mathf.Min(numberOfTopColorLayers, 4 - bottleControllerRef.numberOfColorsInBottle);
        for(int i = 0; i< numberOfColorsToTransfer; i++){
            bottleControllerRef.bottleColors[bottleControllerRef.numberOfColorsInBottle + i] = topColor;
        }
        bottleControllerRef.UpdateColorsOnShader();
        CaculateRotationIndex(4 - bottleControllerRef.numberOfColorsInBottle);
        transform.GetComponent<SpriteRenderer>().sortingOrder += 2;
        bottleMaskSR.sortingOrder += 2;
        StartCoroutine(MoveBottle());
        }
    }
    public float timeToRotate = 1.0f;
    void UpdateColorsOnShader(){
        bottleMaskSR.material.SetColor("_C1", bottleColors[0]);
        bottleMaskSR.material.SetColor("_C2", bottleColors[1]);
        bottleMaskSR.material.SetColor("_C3", bottleColors[2]);
        bottleMaskSR.material.SetColor("_C4", bottleColors[3]);
    }
    IEnumerator MoveBottle(){
        startPosition = transform.position;
        if(chosenRotationPoint == leftRotationPoint){
            endPosition = bottleControllerRef. rightRotationPoint.position;
        }else{
            endPosition = bottleControllerRef.leftRotationPoint.position;
        }
        float t = 0;

        while(t <= 1){
            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            t += Time.deltaTime * 2;
            yield return new WaitForEndOfFrame();
        }
        transform.position = endPosition;
        StartCoroutine(RotateBottle());
    }
    IEnumerator MoveBottleBack(){
        startPosition = transform.position;
        endPosition = originalPosition;
        float t = 0;

        while(t <= 1){
            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            t += Time.deltaTime * 2;
            yield return new WaitForEndOfFrame();
        }
        transform.position = endPosition;
        transform.GetComponent<SpriteRenderer>().sortingOrder -= 2;
        bottleMaskSR.sortingOrder -= 2;
    }
    IEnumerator RotateBottle(){
        float t = 0;
        float lerpValue;
        float angleValue;
        float lastAngleValue = 0;
        
        while(t< timeToRotate){
            lerpValue = t/timeToRotate;
            angleValue = Mathf.Lerp(0.0f, directionMultiplier * rotationValues[rotationIndex], lerpValue);

            //transform.eulerAngles = new Vector3(0, 0, angleValue);
            transform.RotateAround(chosenRotationPoint.position,Vector3.forward, lastAngleValue - angleValue);
            bottleMaskSR.material.SetFloat("_SARM", scaleAndRotationMultiplierCurve.Evaluate(angleValue));
            if(fillAmounts[numberOfColorsInBottle] > fillAmountCurve.Evaluate(angleValue) + 0.005f){
                if(lineRenderer.enabled == false){
                    // lineRenderer.startColor = topColor;
                    // lineRenderer.endColor = topColor;
                    lineRenderer.material.SetColor("_Color", topColor);
                    //Debug.Log(lineRenderer.startColor + " " + lineRenderer.endColor);
                    lineRenderer.SetPosition(0, chosenRotationPoint.position);
                    lineRenderer.SetPosition(1, chosenRotationPoint.position - Vector3.up * 1.45f);
                    lineRenderer.enabled = true;
                    if (!pourSound.isPlaying)
                    {
                        pourSound.Play();
                    }
                }
                bottleMaskSR.material.SetFloat("_FillAmount", fillAmountCurve.Evaluate(angleValue));
                bottleControllerRef.FillUp(fillAmountCurve.Evaluate(lastAngleValue) - fillAmountCurve.Evaluate(angleValue));
            }
            t += Time.deltaTime * rotationSpeedMultiplier.Evaluate(angleValue);
            lastAngleValue = angleValue;
            yield return new WaitForEndOfFrame();
        }
        angleValue = directionMultiplier *  rotationValues[rotationIndex];
        //transform.eulerAngles = new Vector3(0, 0, angleValue);
        bottleMaskSR.material.SetFloat("_SARM", scaleAndRotationMultiplierCurve.Evaluate(angleValue));
        bottleMaskSR.material.SetFloat("_FillAmount", fillAmountCurve.Evaluate(angleValue));
        numberOfColorsInBottle -= numberOfColorsToTransfer;
        bottleControllerRef.numberOfColorsInBottle += numberOfColorsToTransfer;
        lineRenderer.enabled = false;
        StartCoroutine(RotateBottleBack());
    }
    IEnumerator RotateBottleBack(){
        float t = 0;
        float lerpValue;
        float angleValue;
        float lastAngleValue = directionMultiplier * rotationValues[rotationIndex];
        
        while(t< timeToRotate){
            lerpValue = t/timeToRotate;
            angleValue = Mathf.Lerp(directionMultiplier * rotationValues[rotationIndex], 0.0f, lerpValue);

            //transform.eulerAngles = new Vector3(0, 0, angleValue);
            transform.RotateAround(chosenRotationPoint.position, Vector3.forward,lastAngleValue - angleValue);
            bottleMaskSR.material.SetFloat("_SARM", scaleAndRotationMultiplierCurve.Evaluate(angleValue));
            lastAngleValue = angleValue;
            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        UpdateTopColorValues();
        angleValue = 0;
        transform.eulerAngles = new Vector3(0, 0, angleValue);
        bottleMaskSR.material.SetFloat("_SARM", scaleAndRotationMultiplierCurve.Evaluate(angleValue));
        StartCoroutine(MoveBottleBack());
    }
    public void UpdateTopColorValues(){
        if(numberOfColorsInBottle != 0){
            numberOfTopColorLayers = 1;
            topColor = bottleColors[numberOfColorsInBottle - 1];
            if(numberOfColorsInBottle == 4){
                if(bottleColors[3].Equals(bottleColors[2])){
                    numberOfTopColorLayers = 2;
                    if(bottleColors[2].Equals(bottleColors[1])){
                     numberOfTopColorLayers = 3;
                     if(bottleColors[1].Equals(bottleColors[0])){
                        numberOfTopColorLayers = 4;
                     }
                    }
                }
            }
            else if(numberOfColorsInBottle == 3){
                if(bottleColors[2].Equals( bottleColors[1])){
                    numberOfTopColorLayers = 2;
                    if(bottleColors[1].Equals(bottleColors[0])){
                     numberOfTopColorLayers = 3;
                    }
                }
            }
            else if(numberOfColorsInBottle == 2){
                if(bottleColors[1].Equals( bottleColors[0])){
                    numberOfTopColorLayers = 2;
                }
            }
            rotationIndex = 3 - (numberOfColorsInBottle - numberOfTopColorLayers);
        }
    }
    public bool FillBottleCheck(Color colorToCheck){
        if(numberOfColorsInBottle == 0){
            return true;
        }
        else{
            if(numberOfColorsInBottle == 4){
                return false;
            }
            else{
                if(topColor.Equals(colorToCheck)){
                    return true;
                }
                else{
                    return false;
                }
            }
        }
    }
    private void CaculateRotationIndex(int numberOfEmtySpacesInSecondBottle){
        rotationIndex = 3 - (numberOfColorsInBottle - Mathf.Min(numberOfEmtySpacesInSecondBottle, numberOfTopColorLayers));
    }
    private void FillUp(float fillAmountToAdd){
        float currentFillAmount = bottleMaskSR.material.GetFloat("_FillAmount");
        float newFillAmount = Mathf.Clamp(currentFillAmount + fillAmountToAdd, -0.5f, 0.34f);
        bottleMaskSR.material.SetFloat("_FillAmount", newFillAmount);
    }
    private void ChoseRotationPointAndDirection(){
        if(transform.position.x > bottleControllerRef.transform.position.x){
            chosenRotationPoint = leftRotationPoint;
            directionMultiplier = -1.0f;
        }else{
            chosenRotationPoint = rightRotationPoint;
            directionMultiplier = 1.0f;
        }
    }
    void VictoryCondition(){
        win = false;
        if(CheckAllColorsSame() || numberOfColorsInBottle == 0){
            win = true;
        }
    }
    
    public static bool CheckVictory(){
        foreach (BottleController bottle in allBottlesInScene){
            bottle.VictoryCondition();
            if (!bottle.win){
            return false;
            }
        }
    
    Debug.Log("Victory Achieved!");
    return true;
    }
    IEnumerator FewSecsBeforeTurnNextScene(){
        yield return new WaitForSeconds(2f);
        SceneMana.SceneManagement();
    }
}
