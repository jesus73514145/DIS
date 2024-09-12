// Función para mostrar la tarjeta
function mostrarTarjeta() {
    const tarjeta = document.getElementById("tarjeta");
    tarjeta.classList.add("abierta");
}

// Función para cerrar la tarjeta
function cerrarTarjeta() {
    const tarjeta = document.getElementById("tarjeta");
    tarjeta.classList.remove("abierta");
}

// Asociar funciones a los botones
document.getElementById("mostrarTarjeta").addEventListener("click", mostrarTarjeta);
document.getElementById("cerrarTarjeta").addEventListener("click", cerrarTarjeta);
