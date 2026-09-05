library(readxl)

carpeta <- "G:/Otros ordenadores/Mi PC/Francisco/Documentos/Universidad/PostDoc Universidad Extremadura/Artículos/Color Adaptation in VR/Datos"
archivos <- list.files(carpeta, pattern = "\\.xlsx$", full.names = TRUE)

extraer_edad <- function(f) {
  x <- gsub("\\.xlsx$", "", basename(f))
  n <- as.numeric(regmatches(x, gregexpr("[0-9]+", x))[[1]])
  n <- n[n != 1]
  if (length(n)) n[1] else NA
}
extraer_nombre <- function(f) {
  x <- gsub("\\.xlsx$", "", basename(f))
  gsub("[_()].*", "", x)
}
procesar <- function(f) {
  d <- read_excel(f)
  data.frame(Sujeto=extraer_nombre(f), Edad=extraer_edad(f),
             Lighting=d$Lighting, Tiempo=d$Time,
             Valor=ifelse(grepl("S05N", d$`Choose Option`),1,0))
}
datos_final <- do.call(rbind, lapply(archivos, procesar))
write.csv(datos_final, file.path(carpeta,"datos_finales.csv"), row.names=FALSE)
