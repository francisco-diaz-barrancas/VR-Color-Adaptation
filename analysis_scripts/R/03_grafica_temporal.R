library(dplyr)
library(ggplot2)
resumen <- datos_final %>%
  filter(Iluminante %in% c("Red","Blue","D65","Green"),
         Tiempo %in% c(20,40,60,80,100,120), Valor %in% c(0,1)) %>%
  group_by(Iluminante,Tiempo) %>%
  summarise(Accuracy=mean(Valor),.groups="drop")
ggplot(resumen,aes(Tiempo,Accuracy,color=Iluminante,group=Iluminante))+
  geom_line(linewidth=1)+geom_point(size=2.5)+
  scale_color_manual(values=c(Red="red",Blue="blue",D65="grey50",Green="green"))+
  scale_x_continuous(breaks=c(20,40,60,80,100,120))+
  coord_cartesian(ylim=c(0,1))+labs(x="Time (s)",y="Accuracy")+
  theme_minimal(base_size=14)
