$(document).ready(function () {
    let IsMain = $('#IsMain').is(':checked');

    if (IsMain) {
        $('#fileInput').removeClass('d-none')
        $('#parentList').addClass('d-none')
    } else {
        $('#parentList').removeClass('d-none')
        $('#fileInput').addClass('d-none')

    }

    $('#IsMain').click(function () {
        let IsMain = $(this).is(':checked');

        if (IsMain) {
            $('#fileInput').removeClass('d-none')
            $('#parentList').addClass('d-none')
        } else {
            $('#parentList').removeClass('d-none')
            $('#fileInput').addClass('d-none')

        }
    })
})