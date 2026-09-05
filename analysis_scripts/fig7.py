"""Regenerate Fig. 7 (CIE xy) and its CIELAB companion with the corrected patch labels."""
import numpy as np, matplotlib
matplotlib.use('Agg')
import matplotlib.pyplot as plt

# Table 2 (05 set): measured x, y through the HMD under D65; nominal L*, a*, b* (spectral specification)
T2 = {'S0500N': (0.3144, 0.3299, 94.04, 0.18, 1.26), 'S0505Y': (0.3235, 0.3446, 94.36, -2.14, 9.55), 'S0505Y50R': (0.3295, 0.3418, 91.61, 2.02, 9.05), 'S0505R': (0.3182, 0.3285, 92.17, 3.18, 2.06), 'S0505R50B': (0.3104, 0.3215, 91.12, 2.36, -2.86), 'S0505B': (0.3043, 0.3247, 91.43, -2.54, -3.36), 'S0505B50G': (0.3063, 0.3276, 92.64, -3.66, -1.29), 'S0505G': (0.3103, 0.3332, 93.01, -3.94, 1.96), 'S0505G50Y': (0.3188, 0.3416, 94.71, -4.02, 7.28)}
COLS = ['S0505B', 'S0505B50G', 'S0505G', 'S0505G50Y', 'S0505R', 'S0505R50B', 'S0505Y', 'S0505Y50R']
T8 = {'D65': [11.0, 33.0, 19.8, 0.0, 15.4, 18.7, 0.0, 2.2], 'Blue': [5.6, 4.7, 7.5, 5.6, 27.1, 14.0, 15.0, 20.6],
      'Green': [4.0, 7.3, 7.3, 2.6, 19.9, 58.9, 0.0, 0.0], 'Red': [9.5, 24.8, 43.8, 17.1, 1.9, 1.9, 0.0, 1.0]}
PC = {'S0505B': '#3b6fd1', 'S0505B50G': '#3aa89a', 'S0505G': '#4caf50', 'S0505G50Y': '#9ccc3c',
      'S0505R': '#d9534f', 'S0505R50B': '#9b59b6', 'S0505Y': '#e6b800', 'S0505Y50R': '#f08a24'}
ILL = {'D65': (0.3017, 0.3224), 'Red': (0.3163, 0.2559), 'Green': (0.2922, 0.3745), 'Blue': (0.2497, 0.2684)}

def lab_dir(xy):  # a*b* hue direction of an illuminant chromaticity relative to the D65 white
    def XYZ(x, y): return np.array([x / y * 100, 100, (1 - x - y) / y * 100])
    w = XYZ(*ILL['D65']); s = XYZ(*xy)
    f = np.cbrt(s / w)
    a, b = 500 * (f[0] - f[1]), 200 * (f[1] - f[2])
    return np.array([a, b]) / np.hypot(a, b)

def panel(ax, il, space):
    p = T8[il]
    for c, pp in zip(COLS, p):
        if space == 'xy': X, Y = T2[c][0], T2[c][1]
        else: X, Y = T2[c][3], T2[c][4]
        ax.scatter(X, Y, s=30 + pp * 14, color=PC[c], alpha=.85, edgecolor='k', lw=.5, zorder=3)
        tx0, ty0 = (T2['S0500N'][0], T2['S0500N'][1]) if space == 'xy' else (T2['S0500N'][3], T2['S0500N'][4])
        v = np.array([X - tx0, Y - ty0]); v = v / (np.linalg.norm(v) + 1e-9)
        r = 10 + np.sqrt(30 + pp * 14) * 0.55
        ax.annotate(f"{c}\n{pp:.1f}%", (X, Y), textcoords='offset points', xytext=(v[0] * r * 1.7, v[1] * r * 1.1),
                    ha='center', va='center', fontsize=6.2, zorder=6)
    tx, ty = (T2['S0500N'][0], T2['S0500N'][1]) if space == 'xy' else (T2['S0500N'][3], T2['S0500N'][4])
    ax.scatter(tx, ty, marker='x', s=70, color='k', lw=1.5, zorder=4)
    ax.annotate('S0500N', (tx, ty), textcoords='offset points', xytext=(0, -11), ha='center', fontsize=7, fontweight='bold')
    # weighted centroid of incorrect selections
    w = np.array(p);
    if space == 'xy': cx = sum(w[i] * T2[c][0] for i, c in enumerate(COLS)) / w.sum(); cy = sum(w[i] * T2[c][1] for i, c in enumerate(COLS)) / w.sum()
    else: cx = sum(w[i] * T2[c][3] for i, c in enumerate(COLS)) / w.sum(); cy = sum(w[i] * T2[c][4] for i, c in enumerate(COLS)) / w.sum()
    ax.annotate('', xy=(cx, cy), xytext=(tx, ty), arrowprops=dict(arrowstyle='-|>', color='k', lw=1.0), zorder=5)
    if il != 'D65':
        if space == 'xy':
            d = np.array(ILL[il]) - np.array(ILL['D65']); d = d / np.linalg.norm(d) * 0.010
        else:
            d = lab_dir(ILL[il]) * 3.5
        ax.annotate('', xy=(tx + d[0], ty + d[1]), xytext=(tx, ty),
                    arrowprops=dict(arrowstyle='-|>', color='0.45', lw=1.0, ls='--'), zorder=2)
        ax.text(tx + d[0] * 1.25, ty + d[1] * 1.25, 'illuminant\nhue direction', fontsize=5, color='0.35', ha='center', va='center')
    ax.set_title(il, fontsize=12); ax.grid(alpha=.3)
    if space == 'xy':
        ax.set_xlim(0.2985, 0.3355); ax.set_ylim(0.3145, 0.3515); ax.set_xlabel('$x$'); ax.set_ylabel('$y$'); ax.set_aspect('equal')
        ax.set_xticks(np.arange(0.300, 0.3351, 0.005)); ax.set_yticks(np.arange(0.315, 0.3501, 0.005)); ax.tick_params(labelsize=8)
    else:
        ax.set_xlim(-8.0, 8.0); ax.set_ylim(-6.5, 12.5); ax.set_aspect('equal'); ax.set_xlabel('$a^*$'); ax.set_ylabel('$b^*$'); ax.tick_params(labelsize=8)
        th = np.linspace(0, 2 * np.pi, 200); ax.plot(tx + 5 * np.cos(th), ty + 5 * np.sin(th), ls=':', lw=.6, color='grey', zorder=1)

for space, out in [('xy', '../figures/selected_chips_cromaticity.png'), ('lab', '../figures/selected_chips_cielab.png')]:
    fig, axs = plt.subplots(2, 2, figsize=(9.5, 9.0 if space == 'lab' else 8.6), dpi=300)
    for ax, il in zip(axs.ravel(), ['D65', 'Blue', 'Green', 'Red']):
        panel(ax, il, space)
    fig.tight_layout(); fig.savefig(out); plt.close(fig)
print('done')
