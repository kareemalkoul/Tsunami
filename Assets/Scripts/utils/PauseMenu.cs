using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Kareem.Fluid.SPH;
using System;

public class PauseMenu : MonoBehaviour
{
    public GameObject Menu;
    public Dropdown particles;
    public Dropdown Method;
    public GameObject CubeMethod;
    public GameObject SphereMethod;
    public GameObject[] OtherUiElements;
    private bool start = false;
    public Text Ruseme_Start;
 

    [SerializeField]
    public Fluid3D fluid;

    // Start is called before the first frame update
    private void Start()
    {
        DropDwonWithParticlesEnum(particles, fluid.ParticleNum);
        DropDwonWithInitEnum(Method, fluid.initParticleWay);
        Smootheln.text = fluid.Smootheln.ToString();
        Pressure.text = fluid.pressureStiffness.ToString();
        RestDenstiy.text = fluid.restDensity.ToString();
        ParticleMass.text = fluid.particleMass.ToString();
        Viscosity.text = fluid.viscosity.ToString();
        MaxDeltaTime.text = fluid.maxAllowableTimestep.ToString();
        WallStiffness.text = fluid.wallStiffness.ToString();
        Iterations.text = fluid.iterations.ToString();
        GravityX.text = fluid.gravity.x.ToString();
        GravityY.text = fluid.gravity.y.ToString();
        Gravityz.text = fluid.gravity.z.ToString();
        RangeX.text = fluid.range.x.ToString();
        RangeY.text = fluid.range.y.ToString();
        RangeZ.text = fluid.range.z.ToString();
        Dimensions.text = fluid.dimensions.ToString();
        MaxParticles.text = fluid.maximumParticlesPerCell.ToString();
        SphereRaduis.text = fluid.ballRadius.ToString();
        ParticleRaduis.text = fluid.particleRadius.ToString();
        SperationFactor.text = fluid.separationFactor.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Escape) && Menu.active == false)
        // {
        //     Menu.SetActive(true);
        //     SetActiveForElment(false);
        // }
        // else if (Input.GetKeyDown(KeyCode.Escape) && Menu.active == true)
        // {
        //     Menu.SetActive(false);
        //     SetActiveForElment(true);
        // }
        
    
        if (!start)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ChangeGameState();
        }
    }

    public void DropDwonWithParticlesEnum(Dropdown dropdown, Enum targetEnum) //You can populate any dropdown with any enum with this method
    {
        Type enumType = targetEnum.GetType(); //Type of enum(FormatPresetType in my example)
        List<Dropdown.OptionData> newOptions = new List<Dropdown.OptionData>();
        string[] enums = Enum.GetNames(enumType);
        for (int i = 0; i < enums.Length; i++) //Populate new Options
        {
            string name = enums[i];
            newOptions.Add(new Dropdown.OptionData(name));
        }

        dropdown.ClearOptions(); //Clear old options
        dropdown.AddOptions(newOptions); //Add new options
        dropdown.onValueChanged.AddListener(
            delegate
            {
                NumParticleEnum num = (NumParticleEnum)
                    Enum.GetValues(enumType).GetValue(dropdown.value);

                fluid.ParticleNum = num;
            }
        );
    }

    public void DropDwonWithInitEnum(Dropdown dropdown, Enum targetEnum) //You can populate any dropdown with any enum with this method
    {
        Type enumType = targetEnum.GetType(); //Type of enum(FormatPresetType in my example)
        List<Dropdown.OptionData> newOptions = new List<Dropdown.OptionData>();
        string[] enums = Enum.GetNames(enumType);
        for (int i = 0; i < enums.Length; i++) //Populate new Options
        {
            string name = enums[i];
            newOptions.Add(new Dropdown.OptionData(name));
        }

        dropdown.ClearOptions(); //Clear old options
        dropdown.AddOptions(newOptions); //Add new options
        dropdown.onValueChanged.AddListener(
            delegate
            {
                InitParticleWay init = (InitParticleWay)
                    Enum.GetValues(enumType).GetValue(dropdown.value);

                fluid.initParticleWay = init;
                switch (init)
                {
                    case InitParticleWay.CUBE:
                        CubeMethod.SetActive(true);
                        SphereMethod.SetActive(false);
                        break;
                    case InitParticleWay.SPHERE:
                        CubeMethod.SetActive(false);
                        SphereMethod.SetActive(true);

                        break;
                }
            }
        );
    }

    ///
    ///this function convert from active game to inactive and the reverse.
    void ChangeGameState()
    {
        SetActiveForElment(Menu.active);
        Menu.SetActive(!Menu.active);
    }

    void SetActiveForElment(bool active)
    {
        foreach (GameObject gameobj in OtherUiElements)
        {
            gameobj.SetActive(active);
        }
    }

    #region ClickFunction

    public void Resume()
    {
        if (!start)
        {
            //the start button is pressed
            Ruseme_Start.text = "Ruseme";
            start = true;
            ChangeGameState();
            return;
        }

        //the ruseme button is pressed

        if (Menu.active == false)
        {
            ChangeGameState();
        }
        else if (Menu.active == true)
        {
            ChangeGameState();
        }
    }

    public void Startbutton() { }

    public void Quit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void Restart()
    {
        Application.LoadLevel(Application.loadedLevel);
    }
    //Ouput the new value of the Dropdown into Text
    #endregion

    #region  inputFields
    public InputField Smootheln;
    public InputField Pressure;
    public InputField RestDenstiy;
    public InputField ParticleMass;
    public InputField Viscosity;
    public InputField MaxDeltaTime;
    public InputField WallStiffness;
    public InputField Iterations;
    public InputField GravityX;
    public InputField GravityY;
    public InputField Gravityz;
    public InputField RangeX;
    public InputField RangeY;
    public InputField RangeZ;
    public InputField Dimensions;
    public InputField MaxParticles;
    public InputField SphereRaduis;
    public InputField ParticleRaduis;
    public InputField SperationFactor;

    #endregion
}
