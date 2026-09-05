Scripts and data used to regenerate the quantitative material of the revision.

Python (colorimetry):
- colorimetry.py : Figure 5 (LightSources.png) and the nominal colorimetry of the patches under each illuminant
  (PATCHES = S0505 set; output stimuli_by_illuminant.json -> Table 4 rows and the nominal columns of Table 2).
- fig7.py        : Figures 7 (CIE xy, measured D65 chromaticities) and 8 (CIELAB a*b*, nominal) with the incorrect-selection percentages of Table 10.
- spectra/ : illuminant spectra (D65, Red, Green, Blue; 1 nm, 380-780) and CIE 1931 2-deg CMFs, from the public repository.
- refl/    : NCS sample reflectances (4 nm), from the public repository (Assets/Resources).
- measured05_D65.json : chromaticity and luminance of the nine patches measured through the HMD under D65 (mean of three scene positions).
Run from this folder; the scripts write figures to ../figures by default.

R (statistics), folder R/:
- 01_..._05_*.R  : original analysis scripts (data preparation, plots, GLMMs).
- analysis_glmm.R : reproducible script that regenerates Tables 7-10 and every statistical value of the Results section from
  Datasets/vr_chromatic_adaptation_datasets.csv (run from the repository root). Trials without a valid selection are scored as incorrect.
- analysis_output.txt : console output of analysis_glmm.R with R 4.3.3, glmmTMB 1.1.8, emmeans 1.10.0.
