$(document).ready(function () {
    // Código para abrir el primer modal
    $('#btnOpenModal').click(function () {
        var modal = new bootstrap.Modal(document.getElementById('exampleModal'), {
            keyboard: false
        });
        modal.show();
    });

    // Código para abrir el modal de costeo
    $('#btnOpenCosteoModal').click(function () {
        var modal = new bootstrap.Modal(document.getElementById('costeoModal'), {
            keyboard: false
        });
        modal.show();
    });

    // Maneja el cierre del primer modal
    $('#exampleModal').on('hidden.bs.modal', function (e) {
        $('body').removeClass('modal-open');
        $('.modal-backdrop').remove();
    });

    // Maneja el cierre del modal de costeo
    $('#costeoModal').on('hidden.bs.modal', function (e) {
        $('body').removeClass('modal-open');
        $('.modal-backdrop').remove();
    });
});
