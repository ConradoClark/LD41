using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.PostProcessing;

public enum VolumeShape
{
    BOX, SPHERE
}

public class PostProcessVolume : MonoBehaviour
{
    [Header("VolumeMode")]
    public VolumeShape ShapeOfVolume = VolumeShape.BOX;
    [Header("SphereVolumeSetting")]
    public float OuterSphereRadius=1;
    public float InnerSphereRadius;
    [Header("BoxVolumeSetting")]
    public Vector3 OuterBoxSize=Vector3.one;
    private Vector3 _outerBoxSize=Vector3.one;
  
    public float OuterBoxSizeMultiplier = 1;
    private float _outerBoxSizeMultiplier = 1;
    public Vector3 InnerBoxSize = Vector3.zero;
    private Vector3 _innerBoxSize = Vector3.zero;
    public float InnerBoxSizeMultiplier = 1;
    private float _innerBoxSizeMultiplier = 1;

    [Header("PostProcessing Effects")]
    //public FogModel fog = new FogModel();
    public AntialiasingModel antialiasing = new AntialiasingModel();
    public AmbientOcclusionModel ambientOcclusion = new AmbientOcclusionModel();
    public ScreenSpaceReflectionModel screenSpaceReflection = new ScreenSpaceReflectionModel();
    public DepthOfFieldModel depthOfField = new DepthOfFieldModel();
    public MotionBlurModel motionBlur = new MotionBlurModel();
    public EyeAdaptationModel eyeAdaptation = new EyeAdaptationModel();
    public BloomModel bloom = new BloomModel();
    public ColorGradingModel colorGrading = new ColorGradingModel();
    public UserLutModel userLut = new UserLutModel();
    public ChromaticAberrationModel chromaticAberration = new ChromaticAberrationModel();
    public GrainModel grain = new GrainModel();
    public VignetteModel vignette = new VignetteModel();
    public DitheringModel dithering = new DitheringModel();


    [Header("ResetAllValues")]
    public PostProcessingProfile _ResetProfile;


    private bool _hasJustStarted = true;
    // Use this for initialization
    void Start () {
       
        this.transform.localScale = Vector3.one;
    }

    void OnValidate()
    {
    }
	// Update is called once per frame
	void Update () {
        PostProcessVolumeReceiver receivers = gameObject.GetComponent<PostProcessVolumeReceiver>();
        if (receivers != null)
        {
            var thisVolume = this.GetComponent<PostProcessVolume>();
            receivers.SetValues(ref thisVolume, 1f);
        }
    }


    void OnTriggerStay(Collider other)
    {

    }

    void OnTriggerExit(Collider other)
    {
        PostProcessVolumeReceiver receivers = other.gameObject.GetComponent<PostProcessVolumeReceiver>();
        if (receivers != null)
        {
            receivers.ResetValues();
        }
    }

    public void ResetValues()
    {
        if (_ResetProfile==null)
        {
            Debug.LogError("No ProfileEntered");
            return;
        }

        //Enable/disable elements
        antialiasing.enabled = _ResetProfile.antialiasing.enabled;
        ambientOcclusion.enabled = _ResetProfile.ambientOcclusion.enabled;
        screenSpaceReflection.enabled = _ResetProfile.screenSpaceReflection.enabled;
        depthOfField.enabled = _ResetProfile.depthOfField.enabled;
        motionBlur.enabled = _ResetProfile.motionBlur.enabled;
        eyeAdaptation.enabled = _ResetProfile.eyeAdaptation.enabled;
        bloom.enabled = _ResetProfile.bloom.enabled;
        colorGrading.enabled = _ResetProfile.colorGrading.enabled;
        userLut.enabled = _ResetProfile.userLut.enabled;
        chromaticAberration.enabled = _ResetProfile.chromaticAberration.enabled;
        grain.enabled = _ResetProfile.grain.enabled;
        vignette.enabled = _ResetProfile.vignette.enabled;
        dithering.enabled = _ResetProfile.dithering.enabled;

        //Copy settings
        antialiasing.settings = _ResetProfile.antialiasing.settings;
        ambientOcclusion.settings = _ResetProfile.ambientOcclusion.settings;
        screenSpaceReflection.settings = _ResetProfile.screenSpaceReflection.settings;
        depthOfField.settings = _ResetProfile.depthOfField.settings;
        motionBlur.settings = _ResetProfile.motionBlur.settings;
        eyeAdaptation.settings = _ResetProfile.eyeAdaptation.settings;
        bloom.settings = _ResetProfile.bloom.settings;
        colorGrading.settings = _ResetProfile.colorGrading.settings;
        userLut.settings = _ResetProfile.userLut.settings;
        chromaticAberration.settings = _ResetProfile.chromaticAberration.settings;
        grain.settings = _ResetProfile.grain.settings;
        vignette.settings = _ResetProfile.vignette.settings;
        dithering.settings = _ResetProfile.dithering.settings;

    }
}
