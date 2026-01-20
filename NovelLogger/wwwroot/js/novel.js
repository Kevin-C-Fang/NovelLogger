var dataTable;

$(document).ready(function () {
    loadDataTable();
})

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/Novel/getall' },
        autoWidth: false,

        "columns": [
            { data: 'novelTitle', "width": "50%", className: "td-truncate" },
            { data: 'novelStatus', "width": "25%" },
            {
                data: 'novelId',
                "render": function (data) {
                    return `<div class="w-100 btn-group" role="group">
                        <a href="/Novel/Edit?novelId=${data}" class="btn btn-primary mx-2"> <i class="bi bi-pencil-square"></i> Edit</a>
                        <a onClick=Delete('/Novel/Delete?novelId=${data}') class="btn btn-danger mx-2"> <i class="bi bi-trash-fill"></i> Delete</a>
                    </div>`
                },
                "width": "25%"
            }
        ]
    });
}

function Delete(url) {
    Swal.fire({
        title: "Are you sure?",
        text: "This will also delete bookmarks with the same novel title! You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function (data) {
                    dataTable.ajax.reload();
                }
            })
        }
    });
}
