# Taller de Azure Functions

Descripción del Problema
Una empresa tiene unos relojes electrónicos que sirven para tomar el tiempo de entrada y salida de los empleados. Estos dispositivos pueden consumir servicios HTTP para que un empleado marque la entrada o la salida.

Adicionalmente, el departamento de TI está construyendo una interfaz WEB en Angular para poder hacer la administración del sistema de control de tiempos de los empleados.
Para satisfacer ambas necesidades, usted debe construir una REST API en Azure Functions que tenga los métodos:

Crear entrada.
Editar entrada.
Obtener todas las entradas.
Obtener entrada por ID
Borrar entrada.

Y debe almacenar la siguiente información en una TABLA de un BLOB STORAGE de Azure:

ID Empleo (int)
Fecha/Hora de entrada (date time)
Fecha/Hora de salida (date time)
Tipo (0: Entrada, 1: Salida)
Consolidado (bool)

Cada que agregue un nuevo registro, el campo de consolidado queda en falso. Adicional a esto debes de correr un proceso todos los días a las 23:59 que consolide la información de registros producidos durante el día en otra TABLA de un BLOB STORAGE y almacene esta información:

ID Empleo (int)
Fecha (date time)
Tiempo trabajado (HH:MM)

Cada que consolide un trabajo coloca el valor consolidado en el detalle verdadero. Debes de crear otro método GET que pasándo como parámetro la fecha, debe retornar el consolidado de tiempos de un día.

Todos los métodos deben tener al menos una prueba unitaria.

