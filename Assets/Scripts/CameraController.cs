using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CameraController : MonoBehaviour
{
    float[] sectionCeilings = {-5f, 5f, 15f, 25f, 35f, 45f, 55f};
    float sectionHeight = 10f;
    int sectionNum;
    float cameraSpeed = 37.5f;
    float positionTolerance = 0.1f;
    bool newSection;
    bool shake;
    bool shakeStarted;
    float shakeStartY;
    float shakeStartTime;
    float shakeDistance = 0.6f;
    float shakeDuration = 0.25f;
    public PlayerController player;
    float mapStartY;
    float mapEndY = 17f;
    GameObject endFadeBox;
    public TMP_Text progressText;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector3(0, 0, -10); 
        sectionNum = 1;
        newSection = false;
        shake = false;
        shakeStarted = false;
        endFadeBox = transform.GetChild(0).gameObject;
        endFadeBox.SetActive(false);
        mapStartY = player.gameObject.transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        float playerY = player.gameObject.transform.position.y;

        // Player reached top -> trigger transition to win scene
        if (playerY >= mapEndY)
        {
            player.rb.velocity = new Vector2(0, 0);
            player.rb.gravityScale = 0f;
            endFadeBox.SetActive(true);
        }

        if (!newSection)
        {
            if (playerY > sectionCeilings[sectionNum])
            {
                sectionNum++;
                newSection = true;
            }
            else if (playerY < sectionCeilings[sectionNum - 1])
            {
                sectionNum--;
                newSection = true;
            }
        }

        if (newSection)
        {
            moveCamera(sectionCeilings[sectionNum] - sectionHeight / 2);
        }

        if (shake)
        {
            cameraShakePrivate();
        }

        progressText.text = ((int) ((playerY - mapStartY)/ (mapEndY - mapStartY) * 100)).ToString() + "%";
    }

    void moveCamera(float targetY)
    {
        // Move camera to target y position, with a speed that gradually decreases as camera approaches target
        float newY = transform.position.y;
        if (transform.position.y < targetY - positionTolerance)
        {
            newY += cameraSpeed * (Mathf.Abs(targetY - transform.position.y) / sectionHeight + 0.1f) * Time.deltaTime;
        }
        else if (transform.position.y > targetY + positionTolerance)
        {
            newY -= cameraSpeed * (Mathf.Abs(targetY - transform.position.y) / sectionHeight + 0.1f) * Time.deltaTime;
        }
        else
        {
            newY = targetY;
            newSection = false;
        }

        transform.position = new Vector3(transform.position.x, newY, -10);
    }

    public void cameraShake()
    {
        // Initiate shake if camera not panning to new section
        if (!newSection)
        {
            shakeStartY = transform.position.y;
            shakeStartTime = Time.time;
            shake = true;
        }
    }

    void cameraShakePrivate()
    {
        // If camera returns to start position and is traveling upwards, shake ends
        if (Mathf.Abs(transform.position.y - shakeStartY) <= shakeDistance / 0.25f && shakeStarted)
        {
            shake = false;
            shakeStarted = false;
            transform.position = new Vector3(transform.position.x, shakeStartY, -10);
        }
        else
        {
            // Update position according to a sinusoidal function
            float newY = -1 * shakeDistance * Mathf.Sin(2 * Mathf.PI / shakeDuration * (Time.time - shakeStartTime)) + shakeStartY;
            transform.position = new Vector3(transform.position.x, newY, -10);

            // If camera moves sufficiently down, shake has started
            if (newY > shakeStartY - shakeDistance / 2 && !shakeStarted)
            {
                shakeStarted = true;
            }
        }
    }
}
