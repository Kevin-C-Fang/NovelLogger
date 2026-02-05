var dataTable;

$(document).ready(function () {
    loadDataTable();
})

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/Novel/getall' },
        autoWidth: false,

        "columns": [
            { data: 'novelTitle', "width": "35%" },
            { data: 'novelStatus', "width": "35%" },
            {
                data: 'novelId',
                "render": function (data) {
                    return `
                    <div class="dropdown">
                      <button class="btn btn-primary dropdown-toggle w-100" data-bs-toggle="dropdown">Actions</button>
                      <ul class="dropdown-menu">
                        <li><a href="/Novel/Edit?novelId=${data}" class="dropdown-item">Edit</a></li>
                        <li><a href="#" onClick=Delete('/Novel/Delete?novelId=${data}') class="dropdown-item text-danger">Delete</a></li>
                      </ul>
                    </div>
                    `
                },
                "width": "30%",
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
