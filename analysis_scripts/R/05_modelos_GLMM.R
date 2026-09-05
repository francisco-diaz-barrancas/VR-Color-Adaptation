library(glmmTMB)
library(emmeans)
datos_final$Sujeto <- factor(datos_final$Sujeto)
datos_final$Tiempo <- factor(datos_final$Tiempo,levels=c(20,40,60,80,100,120),
 labels=c("20s","40s","60s","80s","100s","120s"))
datos_final$Iluminante <- factor(datos_final$Iluminante,levels=c("Red","Blue","D65","Green"))
mod <- glmmTMB(Valor ~ Tiempo*Iluminante+(1|Sujeto),data=datos_final,
 family=binomial(link="logit"))
emm <- emmeans(mod,~Tiempo|Iluminante)
comp <- contrast(emm,method=list("20s vs 60s"=c(-1,0,1,0,0,0),
 "80s vs 120s"=c(0,0,0,-1,0,1)))
summary(comp,infer=TRUE)

datos_trend <- datos_final
datos_trend$Tiempo_num <- c(20,40,60,80,100,120)[as.numeric(datos_trend$Tiempo)]
mod_trend <- glmmTMB(Valor ~ Tiempo_num*Iluminante+(1|Sujeto),
 data=datos_trend,family=binomial(link="logit"))
pendientes <- emtrends(mod_trend,~Iluminante,var="Tiempo_num")
summary(pendientes)
comparaciones <- emtrends(mod_trend,pairwise~Iluminante,var="Tiempo_num")
summary(comparaciones$contrasts)
