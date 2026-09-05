"""Colorimetric computations for the revision (Fig. 5, per-illuminant stimulus tables, CIELAB Fig. 7).
Uses the spectral data of the public repo (Assets/Ficheros, Assets/Resources)."""
import numpy as np, json, glob, os
import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt

def rd(p):
    return np.array([float(l.strip().replace(',', '.')) for l in open(p) if l.strip()])

WL = np.arange(380, 781)                       # 1 nm
CMF = np.array([[float(v.replace(',', '.')) for v in l.strip().split(';')]
                for l in open('spectra/xbarybarzbar.csv') if l.strip()])   # CIE 1931 2deg
ILL = {n: rd(f'spectra/{n}.csv') for n in ['D65', 'Red', 'Green', 'Blue']}

# Optional measured luminance of the scene white (cd/m2) -> absolute spectral radiance; None -> relative
LUM = json.load(open('luminance.json')) if os.path.exists('luminance.json') else None

def XYZ(spd, refl=None, wl_step=1):
    s = spd if refl is None else spd * refl
    return (s[:, None] * CMF[::wl_step]).sum(0)

def xy(X):
    return X[0] / X.sum(), X[1] / X.sum()

def upvp(X):
    X_, Y_, Z_ = X
    d = X_ + 15 * Y_ + 3 * Z_
    return 4 * X_ / d, 9 * Y_ / d

def lab(X, Xw):
    f = lambda t: np.where(t > (6/29)**3, np.cbrt(t), t / (3 * (6/29)**2) + 4/29)
    r = f(X / Xw)
    return 116 * r[1] - 16, 500 * (r[0] - r[1]), 200 * (r[1] - r[2])

def de00(l1, l2, kL=1, kC=1, kH=1):
    L1, a1, b1 = l1; L2, a2, b2 = l2
    C1 = np.hypot(a1, b1); C2 = np.hypot(a2, b2); Cb = (C1 + C2) / 2
    G = 0.5 * (1 - np.sqrt(Cb**7 / (Cb**7 + 25**7)))
    a1p, a2p = (1 + G) * a1, (1 + G) * a2
    C1p, C2p = np.hypot(a1p, b1), np.hypot(a2p, b2)
    h = lambda a, b: (np.degrees(np.arctan2(b, a)) + 360) % 360 if (a != 0 or b != 0) else 0.0
    h1p, h2p = h(a1p, b1), h(a2p, b2)
    dLp = L2 - L1; dCp = C2p - C1p
    if C1p * C2p == 0: dhp = 0
    elif abs(h2p - h1p) <= 180: dhp = h2p - h1p
    elif h2p - h1p > 180: dhp = h2p - h1p - 360
    else: dhp = h2p - h1p + 360
    dHp = 2 * np.sqrt(C1p * C2p) * np.sin(np.radians(dhp / 2))
    Lbp = (L1 + L2) / 2; Cbp = (C1p + C2p) / 2
    if C1p * C2p == 0: hbp = h1p + h2p
    elif abs(h1p - h2p) <= 180: hbp = (h1p + h2p) / 2
    elif h1p + h2p < 360: hbp = (h1p + h2p + 360) / 2
    else: hbp = (h1p + h2p - 360) / 2
    T = 1 - 0.17 * np.cos(np.radians(hbp - 30)) + 0.24 * np.cos(np.radians(2 * hbp)) \
        + 0.32 * np.cos(np.radians(3 * hbp + 6)) - 0.20 * np.cos(np.radians(4 * hbp - 63))
    dth = 30 * np.exp(-((hbp - 275) / 25)**2)
    RC = 2 * np.sqrt(Cbp**7 / (Cbp**7 + 25**7))
    SL = 1 + 0.015 * (Lbp - 50)**2 / np.sqrt(20 + (Lbp - 50)**2)
    SC = 1 + 0.045 * Cbp; SH = 1 + 0.015 * Cbp * T
    RT = -np.sin(np.radians(2 * dth)) * RC
    return np.sqrt((dLp / (kL * SL))**2 + (dCp / (kC * SC))**2 + (dHp / (kH * SH))**2
                   + RT * (dCp / (kC * SC)) * (dHp / (kH * SH)))

# ---------------- Figure 5 ----------------
def fig5(out='../figures/LightSources.png'):
    fig, ax = plt.subplots(figsize=(6.6, 4.4), dpi=300)
    cols = {'D65': 'k', 'Red': '#d62728', 'Green': '#2ca02c', 'Blue': '#1f77b4'}
    ybar = CMF[:, 1]
    for n, s in ILL.items():
        if LUM:  # absolute spectral radiance: Lv = 683 * sum(Le * ybar) dlambda
            Le = s * LUM[n] / (683.0 * (s * ybar).sum())
            ax.plot(WL, Le * 1e3, color=cols[n], lw=1.4, label=n)
        else:    # relative spectral radiance normalised to unit luminance factor (Y = 100)
            ax.plot(WL, s / s.max(), color=cols[n], lw=1.4, label=n)
    ax.set_xlim(380, 780); ax.set_xticks(range(380, 781, 50))
    ax.set_xlabel('Wavelength (nm)')
    if LUM:
        ax.set_ylabel(r'Spectral radiance (mW$\cdot$sr$^{-1}\cdot$m$^{-2}\cdot$nm$^{-1}$)')
    else:
        ax.set_ylabel('Relative spectral radiance\n(peak-normalized, dimensionless)')
    ax.set_ylim(bottom=0); ax.grid(alpha=.3); ax.legend(frameon=False)
    fig.tight_layout(); fig.savefig(out); plt.close(fig)

# ---------------- per-illuminant stimulus colorimetry ----------------
PATCHES = ['S0500N', 'S0505Y', 'S0505Y50R', 'S0505R', 'S0505R50B', 'S0505B', 'S0505B50G', 'S0505G', 'S0505G50Y']
def refl(name):
    f = glob.glob(f'refl/*/{name}.csv')[0]
    r4 = rd(f) / 100.0            # 101 samples, 4 nm (380..780)
    return np.interp(WL, np.arange(380, 781, 4), r4)   # linear interpolation to 1 nm

def stimulus_tables():
    rows = {}
    for n, s in ILL.items():
        Xw = XYZ(s, np.ones(401), 1)               # perfect white under illuminant (1 nm)
        Xw = Xw / Xw[1] * 100
        scale = 100 / XYZ(s, np.ones(401), 1)[1]
        res = []
        for p in PATCHES:
            X = XYZ(s, refl(p), 1) * scale         # Y = luminance factor (%)
            res.append(dict(patch=p, x=xy(X)[0], y=xy(X)[1], up=upvp(X)[0], vp=upvp(X)[1], Y=X[1],
                            lab=lab(X, Xw)))
        for r in res:
            r['dE'] = de00(res[0]['lab'], r['lab'])
        rows[n] = dict(white=dict(x=xy(Xw)[0], y=xy(Xw)[1], up=upvp(Xw)[0], vp=upvp(Xw)[1]), patches=res)
    return rows

if __name__ == '__main__':
    fig5()
    T = stimulus_tables()
    json.dump(T, open('stimuli_by_illuminant.json', 'w'), indent=1, default=float)
    for n, d in T.items():
        print(f"\n{n}: white xy=({d['white']['x']:.4f},{d['white']['y']:.4f}) u'v'=({d['white']['up']:.4f},{d['white']['vp']:.4f})")
        for r in d['patches']:
            L, a, b = r['lab']
            print(f"  {r['patch']:10s} x={r['x']:.4f} y={r['y']:.4f} Y={r['Y']:6.2f}  L*={L:6.2f} a*={a:6.2f} b*={b:6.2f}  dE00={r['dE']:.2f}")
