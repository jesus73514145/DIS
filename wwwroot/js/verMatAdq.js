const modals = document.querySelectorAll('.modalExportar');
const showButtons = document.querySelectorAll('.showModalExportar');
const closeButtons = document.querySelectorAll('.closeModalExportar');

showButtons.forEach((button, index) => {
    button.addEventListener('click', () => {
        modals[index].showModal(); // Abre el modal correspondiente al botón
    });
});

closeButtons.forEach((button, index) => {
    button.addEventListener('click', () => {
        modals[index].close(); // Cierra el modal correspondiente
    });
});
