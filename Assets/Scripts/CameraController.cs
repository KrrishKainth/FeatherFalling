using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    float mapEndY = 55f;
    GameObject endFadeBox;
    public Text progressText;
    public Text timerText;
    bool complete;
    public GameObject overlay;
    AudioSource audioSource;
    float fadeTime = 3f;

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
        complete = false;
        audioSource = gameObject.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!complete)  // If player not at end
        {
            float playerY = player.gameObject.transform.position.y;

            // Player reached top -> trigger transition to win scene
            if (playerY >= mapEndY)
            {
                // Game time
                string minutes = ((int)(player.inGameTimer / 60)).ToString();
                int sec = (int) (player.inGameTimer % 60);
                string seconds;
                if (sec < 10)
                {
                    seconds = "0" + sec.ToString();
                }
                else
                {
                    seconds = sec.ToString();
                }
                timerText.text = minutes + ":" + seconds;

                player.gameObject.SetActive(false);
                overlay.SetActive(false);
                endFadeBox.SetActive(true);
                complete = true;
                return;
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
        else  // player reached end
        {
            volumeFade();
        }
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

    void volumeFade()
    {
        if (audioSource.volume > 0)
        {
            audioSource.volume -= (1 / fadeTime) * Time.deltaTime;
        }
    }
}
