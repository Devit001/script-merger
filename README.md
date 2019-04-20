# script merger

Programa para unir varios archivos en un solo script SQL de acuerdo a los que se listen en la carpeta especificada. 
- Soporta comentarios de una sola línea 
- Esribe el nombre del archivo antes de p
- Guarda las últimas 5 rutas utilizadas
- El ejecutable generado es portable

### Notas
Los comentarios deben iniciar con **//**
Los archivos de la carpeta seleccionada se procesan alfabéticamente, pero el contenido de estos se procesa tal y como está.
Se recomienda que los archivos estén codificados con **UTF-8**. 




### Cosas que podrían mejorarse

Una pantalla para escoger algunas preferencias como:
- Nombre del directorio raíz para Scripts (Default "Scripts")
- Cantidad de rutas a recordar (Default 5)
- Separador de scripts (Default "GO")
- Tamaño del buffer (Default 512 KB)
