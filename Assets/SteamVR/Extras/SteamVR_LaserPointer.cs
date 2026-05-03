//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// SteamVR_LaserPointer.cs
//
// Adapted for a chromatic visual adaptation experiment in virtual reality.
//
// This script controls:
// - SteamVR laser pointer interaction.
// - Randomized presentation of NCS color patches.
// - Spectral-to-RGB conversion for illuminants and patches.
// - Experimental timing and automatic response logging.
//
// Reproducibility notes:
// - Assign all spectral CSV files as TextAsset objects in the Unity Inspector.
// - Output files are saved in Application.persistentDataPath/Results.
// - Use anonymized participant identifiers before releasing data publicly.

using MathNet.Numerics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.Networking.Types;
using Valve.VR;

public class SteamVR_LaserPointer : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Experiment timing and design parameters
    // -------------------------------------------------------------------------
    private const int numTrial = 1;
    private const int numLuces = 4;
    private const int tiempoInicial = 30;
    private const int tiempoIntermedio = 30;
    private const int tiempoMaximo = 20;
    private const int tiempoSonido = 5;
    private const int spectralSamples = 101;
    private const float patchHeight = 0.65f;

    // -------------------------------------------------------------------------
    // Data structures
    // -------------------------------------------------------------------------
    public struct cmsCIEXYZ
    {
        public double x, y, z;
    }

    public struct posiciones
    {
        public float x, z;
    }

    public struct lucesController
    {
        public int veces1;
        public int veces2;
        public bool anterior;
        public bool muestra_facil;
    }

    // -------------------------------------------------------------------------
    // Spectral input files assigned from the Unity Inspector
    // -------------------------------------------------------------------------
    [Header("Color matching functions")]
    public TextAsset csvFilecoord;

    [Header("Illuminant spectra")]
    public TextAsset csvFileluz1; // D65
    public TextAsset csvFileluz2; // Red
    public TextAsset csvFileluz3; // Green
    public TextAsset csvFileluz4; // Blue

    [Header("Scene and reference spectra")]
    public TextAsset csv_PastBlanca;
    public TextAsset csv_Cabina;
    public TextAsset csvAnchor1;
    public TextAsset csvAnchor2;
    public TextAsset csvAnchor3;
    public TextAsset csvAnchor4;

    [Header("Low-saturation NCS patch spectra")]
    public TextAsset csvS0500N;
    public TextAsset csvS0502Y;
    public TextAsset csvS0502Y50R;
    public TextAsset csvS0502R;
    public TextAsset csvS0502R50B;
    public TextAsset csvS0502B;
    public TextAsset csvS0502B50G;
    public TextAsset csvS0502G;
    public TextAsset csvS0502G50Y;

    [Header("High-saturation NCS patch spectra for training/check scenes")]
    public TextAsset csvS0505Y;
    public TextAsset csvS0505Y50R;
    public TextAsset csvS0505R;
    public TextAsset csvS0505R50B;
    public TextAsset csvS0505B;
    public TextAsset csvS0505B50G;
    public TextAsset csvS0505G;
    public TextAsset csvS0505G50Y;

    // -------------------------------------------------------------------------
    // Participant metadata
    // -------------------------------------------------------------------------
    [Header("Participant metadata")]
    [Tooltip("Use an anonymized identifier, e.g., P01.")]
    public string nombre;

    [Tooltip("Session/attempt identifier.")]
    public string intento;

    [HideInInspector]
    public string eleccion;

    // -------------------------------------------------------------------------
    // Scene objects assigned in the Unity Inspector
    // -------------------------------------------------------------------------
    [Header("NCS patches")]
    public GameObject PastillaBlanca;
    public GameObject S0502Y;
    public GameObject S0502Y50R;
    public GameObject S0502R;
    public GameObject S0502R50B;
    public GameObject S0502B;
    public GameObject S0502B50G;
    public GameObject S0502G;
    public GameObject S0502G50Y;

    [Header("Scene surfaces")]
    public GameObject suelo;
    public GameObject fondo;
    public GameObject lateralIzqdo;
    public GameObject lateralDrecho;
    public GameObject frontal;
    public GameObject techo;

    [Header("Anchor patches")]
    public GameObject anchor1;
    public GameObject anchor2;
    public GameObject anchor3;
    public GameObject anchor4;

    // -------------------------------------------------------------------------
    // SteamVR laser pointer settings
    // -------------------------------------------------------------------------
    [Header("SteamVR input")]
    public GameObject rightHand;
    public SteamVR_Behaviour_Pose pose;
    public SteamVR_Action_Boolean interactWithUI = SteamVR_Input.GetBooleanAction("InteractUI");

    [Header("Laser pointer settings")]
    public bool active = true;
    public Color color = Color.red;
    public float thickness = 0.002f;
    public Color clickColor = Color.green;
    public GameObject holder;
    public GameObject pointer;
    public bool addRigidBody = false;
    public Transform reference;

    public event PointerEventHandler PointerIn;
    public event PointerEventHandler PointerOut;
    public event PointerEventHandler PointerClick;

    private bool isActive = false;
    private Transform previousContact = null;

    [Header("Audio cue")]
    public AudioClip clip;
    private AudioSource source { get { return GetComponent<AudioSource>(); } }

    // -------------------------------------------------------------------------
    // Internal state
    // -------------------------------------------------------------------------
    private readonly CultureInfo culture = CultureInfo.InvariantCulture;
    private string[] records;

    private float[] X, Y, Z;
    private float[] luz1, luz2, luz3, luz4;
    private float[] sumastot;
    private float[] Xluz1, Yluz1, Zluz1;

    private cmsCIEXYZ xyz;
    private Color Coloraux;
    private Color[] colorPastillasNCS;

    private GameObject[] luces = new GameObject[12];
    private GameObject tubo1;
    private GameObject tubo2;

    private posiciones[] vector_posiciones;
    private lucesController[] luces_controller;

    private string chooseSample = "error";
    private string pos0, pos1, pos2, pos3, pos4, pos5, pos6, pos7, pos8;

    private double[] RGB_Light_Source0;
    private double[] RGB_Light_Source;

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------
    private void Start()
    {
        InitializeSteamVRPointer();
        InitializeSpectralArrays();
        InitializeSceneLights();
        InitializePatchPositions();
        InitializeIlluminantController();
        InitializeAudio();

        StartCoroutine(esperar());
    }

    private void Update()
    {
        UpdateLaserPointer();
    }

    // -------------------------------------------------------------------------
    // Initialization methods
    // -------------------------------------------------------------------------
    private void InitializeSteamVRPointer()
    {
        if (pose == null)
            pose = GetComponent<SteamVR_Behaviour_Pose>();

        if (pose == null)
            Debug.LogError("No SteamVR_Behaviour_Pose component found on this object.");

        if (interactWithUI == null)
            Debug.LogError("No UI interaction action has been set on this component.");

        holder = new GameObject("LaserPointerHolder");
        holder.transform.parent = transform;
        holder.transform.localPosition = Vector3.zero;
        holder.transform.localRotation = Quaternion.identity;

        pointer = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pointer.name = "LaserPointer";
        pointer.transform.parent = holder.transform;
        pointer.transform.localScale = new Vector3(thickness, thickness, 100f);
        pointer.transform.localPosition = new Vector3(0f, 0f, 50f);
        pointer.transform.localRotation = Quaternion.identity;

        BoxCollider collider = pointer.GetComponent<BoxCollider>();
        if (addRigidBody)
        {
            if (collider != null)
                collider.isTrigger = true;

            Rigidbody rigidBody = pointer.AddComponent<Rigidbody>();
            rigidBody.isKinematic = true;
        }
        else if (collider != null)
        {
            Destroy(collider);
        }

        Material newMaterial = new Material(Shader.Find("Unlit/Color"));
        newMaterial.SetColor("_Color", color);
        pointer.GetComponent<MeshRenderer>().material = newMaterial;
    }

    private void InitializeSpectralArrays()
    {
        colorPastillasNCS = new Color[9];

        luz1 = new float[spectralSamples];
        luz2 = new float[spectralSamples];
        luz3 = new float[spectralSamples];
        luz4 = new float[spectralSamples];

        Xluz1 = new float[spectralSamples];
        Yluz1 = new float[spectralSamples];
        Zluz1 = new float[spectralSamples];

        sumastot = new float[12];
        xyz.x = xyz.y = xyz.z = 0.0;

        leerCSVtresfich(csvFilecoord, ref X, ref Y, ref Z);
    }

    private void InitializeSceneLights()
    {
        tubo1 = GameObject.Find("TuboFluo1");
        tubo2 = GameObject.Find("TuboFluo2");

        if (tubo1 == null || tubo2 == null)
        {
            Debug.LogError("TuboFluo1 or TuboFluo2 could not be found in the scene.");
            return;
        }

        for (int i = 0; i < 6; i++)
            luces[i] = tubo1.transform.GetChild(i).gameObject;

        int childIndex = 0;
        for (int i = 6; i < 12; i++)
            luces[i] = tubo2.transform.GetChild(childIndex++).gameObject;

        tubo1.SetActive(false);
        tubo2.SetActive(false);
    }

    private void InitializePatchPositions()
    {
        vector_posiciones = new posiciones[9];

        vector_posiciones[0].x = -11.92f; vector_posiciones[0].z = 2.87f;
        vector_posiciones[1].x = -5.87f; vector_posiciones[1].z = -9.6f;
        vector_posiciones[2].x = 8.84f; vector_posiciones[2].z = 2.61f;
        vector_posiciones[3].x = -6.6f; vector_posiciones[3].z = 4.34f;
        vector_posiciones[4].x = -11.85f; vector_posiciones[4].z = -5.64f;
        vector_posiciones[5].x = 4.63f; vector_posiciones[5].z = -9.07f;
        vector_posiciones[6].x = 2.34f; vector_posiciones[6].z = 3.82f;
        vector_posiciones[7].x = 9.81f; vector_posiciones[7].z = -4.52f;
        vector_posiciones[8].x = -0.88f; vector_posiciones[8].z = -3.56f;
    }

    private void InitializeIlluminantController()
    {
        luces_controller = new lucesController[numLuces];

        for (int i = 0; i < numLuces; i++)
        {
            luces_controller[i].anterior = false;
            luces_controller[i].veces1 = 0;
            luces_controller[i].veces2 = 0;
            luces_controller[i].muestra_facil = false;
        }
    }

    private void InitializeAudio()
    {
        if (GetComponent<AudioSource>() == null)
            gameObject.AddComponent<AudioSource>();
    }

    // -------------------------------------------------------------------------
    // Randomization and patch placement
    // -------------------------------------------------------------------------
    public static void Barajar<T>(IList<T> values)
    {
        System.Random rnd = new System.Random();

        for (int i = values.Count - 1; i > 0; i--)
        {
            int j = rnd.Next(0, i + 1);
            T temp = values[i];
            values[i] = values[j];
            values[j] = temp;
        }
    }
    /// <summary>
    /// Randomly assigns spatial positions to all color patches (including the white reference)
    /// within the scene before each trial.
    ///
    /// This method ensures that:
    /// - The spatial arrangement of patches changes across trials to avoid positional bias.
    /// - Each patch (NCS samples and white reference) is placed in one of the predefined positions.
    /// - Participants cannot rely on memory of location, forcing perceptual decision-making.
    ///
    /// Typically, this function:
    /// 1. Shuffles the predefined list of positions.
    /// 2. Assigns each patch GameObject to a new position.
    /// 3. Maintains consistent height while varying XZ plane placement.
    ///
    /// This is a key step for experimental validity, preventing learning effects
    /// and ensuring that responses are driven by color perception rather than spatial cues.
    /// </summary>
    
    private void PlacePatches()
    {
        S0502Y.transform.position = new Vector3(vector_posiciones[0].x, patchHeight, vector_posiciones[0].z);
        S0502Y50R.transform.position = new Vector3(vector_posiciones[1].x, patchHeight, vector_posiciones[1].z);
        S0502R.transform.position = new Vector3(vector_posiciones[2].x, patchHeight, vector_posiciones[2].z);
        S0502R50B.transform.position = new Vector3(vector_posiciones[3].x, patchHeight, vector_posiciones[3].z);
        S0502B.transform.position = new Vector3(vector_posiciones[4].x, patchHeight, vector_posiciones[4].z);
        S0502B50G.transform.position = new Vector3(vector_posiciones[5].x, patchHeight, vector_posiciones[5].z);
        S0502G.transform.position = new Vector3(vector_posiciones[6].x, patchHeight, vector_posiciones[6].z);
        S0502G50Y.transform.position = new Vector3(vector_posiciones[7].x, patchHeight, vector_posiciones[7].z);
        PastillaBlanca.transform.position = new Vector3(vector_posiciones[8].x, patchHeight, vector_posiciones[8].z);
    }

    // -------------------------------------------------------------------------
    // Spectral color computation
    // -------------------------------------------------------------------------

    public void Calcular_Pintar_Facil(TextAsset fichero, float factor)
    {
        PaintSceneFromSpectra(fichero, factor, true);
    }

    private void PaintSceneFromSpectra(TextAsset illuminantFile, float factor, bool useHighSaturationPatches)
    {
        tubo1.SetActive(true);
        tubo2.SetActive(true);

        leerCSVuno(illuminantFile, ref luz1);

        productosuma(luz1, X, ref sumastot[0]);
        productosuma(luz1, Y, ref sumastot[1]);
        productosuma(luz1, Z, ref sumastot[2]);

        // Normalize XYZ by Y.
        sumastot[0] = sumastot[0] / sumastot[1];
        sumastot[2] = sumastot[2] / sumastot[1];
        sumastot[1] = 1.0f;

        xyz.x = sumastot[0] * factor;
        xyz.y = sumastot[1] * factor;
        xyz.z = sumastot[2] * factor;

        RGB_Light_Source0 = new double[3];
        XYZ2RGB(xyz.x, xyz.y, xyz.z, ref RGB_Light_Source0);

        Coloraux.r = (float)(RGB_Light_Source0[0] / 255.0);
        Coloraux.g = (float)(RGB_Light_Source0[1] / 255.0);
        Coloraux.b = (float)(RGB_Light_Source0[2] / 255.0);

        for (int i = 0; i < luces.Length; i++)
            luces[i].GetComponent<Light>().color = Coloraux;

        float[] illuminant = new float[spectralSamples];
        leerCSVuno(illuminantFile, ref illuminant);

        leerCSVtresfich(csvFilecoord, ref X, ref Y, ref Z);
        calculoprod(illuminant, X, Y, Z, ref Xluz1, ref Yluz1, ref Zluz1);

        float[] whitePatch = new float[spectralSamples];
        leerCSVunoPastilla(csv_PastBlanca, ref whitePatch);

        for (int p = 0; p < 15; p++)
        {
            float[] testPatch = GetPatchSpectrumByIndex(p, useHighSaturationPatches);
            Color computedColor = ComputePatchColor(testPatch, whitePatch, factor);
            ApplyComputedColorToObject(p, computedColor);
        }
    }

    private float[] GetPatchSpectrumByIndex(int index, bool useHighSaturationPatches)
    {
        float[] patch = new float[spectralSamples];

        switch (index)
        {
            case 0: leerCSVunoPastilla(csv_PastBlanca, ref patch); break;
            case 1: leerCSVunoPastilla(csvAnchor1, ref patch); break;
            case 2: leerCSVunoPastilla(csvAnchor2, ref patch); break;
            case 3: leerCSVunoPastilla(csvAnchor3, ref patch); break;
            case 4: leerCSVunoPastilla(csvAnchor4, ref patch); break;
            case 5: leerCSVunoPastilla(csvS0500N, ref patch); break;
            case 6: leerCSVunoPastilla(useHighSaturationPatches ? csvS0505Y : csvS0502Y, ref patch); break;
            case 7: leerCSVunoPastilla(useHighSaturationPatches ? csvS0505Y50R : csvS0502Y50R, ref patch); break;
            case 8: leerCSVunoPastilla(useHighSaturationPatches ? csvS0505R : csvS0502R, ref patch); break;
            case 9: leerCSVunoPastilla(useHighSaturationPatches ? csvS0505R50B : csvS0502R50B, ref patch); break;
            case 10: leerCSVunoPastilla(useHighSaturationPatches ? csvS0505B : csvS0502B, ref patch); break;
            case 11: leerCSVunoPastilla(useHighSaturationPatches ? csvS0505B50G : csvS0502B50G, ref patch); break;
            case 12: leerCSVunoPastilla(useHighSaturationPatches ? csvS0505G : csvS0502G, ref patch); break;
            case 13: leerCSVunoPastilla(useHighSaturationPatches ? csvS0505G50Y : csvS0502G50Y, ref patch); break;
            case 14: leerCSVuno(csv_Cabina, ref patch); break;
        }

        return patch;
    }

    private Color ComputePatchColor(float[] testPatch, float[] whitePatch, float factor)
    {
        double xWhite = 0.0, yWhite = 0.0, zWhite = 0.0;
        double xPatch = 0.0, yPatch = 0.0, zPatch = 0.0;

        for (int i = 0; i < spectralSamples; i++)
        {
            xWhite += Xluz1[i] * whitePatch[i];
            yWhite += Yluz1[i] * whitePatch[i];
            zWhite += Zluz1[i] * whitePatch[i];

            xPatch += Xluz1[i] * testPatch[i];
            yPatch += Yluz1[i] * testPatch[i];
            zPatch += Zluz1[i] * testPatch[i];
        }

        xyz.x = (xPatch / yWhite) * factor;
        xyz.y = (yPatch / yWhite) * factor;
        xyz.z = (zPatch / yWhite) * factor;

        RGB_Light_Source = new double[3];
        XYZ2RGB(xyz.x, xyz.y, xyz.z, ref RGB_Light_Source);

        Color result = new Color();
        result.r = SafeNormalizeRgb(RGB_Light_Source[0], RGB_Light_Source0[0]);
        result.g = SafeNormalizeRgb(RGB_Light_Source[1], RGB_Light_Source0[1]);
        result.b = SafeNormalizeRgb(RGB_Light_Source[2], RGB_Light_Source0[2]);
        return result;
    }

    private float SafeNormalizeRgb(double patchChannel, double illuminantChannel)
    {
        if (Math.Abs(illuminantChannel) < 1e-12)
            return 0.0f;

        return (float)((patchChannel / 255.0) / (illuminantChannel / 255.0));
    }

    private void ApplyComputedColorToObject(int index, Color computedColor)
    {
        switch (index)
        {
            case 0: PastillaBlanca.GetComponent<Renderer>().material.color = computedColor; break;
            case 1: anchor1.GetComponent<Renderer>().material.color = computedColor; break;
            case 2: anchor2.GetComponent<Renderer>().material.color = computedColor; break;
            case 3: anchor3.GetComponent<Renderer>().material.color = computedColor; break;
            case 4: anchor4.GetComponent<Renderer>().material.color = computedColor; break;
            case 5: PastillaBlanca.GetComponent<Renderer>().material.color = computedColor; colorPastillasNCS[0] = computedColor; break;
            case 6: S0502Y.GetComponent<Renderer>().material.color = computedColor; colorPastillasNCS[1] = computedColor; break;
            case 7: S0502Y50R.GetComponent<Renderer>().material.color = computedColor; colorPastillasNCS[2] = computedColor; break;
            case 8: S0502R.GetComponent<Renderer>().material.color = computedColor; colorPastillasNCS[3] = computedColor; break;
            case 9: S0502R50B.GetComponent<Renderer>().material.color = computedColor; colorPastillasNCS[4] = computedColor; break;
            case 10: S0502B.GetComponent<Renderer>().material.color = computedColor; colorPastillasNCS[5] = computedColor; break;
            case 11: S0502B50G.GetComponent<Renderer>().material.color = computedColor; colorPastillasNCS[6] = computedColor; break;
            case 12: S0502G.GetComponent<Renderer>().material.color = computedColor; colorPastillasNCS[7] = computedColor; break;
            case 13: S0502G50Y.GetComponent<Renderer>().material.color = computedColor; colorPastillasNCS[8] = computedColor; break;
            case 14:
                suelo.GetComponent<Renderer>().material.color = computedColor;
                fondo.GetComponent<Renderer>().material.color = computedColor;
                lateralDrecho.GetComponent<Renderer>().material.color = computedColor;
                lateralIzqdo.GetComponent<Renderer>().material.color = computedColor;
                frontal.GetComponent<Renderer>().material.color = computedColor;
                techo.GetComponent<Renderer>().material.color = computedColor;
                break;
        }
    }

    // -------------------------------------------------------------------------
    // Main experiment sequence
    // -------------------------------------------------------------------------
    public IEnumerator esperar()
    {
        eleccion = "";
        rightHand.transform.localScale = Vector3.zero;

        yield return new WaitForSeconds(tiempoInicial);
        PlaySound();
        yield return new WaitForSeconds(tiempoSonido);

        for (int j = 0; j < (numLuces * numTrial); j++)
        {
            tubo1.SetActive(true);
            tubo2.SetActive(true);

            Barajar(vector_posiciones);
            PlacePatches();

            int illuminantIndex = GetAvailableIlluminantIndex();
            string illuminantName = ApplyIlluminant(illuminantIndex);
            luces_controller[illuminantIndex].veces1++;

            int repetition = 1;
            while (repetition <= 6)
            {
                int tiempoSegundos = tiempoMaximo;

                yield return new WaitForSeconds(tiempoSegundos - tiempoSonido);
                PlaySound();

                rightHand.transform.localScale = Vector3.one;
                luces_controller[illuminantIndex].anterior = true;

                yield return new WaitForSeconds(tiempoSonido);

                autoSave(illuminantName, tiempoSegundos * repetition, 0, chooseSample);

                rightHand.transform.localScale = Vector3.zero;
                repetition++;

                Barajar(vector_posiciones);
                PlacePatches();
            }

            eleccion = "";
            chooseSample = "error";

            tubo1.SetActive(false);
            tubo2.SetActive(false);

            yield return new WaitForSeconds(tiempoIntermedio);
            PlaySound();
            yield return new WaitForSeconds(tiempoSonido);
        }
    }

    private int GetAvailableIlluminantIndex()
    {
        int index = UnityEngine.Random.Range(0, numLuces);

        while (luces_controller[index].veces1 + luces_controller[index].veces2 == numTrial)
            index = UnityEngine.Random.Range(0, numLuces);

        return index;
    }

    private string ApplyIlluminant(int illuminantIndex)
    {
        float fac = 0.9f;

        switch (illuminantIndex)
        {
            case 0: Calcular_Pintar_Facil(csvFileluz1, fac); return "D65";
            case 1: Calcular_Pintar_Facil(csvFileluz2, fac); return "Red";
            case 2: Calcular_Pintar_Facil(csvFileluz3, fac); return "Green";
            default: Calcular_Pintar_Facil(csvFileluz4, fac); return "Blue";
        }
    }

    // -------------------------------------------------------------------------
    // Data export
    // -------------------------------------------------------------------------
    private void autoSave(string l, int t, int c, string o)
    {
        convertirAPosiciones(PastillaBlanca);
        convertirAPosiciones(S0502Y);
        convertirAPosiciones(S0502Y50R);
        convertirAPosiciones(S0502R);
        convertirAPosiciones(S0502R50B);
        convertirAPosiciones(S0502B);
        convertirAPosiciones(S0502B50G);
        convertirAPosiciones(S0502G);
        convertirAPosiciones(S0502G50Y);

        string resultsDirectory = Path.Combine(Application.persistentDataPath, "Results");
        Directory.CreateDirectory(resultsDirectory);

        string safeParticipant = string.IsNullOrWhiteSpace(nombre) ? "participant" : nombre;
        string safeAttempt = string.IsNullOrWhiteSpace(intento) ? "session" : intento;
        string filePath = Path.Combine(resultsDirectory, safeParticipant + "_" + safeAttempt + ".csv");

        bool fileExists = File.Exists(filePath);

        using (StreamWriter writer = new StreamWriter(filePath, true, System.Text.Encoding.UTF8))
        {
            if (!fileExists)
                writer.WriteLine("Lighting\tTime\tCheckScene\tChoose Option\tPos 0\tPos 1\tPos 2\tPos 3\tPos 4\tPos 5\tPos 6\tPos 7\tPos 8");

            writer.WriteLine(l + "\t" + t + "\t" + c + "\t" + o + "\t" + pos0 + "\t" + pos1 + "\t" + pos2 + "\t" + pos3 + "\t" + pos4 + "\t" + pos5 + "\t" + pos6 + "\t" + pos7 + "\t" + pos8);
        }
    }

    private void convertirAPosiciones(GameObject sample)
    {
        string materialName = sample.GetComponent<Renderer>().material.name;

        if (Mathf.Approximately(sample.transform.position.x, -0.88f)) pos0 = materialName;
        else if (Mathf.Approximately(sample.transform.position.z, 3.82f)) pos1 = materialName;
        else if (Mathf.Approximately(sample.transform.position.x, 8.84f)) pos2 = materialName;
        else if (Mathf.Approximately(sample.transform.position.x, 9.81f)) pos3 = materialName;
        else if (Mathf.Approximately(sample.transform.position.x, 4.63f)) pos4 = materialName;
        else if (Mathf.Approximately(sample.transform.position.x, -5.87f)) pos5 = materialName;
        else if (Mathf.Approximately(sample.transform.position.x, -11.85f)) pos6 = materialName;
        else if (Mathf.Approximately(sample.transform.position.x, -11.92f)) pos7 = materialName;
        else if (Mathf.Approximately(sample.transform.position.x, -6.6f)) pos8 = materialName;
    }

    // -------------------------------------------------------------------------
    // CSV parsing
    // -------------------------------------------------------------------------
    public void leerCSVuno(TextAsset fichero, ref float[] uno)
    {
        records = fichero.text.Split('\n');
        uno = new float[spectralSamples];

        int sourceIndex = 0;
        int targetIndex = 0;

        while (sourceIndex < records.Length - 1 && targetIndex < spectralSamples)
        {
            uno[targetIndex] = ParseFloat(records[sourceIndex]) * 100.0f;
            targetIndex++;
            sourceIndex += 4;
        }
    }

    public void leerCSVunoPastilla(TextAsset fichero, ref float[] uno)
    {
        records = fichero.text.Split('\n');
        uno = new float[spectralSamples];

        int sourceIndex = 0;
        int targetIndex = 0;

        while (sourceIndex < records.Length - 1 && targetIndex < spectralSamples)
        {
            uno[targetIndex] = ParseFloat(records[sourceIndex]);
            targetIndex++;
            sourceIndex++;
        }
    }

    private void leerCSVtresfich(TextAsset fichero, ref float[] uno, ref float[] dos, ref float[] tres)
    {
        records = fichero.text.Split('\n');
        uno = new float[spectralSamples];
        dos = new float[spectralSamples];
        tres = new float[spectralSamples];

        int sourceIndex = 0;
        int targetIndex = 0;

        while (sourceIndex < records.Length - 1 && targetIndex < spectralSamples)
        {
            string[] campos = records[sourceIndex].Split(';');

            uno[targetIndex] = ParseFloat(campos[0]) * 100.0f;
            dos[targetIndex] = ParseFloat(campos[1]) * 100.0f;
            tres[targetIndex] = ParseFloat(campos[2]) * 100.0f;

            targetIndex++;
            sourceIndex += 4;
        }
    }

    private float ParseFloat(string value)
    {
        return float.Parse(value.Trim(), culture);
    }

    // -------------------------------------------------------------------------
    // Colorimetric computations
    // -------------------------------------------------------------------------
    public void productosuma(float[] luz, float[] coord, ref float suma)
    {
        suma = 0.0f;

        for (int i = 0; i < luz.Length - 1; i++)
            suma += luz[i] * coord[i];
    }

    public void calculoprod(float[] luz, float[] _X, float[] _Y, float[] _Z, ref float[] luzX, ref float[] luzY, ref float[] luzZ)
    {
        for (int i = 0; i < luz.Length; i++)
        {
            luzX[i] = luz[i] * _X[i];
            luzY[i] = luz[i] * _Y[i];
            luzZ[i] = luz[i] * _Z[i];
        }
    }
    /// <summary>
    /// Converts a color from CIE XYZ color space to device-dependent RGB values.
    ///
    /// This transformation uses a calibrated XYZ-to-RGB matrix obtained from
    /// the chromatic characterization of the HTC Vive display. This ensures that
    /// the rendered colors accurately match the intended perceptual stimuli
    /// within the VR environment.
    ///
    /// The process consists of:
    /// 1. Linear transformation from XYZ to linear RGB using a device-specific matrix.
    /// 2. Application of a non-linear gamma correction per channel.
    /// 3. Scaling to the 8-bit RGB range [0, 255].
    ///
    /// Parameters:
    /// - X, Y, Z: Tristimulus values in the CIE XYZ color space.
    /// - RGB: Output array containing the corresponding RGB values.
    ///
    /// Note:
    /// - The transformation is display-dependent and should be recalibrated
    ///   if a different HMD or display device is used.
    /// - Gamma correction is applied independently to each channel to match
    ///   the device response characteristics.
    /// </summary>
    private void XYZ2RGB(double X, double Y, double Z, ref double[] RGB)
    {
        RGB = new double[3];

        // Linear transformation using calibrated matrix for HTC Vive display
        double r = 2.250581448970580 * X - 0.643661081785003 * Y - 0.373107414306178 * Z;
        double g = -0.923402888955642 * X + 1.754327140953632 * Y + 0.082098647829907 * Z;
        double b = 0.051827952426986 * X - 0.101601220904830 * Y + 0.935992736069851 * Z;

        //Apply gamma correction and scale to 8 - bit RGB range
        RGB[0] = gamma(r, 1) * 255.0;
        RGB[1] = gamma(g, 2) * 255.0;
        RGB[2] = gamma(b, 3) * 255.0;
    }

    private double gamma(double value, int channel)
    {
        double corrected;

        if (channel == 1)
            corrected = Math.Pow(value, 1.0 / 2.34153663077439);
        else if (channel == 2)
            corrected = Math.Pow(value, 1.0 / 2.30542383673930);
        else
            corrected = Math.Pow(value, 1.0 / 2.26398094679332);

        if (Double.IsNaN(corrected) || Double.IsInfinity(corrected))
            corrected = 0.0;

        return corrected;
    }

    // -------------------------------------------------------------------------
    // Audio
    // -------------------------------------------------------------------------
    private void PlaySound()
    {
        if (source != null && clip != null)
            source.PlayOneShot(clip);
    }

    // -------------------------------------------------------------------------
    // Pointer events
    // -------------------------------------------------------------------------
    public virtual void OnPointerIn(PointerEventArgs e)
    {
        if (PointerIn != null)
            PointerIn(this, e);
    }

    public virtual void OnPointerClick(PointerEventArgs e)
    {
        if (PointerClick != null)
            PointerClick(this, e);
    }

    public virtual void OnPointerOut(PointerEventArgs e)
    {
        if (PointerOut != null)
            PointerOut(this, e);
    }

    // -------------------------------------------------------------------------
    // Laser pointer update
    // -------------------------------------------------------------------------
    private void UpdateLaserPointer()
    {
        if (!isActive)
        {
            isActive = true;

            if (transform.childCount > 0)
                transform.GetChild(0).gameObject.SetActive(true);
        }

        float dist = 100f;
        Ray raycast = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        bool bHit = Physics.Raycast(raycast, out hit);

        HandlePointerEnterExit(bHit, hit);

        if (bHit && hit.distance < 100f)
            dist = hit.distance;

        if (bHit && interactWithUI.GetStateUp(pose.inputSource))
            RaisePointerClick(hit);

        if (interactWithUI != null && interactWithUI.GetState(pose.inputSource))
        {
            pointer.transform.localScale = new Vector3(thickness * 5f, thickness * 5f, dist);
            pointer.GetComponent<MeshRenderer>().material.color = clickColor;

            if (Physics.Raycast(raycast, out hit))
                UpdateSelectedPatch(hit);
        }
        else
        {
            pointer.transform.localScale = new Vector3(thickness, thickness, dist);
            pointer.GetComponent<MeshRenderer>().material.color = color;
        }

        pointer.transform.localPosition = new Vector3(0f, 0f, dist / 2f);
    }

    private void HandlePointerEnterExit(bool bHit, RaycastHit hit)
    {
        if (previousContact != null && (!bHit || previousContact != hit.transform))
        {
            PointerEventArgs args = new PointerEventArgs();
            args.fromInputSource = pose.inputSource;
            args.distance = 0f;
            args.flags = 0;
            args.target = previousContact;

            OnPointerOut(args);
            previousContact = null;
        }

        if (bHit && previousContact != hit.transform)
        {
            PointerEventArgs argsIn = new PointerEventArgs();
            argsIn.fromInputSource = pose.inputSource;
            argsIn.distance = hit.distance;
            argsIn.flags = 0;
            argsIn.target = hit.transform;

            OnPointerIn(argsIn);
            previousContact = hit.transform;
        }
    }

    private void RaisePointerClick(RaycastHit hit)
    {
        PointerEventArgs argsClick = new PointerEventArgs();
        argsClick.fromInputSource = pose.inputSource;
        argsClick.distance = hit.distance;
        argsClick.flags = 0;
        argsClick.target = hit.transform;

        OnPointerClick(argsClick);
    }

    private void UpdateSelectedPatch(RaycastHit hit)
    {
        if (hit.collider.name.Contains("Pastilla"))
        {
            eleccion = hit.collider.name;
            chooseSample = hit.collider.GetComponent<Renderer>().material.name;
        }
        else
        {
            chooseSample = "error";
        }
    }
}

public struct PointerEventArgs
{
    public SteamVR_Input_Sources fromInputSource;
    public uint flags;
    public float distance;
    public Transform target;
}

public delegate void PointerEventHandler(object sender, PointerEventArgs e);
