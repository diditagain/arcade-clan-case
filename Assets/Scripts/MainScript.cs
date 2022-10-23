using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MainScript : MonoBehaviour
{
    #region Serialized Variables
    [Header("Speed Settings")]
    [SerializeField, Tooltip("Lower the value for faster movement")] float courseCompletionTime;
    [SerializeField] float sideMovementSpeed;
    [SerializeField, Tooltip("Use this curve to ease movement")] AnimationCurve forwardMovementCurve;
    [SerializeField] Vector3 cameraOffset;
    #endregion

    #region Dependencies
    [Header("Dependencies")]
    [SerializeField] Button startButton;
    [SerializeField] Transform sphere, collectablesParent, startPoint, endPoint, stack;
    #endregion

    #region Private Variables
    float lerpZValue, lerpXValue, startZValue, endZValue, endXValue;
    Transform cameraTransform;
    List<GameObject> collectables = new List<GameObject>();
    Vector3 stackPoint = Vector3.zero;
    #endregion

    #region Monobehavior Methods
    void Start()
    {
        startButton.gameObject.SetActive(true);
        //Set start and end values for movement
        startZValue = startPoint.position.z;
        endZValue = endPoint.position.z;
        cameraTransform = Camera.main.transform;
        collectables.Add(sphere.gameObject);
    }

    private void Update()
    {
        StartHorizontalMove();
    }

    private void LateUpdate()
    {
        CameraFollowSystem();
    }
    #endregion

    #region UI
    public void OnStartButtonPressed()
    {
        startButton.gameObject.SetActive(false);
        StartCoroutine(ForwardLerp());
    }
    #endregion

    #region Player Movement System
    IEnumerator ForwardLerp()
    {
        float timeElapsed = 0;
        while (timeElapsed < courseCompletionTime)
        {
            lerpZValue = Mathf.Lerp(startZValue, endZValue, forwardMovementCurve.Evaluate(timeElapsed / courseCompletionTime));
            timeElapsed += Time.deltaTime;
            transform.position = new Vector3(transform.position.x, transform.position.y, lerpZValue);
            yield return null;
        }
        lerpZValue = endZValue;
    }

    void StartHorizontalMove()
    {
        if (Input.GetMouseButton(0))
        {
            endXValue = transform.position.x + Input.GetAxis("Mouse X");
            endXValue = Mathf.Clamp(endXValue, -2.40f, 2.40f);
            lerpXValue = Mathf.Lerp(transform.position.x, endXValue, sideMovementSpeed * Time.fixedDeltaTime);
            transform.position = new Vector3(lerpXValue, transform.position.y, transform.position.z);
        }
    }

    #endregion

    #region Camera System
    void CameraFollowSystem()
    {
        cameraTransform.position = Vector3.Lerp(cameraTransform.position, transform.position + cameraOffset, Time.deltaTime);
    }
    #endregion

    #region Stacking System
    private void OnTriggerEnter(Collider other)
    {
        float jumpPower = 1f;
        for (int i = collectables.Count -1; i >= 0; i--)
        {
            collectables[i].transform.DOLocalJump(collectables[i].transform.localPosition, jumpPower, 1, 0.3f);
            jumpPower += 0.5f;
        }
        if (other.gameObject.CompareTag("Collectable"))
        {
            DOTween.CompleteAll();
            other.GetComponent<Collider>().enabled = false;
            for(int i = 0; i < collectables.Count; i++)
            {
                collectables[i].transform.DOLocalJump(collectables[i].transform.localPosition + Vector3.up * 1.1f, 1, 1, 0.5f);
            }
            collectables.Add(other.gameObject);
            other.transform.SetParent(stack);
            other.transform.DOLocalMove(stackPoint, 0.5f);
        }
        else if(other.gameObject.CompareTag("Obstacle"))
        {
            DOTween.CompleteAll();
            other.gameObject.GetComponent<Collider>().enabled = false;
            GameObject lastCube = collectables[collectables.Count - 1];
            lastCube.transform.DOKill();
            lastCube.transform.SetParent(collectablesParent);
            collectables.Remove(lastCube);
            for (int i = 0; i < collectables.Count; i++)
            {
                collectables[i].transform.DOLocalJump(collectables[i].transform.localPosition - Vector3.up * 1.1f, 0.9f, 1, 0.5f);
            }
        }
    }
    #endregion
}
