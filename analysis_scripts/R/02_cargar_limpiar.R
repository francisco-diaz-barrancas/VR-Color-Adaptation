archivo <- "G:/Otros ordenadores/Mi PC/Francisco/Documentos/Universidad/PostDoc Universidad Extremadura/Artículos/Color Adaptation in VR/Datos/datos_finales.csv"
datos_raw <- read.csv(archivo, stringsAsFactors=FALSE)
if(ncol(datos_raw)==1){
  datos_final <- as.data.frame(do.call(rbind,strsplit(datos_raw[[1]],",")), stringsAsFactors=FALSE)
  names(datos_final) <- c("Sujeto","Edad","Iluminante","Tiempo","Valor")
} else {
  datos_final <- datos_raw
  names(datos_final)[names(datos_final)=="Lighting"] <- "Iluminante"
}
for(v in c("Sujeto","Iluminante","Tiempo","Valor","Edad"))
  datos_final[[v]] <- trimws(gsub('"','',as.character(datos_final[[v]])))
datos_final$Edad <- as.numeric(datos_final$Edad)
datos_final$Tiempo <- as.numeric(datos_final$Tiempo)
datos_final$Valor <- as.numeric(datos_final$Valor)
