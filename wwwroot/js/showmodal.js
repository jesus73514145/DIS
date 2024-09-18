// Asegúrate de incluir jQuery y Bootstrap en tu proyecto
// Puedes usar los siguientes CDN si aún no los has incluido
// <script src="https://cdnjs.cloudflare.com/ajax/libs/jquery/3.6.0/jquery.min.js"></script>
// <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/js/bootstrap.bundle.min.js"></script>

$(document).ready(function () {
    $('#btnOpenModal').click(function () {
        var modal = new bootstrap.Modal(document.getElementById('exampleModal'), {
            keyboard: false
        });
        modal.show();
    });
});