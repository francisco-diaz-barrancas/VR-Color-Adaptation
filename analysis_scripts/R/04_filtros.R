library(dplyr)
datos_filtrado <- datos_final %>% filter(!is.na(Edad), Edad <= 60)
rendimiento <- datos_filtrado %>% group_by(Sujeto) %>%
  summarise(aciertos=sum(Valor==1,na.rm=TRUE),accuracy=mean(Valor,na.rm=TRUE),.groups="drop") %>%
  arrange(desc(aciertos),desc(accuracy))
top10 <- slice_head(rendimiento,n=10)
datos_top10 <- datos_filtrado %>% filter(Sujeto %in% top10$Sujeto)
print(top10)
