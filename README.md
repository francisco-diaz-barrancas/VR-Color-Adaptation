# Chromatic Adaptation in Immersive Virtual Reality: Effects of Illuminant Conditions on Rendering Consistency

This repository contains the implementation and experimental framework used to study **color constancy and temporal chromatic adaptation in immersive Virtual Reality (VR) environments**.

The system has been developed in Unity and enables controlled perceptual experiments under different illumination conditions using a VR headset.

---

## 📄 Related Publication

> **Title:** *Chromatic Adaptation in Immersive Virtual Reality: Effects of Illuminant Conditions on Rendering Consistency*
> **Journal:** *Scientific Reports*

If you use this repository, please cite:

```bibtex
@article{vr_color_constancy,
  title   = {Chromatic Adaptation in Immersive Virtual Reality: Effects of Illuminant Conditions on Rendering Consistency},
  journal = {Scientific Reports}
}
```

---

## 🧠 Overview

This project investigates how human observers adapt to different lighting conditions in VR by measuring:

* Color constancy performance
* Temporal stabilization of perception
* Selection of achromatic (neutral) surfaces

Participants are immersed in a virtual scene containing color samples (NCS patches) under varying illuminants and are asked to select the patch perceived as neutral.

![Figure 7](seleccion%20(3).png)

---

## 🕶️ System Description

The VR system includes:

* Dynamic illumination simulation using spectral data
* Color transformation pipeline (CIE XYZ → RGB) calibrated for the display
* Randomized spatial arrangement of color patches
* Interactive selection via VR controller (laser pointer)
* Automated data logging of user responses

---

## 🧪 Experimental Workflow

1. **Adaptation phase**
   The participant is exposed to a given illumination for a fixed duration.

2. **Stimulus presentation**
   A set of color patches is displayed in randomized positions.

3. **User interaction**
   The participant selects the patch perceived as achromatic using the VR controller.

4. **Data recording**
   The system logs:

   * Illuminant condition
   * Time interval
   * Selected patch
   * Spatial configuration

5. **Iteration**
   The process is repeated across multiple illuminants and time intervals.

---

## ⚙️ Requirements

To run this project, you need:

* Unity (recommended: Unity 2019.1.5 LTS or compatible)
* SteamVR SDK
* VR headset (HTC Vive or compatible)
* Windows OS (recommended)

---

## 🚀 How to Run

### 1. Clone the repository

```bash
git clone https://github.com/YOUR_USERNAME/VR-Color-Adaptation.git
```

### 2. Open in Unity

* Launch Unity Hub
* Add the project folder
* Open with the appropriate Unity version

### 3. Setup VR

* Ensure SteamVR is installed and running
* Connect and calibrate your VR headset

### 4. Run the experiment

* Open the main scene
* Press **Play**
* Follow the instructions inside the VR environment

---

## 📊 Dataset

Experimental results are stored in:

```
Assets/Results/
```

Each CSV file contains:

* Lighting condition
* Time interval
* Selected patch
* Spatial arrangement

Data is anonymized to ensure participant privacy.

---

## 🔬 Reproducibility

This repository provides:

* Full Unity source code
* Spectral data used for stimuli generation
* Experimental logging system

To reproduce the experiment:

1. Use the provided spectral CSV files
2. Run the Unity scene under controlled VR conditions
3. Collect output data from the results folder

---

## ⚠️ Notes

* The color transformation (XYZ → RGB) is device-dependent
* Calibration is based on a specific VR display configuration (HTC Vive Pro)
* Results may vary across hardware

---

## 📁 Project Structure

```
Assets/              # Unity assets and scripts
ProjectSettings/     # Unity configuration
Packages/            # Unity dependencies
Dataset/             # Experimental data (if included)
```

---

## 🤝 Contributions

This repository is intended for research and reproducibility purposes.
Feel free to use and extend it in your own work.

---

## 📜 License

Specify your license here (e.g., MIT License)

---

## ⭐ Acknowledgment

If you use this work, please consider citing the associated publication.
