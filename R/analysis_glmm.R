# Reproducible statistical analysis for
# "Perceptual evaluation of color rendering consistency under chromatic virtual illumination in immersive virtual reality"
# Input : Datasets/vr_chromatic_adaptation_datasets.csv (public repository)
# Output: all quantities reported in Tables 7-10 and in the Results section (printed to the console).
# Requires: glmmTMB, emmeans. Numbers in the manuscript were obtained with R 4.3.3, glmmTMB 1.1.8, emmeans 1.10.0.

suppressMessages({ library(glmmTMB); library(emmeans) })

d <- read.csv("Datasets/vr_chromatic_adaptation_datasets.csv")
# Trials without a valid selection (selection_valid == 0; 8 of 960) are scored as incorrect,
# as in the original data-preparation script (01_unificar_datos.R).
d$Valor      <- ifelse(d$selection_valid == 1, d$correct, 0)
d$Sujeto     <- factor(d$participant_id)
d$Tiempo     <- factor(d$time_seconds, levels = c(20, 40, 60, 80, 100, 120),
                       labels = c("20s", "40s", "60s", "80s", "100s", "120s"))
d$Iluminante <- factor(d$illuminant, levels = c("Red", "Blue", "D65", "Green"))
d$Tiempo_num <- d$time_seconds

## ---- Table 7: accuracy per exposure time and illuminant, with 95% Wilson confidence intervals
wilson <- function(k, n, z = 1.96) {
  p <- k / n; den <- 1 + z^2 / n
  c <- (p + z^2 / (2 * n)) / den; h <- z * sqrt(p * (1 - p) / n + z^2 / (4 * n^2)) / den
  c(mean = p, lo = c - h, hi = c + h)
}
tab5 <- aggregate(Valor ~ time_seconds + Iluminante, d, function(v) wilson(sum(v), length(v)))
print(tab5, digits = 3)

## ---- Primary model: Time as a six-level factor x Illuminant, random intercept per participant
# Note: glmmTMB reports a 'singular convergence' warning for this model; it is caused by the quasi-complete separation
# of the Red condition at 20 s (no correct response), which the manuscript discusses. Estimates for the other cells are unaffected.
mod <- glmmTMB(Valor ~ Tiempo * Iluminante + (1 | Sujeto), data = d, family = binomial(link = "logit"))

## Likelihood-ratio tests for the fixed effects
add <- glmmTMB(Valor ~ Tiempo + Iluminante + (1 | Sujeto), data = d, family = binomial)
noT <- glmmTMB(Valor ~ Iluminante + (1 | Sujeto), data = d, family = binomial)
noI <- glmmTMB(Valor ~ Tiempo + (1 | Sujeto), data = d, family = binomial)
print(anova(noT, add))   # effect of Time
print(anova(noI, add))   # effect of Illuminant
print(anova(add, mod))   # Time x Illuminant interaction

## ---- Table 8: planned contrasts between exposure times within each illuminant (unadjusted)
emm  <- emmeans(mod, ~ Tiempo | Iluminante)
comp <- contrast(emm, method = list("20s vs 60s"  = c(-1, 0, 1, 0, 0, 0),
                                    "80s vs 120s" = c(0, 0, 0, -1, 0, 1)))
print(summary(comp, infer = TRUE))

## ---- Table 9: illuminant comparisons averaged over exposure time (odds ratios, Tukey-adjusted)
emm_il <- emmeans(mod, ~ Iluminante)
print(summary(emm_il, type = "response"))
print(pairs(emm_il, type = "response"))

## ---- Slopes: Time as a continuous covariate (seconds) x Illuminant
mod_trend <- glmmTMB(Valor ~ Tiempo_num * Iluminante + (1 | Sujeto), data = d, family = binomial(link = "logit"))
print(summary(emtrends(mod_trend, ~ Iluminante, var = "Tiempo_num")))
print(summary(emtrends(mod_trend, pairwise ~ Iluminante, var = "Tiempo_num")$contrasts))

## ---- Table 10: distribution of incorrect selections
inc <- d[d$selection_valid == 1 & d$correct == 0, ]
print(round(100 * prop.table(table(inc$illuminant, inc$selected_chip), 1), 1))

cat("\nR", R.version$major, ".", R.version$minor, " glmmTMB ", as.character(packageVersion("glmmTMB")),
    " emmeans ", as.character(packageVersion("emmeans")), "\n", sep = "")
