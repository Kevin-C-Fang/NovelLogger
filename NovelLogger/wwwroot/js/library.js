var dataTable;

$(document).ready(function () {
    loadDataTable();
})

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/library/getall' },

        "columns": [
            { data: 'novel.title', "width": "20%" },
            {
                data: 'url',
                "width": "30%",
                "render": function (data) {
                    return `<a href="${data}">${data}</a>`
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
                data: 'id',
                "render": function (data) {
                    return `<div class="w-75 btn-group" role="group">
                        <a href="/Library/ViewBookmark?id=${data}" class="btn btn-primary mx-2"> <i class="bi bi-folder2-open"></i> View</a>
                        <a href="/Library/Edit?id=${data}" class="btn btn-primary mx-2"> <i class="bi bi-pencil-square"></i> Edit</a>
                        <a href="/Library/Delete?id=${data}" class="btn btn-danger mx-2"> <i class="bi bi-trash-fill"></i> Delete</a>
                    </div>`
                },
                "width": "25%"
            }
        ]
    });
}
