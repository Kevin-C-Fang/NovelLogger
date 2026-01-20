var dataTable;

$(document).ready(function () {
    loadDataTable();
})

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/Bookmark/getall' },
        autoWidth: false,
        "columns": [
            { data: 'novel.title', "width": "35%" },
            {
                data: 'url',
                width: "25%",
                render: function (data) {
                    return `<a href="${data}" target="_blank">${data}</a>`
                },
                className: "td-truncate"
            },
            {
                data: 'dateAdded',
                width: "10%",
                render: {
                    _: 'display',  
                    sort: 'sort'   
                }
            },
            { data: 'hasNotes', "width": "10%" },
            { data: 'isSaved', "width": "10%" },
            {
                data: 'bookmarkId',
                "render": function (data) {
                    return`
                    <div class="dropdown">
                      <button class="btn btn-primary dropdown-toggle w-100 action-btn" data-bs-toggle="dropdown">Actions</button>
                      <ul class="dropdown-menu">
                        <li><a href="/Bookmark/ViewBookmark?bookmarkId=${data}" class="dropdown-item">View</a></li>
                        <li><a href="/Bookmark/Edit?bookmarkId=${data}" class="dropdown-item">Edit</a></li>
                        <li><a href="#" onClick=Delete('/Bookmark/Delete?bookmarkId=${data}') class="dropdown-item text-danger">Delete</a></li>
                      </ul>
                    </div>
                    `
                },
                "width": "10%"
            }
        ]
    });
}


function Delete(url) {
    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
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
