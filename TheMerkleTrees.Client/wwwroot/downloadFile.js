function downloadFileFromUrl(fileName, fileUrl) {
    var link = document.createElement('a');
    link.download = fileName;
    link.href = fileUrl;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}