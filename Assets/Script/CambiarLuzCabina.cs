using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;
using MathNet.Numerics.Interpolation;
using MathNet.Numerics;

public class CambiarLuzCabina : MonoBehaviour
{
    const int numTrial = 10;
    const int numLuces = 4;
    const int tiempoInicial = 5;
    const int tiempoIntermedio = 0;
    const int tiempoMinimo = 10;
    const int tiempoMaximo = 10;
    const int tiempoSonido = 3;
    static readonly double[] varjoLUT = { 0.0, 0.0196, 0.0392, 0.0588, 0.0784, 0.0980, 0.1176, 0.1373, 0.1569, 0.1765, 0.1961, 0.2157, 0.2353, 0.2549, 0.2745, 0.2941, 0.3137, 0.3333, 0.3529, 0.3725, 0.3922, 0.4118, 0.4314, 0.4510, 0.4706, 0.4902, 0.5098, 0.5294, 0.5490, 0.5686, 0.5882, 0.6078, 0.6275, 0.6471, 0.6667, 0.6863, 0.7059, 0.7255, 0.7451, 0.7647, 0.7843, 0.8039, 0.8235, 0.8431, 0.8627, 0.8824, 0.9020, 0.9216, 0.9412, 0.9608, 0.9804, 1 };
    static readonly double[] radiometricR = { 0.0036415153, 0.0041136937, 0.0047619748, 0.0064641768, 0.0089457277, 0.012948697, 0.016985634, 0.019186245, 0.022242598, 0.028412078, 0.033407703, 0.042138238, 0.049356334, 0.056864291, 0.065402433, 0.072489858, 0.081282705, 0.098195739, 0.10663380, 0.12188833, 0.13299249, 0.14799885, 0.17247130, 0.19155687, 0.20278160, 0.21657750, 0.23927799, 0.25966731, 0.28565082, 0.30893618, 0.33334726, 0.36088830, 0.38053948, 0.40347749, 0.43126181, 0.45639011, 0.48655039, 0.51351810, 0.54107213, 0.58694190, 0.59911501, 0.62261444, 0.65892726, 0.68665349, 0.72442943, 0.75908828, 0.80241746, 0.82205957, 0.85975766, 0.89704907, 0.94101459, 1.0060135 };
    static readonly double[] radiometricG = { 0.0043329024, 0.0046412973, 0.0051470213, 0.0067076702, 0.0087277573, 0.011387295, 0.016925396, 0.019422691, 0.023243951, 0.027624382, 0.032041144, 0.040408712, 0.050144281, 0.057830770, 0.063152038, 0.074188806, 0.083909765, 0.097232908, 0.11133846, 0.12571031, 0.14183782, 0.15270270, 0.17799506, 0.19231001, 0.21454170, 0.23336072, 0.24983798, 0.27906501, 0.29830056, 0.32382405,  0.35674945, 0.37796408, 0.39949873, 0.43438414, 0.46372908, 0.49449611, 0.52470195, 0.55296391, 0.58393985, 0.62266701, 0.64673519, 0.67222977, 0.70356941, 0.73696029, 0.77233106, 0.80154687, 0.84800565, 0.88494182, 0.91741985, 0.95352703, 1.0078324, 1.0419444 };
    static readonly double[] radiometricB = { 0.0038866361,  0.0043615871,    0.0049015936,    0.0067822309,    0.0092643378,    0.012700945, 0.018106382, 0.022801554, 0.025894418, 0.031868462, 0.037389204, 0.045434039, 0.054088153, 0.063777506, 0.074589096, 0.081829809, 0.093089923, 0.10532232,  0.12310474,  0.14330487,  0.15345378,  0.16921461,  0.18878457,  0.20179060,  0.22424343,  0.24407145,  0.26330540,  0.28868708,  0.31102318,  0.34151322, 0.37087816,  0.39708838,  0.42392385,  0.44979119,  0.46865204,  0.51126760,  0.53565985,  0.56478530,  0.59470272,  0.63393497,  0.65271789,  0.67853159,  0.70237482,  0.73912203,  0.77247822,  0.79398704,  0.84356773, 0.88042206,  0.91820866,  0.95663768,  0.97467411,  1.0095981 };
    //******************************************codigo relacionado con lcms2  ****************************************
    //simulamos el tipo de datos cmsCIEXYZ de la libreria lcms2
    public struct cmsCIEXYZ
    {
        public double x, y, z;
    }
    cmsCIEXYZ[] NuevaText;

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
    posiciones[] vector_posiciones;
    lucesController[] luces_controller;
    //Definimos variables de tipo TextAsset para los archivos .csv
    //coordenadas xbar, ybar zbar
    public TextAsset csvFilecoord;
    //luces
    public TextAsset csvFileluz1;
    public TextAsset csvFileluz2;
    public TextAsset csvFileluz3;
    public TextAsset csvFileluz4;
    public TextAsset csv_PastBlanca;
    public TextAsset csv_Cabina;
    public TextAsset csvAnchor1;
    public TextAsset csvAnchor2;
    public TextAsset csvAnchor3;
    public TextAsset csvAnchor4;
    public TextAsset csvS0500N;
    public TextAsset csvS0502Y;
    public TextAsset csvS0505Y;
    public TextAsset csvS0502Y50R;
    public TextAsset csvS0505Y50R;
    public TextAsset csvS0502R;
    public TextAsset csvS0505R;
    public TextAsset csvS0502R50B;
    public TextAsset csvS0505R50B;
    public TextAsset csvS0502B;
    public TextAsset csvS0505B;
    public TextAsset csvS0502B50G;
    public TextAsset csvS0505B50G;
    public TextAsset csvS0502G;
    public TextAsset csvS0505G;
    public TextAsset csvS0502G50Y;
    public TextAsset csvS0505G50Y;
    [SerializeField]
    public String nombre;
    [SerializeField]
    public String intento;

    
    public StreamWriter escribir;
    public List <FileInfo> lineasGuardar;
    //**********************************

    //*******************
    string[] records;
     //arrays xbar, ybar zbar
     float[] X;
     float[] Y;
     float[] Z;


     double[] NuevoX;
     double[] NuevoY;
     double[] NuevoZ;


     //arrays para las luces
     float[] luz1;
     float[] luz2;
     float[] luz3;
     float[] luz4;

     //array para las sumas totales de cada coordenada de cada luz

     float[] sumastot;//de 12 posiciones, 3 por cada luz a evaluar


     int width, height;

     cmsCIEXYZ xyz;
     byte[] rgb;//byte equivale a unsigned char
     byte[] rgb2;

     public int tama;//la pongo publica

     //************* variables del escenario **********************
     private Color Coloraux;
     private Color[] colorPastillasNCS;
     private GameObject[] luces = new GameObject[12];
     private GameObject tubo1;
     private GameObject tubo2;

     int extension = 380;
     //arrays para los productos
     float[] Xluz1;
     float[] Yluz1;
     float[] Zluz1;

     TextAsset imagen;
     Texture2D text;
     //En la escena hay que crear un plano para poner la textura resultante

     public GameObject PastillaBlanca;
     public GameObject S0502Y;
     public GameObject S0502Y50R;
     public GameObject S0502R;
     public GameObject S0502R50B;
     public GameObject S0502B;
     public GameObject S0502B50G;
     public GameObject S0502G;
     public GameObject S0502G50Y;
     public GameObject suelo;
     public GameObject fondo;
     public GameObject lateralIzqdo;
     public GameObject lateralDrecho;
     public GameObject frontal;
     public GameObject techo;
     public GameObject anchor1;
     public GameObject anchor2;
     public GameObject anchor3;
     public GameObject anchor4;


     double[] RGB_Light_Source;
     double BlancoY;
     public AudioSource source { get { return GetComponent<AudioSource>(); } }
     public AudioClip clip;
    public GameObject rightHand;
     // Use this for initialization
     void Start()
     {
          colorPastillasNCS = new Color[9];
          //Inicializacion de variables de lcms2
          tama = 2048 * 2048;//+1078;
          BlancoY = 0;
          rgb = new byte[3];
          rgb2 = new byte[tama * 3];
          NuevoX = new double[tama];
          NuevoY = new double[tama];
          NuevoZ = new double[tama];


          xyz.x = 0f;
          xyz.y = 0f;
          xyz.z = 0f;

          width = 1;
          height = 1;

          imagen = new TextAsset();

          //arrays para las luces
          luz1 = new float[101];
          luz2 = new float[101];
          luz3 = new float[101];
          luz4 = new float[101];

          Xluz1 = new float[101];
          Yluz1 = new float[101];
          Zluz1 = new float[101];



          sumastot = new float[12];
          //inicializamos todo a 0.0
          for (int i = 0; i < 12; i++)
          {
               sumastot[i] = 0.0f;
          }



          //leer los ficheros csv de coordenadas x y zbar
          leerCSVtresfich(csvFilecoord, ref X, ref Y, ref Z);


          tubo1 = GameObject.Find("TuboFluo1");
          tubo2 = GameObject.Find("TuboFluo2");


          for (int i = 0; i < 6; i++)
          {
               luces[i] = tubo1.transform.GetChild(i).gameObject;
          }
          int j = 0;
          for (int i = 6; i < 12; i++)
          {
               luces[i] = tubo2.transform.GetChild(j).gameObject;
               j++;
          }

          //Apagamos las luces
          tubo1.SetActive(false);
          tubo2.SetActive(false);



          vector_posiciones = new posiciones[9];
          vector_posiciones[0].x = -11.92f;
          vector_posiciones[0].z = 2.87f;
          vector_posiciones[1].x = -5.87f;
          vector_posiciones[1].z = -9.6f;
          vector_posiciones[2].x = 8.84f;
          vector_posiciones[2].z = 2.61f;
          vector_posiciones[3].x = -6.6f;
          vector_posiciones[3].z = 4.34f;
          vector_posiciones[4].x = -11.85f;
          vector_posiciones[4].z = -5.64f;
          vector_posiciones[5].x = 4.63f;
          vector_posiciones[5].z = -9.07f;
          vector_posiciones[6].x = 2.34f;
          vector_posiciones[6].z = 3.82f;
          vector_posiciones[7].x = 9.81f;
          vector_posiciones[7].z = -4.52f;
          vector_posiciones[8].x = -0.88f;
          vector_posiciones[8].z = -3.56f;

          luces_controller = new lucesController[4];
          //Luz D65
          luces_controller[0].anterior = false;
          luces_controller[0].veces1 = 0;
          luces_controller[0].veces2 = 0;
          luces_controller[0].muestra_facil = false;

          //Luz FL11
          luces_controller[1].anterior = false;
          luces_controller[1].veces1 = 0;
          luces_controller[1].veces2 = 0;
          luces_controller[1].muestra_facil = false;

          //Warm LED
          luces_controller[2].anterior = false;
          luces_controller[2].veces1 = 0;
          luces_controller[2].veces2 = 0;
          luces_controller[2].muestra_facil = false;

          //Blue LED
          luces_controller[3].anterior = false;
          luces_controller[3].veces1 = 0;
          luces_controller[3].veces2 = 0;
          luces_controller[3].muestra_facil = false;



          gameObject.AddComponent<AudioSource>();

          
          //Encendemos las luces despues de los 150 segundos de adaptacion
          StartCoroutine("esperar");





     }

     // Update is called once per frame
     void Update()
     {
         
     }
     public static void Barajar<T>(IList<T> values)
     {
          var n = values.Count;
          var rnd = new System.Random();
          for (int i = 9 - 1; i > 0; i--)
          {
               var j = rnd.Next(0, i);
               var temp = values[i];
               values[i] = values[j];
               values[j] = temp;
          }
     }

     public void Calcular_Pintar(TextAsset fichero, float factor)
     {
          //Lo primero que hacemos es activar los tubos de luces
          tubo1.SetActive(true);
          tubo2.SetActive(true);

          //Calculamos luces
          leerCSVuno(fichero, ref luz1);
          productosuma(luz1, X, ref sumastot[0]);
          productosuma(luz1, Y, ref sumastot[1]);
          productosuma(luz1, Z, ref sumastot[2]);

          //Normalizo dividiendo por Y
          sumastot[0] = sumastot[0] / sumastot[1];
          sumastot[2] = sumastot[2] / sumastot[1];
          sumastot[1] = sumastot[1] / sumastot[1];


          xyz.x = (double)(sumastot[0]) * factor;
          xyz.y = (double)(sumastot[1]) * factor;
          xyz.z = (double)(sumastot[2]) * factor;




          RGB_Light_Source = new double[3];

          XYZ2RGB(xyz.x, xyz.y, xyz.z, ref RGB_Light_Source);

          //este código seria comun a todas las luces
          Coloraux.r = (float)((RGB_Light_Source[0] / 255f));
          Coloraux.g = (float)((RGB_Light_Source[1] / 255f));
          Coloraux.b = (float)((RGB_Light_Source[2] / 255f));

          //Modificamos las luces
          for (int i = 0; i < 12; i++)
          {
               luces[i].GetComponent<Light>().color = Coloraux;
          }



          for (int p = 0; p < 15; p++)
          {


               //**************************************************
               //Pastilla Blanca

               double NuevoXC = 0;
               double NuevoYC = 0;
               double NuevoZC = 0;
               double XBlanco = 0;
               double YBlanco = 0;
               double ZBlanco = 0;

               float[] calculuz = new float[101];
               leerCSVuno(fichero, ref calculuz);

               leerCSVtresfich(csvFilecoord, ref X, ref Y, ref Z);

               //tengo 3 nuevos arrays con el producto de la luz y x,y,z
               calculoprod(calculuz, X, Y, Z, ref Xluz1, ref Yluz1, ref Zluz1);




               float[] PastBlanca = new float[101];
               leerCSVunoPastilla(csv_PastBlanca, ref PastBlanca);

               float[] PastTest = new float[101];
               switch (p)
               {
                    case 0:
                         leerCSVunoPastilla(csv_PastBlanca, ref PastTest);
                         break;

                    case 1:
                         leerCSVunoPastilla(csvAnchor1, ref PastTest);
                         break;
                    case 2:
                         leerCSVunoPastilla(csvAnchor2, ref PastTest);
                         break;
                    case 3:
                         leerCSVunoPastilla(csvAnchor3, ref PastTest);
                         break;
                    case 4:
                         leerCSVunoPastilla(csvAnchor4, ref PastTest);
                         break;
                    case 5:
                         leerCSVunoPastilla(csvS0500N, ref PastTest);
                         break;
                    case 6:
                         leerCSVunoPastilla(csvS0502Y, ref PastTest);
                         break;
                    case 7:
                         leerCSVunoPastilla(csvS0502Y50R, ref PastTest);
                         break;
                    case 8:
                         leerCSVunoPastilla(csvS0502R, ref PastTest);
                         break;
                    case 9:
                         leerCSVunoPastilla(csvS0502R50B, ref PastTest);
                         break;
                    case 10:
                         leerCSVunoPastilla(csvS0502B, ref PastTest);
                         break;
                    case 11:
                         leerCSVunoPastilla(csvS0502B50G, ref PastTest);
                         break;
                    case 12:
                         leerCSVunoPastilla(csvS0502G, ref PastTest);
                         break;
                    case 13:
                         leerCSVunoPastilla(csvS0502G50Y, ref PastTest);
                         break;
                    case 14:
                         leerCSVuno(csv_Cabina, ref PastTest);
                         break;

               }



               //bucle para recorrer 101 valores de pastilla blanca

               //Calculo la pastilla Blanca
               for (int i = 0; i < 101; i++)
               {
                    XBlanco = XBlanco + Xluz1[i] * PastBlanca[i];
                    YBlanco = YBlanco + Yluz1[i] * PastBlanca[i];


                    ZBlanco = ZBlanco + Zluz1[i] * PastBlanca[i];
                    //print ("dentro de for");
               }

               //Calculo el X,Y,Z de la pastillas a pintar
               for (int i = 0; i < 101; i++)
               {
                    NuevoXC = NuevoXC + Xluz1[i] * PastTest[i];
                    NuevoYC = NuevoYC + Yluz1[i] * PastTest[i];


                    NuevoZC = NuevoZC + Zluz1[i] * PastTest[i];
                    //print ("dentro de for");
               }



               xyz.x = (NuevoXC / YBlanco) * factor;
               xyz.y = (NuevoYC / YBlanco) * factor;
               xyz.z = (NuevoZC / YBlanco) * factor;
               RGB_Light_Source = new double[3];

               XYZ2RGB(xyz.x, xyz.y, xyz.z, ref RGB_Light_Source);

               Coloraux.r = (float)((RGB_Light_Source[0] / 255f));// / (rgb[0]/255f));///rgb[0];//dividimos por las luces

               Coloraux.g = (float)((RGB_Light_Source[1] / 255f));// / (rgb[1]/255f));///rgb[1];

               Coloraux.b = (float)((RGB_Light_Source[2] / 255f) * 1.063);// / (rgb[2]/255f));///rgb[2];

               switch (p)
               {
                    case 0:
                         PastillaBlanca.GetComponent<Renderer>().material.color = Coloraux;
                         break;

                    case 1:
                         anchor1.GetComponent<Renderer>().material.color = Coloraux;
                         Debug.Log("Valores Anchor1: " + xyz.x + "," + xyz.y + ", " + xyz.z);

                         break;
                    case 2:
                         anchor2.GetComponent<Renderer>().material.color = Coloraux;
                         break;
                    case 3:
                         anchor3.GetComponent<Renderer>().material.color = Coloraux;
                         break;
                    case 4:
                         anchor4.GetComponent<Renderer>().material.color = Coloraux;
                         break;
                    case 5:
                         PastillaBlanca.GetComponent<Renderer>().material.color = Coloraux;
                         colorPastillasNCS[0] = Coloraux;
                         break;
                    case 6:
                         S0502Y.GetComponent<Renderer>().material.color = Coloraux;
                         colorPastillasNCS[1] = Coloraux;
                         break;
                    case 7:
                         S0502Y50R.GetComponent<Renderer>().material.color = Coloraux;
                         colorPastillasNCS[2] = Coloraux;
                         break;
                    case 8:
                         S0502R.GetComponent<Renderer>().material.color = Coloraux;
                         colorPastillasNCS[3] = Coloraux;
                         break;
                    case 9:
                         S0502R50B.GetComponent<Renderer>().material.color = Coloraux;
                         colorPastillasNCS[4] = Coloraux;
                         break;
                    case 10:
                         S0502B.GetComponent<Renderer>().material.color = Coloraux;
                         colorPastillasNCS[5] = Coloraux;
                         break;
                    case 11:
                         S0502B50G.GetComponent<Renderer>().material.color = Coloraux;
                         colorPastillasNCS[6] = Coloraux;
                         break;
                    case 12:
                         S0502G.GetComponent<Renderer>().material.color = Coloraux;
                         colorPastillasNCS[7] = Coloraux;
                         break;
                    case 13:
                         S0502G50Y.GetComponent<Renderer>().material.color = Coloraux;
                         colorPastillasNCS[8] = Coloraux;
                         break;
                    case 14:
                         suelo.GetComponent<Renderer>().material.color = Coloraux;
                         fondo.GetComponent<Renderer>().material.color = Coloraux;
                         lateralDrecho.GetComponent<Renderer>().material.color = Coloraux;
                         lateralIzqdo.GetComponent<Renderer>().material.color = Coloraux;
                         frontal.GetComponent<Renderer>().material.color = Coloraux;
                         techo.GetComponent<Renderer>().material.color = Coloraux;
                         break;
               }
          }//fin del for
     }
     //Checkeo del usuario con pastillas de más saturación
     public void Calcular_Pintar_Facil(TextAsset fichero, float factor)
     {
          //Lo primero que hacemos es activar los tubos de luces
          tubo1.SetActive(true);
          tubo2.SetActive(true);

          //Calculamos luces
          leerCSVuno(fichero, ref luz1);
          productosuma(luz1, X, ref sumastot[0]);
          productosuma(luz1, Y, ref sumastot[1]);
          productosuma(luz1, Z, ref sumastot[2]);

          //Normalizo dividiendo por Y
          sumastot[0] = sumastot[0] / sumastot[1];
          sumastot[2] = sumastot[2] / sumastot[1];
          sumastot[1] = sumastot[1] / sumastot[1];


          xyz.x = (double)(sumastot[0]) * factor;
          xyz.y = (double)(sumastot[1]) * factor;
          xyz.z = (double)(sumastot[2]) * factor;




          RGB_Light_Source = new double[3];

          XYZ2RGB(xyz.x, xyz.y, xyz.z, ref RGB_Light_Source);

          //este código seria comun a todas las luces
          Coloraux.r = (float)((RGB_Light_Source[0] / 255f));
          Coloraux.g = (float)((RGB_Light_Source[1] / 255f));
          Coloraux.b = (float)((RGB_Light_Source[2] / 255f));

          //Modificamos las luces
          for (int i = 0; i < 12; i++)
          {
               luces[i].GetComponent<Light>().color = Coloraux;
          }



          for (int p = 0; p < 15; p++)
          {


               //**************************************************
               //Pastilla Blanca

               double NuevoXC = 0;
               double NuevoYC = 0;
               double NuevoZC = 0;
               double XBlanco = 0;
               double YBlanco = 0;
               double ZBlanco = 0;

               float[] calculuz = new float[101];
               leerCSVuno(fichero, ref calculuz);

               leerCSVtresfich(csvFilecoord, ref X, ref Y, ref Z);

               //tengo 3 nuevos arrays con el producto de la luz y x,y,z
               calculoprod(calculuz, X, Y, Z, ref Xluz1, ref Yluz1, ref Zluz1);




               float[] PastBlanca = new float[101];
               leerCSVunoPastilla(csv_PastBlanca, ref PastBlanca);

               float[] PastTest = new float[101];
               switch (p)
               {
                    case 0:
                         leerCSVunoPastilla(csv_PastBlanca, ref PastTest);
                         break;

                    case 1:
                         leerCSVunoPastilla(csvAnchor1, ref PastTest);
                         break;
                    case 2:
                         leerCSVunoPastilla(csvAnchor2, ref PastTest);
                         break;
                    case 3:
                         leerCSVunoPastilla(csvAnchor3, ref PastTest);
                         break;
                    case 4:
                         leerCSVunoPastilla(csvAnchor4, ref PastTest);
                         break;
                    case 5:
                         leerCSVunoPastilla(csvS0500N, ref PastTest);
                         break;
                    case 6:
                         leerCSVunoPastilla(csvS0505Y, ref PastTest);
                         break;
                    case 7:
                         leerCSVunoPastilla(csvS0505Y50R, ref PastTest);
                         break;
                    case 8:
                         leerCSVunoPastilla(csvS0505R, ref PastTest);
                         break;
                    case 9:
                         leerCSVunoPastilla(csvS0505R50B, ref PastTest);
                         break;
                    case 10:
                         leerCSVunoPastilla(csvS0505B, ref PastTest);
                         break;
                    case 11:
                         leerCSVunoPastilla(csvS0505B50G, ref PastTest);
                         break;
                    case 12:
                         leerCSVunoPastilla(csvS0505G, ref PastTest);
                         break;
                    case 13:
                         leerCSVunoPastilla(csvS0505G50Y, ref PastTest);
                         break;
                    case 14:
                         leerCSVuno(csv_Cabina, ref PastTest);
                         break;

               }



               //bucle para recorrer 101 valores de pastilla blanca

               //Calculo la pastilla Blanca
               for (int i = 0; i < 101; i++)
               {
                    XBlanco = XBlanco + Xluz1[i] * PastBlanca[i];
                    YBlanco = YBlanco + Yluz1[i] * PastBlanca[i];


                    ZBlanco = ZBlanco + Zluz1[i] * PastBlanca[i];
                    //print ("dentro de for");
               }

               //Calculo el X,Y,Z de la pastillas a pintar
               for (int i = 0; i < 101; i++)
               {
                    NuevoXC = NuevoXC + Xluz1[i] * PastTest[i];
                    NuevoYC = NuevoYC + Yluz1[i] * PastTest[i];


                    NuevoZC = NuevoZC + Zluz1[i] * PastTest[i];
                    //print ("dentro de for");
               }



               xyz.x = (NuevoXC / YBlanco) * factor;
               xyz.y = (NuevoYC / YBlanco) * factor;
               xyz.z = (NuevoZC / YBlanco) * factor;
               RGB_Light_Source = new double[3];

               XYZ2RGB(xyz.x, xyz.y, xyz.z, ref RGB_Light_Source);

               Coloraux.r = (float)((RGB_Light_Source[0] / 255f));// / (rgb[0]/255f));///rgb[0];//dividimos por las luces

               Coloraux.g = (float)((RGB_Light_Source[1] / 255f));// / (rgb[1]/255f));///rgb[1];

               Coloraux.b = (float)((RGB_Light_Source[2] / 255f) * 1.063);// / (rgb[2]/255f));///rgb[2];

               switch (p)
               {
                    case 0:
                         PastillaBlanca.GetComponent<Renderer>().material.color = Coloraux;
                         break;

                    case 1:
                         anchor1.GetComponent<Renderer>().material.color = Coloraux;
                         break;
                    case 2:
                         anchor2.GetComponent<Renderer>().material.color = Coloraux;
                         break;
                    case 3:
                         anchor3.GetComponent<Renderer>().material.color = Coloraux;
                         break;
                    case 4:
                         anchor4.GetComponent<Renderer>().material.color = Coloraux;
                         break;
                    case 5:
                         PastillaBlanca.GetComponent<Renderer>().material.color = Coloraux;
                         colorPastillasNCS[0] = Coloraux;
                         break;
                    case 6:
                         S0502Y.GetComponent<Renderer>().material.color = Coloraux;
                         colorPastillasNCS[1] = Coloraux;
                         break;
                    case 7:
                         S0502Y50R.GetComponent<Renderer>().material.color = Coloraux;
                         colorPastillasNCS[2] = Coloraux;
                         break;
                    case 8:
                         S0502R.GetComponent<Renderer>().material.color = Coloraux;
                         colorPastillasNCS[3] = Coloraux;
                         break;
                    case 9:
                         S0502R50B.GetComponent<Renderer>().material.color = Coloraux;
                         colorPastillasNCS[4] = Coloraux;
                         break;
                    case 10:
                         S0502B.GetComponent<Renderer>().material.color = Coloraux;
                         colorPastillasNCS[5] = Coloraux;
                         break;
                    case 11:
                         S0502B50G.GetComponent<Renderer>().material.color = Coloraux;
                         colorPastillasNCS[6] = Coloraux;
                         break;
                    case 12:
                         S0502G.GetComponent<Renderer>().material.color = Coloraux;
                         colorPastillasNCS[7] = Coloraux;
                         break;
                    case 13:
                         S0502G50Y.GetComponent<Renderer>().material.color = Coloraux;
                         colorPastillasNCS[8] = Coloraux;
                         break;
                    case 14:
                         suelo.GetComponent<Renderer>().material.color = Coloraux;
                         fondo.GetComponent<Renderer>().material.color = Coloraux;
                         lateralDrecho.GetComponent<Renderer>().material.color = Coloraux;
                         lateralIzqdo.GetComponent<Renderer>().material.color = Coloraux;
                         frontal.GetComponent<Renderer>().material.color = Coloraux;
                         techo.GetComponent<Renderer>().material.color = Coloraux;
                         break;
               }
          }//fin del for
     }

     public IEnumerator esperar()
     {
          yield return new WaitForSeconds(tiempoInicial - tiempoSonido);
          PlaySound();
          yield return new WaitForSeconds(tiempoSonido);
          float fac;
          Barajar(vector_posiciones);
          for (int i = 0; i < 9; i++)
          {
               S0502Y.transform.position = new Vector3(vector_posiciones[0].x, 1.16f, vector_posiciones[0].z);

               S0502Y50R.transform.position = new Vector3(vector_posiciones[1].x, 1.16f, vector_posiciones[1].z);


               S0502R.transform.position = new Vector3(vector_posiciones[2].x, 1.16f, vector_posiciones[2].z);
               S0502R50B.transform.position = new Vector3(vector_posiciones[3].x, 1.16f, vector_posiciones[3].z);
               S0502B.transform.position = new Vector3(vector_posiciones[4].x, 1.16f, vector_posiciones[4].z);
               S0502B50G.transform.position = new Vector3(vector_posiciones[5].x, 1.16f, vector_posiciones[5].z);
               S0502G.transform.position = new Vector3(vector_posiciones[6].x, 1.16f, vector_posiciones[6].z);
               S0502G50Y.transform.position = new Vector3(vector_posiciones[7].x, 1.16f, vector_posiciones[7].z);
               PastillaBlanca.transform.position = new Vector3(vector_posiciones[8].x, 1.16f, vector_posiciones[8].z);
          }

          var rnd = new System.Random();
          int a = rnd.Next(0, 4);
          Debug.Log("Luz= " + a);
          if (a == 0)
          {
               fac = 0.8f;
               Calcular_Pintar(csvFileluz1, fac);
          }
          else if (a == 1)
          {
               fac = 0.8f;
               Calcular_Pintar(csvFileluz2, fac);
          }
          else if (a == 2)
          {
               fac = 0.8f;
               Calcular_Pintar(csvFileluz3, fac);
          }
          else
          {
               fac = 0.8f;
               Calcular_Pintar(csvFileluz4, fac);
          }

          tubo1.SetActive(true);
          tubo2.SetActive(true);
          var rndTiempo = new System.Random();
          int randonTiempo = rndTiempo.Next(0, 2);
          int tiempoSegundos = 0;
          if (randonTiempo == 0)
          {
               tiempoSegundos = tiempoMinimo;
               //y sumamos uno al numero de veces de 30 segundos
               luces_controller[a].veces1++;
          }
          else
          {
               tiempoSegundos = tiempoMaximo;
               //y sumamos uno al numero de veces de 60 segundos
               luces_controller[a].veces2++;
          }

          yield return new WaitForSeconds(tiempoSegundos - tiempoSonido);
          PlaySound();
          rightHand.SetActive(true);
          //Ponemos a true anterior para que no se repita
          luces_controller[a].anterior = true;

          Debug.Log("Luz= " + a + " numVeces 30 segundos= " + luces_controller[a].veces1 + " numVeces 60 segundos= " + luces_controller[a].veces2);
          yield return new WaitForSeconds(tiempoSonido);

          rightHand.SetActive(false);
          tubo1.SetActive(false);
          tubo2.SetActive(false);
          yield return new WaitForSeconds(tiempoIntermedio);

          for (int j = 0; j < (numLuces * numTrial) - 1; j++)
          {
               tubo1.SetActive(true);
               tubo2.SetActive(true);

               Barajar(vector_posiciones);
               for (int i = 0; i < 9; i++)
               {
                    S0502Y.transform.position = new Vector3(vector_posiciones[0].x, 1.16f, vector_posiciones[0].z);

                    S0502Y50R.transform.position = new Vector3(vector_posiciones[1].x, 1.16f, vector_posiciones[1].z);


                    S0502R.transform.position = new Vector3(vector_posiciones[2].x, 1.16f, vector_posiciones[2].z);
                    S0502R50B.transform.position = new Vector3(vector_posiciones[3].x, 1.16f, vector_posiciones[3].z);
                    S0502B.transform.position = new Vector3(vector_posiciones[4].x, 1.16f, vector_posiciones[4].z);
                    S0502B50G.transform.position = new Vector3(vector_posiciones[5].x, 1.16f, vector_posiciones[5].z);
                    S0502G.transform.position = new Vector3(vector_posiciones[6].x, 1.16f, vector_posiciones[6].z);
                    S0502G50Y.transform.position = new Vector3(vector_posiciones[7].x, 1.16f, vector_posiciones[7].z);
                    PastillaBlanca.transform.position = new Vector3(vector_posiciones[8].x, 1.16f, vector_posiciones[8].z);
               }
               var rnd2 = new System.Random();
               int a2 = rnd2.Next(0, 4);
               //Bucle que evita que una luz aparezca dos veces seguidas
               /*while(luces_controller[a2].anterior==true || (luces_controller[a2].veces1 == 5 && luces_controller[a2].veces2 == 5))
               {
                    rnd2 = new System.Random();
                    a2 = rnd2.Next(0, 4);
               }*/
               Debug.Log("Luz= " + a2);

               var rnd3 = new System.Random();
               int checkear = rnd3.Next(0, 2);
               if (a2 == 0)
               {
                    fac = 0.8f;
                    if ((luces_controller[a2].muestra_facil == false && checkear == 1) || (luces_controller[a2].veces1 + luces_controller[a2].veces2 == 5))
                    {
                         Calcular_Pintar_Facil(csvFileluz1, fac);
                         luces_controller[a2].muestra_facil = true;
                         Debug.Log("Checkeo ");
                    }
                    else
                    {
                         Calcular_Pintar(csvFileluz1, fac);
                    }

               }
               else if (a2 == 1)
               {
                    fac = 0.8f;
                    if ((luces_controller[a2].muestra_facil == false && checkear == 1) || (luces_controller[a2].veces1 + luces_controller[a2].veces2 == 5))
                    {
                         Calcular_Pintar_Facil(csvFileluz2, fac);
                         luces_controller[a2].muestra_facil = true;
                         Debug.Log("Checkeo ");
                    }
                    else
                    {
                         Calcular_Pintar(csvFileluz2, fac);
                    }
               }
               else if (a2 == 2)
               {
                    fac = 0.8f;
                    if ((luces_controller[a2].muestra_facil == false && checkear == 1) || (luces_controller[a2].veces1 + luces_controller[a2].veces2 == 5))
                    {
                         Calcular_Pintar_Facil(csvFileluz3, fac);
                         luces_controller[a2].muestra_facil = true;
                         Debug.Log("Checkeo ");
                    }
                    else
                    {
                         Calcular_Pintar(csvFileluz3, fac);
                    }
               }
               else
               {
                    fac = 0.8f;
                    if ((luces_controller[a2].muestra_facil == false && checkear == 1) || (luces_controller[a2].veces1 + luces_controller[a2].veces2 == 5))
                    {
                         Calcular_Pintar_Facil(csvFileluz4, fac);
                         luces_controller[a2].muestra_facil = true;
                         Debug.Log("Checkeo ");
                    }
                    else
                    {
                         Calcular_Pintar(csvFileluz4, fac);
                    }
               }

               rndTiempo = new System.Random();
               randonTiempo = rndTiempo.Next(0, 2);
               //Si sale 0 elegimos la opcion de 30 segundos, también lo hacemos si la opcion de 60 ya tiene 5 veces
               if ((randonTiempo == 0 && luces_controller[a2].veces1 < 5) || luces_controller[a2].veces2 == 5)
               {
                    tiempoSegundos = tiempoMinimo;
                    //y sumamos uno al numero de veces
                    luces_controller[a2].veces1++;
               }
               else
               {
                    tiempoSegundos = tiempoMaximo;
                    //y sumamos uno al numero de veces
                    luces_controller[a2].veces2++;
               }

               yield return new WaitForSeconds(tiempoSegundos - tiempoSonido);
               PlaySound();
               rightHand.SetActive(true);
               //Ponemos a true anterior para que no se repita y actualizamos valores
               luces_controller[a2].anterior = true;
               luces_controller[a].anterior = false;
               a = a2;
               Debug.Log("Luz= " + a2 + " numVeces 30 segundos= " + luces_controller[a2].veces1 + " numVeces 60 segundos= " + luces_controller[a2].veces2);
               yield return new WaitForSeconds(tiempoSonido);
               rightHand.SetActive(false);
               tubo1.SetActive(false);
               tubo2.SetActive(false);

               yield return new WaitForSeconds(tiempoIntermedio);
          }

        autoSave();
     }
    void autoSave()
    {
        escribir = new StreamWriter(@".\\Assets\\Results\\" + nombre + "_" + intento + ".csv", true, Encoding.ASCII);//true es de append
        
        
        escribir.WriteLine("Lighting" +"Time"+ "CheckScene"+ "Option");
    }
     void PlaySound()
     {
          source.PlayOneShot(clip);
     }

     //lee 401 valores de cada luz (una a una) y almacena 101. Además multiplica por 100 porque los valores de las pastillas normaliza en 100 y no en 1
     public void leerCSVuno(TextAsset fichero, ref float[] uno)
     {
          records = fichero.text.Split('\n');
          uno = new float[101];

          int i = 0;
          int j = 0;
          while (i < records.Length - 1)
          {
               uno[j] = float.Parse(records[i]) * 100;
               j++;
               i = i + 4;
          }

     }
     public void leerCSVunoPastilla(TextAsset fichero, ref float[] uno)
     {
          records = fichero.text.Split('\n');
          uno = new float[101];

          int i = 0;
          int j = 0;
          while (i < records.Length - 1)
          {
               //Debug.Log ("luz " + j + " " + records [i]);
               uno[j] = float.Parse(records[i]);
               j++;
               i = i + 1;
          }

     }
     //lee los 402 valores y solo almacena 101 (los va guardando de 4 en 4
     void leerCSVtresfich(TextAsset fichero, ref float[] uno, ref float[] dos, ref float[] tres)
     {
          records = fichero.text.Split('\n');
          uno = new float[101];
          dos = new float[101];
          tres = new float[101];
          //Debug.Log ("longitud en leerCSVluces" + records.Length);
          int i = 0;
          int j = 0;
          while (i < records.Length - 1)
          {
               //Debug.Log ("luces " + j+" "+records [i]);
               string[] campos = records[i].Split(';');
               uno[j] = float.Parse(campos[0]) * 100;
               dos[j] = float.Parse(campos[1]) * 100;
               tres[j] = float.Parse(campos[2]) * 100;
               j++;
               i = i + 4;
          }
     }

     public void productosuma(float[] luz, float[] coord, ref float suma)
     {
          suma = 0.0f;
          //Debug.Log ("primer dato " + luz [0] + " coord 0: " + coord [0]);
          //Debug.Log ("long luz "+(luz.Length - 1) + " long coord "+(coord.Length- 1));
          for (int i = 0; i < luz.Length - 1; i++)
          {
               suma = suma + luz[i] * coord[i];
          }

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

     // Funtion to transform colors from XYZ to RGB values (0-255)
     void XYZ2RGB(double X, double Y, double Z, ref double[] RGB)
     {


        RGB = new double[3];
        double r, g, b;

        // XYZ2RGB matrix from Chromatic Characterization of Display HTC 29/10/2020
       /* r = 2.19401983435662 * X - 0.692958803514161 * Y - 0.322875904092810 * Z;
        g = (-0.873465346287721) * X + 1.79985848664979 * Y + 0.0230227623531801 * Z;
        b = 0.0410600938980302 * X - 0.0865967401325783 * Y + 0.878290921190566 * Z;*/
        /*r = 3.2404542 * X - 1.5371385 * Y - 0.4985314 * Z;
        g = (-0.9692660) * X + 1.8760108 * Y + 0.0415560 * Z;
        b = 0.0556434 * X - 0.2040259 * Y + 1.0572252 * Z;*/

        r = 0.023945363 * X - 0.0064386367 * Y + 0.00047437710 * Z;
        g = -0.011394591 * X + 0.012642452 * Y - 0.0015468607 * Z;
        b = -0.0037081367 * X + 0.0000043218997 * Y + 0.0078205680 * Z;
        // Apply gamma transform
        /*(RGB[0]) = (gamma(r, 1) * 255.0);
        (RGB[1]) = (gamma(g, 2) * 255.0);
        (RGB[2]) = (gamma(b, 3) * 255.0);*/
        (RGB[0]) = (gamma(r, 1) * 255.0);
        (RGB[1]) = (gamma(g, 2) * 255.0);
        (RGB[2]) = (gamma(b, 3) * 255.0); 
     }

     // Funtion to apply gamma transform
     private double gamma(double r, int opcion)    
     {
        double R;

        

        if (opcion == 1)
        {
            // Crear un interpolador lineal
            IInterpolation interpolator = Interpolate.Linear(radiometricR, varjoLUT);
            // Realizar interpolación
            R = interpolator.Interpolate(r);
        }
        else if (opcion == 2)
        {
            // Crear un interpolador lineal
            IInterpolation interpolator = Interpolate.Linear(radiometricG, varjoLUT);
            // Realizar interpolación
            R = interpolator.Interpolate(r);
        }
        else
        {
            // Crear un interpolador lineal
            IInterpolation interpolator = Interpolate.Linear(radiometricB, varjoLUT);
            // Realizar interpolación
            R = interpolator.Interpolate(r);
        }
        
        return R;

        /*if (r <= 0.0031308)
                R = 12.92 * r;
           else
                R = Math.Pow(r, 1.0 / 2.4) * 1.055 - 0.055;

          if (opcion == 1)
          {
               R = Math.Pow(r, (1.0 / 2.34153663077439));
          }
          else if (opcion == 2)
          {
               R = Math.Pow(r, (1.0 / 2.30542383673930));
          }
          else
          {
               R = Math.Pow(r, (1.0 / 2.26398094679332));
          }
          if (Double.IsNaN(R))
          {
               R = 0;
          }
          return R;*/
     }


}
