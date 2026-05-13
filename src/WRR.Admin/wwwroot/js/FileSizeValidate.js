

    //this code will be executed when a new file is selected
$('#FileUpload2').bind('change', function () {

            //converts the file size from bytes to MB
            var fileSize = this.files[0].size / 1024 / 1024;

    //checks whether the file is .png and less than 1 MB
            if (fileSize >= 5) {
                alert("Image size is too large");
                $('#FileUpload2').val('');
}
});

$('#FileUpload1').bind('change', function (e) {
    e.preventDefault();
            //converts the file size from bytes to MB
     var fileSize = this.files[0].size / 1024 / 1024;

    //checks whether the file is .png and less than 1 MB
            if (fileSize >= 5) {
                alert("Image size is too large");
                $('#FileUpload1').val('');
}
});

$('#file').bind('change', function (e) {
    e.preventDefault();
    //converts the file size from bytes to MB
    var fileSize = this.files[0].size / 1024 / 1024;

    //checks whether the file is .png and less than 1 MB
    if (fileSize >= 5) {
        alert("Image size is too large");
        //e.currentTarget.parentNode.clear();
        $('#file').val('');
    }
});
