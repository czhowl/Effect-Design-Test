using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
#if UNITY_IOS && !UNITY_EDITOR
using UnityEngine.XR.ARKit;
#endif
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(ARFace))]
public class EyebrowTracker : MonoBehaviour
{
    public Vector3 HeadPos;     // parameters sending to fallingballmanager
    public Vector3 EyePos;
    public bool IsHeadDetected;
    FallingBallManager fallingBallManager;
    int currEat = 0;     // check for when to update eyebrow color
    TextMeshPro ScoreDisp;

#if UNITY_IOS && !UNITY_EDITOR
    ARKitFaceSubsystem m_ARKitFaceSubsystem;
    Dictionary<ARKitBlendShapeLocation, int> m_FaceArkitBlendShapeIndexMap;
#endif
    ARFace m_Face;
    XRFaceSubsystem m_FaceSubsystem;

    [SerializeField]
    float leftOutterEyebrowCoefficient;
    [SerializeField]
    float rightOutterEyebrowCoefficient;
    [SerializeField]
    float leftDownEyebrowCoefficient;
    [SerializeField]
    float rightDownEyebrowCoefficient;
    [SerializeField]
    float mouthCloseCoefficient;
    public float mouthClose
    {
        get => mouthCloseCoefficient;
        set => mouthCloseCoefficient = value;
    }
    [SerializeField]
    float mouthFunnelCoefficient;
    public float mouthFunnel
    {
        get => mouthFunnelCoefficient;
        set => mouthFunnelCoefficient = value;
    }

    [SerializeField]
    GameObject m_EyebrowPrefabL;
    public GameObject eyebrowPrefabL
    {
        get => m_EyebrowPrefabL;
        set => m_EyebrowPrefabL = value;
    }
    [SerializeField]
    GameObject m_EyebrowPrefabR;
    public GameObject eyebrowPrefabR
    {
        get => m_EyebrowPrefabR;
        set => m_EyebrowPrefabR = value;
    }
    [SerializeField]
    GameObject m_EyeballPrefab;
    public GameObject eyeballPrefab
    {
        get => m_EyeballPrefab;
        set => m_EyeballPrefab = value;
    }
    GameObject m_LeftEyebrowGameObject;
    GameObject m_RightEyebrowGameObject;
    GameObject m_LeftEyeballGameObject;
    GameObject m_RightEyeballGameObject;
    
    [SerializeField]
    GameObject m_textPrefab;
    public GameObject textPrefab
    {
        get => m_textPrefab;
        set => m_textPrefab = value;
    }

    GameObject textUiGameObject;
    void Awake()
    {
        m_Face = GetComponent<ARFace>();
    }

    void OnEnable()
    {
#if UNITY_IOS && !UNITY_EDITOR
        var faceManager = FindObjectOfType<ARFaceManager>();
        if (faceManager != null)     // initialize ARKit face tracking
        {
            m_FaceSubsystem = (XRFaceSubsystem)faceManager.subsystem;
            SetVisible((m_Face.trackingState == TrackingState.Tracking) && (ARSession.state > ARSessionState.Ready));
            m_ARKitFaceSubsystem = (ARKitFaceSubsystem)faceManager.subsystem;
        }
#endif
        m_Face.updated += OnUpdated;
        ARSession.stateChanged += OnSystemStateChanged;
    }

    void OnDisable()     // destructor
    {
        m_Face.updated -= OnUpdated;
        ARSession.stateChanged -= OnSystemStateChanged;
        SetVisible(false);
    }

    void OnSystemStateChanged(ARSessionStateChangedEventArgs eventArgs)
    {
    }

    void OnUpdated(ARFaceUpdatedEventArgs eventArgs)
    {
        UpdateFaceFeatures();
        UpdateEyebrows();
        SetVisible((m_Face.trackingState == TrackingState.Tracking) && (ARSession.state > ARSessionState.Ready));
    }
    
    void UpdateFaceFeatures()
    {

#if UNITY_IOS && !UNITY_EDITOR     // get face blend shapes
        using (var blendShapes = m_ARKitFaceSubsystem.GetBlendShapeCoefficients(m_Face.trackableId, Allocator.Temp))
        {
            foreach (var featureCoefficient in blendShapes)
            {
                if(featureCoefficient.blendShapeLocation == ARKitBlendShapeLocation.BrowOuterUpLeft){
                    leftOutterEyebrowCoefficient = featureCoefficient.coefficient;
                }else if(featureCoefficient.blendShapeLocation == ARKitBlendShapeLocation.BrowOuterUpRight){
                    rightOutterEyebrowCoefficient = featureCoefficient.coefficient;
                }else if(featureCoefficient.blendShapeLocation == ARKitBlendShapeLocation.BrowDownLeft){
                    leftDownEyebrowCoefficient = featureCoefficient.coefficient;
                }else if(featureCoefficient.blendShapeLocation == ARKitBlendShapeLocation.BrowDownRight){
                    rightDownEyebrowCoefficient = featureCoefficient.coefficient;
                }else if(featureCoefficient.blendShapeLocation == ARKitBlendShapeLocation.MouthClose){
                    mouthCloseCoefficient = featureCoefficient.coefficient;
                }else if(featureCoefficient.blendShapeLocation == ARKitBlendShapeLocation.MouthFunnel){
                    mouthFunnelCoefficient = featureCoefficient.coefficient;
                }
            }
        }
#endif
    }

    void UpdateEyebrows(){
        HeadPos = transform.position;     // face position to send out
        if(textUiGameObject == null){
            textUiGameObject = Instantiate(m_textPrefab, transform);
            ScoreDisp = textUiGameObject.transform.Find("Score").gameObject.GetComponent<TextMeshPro>();
            textUiGameObject.SetActive(false);
        }
        if (m_Face.leftEye != null && m_LeftEyebrowGameObject == null )
        {
            m_LeftEyebrowGameObject = Instantiate(m_EyebrowPrefabL, m_Face.leftEye);
            m_LeftEyebrowGameObject.SetActive(false);
            if(m_EyeballPrefab != null && m_LeftEyeballGameObject == null ){
                m_LeftEyeballGameObject = Instantiate(m_EyeballPrefab, m_Face.leftEye);
                m_LeftEyeballGameObject.SetActive(false);
            }
        }
        if (m_Face.rightEye != null && m_RightEyebrowGameObject == null)
        {
            m_RightEyebrowGameObject = Instantiate(m_EyebrowPrefabR, m_Face.rightEye);
            m_RightEyebrowGameObject.SetActive(false);
            if(m_EyeballPrefab != null && m_RightEyeballGameObject == null ){
                m_RightEyeballGameObject = Instantiate(m_EyeballPrefab, m_Face.rightEye);
                m_RightEyeballGameObject.SetActive(false);
            }
        }
        if (m_Face.leftEye != null && m_Face.rightEye != null && m_LeftEyebrowGameObject != null && m_RightEyebrowGameObject != null)
        {
            Vector3 LeftPos = m_Face.leftEye.position;
            LeftPos +=  m_Face.leftEye.up * (leftOutterEyebrowCoefficient - leftDownEyebrowCoefficient * 0.2f) * 2f;

            m_LeftEyebrowGameObject.transform.position = LeftPos;
            EyePos = LeftPos;
            m_LeftEyebrowGameObject.transform.rotation = transform.rotation;

            Vector3 RightPos = m_Face.rightEye.position;
            RightPos += m_Face.rightEye.up * (rightOutterEyebrowCoefficient - rightDownEyebrowCoefficient * 0.2f) * 2f;

            m_RightEyebrowGameObject.transform.position = RightPos;
            m_RightEyebrowGameObject.transform.rotation = transform.rotation;
        }
    }

    void Update(){
        if(fallingBallManager == null){
            fallingBallManager = FindObjectOfType<FallingBallManager>();
        }
        if(fallingBallManager.eat != currEat && m_LeftEyebrowGameObject != null){     // update eyebrow color
            currEat = fallingBallManager.eat;
            GameObject platformL = m_LeftEyebrowGameObject.transform.Find("GameObject/platform").gameObject;
            GameObject platformR = m_RightEyebrowGameObject.transform.Find("GameObject/platform").gameObject;
            platformL.GetComponent<Renderer>().sharedMaterial.color = fallingBallManager.ballColor;
            platformL.GetComponent<Animator>().SetTrigger("bounce");
            platformR.GetComponent<Animator>().SetTrigger("bounce");
        }
        if(ScoreDisp != null){     // update score
            ScoreDisp.text = fallingBallManager.score.ToString();
        }
    }

    void SetVisible(bool visible)
    {
        IsHeadDetected = visible;
        if (m_LeftEyebrowGameObject != null && m_RightEyebrowGameObject != null)
        {
            m_LeftEyebrowGameObject.SetActive(visible);
            m_RightEyebrowGameObject.SetActive(visible);
            if(m_EyeballPrefab != null){
                m_LeftEyeballGameObject.SetActive(visible);
                m_RightEyeballGameObject.SetActive(visible);
            }
            textUiGameObject.SetActive(visible);
            if(!visible){
                HeadPos = Vector3.zero;
            }
        }
    }
}
