var dataTable;

$(document).ready(function () {
    loadDataTable();
})

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/Bookmark/getall' },

        "columns": [
            { data: 'novel.title', "width": "20%" },
            {
                data: 'url',
                "width": "30%",
                "render": function (data) {
                    return `<a href="${data}" target="_blank">${data}</a>`
                }
            },
            {
                data: 'dateAdded',
                width: "10%",
                render: {
                    _: 'display',  
                    sort: 'sort'   
                }
            },
            { data: 'hasNotes', "width": "5%" },
            { data: 'isSaved', "width": "5%" },
            {
                data: 'bookmarkId',
                "render": function (data) {
                    return `<div class="w-75 btn-group" role="group">
                        <a href="/Bookmark/ViewBookmark?bookmarkId=${data}" class="btn btn-primary mx-2"> <i class="bi bi-folder2-open"></i> View</a>
                        <a href="/Bookmark/Edit?bookmarkId=${data}" class="btn btn-primary mx-2"> <i class="bi bi-pencil-square"></i> Edit</a>
                        <a onClick=Delete('/Bookmark/Delete?bookmarkId=${data}') class="btn btn-danger mx-2"> <i class="bi bi-trash-fill"></i> Delete</a>
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
